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

        private Vector3 _velocityBeforePause;
        private Vector3 _angularVelocityBeforePause;
        
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
            foreach(var wheel in Wheels)
            {
                wheel.DoPhysics();
                wheel.DoInput();
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