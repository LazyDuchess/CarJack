#if PLUGIN
using DG.Tweening;
using Reptile;
#endif
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

namespace CarJack.Common
{
    public class DrivableCar : MonoBehaviour
    {
        private const int CurrentVersion = 1;
        [SerializeField]
        private int Version = 0;

        [Header("Unique identifier")]
        public string InternalName = "";

        [Header("How much the car should countersteer to avoid losing control")]
        public float CounterSteerMultiplier = 0.1f;

        [Header("How sensible the car is to sliding")]
        public float SlipMultiplier = 0.75f;

        public bool HasSurfaceAngleLimit = true;
        public float SurfaceAngleLimit = 60f;

        public float Deacceleration = 100f;

        [Header("Curves based on the car's speed")]
        public AnimationCurve ReverseCurve;
        public float ReverseCurveMax = 50f;
        public float BrakeForce = 1000f;
        public float HandBrakeForce = 500f;
        public AnimationCurve SteerCurve;
        public float SteerCurveMax = 50f;
        public AnimationCurve SpeedCurve;
        public float SpeedCurveMax = 50f;
        public AnimationCurve TractionCurve;
        public float TractionCurveMax = 50f;
        public LayerMask GroundMask;
        public Transform CenterOfMass;
        [NonSerialized]
        public Rigidbody Rigidbody;
        [NonSerialized]
        public CarWheel[] Wheels;
        [NonSerialized]
        public GameObject Chassis;
        [NonSerialized]
        public bool Driving = false;
        [NonSerialized]
        public bool InCar = false;

        private const float ControllerRotationDeadZone = 0.2f;
        [NonSerialized]
        public bool InputEnabled = true;
        [NonSerialized]
        public float ThrottleAxis = 0f;
        [NonSerialized]
        public float SteerAxis = 0f;
        [NonSerialized]
        public bool HornHeld = false;
        [NonSerialized]
        public bool GetOutOfCarButtonNew = false;
        [NonSerialized]
        public float PitchAxis = 0f;
        [NonSerialized]
        public float YawAxis = 0f;
        [NonSerialized]
        public float RollAxis = 0f;
        [NonSerialized]
        public bool BrakeHeld = false;
        [NonSerialized]
        public bool LockDoorsButtonNew = false;

        private Vector3 _velocityBeforePause;
        private Vector3 _angularVelocityBeforePause;

        private Vector3 _previousVelocity = Vector3.zero;
        private Vector3 _previousAngularVelocity = Vector3.zero;

        private OneShotAudioSource _oneShotAudioSource;

        private ScrapeAudio _scrapeAudio;

        private float _crashAudioCooldown = 0f;

        [NonSerialized]
        public CarDriverSeat DriverSeat;

        public Action OnHandleInput;

        [Header("Air Control")]
        public float AirControlStrength = 1f;
        public float AirControlTopSpeed = 2f;
        public float AirDeacceleration = 1f;

        [Header("How much you can alter the car's direction in the air.")]
        public float AirAerodynamics = 0f;

        [Header("Camera")]
        public float ExtraDistance = 0f;
        public float ExtraHeight = 0f;
        public float ExtraPitch = 0f;

        public bool Grounded => _grounded;
        private bool _grounded = false;
        private bool _steep = false;
        private bool _resting = false;

        private const float LastSafeLocationInterval = 0.5f;
        private float _lastSafeLocationTimer = LastSafeLocationInterval;
        private Vector3 _prevLastSafePosition = Vector3.zero;
        private Quaternion _prevLastSafeRotation = Quaternion.identity;
        private Vector3 _lastSafePosition = Vector3.zero;
        private Quaternion _lastSafeRotation = Quaternion.identity;

        public const float MaximumSpeedForStill = 0.15f;
        private const float MaximumAngleForStill = 20f;
        private const float StillTime = 0.25f;
        public bool Still => _still;
        private bool _still = false;
        private float _stillTimer = 0f;


        public const float MinimumSidewaysVelocityForDrift = 1f;
        public const float DriftMinimumAngle = 20f;
        public const float DriftingLerp = 5f;
        public const float DriftTraction = 0.1f;

        [NonSerialized]
        public float DriftingAmount = 0f;
        [NonSerialized]
        public float CounterSteering = 0f;
        [NonSerialized]
        public bool DoorsLocked = false;

