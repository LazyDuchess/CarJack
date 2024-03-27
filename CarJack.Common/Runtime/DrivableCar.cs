using UnityEngine;

namespace CarJack.Common
{
    public class DrivableCar : MonoBehaviour
    {
        public Transform CenterOfMass;
        [HideInInspector]
        public Rigidbody Rigidbody;
        [HideInInspector]
        public CarWheel[] Wheels;
        public GameObject Chassis;
        public bool Driving = true;
        
        private void Awake()
        {
            Rigidbody = Chassis.GetComponent<Rigidbody>();
            Wheels = Chassis.GetComponentsInChildren<CarWheel>();
            if (CenterOfMass != null)
            {
                Rigidbody.centerOfMass = CenterOfMass.localPosition;
            }
        }

        private void FixedUpdate()
        {
            foreach(var wheel in Wheels)
            {
                wheel.DoPhysics(this);
                if (Driving)
                    wheel.DoInput(this);
            }
        }
    }
}