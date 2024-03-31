#if PLUGIN
using Reptile;
#endif
using System;
using UnityEngine;

namespace CarJack.Common
{
    public class DrivableCar : MonoBehaviour
    {
        public string InternalName = "";

        public float SlipMultiplier = 1f;

        public bool HasSurfaceAngleLimit = true;
        public float SurfaceAngleLimit = 60f;

        public float AirControlStrength = 1f;
        public float AirControlTopSpeed = 2f;
        public float AirDeacceleration = 1f;

        public float Deacceleration = 100f;
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
        [HideInInspector]
        public Rigidbody Rigidbody;
        [HideInInspector]
        public CarWheel[] Wheels;
        public GameObject Chassis;
        public bool Driving = true;

        [HideInInspector]
        public float ThrottleAxis = 0f;
        [HideInInspector]
        public float SteerAxis = 0f;
        [HideInInspector]
        public bool HornHeld = false;
        [HideInInspector]
        public bool GetOutOfCarButtonNew = false;
        [HideInInspector]
        public float PitchAxis = 0f;
        [HideInInspector]
        public float YawAxis = 0f;
        [HideInInspector]
        public float RollAxis = 0f;
        [HideInInspector]
        public bool BrakeHeld = false;

        private Vector3 _velocityBeforePause;
        private Vector3 _angularVelocityBeforePause;

        private Vector3 _previousVelocity = Vector3.zero;
        private Vector3 _previousAngularVelocity = Vector3.zero;

        private OneShotAudioSource _oneShotAudioSource;

        private ScrapeAudio _scrapeAudio;

        private float _crashAudioCooldown = 0f;

        private CarDriver[] _drivers;

        public Action OnHandleInput;

        [Header("Camera")]
        public float ExtraDistance = 0f;
        public float ExtraHeight = 0f;

        private bool _grounded = false;
        private bool _steep = false;
        private Vector3 _lastSafePosition = Vector3.zero;
        private Quaternion _lastSafeRotation = Quaternion.identity;

        public void Initialize()
        {
            Driving = false;
        }

        private void OnCrash(float force, Vector3 point)
        {
            if (force < 4f)
                return;
            if (_crashAudioCooldown > 0f)
                return;
            var crashSFX = CarResources.Instance.GetCrashSFX();
            _oneShotAudioSource.Play(crashSFX);
            _crashAudioCooldown = 0.1f;
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
                if (teleport != null)
                {
                    var transition = teleport.GetComponent<StageTransition>();
                    if (transition == null)
                    {
                        if (teleport.automaticallyReturnPlayerToLastSafeLocation)
                        {
                            PlaceAt(_lastSafePosition, _lastSafeRotation, false);
                        }
                        else if (teleport.teleportTo != null)
                        {
                            PlaceAt(teleport.teleportTo.position, teleport.teleportTo.rotation, teleport.giveSpeedAtSpawn);
                        }
                    }
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
            transform.position = position;
            transform.rotation = rotation;
            if (!keepSpeed)
            {
                Rigidbody.velocity = Vector3.zero;
                Rigidbody.angularVelocity = Vector3.zero;
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
        }
        
        private void PollInputs()
        {
            ResetInputs();
            OnHandleInput?.Invoke();

            if (!Driving) return;
#if PLUGIN
            var gameInput = Core.Instance.GameInput;
            BrakeHeld = gameInput.GetButtonHeld(7, 0);
            SteerAxis = gameInput.GetAxis(5, 0);
            if (BrakeHeld)
                RollAxis = gameInput.GetAxis(5, 0);
            else
                YawAxis = gameInput.GetAxis(5, 0);

            var controllerType = gameInput.GetCurrentControllerType(0);

            if (controllerType == Rewired.ControllerType.Joystick)
            {
                PitchAxis = gameInput.GetAxis(6, 0);
                ThrottleAxis += gameInput.GetAxis(8, 0);
                ThrottleAxis -= gameInput.GetAxis(18, 0);
            }
            else
            {
                ThrottleAxis = gameInput.GetAxis(6, 0);
                if (BrakeHeld)
                    PitchAxis = gameInput.GetAxis(6, 0);
            }
            HornHeld = gameInput.GetButtonHeld(10, 0);
            GetOutOfCarButtonNew = gameInput.GetButtonNew(11, 0);
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
            _drivers = Chassis.GetComponentsInChildren<CarDriver>();
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

            _lastSafePosition = Rigidbody.position;
            _lastSafeRotation = Rigidbody.rotation;
#if PLUGIN
            Core.OnCoreUpdatePaused += OnPause;
            Core.OnCoreUpdateUnPaused += OnUnPause;
            var continuous = CarController.Config.ContinuousCollisionDetection;
            Rigidbody.collisionDetectionMode = continuous ? CollisionDetectionMode.ContinuousDynamic : CollisionDetectionMode.Discrete;
            Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
#endif
        }

#if PLUGIN
        public void EnterCar(Player player)
        {
            foreach(var driver in _drivers)
            {
                driver.PutInCar(player);
            }
        }

        public void ExitCar()
        {
            foreach (var driver in _drivers)
            {
                driver.ExitCar();
            }
        }
#endif

        private void OnPause()
        {
            _velocityBeforePause = Rigidbody.velocity;
            _angularVelocityBeforePause = Rigidbody.angularVelocity;
            Rigidbody.isKinematic = true;
        }

        private void OnUnPause()
        {
            Rigidbody.isKinematic = false;
            Rigidbody.velocity = _velocityBeforePause;
            Rigidbody.angularVelocity = _angularVelocityBeforePause;
        }

        private void FixedUpdate()
        {
#if PLUGIN
            if (Core.Instance.IsCorePaused) return;
#endif
            _grounded = false;
            _steep = false;
            PollInputs();
            var wheelsGrounded = 0;
            foreach(var wheel in Wheels)
            {
                wheel.DoPhysics();
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

            if (_grounded && !_steep)
            {
                _lastSafePosition = Rigidbody.position;
                _lastSafeRotation = Rigidbody.rotation;
            }

            //var airControlMultiplier = (-((float)wheelsGrounded / Wheels.Length)) + 1f;
            //AirControl(airControlMultiplier);
            if (wheelsGrounded == 0)
                AirControl(1f);
#if PLUGIN
            if (GetOutOfCarButtonNew && Driving)
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
            Core.OnCoreUpdatePaused -= OnPause;
            Core.OnCoreUpdateUnPaused -= OnUnPause;
#endif
        }
    }
}