        private CarPassengerSeat[] _passengerSeats;

#if !PLUGIN
        private void OnValidate()
        {
            Version = CurrentVersion;
        }
#endif

        private void UpdateAirAero()
        {
            var fwVelocity = Vector3.Dot(Rigidbody.velocity, transform.forward);
            var velocityMinusForward = Rigidbody.velocity - Vector3.Project(Rigidbody.velocity, transform.forward);

            velocityMinusForward = Vector3.Lerp(velocityMinusForward, Vector3.zero, AirAerodynamics * Time.deltaTime);

            Rigidbody.velocity = velocityMinusForward + (fwVelocity * transform.forward);
        }

        private void UpdateCounterSteer()
        {
            CounterSteering = 0f;
            if (Rigidbody.velocity.magnitude < 5f) return;
            var backwards = Vector3.Dot(transform.forward, Rigidbody.velocity.normalized) < 0f;
            var angle = Vector3.SignedAngle(backwards ? -transform.forward : transform.forward, Rigidbody.velocity.normalized, transform.up);
            var multiplier = CounterSteerMultiplier * (-Mathf.Abs(SteerAxis) + 1f);
            CounterSteering = angle * multiplier * (backwards ? -1f : 1f);
        }

        private void UpdateDrift()
        {
            var targetDrift = GetTargetDrift();
            if (targetDrift < DriftingAmount)
                DriftingAmount = Mathf.Lerp(DriftingAmount, targetDrift, DriftingLerp * Time.deltaTime);
            else
                DriftingAmount = targetDrift;
        }

        private float GetTargetDrift()
        {
            //if (!Grounded) return 0f;
            var sidewaysVelocity = Vector3.Dot(Rigidbody.velocity, transform.right);
            var backwards = Vector3.Dot(transform.forward, Rigidbody.velocity.normalized) < 0f;
            if (backwards) return 0f;
            var angle = Vector3.Angle(backwards ? -transform.forward : transform.forward, Rigidbody.velocity.normalized);
            if (Mathf.Abs(sidewaysVelocity) < MinimumSidewaysVelocityForDrift) return 0f;
            if (angle < DriftMinimumAngle) return 0f;
            return Mathf.Abs(ThrottleAxis);
        }

        private void UpdateStill()
        {
            _still = false;
            if (IsStill())
            {
                if (_stillTimer >= StillTime)
                {
                    Rigidbody.velocity = Vector3.zero;
                    Rigidbody.angularVelocity = Vector3.zero;
                    Rigidbody.Sleep();
                    _still = true;
                }
                else
                    _stillTimer += Time.deltaTime;
            }
            else
            {
                Rigidbody.WakeUp();
                _stillTimer = 0f;
            }
        }

        private bool IsStill()
        {
            if (!_resting) return false;
            if (!_grounded) return false;
            if (_steep) return false;
            if (ThrottleAxis != 0f && !BrakeHeld) return false;
            var vel = Rigidbody.velocity.magnitude + Rigidbody.angularVelocity.magnitude;
            if (vel > MaximumSpeedForStill) return false;
            var angle = Vector3.Angle(Vector3.up, transform.up);
            if (angle >= MaximumAngleForStill) return false;
            return true;
        }

        public void Initialize()
        {
            Driving = false;
            ResetLastSafeLocation();
        }

        /// <summary>
        /// Fix layers and stuff custom cars might not have set properly.
        /// </summary>
        private void FixUp()
        {
            //Enemies
            gameObject.layer = 17;
            var interactionTrigger = transform.Find("Interaction");
            //PlayerInteract
            if (interactionTrigger != null)
                interactionTrigger.gameObject.layer = 9;

            // Update cars
            if (Version < 1)
            {
                foreach(var wheel in Wheels)
                {
                    wheel.HandBrake = wheel.Throttle;
                }
            }
        }

        private void PlaceAtLastSafeLocation()
        {
            PlaceAt(_prevLastSafePosition, _prevLastSafeRotation, false);
            ResetLastSafeLocation();
        }

        private void RecordLastSafeLocation()
        {
            _lastSafeLocationTimer = LastSafeLocationInterval;
            _prevLastSafePosition = _lastSafePosition;
            _prevLastSafeRotation = _lastSafeRotation;
            _lastSafePosition = Rigidbody.position;
            _lastSafeRotation = Rigidbody.rotation;
        }

