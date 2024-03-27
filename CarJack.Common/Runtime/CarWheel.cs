using UnityEngine;

namespace CarJack.Common
{
    public class CarWheel : MonoBehaviour
    {
        public GameObject Mesh;
        public float MeshRadius = 0.5f;

        public float SteerAngle = 45f;
        public float Speed = 10f;
        public bool Throttle = false;
        public bool Steer = false;
        public float Damping = 0f;
        public float Strength = 0f;
        public float MaxDistance = 0f;
        public float RestDistance = 0f;
        public float Mass = 10f;

        public float Traction = 0.5f;

        public void DoPhysics(DrivableCar car)
        {
            var distance = MaxDistance;
            var grounded = false;
            var ray = new Ray(transform.position, -transform.up);
            if (Physics.Raycast(ray, out var hit, MaxDistance))
            {
                distance = hit.distance;
                grounded = true;
                var offset = RestDistance - hit.distance;
                var velocity = Vector3.Dot(car.Rigidbody.GetPointVelocity(transform.position), transform.up);
                var force = (offset * Strength) - (velocity * Damping);
                car.Rigidbody.AddForceAtPosition(transform.up * force, transform.position);
            }

            if (Mesh != null)
            {
                Mesh.transform.position = transform.position - ((distance - MeshRadius) * transform.up);
            }

            if (grounded)
            {
                var slippingVelocity = Vector3.Dot(car.Rigidbody.GetPointVelocity(transform.position), transform.right);
                var force = -slippingVelocity * Traction;
                var acceleration = force / Time.fixedDeltaTime;
                car.Rigidbody.AddForceAtPosition(transform.right * Mass * acceleration, transform.position);
            }
        }

        public void DoInput(DrivableCar car)
        {
            var throttleAxis = Input.GetAxisRaw("Vertical");
            var steerAxis = Input.GetAxisRaw("Horizontal");
            if (Throttle)
            {
                car.Rigidbody.AddForceAtPosition(transform.forward * throttleAxis * Speed, transform.position);
            }
            if (Steer)
            {
                transform.localRotation = Quaternion.Euler(0f, SteerAngle * steerAxis, 0f);
            }
        }
    }
}
