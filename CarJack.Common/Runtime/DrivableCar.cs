#if PLUGIN
using Reptile;
#endif
using UnityEngine;

namespace CarJack.Common
{
    public class DrivableCar : MonoBehaviour
    {
        public float Deacceleration = 100f;
        public AnimationCurve ReverseCurve;
        public float ReverseCurveMax = 50f;
        public float BrakeForce = 500f;
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

        private Vector3 _velocityBeforePause;
        private Vector3 _angularVelocityBeforePause;

        private Vector3 _previousVelocity = Vector3.zero;
        private Vector3 _previousAngularVelocity = Vector3.zero;

        private OneShotAudioSource _oneShotAudioSource;

        private ScrapeAudio _scrapeAudio;

        public void Initialize()
        {
            Driving = false;
        }

        private void OnCrash(float force, Vector3 point)
        {
            if (force < 4f)
                return;
            var crashSFX = CarResources.Instance.GetCrashSFX();
            _oneShotAudioSource.Play(crashSFX);
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
                            if (Driving)
                                CarController.Instance.ExitCar();
                            Destroy(gameObject);
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
        }
        
        private void PollInputs()
        {
            ResetInputs();

            if (!Driving) return;
#if PLUGIN
            var gameInput = Core.Instance.GameInput;
            SteerAxis = gameInput.GetAxis(5, 0);

            var controllerType = gameInput.GetCurrentControllerType(0);
            if (controllerType == Rewired.ControllerType.Joystick)
            {
                ThrottleAxis += gameInput.GetAxis(8, 0);
                ThrottleAxis -= gameInput.GetAxis(18, 0);
            }
            else
                ThrottleAxis = gameInput.GetAxis(6, 0);
            HornHeld = gameInput.GetButtonHeld(10, 0);
#else

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
#if PLUGIN
            Core.OnCoreUpdatePaused += OnPause;
            Core.OnCoreUpdateUnPaused += OnUnPause;
#endif
        }

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
            PollInputs();
            foreach(var wheel in Wheels)
            {
                wheel.DoPhysics();
            }
            _previousAngularVelocity = Rigidbody.angularVelocity;
            _previousVelocity = Rigidbody.velocity;
        }

        private void Update()
        {
#if PLUGIN
            if (Core.Instance.IsCorePaused) return;
#endif
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