        private void ResetLastSafeLocation()
        {
            _lastSafeLocationTimer = LastSafeLocationInterval;
            _lastSafePosition = Rigidbody.position;
            _lastSafeRotation = Rigidbody.rotation;
            _prevLastSafePosition = _lastSafePosition;
            _prevLastSafeRotation = _lastSafeRotation;
        }

        private void OnCrash(float force, Vector3 point)
        {
            if (force < 4f)
                return;
            if (_crashAudioCooldown > 0f)
                return;
            var crashSFX = CarResources.Instance.GetCrashSFX();
            _oneShotAudioSource.Play(crashSFX);
            _crashAudioCooldown = 0.5f;
        }

        private void OnCollisionStay(Collision other)
        {
            if (_scrapeAudio != null)
                _scrapeAudio.OnScrape(other);
        }
        private void OnCollisionEnter(Collision other)
        {
            var speedDifference = (Rigidbody.velocity - _previousVelocity).magnitude + (Rigidbody.angularVelocity - _previousAngularVelocity).magnitude;
            OnCrash(speedDifference, other.contacts[0].point);
#if PLUGIN
            var impactVelocity = _previousVelocity.magnitude;
            var breakable = other.gameObject.GetComponent<BreakableObject>();
            if (breakable != null && impactVelocity >= 5f)
            {
                Rigidbody.velocity = _previousVelocity;
                Rigidbody.angularVelocity = _previousAngularVelocity;
                breakable.Break(false);
                return;
            }
            if (other.gameObject.layer == Layers.Enemies)
            {
                var enemy = other.gameObject.GetComponentInParent<BasicCop>();
                if (enemy != null)
                {
                    Rigidbody.velocity = _previousVelocity;
                    Rigidbody.angularVelocity = _previousAngularVelocity;
                    if (impactVelocity >= 5f && enemy.hitBoxResponse.State == EnemyHitResponse.HitResponseState.NONE)
                    {
                        var heading = (enemy.transform.position - transform.position).normalized;
                        enemy.hitBoxResponse.ManualDamage(EnemyHitResponse.HitType.EXPLOSION, heading, impactVelocity * 0.1f, 1f, 2f, 2);
                        TimedCollisionIgnore.Create(other.collider, Chassis.GetComponentInChildren<Collider>(), 1.5f);
                    }
                    return;
                }
            }
            if (other.gameObject.layer == Layers.Junk)
            {
                var junkHolder = other.gameObject.GetComponentInParent<JunkHolder>();
                if (junkHolder != null)
                {
                    if (!junkHolder.moved)
                    {
                        Rigidbody.velocity = _previousVelocity;
                        Rigidbody.angularVelocity = _previousAngularVelocity;
                    }
                    junkHolder.FallApart(other.contacts[0].point, false);
                    return;
                }
                var junk = other.gameObject.GetComponent<Junk>();
                if (junk)
                {
                    if (junk.rigidBody.isKinematic)
                    {
                        Rigidbody.velocity = _previousVelocity;
                        Rigidbody.angularVelocity = _previousAngularVelocity;
                    }
                    if (junk.interactOn == Junk.Interact.ON_HITBOX)
                        junk.FallApart(false);
                    else
                        junk.FallApart(true);
                }
            }
#endif
        }

        private void OnTriggerStay(Collider other)
        {
#if PLUGIN
            if (other.gameObject.layer == Layers.TriggerDetectPlayer)
            {
                var teleport = other.GetComponentInParent<Teleport>();
                if (teleport != null && InputEnabled && InCar)
                {
                    StartCoroutine(DoTeleport(teleport));
                }
                return;
            }

            if (other.CompareTag("MovingObject"))
            {
                other.GetComponentInParent<MoveAlongPoints>().TriggerDetectLayer(9);
                return;
            }
#endif
        }

