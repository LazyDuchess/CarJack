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

        private Vector3 _velocityBeforePause;
        private Vector3 _angularVelocityBeforePause;

        private void ResetInputs()
        {
            ThrottleAxis = 0f;
            SteerAxis = 0f;
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
#else

            if (Input.GetKey(KeyCode.D))
                SteerAxis += 1f;
            if (Input.GetKey(KeyCode.A))
                SteerAxis -= 1f;

            if (Input.GetKey(KeyCode.W))
                ThrottleAxis += 1f;
            if (Input.GetKey(KeyCode.S))
                ThrottleAxis -= 1f;
#endif
        }

        private void Awake()
        {
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