        public void PlaceAt(Vector3 position, Quaternion rotation, bool keepSpeed = false)
        {
            var oldRightSpeed = Vector3.Dot(transform.right, Rigidbody.velocity);
            var oldUpSpeed = Vector3.Dot(transform.up, Rigidbody.velocity);
            var oldForwardSpeed = Vector3.Dot(transform.forward, Rigidbody.velocity);

            var oldAngularRightSpeed = Vector3.Dot(transform.right, Rigidbody.angularVelocity);
            var oldAngularUpSpeed = Vector3.Dot(transform.up, Rigidbody.angularVelocity);
            var oldAngularForwardSpeed = Vector3.Dot(transform.forward, Rigidbody.angularVelocity);

            transform.position = position;
            transform.rotation = rotation;

            if (!keepSpeed)
            {
                Rigidbody.velocity = Vector3.zero;
                Rigidbody.angularVelocity = Vector3.zero;
            }
            else
            {
                Rigidbody.velocity = (oldRightSpeed * transform.right) + (oldUpSpeed * transform.up) + (oldForwardSpeed * transform.forward);
                Rigidbody.angularVelocity = (oldAngularRightSpeed * transform.right) + (oldAngularUpSpeed * transform.up) + (oldAngularForwardSpeed * transform.forward);
            }
        }

        private void ResetInputs()
        {
            ThrottleAxis = 0f;
            SteerAxis = 0f;
            HornHeld = false;
            GetOutOfCarButtonNew = false;
            PitchAxis = 0f;
            YawAxis = 0f;
            RollAxis = 0f;
            BrakeHeld = false;
            LockDoorsButtonNew = false;
        }

#if PLUGIN
        private IEnumerator DoTeleport(Teleport teleport)
        {
            var transition = teleport.GetComponent<StageTransition>();
            if (transition == null)
            {
                DoForcedPause();
                InputEnabled = false;

                if (teleport.automaticallyReturnPlayerToLastSafeLocation)
                {
                    teleport.fadeToBlackDuration = teleport.fadeToBlackDurationDeathzone;
                    teleport.blackDuration = teleport.blackDurationDeathzone;
                    teleport.fadeOpenDuration = teleport.fadeOpenDurationDeathzone;
                }
                else
                {
                    teleport.fadeToBlackDuration = teleport.fadeToBlackDurationDoor;
                    teleport.blackDuration = teleport.blackDurationDoor;
                    teleport.fadeOpenDuration = teleport.fadeOpenDurationDoor;
                }
                var tween = Core.Instance.UIManager.effects.FadeToBlack(teleport.fadeToBlackDuration);
                yield return tween.WaitForCompletion();
                Core.Instance.UIManager.effects.fullScreenFade.gameObject.SetActive(true);
                Core.Instance.UIManager.effects.fullScreenFade.color = EffectsUI.niceBlack;
                yield return new WaitForSeconds(teleport.blackDuration);
                UndoForcedPause();
                if (teleport.automaticallyReturnPlayerToLastSafeLocation)
                {
                    PlaceAtLastSafeLocation();

                }
                else if (teleport.teleportTo != null)
                {
                    PlaceAt(teleport.teleportTo.position, teleport.teleportTo.rotation, teleport.giveSpeedAtSpawn);
                }
                InputEnabled = true;
                Core.Instance.UIManager.effects.FadeOpen(teleport.fadeOpenDuration);
            }
        }
#endif

#if PLUGIN
        private float GetAxisDeadZone(GameInput gameInput, int actionId, float deadzone)
        {
            var controllerType = gameInput.GetCurrentControllerType(0);
            var axis = gameInput.GetAxis(actionId, 0);
            if (controllerType != Rewired.ControllerType.Joystick)
                return axis;
            if (Mathf.Abs(axis) <= deadzone)
                return 0f;
            return axis;
        }
#endif

        private void PollInputs()
        {
            ResetInputs();
            if (!InputEnabled) return;

            OnHandleInput?.Invoke();
#if PLUGIN
            var player = WorldHandler.instance.GetCurrentPlayer();
            if (player.IsBusyWithSequence() && InCar) return;
#endif

#if PLUGIN
            var gameInput = Core.Instance.GameInput;

            if (InCar)
            {
                GetOutOfCarButtonNew = gameInput.GetButtonNew(11, 0);
            }
#endif
            if (!Driving) return;
#if PLUGIN
            LockDoorsButtonNew = gameInput.GetButtonNew(29, 0);
            BrakeHeld = gameInput.GetButtonHeld(7, 0);
            SteerAxis = gameInput.GetAxis(5, 0);
            if (BrakeHeld)
                RollAxis = GetAxisDeadZone(gameInput, 5, ControllerRotationDeadZone);
            else
                YawAxis = GetAxisDeadZone(gameInput, 5, ControllerRotationDeadZone);

            var controllerType = gameInput.GetCurrentControllerType(0);

            if (controllerType == Rewired.ControllerType.Joystick)
            {
                PitchAxis = GetAxisDeadZone(gameInput, 6, ControllerRotationDeadZone);
                ThrottleAxis += gameInput.GetAxis(8, 0);
                ThrottleAxis -= gameInput.GetAxis(18, 0);
            }
            else
            {
                ThrottleAxis = gameInput.GetAxis(6, 0);
                if (BrakeHeld)
                    PitchAxis = GetAxisDeadZone(gameInput, 6, ControllerRotationDeadZone);
            }
            HornHeld = gameInput.GetButtonHeld(10, 0);
            
#else
            BrakeHeld = Input.GetKey(KeyCode.Space);

            if (BrakeHeld)
            {
                if (Input.GetKey(KeyCode.D))
                    RollAxis += 1f;
                if (Input.GetKey(KeyCode.A))
                    RollAxis -= 1f;

                if (Input.GetKey(KeyCode.W))
                    PitchAxis += 1f;
                if (Input.GetKey(KeyCode.S))
                    PitchAxis -= 1f;
            }
            else
            {
                if (Input.GetKey(KeyCode.D))
                    YawAxis += 1f;
                if (Input.GetKey(KeyCode.A))
                    YawAxis -= 1f;
            }

            if (Input.GetKey(KeyCode.D))
                SteerAxis += 1f;
            if (Input.GetKey(KeyCode.A))
                SteerAxis -= 1f;

            if (Input.GetKey(KeyCode.W))
                ThrottleAxis += 1f;
            if (Input.GetKey(KeyCode.S))
                ThrottleAxis -= 1f;

            HornHeld = Input.GetKey(KeyCode.H);
#endif
        }

        private void Awake()
        {
            Chassis = gameObject;
            _passengerSeats = Chassis.GetComponentsInChildren<CarPassengerSeat>();
            DriverSeat = Chassis.GetComponentInChildren<CarDriverSeat>();
            _scrapeAudio = Chassis.GetComponentInChildren<ScrapeAudio>();
            _oneShotAudioSource = Chassis.GetComponentInChildren<OneShotAudioSource>();
            Rigidbody = Chassis.GetComponent<Rigidbody>();
            Wheels = Chassis.GetComponentsInChildren<CarWheel>();
            if (CenterOfMass != null)
            {
                Rigidbody.centerOfMass = CenterOfMass.localPosition;
            }
            foreach (var wheel in Wheels)
                wheel.Initialize(this);

            ResetLastSafeLocation();
            FixUp();
#if PLUGIN
            Core.OnCoreUpdatePaused += OnPause;
            Core.OnCoreUpdateUnPaused += OnUnPause;
            var continuous = CarController.Config.ContinuousCollisionDetection;
            Rigidbody.collisionDetectionMode = continuous ? CollisionDetectionMode.Continuous : CollisionDetectionMode.Discrete;
            Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
#endif
        }

#if PLUGIN

        public CarPassengerSeat GetPassengerSeat(int index)
        {
            foreach(var seat in _passengerSeats)
            {
                if (seat.SeatIndex == index)
                    return seat;
            }
            return null;
        }

        public void EnterCar(Player player)
        {
            if (DriverSeat == null) return;
            DriverSeat.PutInSeat(player);
        }

        public void ExitCar()
        {
            if (DriverSeat == null) return;
            DriverSeat.ExitSeat();
        }
#endif
        private bool _forcedPause = false;

        private void DoForcedPause()
        {
            OnPause();
            _forcedPause = true;
        }

        private void UndoForcedPause()
        {
            _forcedPause = false;
            OnUnPause();
        }

        private void OnPause()
        {
            if (_forcedPause) return;
            _velocityBeforePause = Rigidbody.velocity;
            _angularVelocityBeforePause = Rigidbody.angularVelocity;
            Rigidbody.isKinematic = true;
        }

        private void OnUnPause()
        {
            if (_forcedPause) return;
            if (Rigidbody == null) return;
            Rigidbody.isKinematic = false;
            Rigidbody.velocity = _velocityBeforePause;
            Rigidbody.angularVelocity = _angularVelocityBeforePause;
        }

        private void FixedUpdate()
        {
#if PLUGIN
            if (Core.Instance.IsCorePaused) return;
#endif
            _resting = true;
            _grounded = false;
            _steep = false;
            PollInputs();
#if PLUGIN
            if (LockDoorsButtonNew)
            {
                PlayerData.Instance.DoorsLocked = !PlayerData.Instance.DoorsLocked;
                PlayerData.Instance.Save();
                Core.Instance.UIManager.ShowNotification("Car doors are now <color=yellow>"+(PlayerData.Instance.DoorsLocked ? "Locked" : "Unlocked")+"</color>");
            }
#endif
            UpdateCounterSteer();
            var wheelsGrounded = 0;
            foreach (var wheel in Wheels)
            {
                wheel.DoPhysics(ref _resting);
                if (wheel.Grounded)
                    wheelsGrounded++;
            }
            _previousAngularVelocity = Rigidbody.angularVelocity;
            _previousVelocity = Rigidbody.velocity;

            if (wheelsGrounded == Wheels.Length)
                _grounded = true;

            var angle = Vector3.Angle(Vector3.up, transform.up);
            if (angle >= 50f)
                _steep = true;

            _lastSafeLocationTimer = Mathf.Max(0f, _lastSafeLocationTimer - Time.deltaTime);

            if (_grounded && !_steep && _lastSafeLocationTimer <= 0f)
            {
                RecordLastSafeLocation();
            }

            //var airControlMultiplier = (-((float)wheelsGrounded / Wheels.Length)) + 1f;
            //AirControl(airControlMultiplier);
            if (wheelsGrounded == 0)
            {
                AirControl(1f);
                UpdateAirAero();
            }

            UpdateStill();
            UpdateDrift();
#if PLUGIN
            if (GetOutOfCarButtonNew && InCar)
            {
                CarController.Instance.ExitCar();
            }
#endif
        }

        private void AirControl(float multiplier)
        {
            var airStrength = AirControlStrength * multiplier;

            var pitch = transform.right * (airStrength * PitchAxis);
            var yaw = transform.up * (airStrength * YawAxis);
            var roll = -transform.forward * (airStrength * RollAxis);

            var currentPitch = Vector3.Dot(transform.right, Rigidbody.angularVelocity);
            var currentYaw = Vector3.Dot(transform.up, Rigidbody.angularVelocity);
            var currentRoll = Vector3.Dot(-transform.forward, Rigidbody.angularVelocity);

            if (currentPitch >= AirControlTopSpeed && PitchAxis > 0f)
                pitch = Vector3.zero;

            if (currentPitch <= -AirControlTopSpeed && PitchAxis < 0f)
                pitch = Vector3.zero;

            if (currentYaw >= AirControlTopSpeed && YawAxis > 0f)
                yaw = Vector3.zero;

            if (currentYaw <= -AirControlTopSpeed && YawAxis < 0f)
                yaw = Vector3.zero;

            if (currentRoll >= AirControlTopSpeed && RollAxis > 0f)
                roll = Vector3.zero;

            if (currentRoll <= -AirControlTopSpeed && RollAxis < 0f)
                roll = Vector3.zero;

            Rigidbody.AddTorque(pitch, ForceMode.Acceleration);
            Rigidbody.AddTorque(yaw, ForceMode.Acceleration);
            Rigidbody.AddTorque(roll, ForceMode.Acceleration);

            Rigidbody.angularVelocity = Vector3.Lerp(Rigidbody.angularVelocity, Vector3.zero, AirDeacceleration * Time.deltaTime);
        }

        private void Update()
        {
#if PLUGIN
            if (Core.Instance.IsCorePaused) return;

            if (Driving)
            {
                DoorsLocked = PlayerData.Instance.DoorsLocked;
            }
#endif
            _crashAudioCooldown = Mathf.Max(0f, _crashAudioCooldown - Time.deltaTime);
            foreach (var wheel in Wheels)
            {
                wheel.DoUpdate();
            }
        }

        private void OnDestroy()
        {
#if PLUGIN
            if (CarController.Instance.CurrentCar == this)
            {
                CarController.Instance.ExitCar();
            }
            Core.OnCoreUpdatePaused -= OnPause;
            Core.OnCoreUpdateUnPaused -= OnUnPause;
#endif
        }
    }
}