using UnityEngine;

namespace CarJack.Common
{
    public class CarWheel : MonoBehaviour
    {
        public float RotationAcceleration = 100f;
        public float RotationDeacceleration = 1f;
        public float RotationMultiplier = 1f;
        [HideInInspector]
        public bool Grounded = false;
        public GameObject Mesh;
        public float MeshRadius = 0.5f;

        public float SteerAngle = 45f;
        public float SteerSpeed = 5f;
        public float ReverseSpeed = 400f;
        public float Speed = 10f;
        public bool Throttle = false;
        public bool Steer = false;
        public float Damping = 0f;
        public float Strength = 0f;
        public float MaxDistance = 0f;
        public float RestDistance = 0f;
        public float Mass = 10f;

        public float Traction = 0.5f;

        private float _currentRoll = 0f;
        private float _currentSpeed = 0f;
        private float _currentSteerAngle = 0f;
        private DrivableCar _car;

        public void Initialize(DrivableCar car)
        {
            _car = car;
        }

        public void DoPhysics()
        {
            var distance = MaxDistance;
            Grounded = false;
            var ray = new Ray(transform.position, -transform.up);
            if (Physics.Raycast(ray, out var hit, MaxDistance, _car.GroundMask))
            {
                distance = hit.distance;
                Grounded = true;
                var offset = RestDistance - hit.distance;
                var velocity = Vector3.Dot(_car.Rigidbody.GetPointVelocity(transform.position), transform.up);
                var force = (offset * Strength) - (velocity * Damping);
                _car.Rigidbody.AddForceAtPosition(transform.up * force, transform.position);
            }

            if (Mesh != null)
            {
                Mesh.transform.position = transform.position - ((distance - MeshRadius) * transform.up);
            }

            if (Grounded)
            {
                var traction = Traction;
                var wheelVelocity = _car.Rigidbody.GetPointVelocity(transform.position);
                var wheelVelocityWithoutUp = (wheelVelocity - Vector3.Project(wheelVelocity, transform.up)).magnitude;
                var tractionT = Mathf.Min(wheelVelocityWithoutUp, _car.TractionCurveMax) / _car.TractionCurveMax;
                var curve = _car.TractionCurve.Evaluate(tractionT);
                traction *= curve;

                var slippingVelocity = Vector3.Dot(wheelVelocity, transform.right);
                var force = -slippingVelocity * traction;
                var acceleration = force / Time.fixedDeltaTime;
                _car.Rigidbody.AddForceAtPosition(transform.right * Mass * acceleration, transform.position);
            }
        }

        public void DoInput()
        {
            var wheelVelocity = _car.Rigidbody.GetPointVelocity(transform.position);
            

            var throttleAxis = GetForwardInput();
            var steerAxis = GetSteeringInput();

            if (Grounded && throttleAxis == 0f)
            {
                var wheelForwardVelocity = Vector3.Dot(wheelVelocity, transform.forward);
                if (wheelForwardVelocity > 0f)
                {
                    _car.Rigidbody.AddForceAtPosition(-transform.forward * _car.Deacceleration, transform.position);
                }
                else
                {
                    _car.Rigidbody.AddForceAtPosition(transform.forward * _car.Deacceleration, transform.position);
                }
            }

            if (Throttle && !Grounded)
            {
                if (Mathf.Abs(_currentSpeed) < 100f)
                    _currentSpeed += throttleAxis * RotationAcceleration * Time.deltaTime;
            }

            if (Throttle && Grounded)
            {
                var wheelVelocityWithoutUp = (wheelVelocity - Vector3.Project(wheelVelocity, transform.up)).magnitude;

                var speed = Speed;
                var speedT = Mathf.Min(wheelVelocityWithoutUp, _car.SpeedCurveMax) / _car.SpeedCurveMax;
                var curve = _car.SpeedCurve.Evaluate(speedT);
                speed *= curve;

                if (throttleAxis < 0f)
                {
                    speed = ReverseSpeed;
                    speedT = Mathf.Min(wheelVelocityWithoutUp, _car.ReverseCurveMax) / _car.ReverseCurveMax;
                    curve = _car.ReverseCurve.Evaluate(speedT);
                    speed *= curve;
                }

                var forwardDot = Vector3.Dot(wheelVelocity, transform.forward);
                if ((forwardDot > 0f && throttleAxis < 0f ) || (forwardDot < 0f && throttleAxis > 0f))
                {
                    speed = _car.BrakeForce;
                }

                _car.Rigidbody.AddForceAtPosition(transform.forward * throttleAxis * speed, transform.position);
            }
            if (Steer)
            {
                var steerAngle = SteerAngle;
                var steerT = Mathf.Min(Mathf.Abs(Vector3.Dot(wheelVelocity, transform.forward)), _car.SteerCurveMax) / _car.SteerCurveMax;
                var curve = _car.SteerCurve.Evaluate(steerT);
                steerAngle *= curve;

                var targetSteerAngle = steerAngle * steerAxis;
                _currentSteerAngle = Mathf.Lerp(_currentSteerAngle, targetSteerAngle, SteerSpeed * Time.deltaTime);
                transform.localRotation = Quaternion.Euler(0f, _currentSteerAngle, 0f);
            }
        }

        public void DoUpdate()
        {
            if (Grounded)
            {
                var wheelVelocity = _car.Rigidbody.GetPointVelocity(transform.position);
                var wheelVelocityFw = Vector3.Dot(wheelVelocity, transform.forward);
                _currentSpeed = wheelVelocityFw;
            }
            else
            {
                if (_currentSpeed > 0f)
                    _currentSpeed = Mathf.Max(_currentSpeed - (RotationDeacceleration * Time.deltaTime), 0f);
                else
                    _currentSpeed = Mathf.Min(_currentSpeed + (RotationDeacceleration * Time.deltaTime), 0f);
            }
            _currentRoll += _currentSpeed * RotationMultiplier * Time.deltaTime;
            _currentRoll -= Mathf.Floor(_currentRoll / 360f) * 360f;
            Mesh.transform.localRotation = Quaternion.Euler(_currentRoll, 0f, 0f);
        }

        private float GetSteeringInput()
        {
            if (!_car.Driving) return 0f;
            var input = 0f;
            if (Input.GetKey(KeyCode.D))
                input += 1f;
            if (Input.GetKey(KeyCode.A))
                input -= 1f;
            return input;
        }

        private float GetForwardInput()
        {
            if (!_car.Driving) return 0f;
            var input = 0f;
            if (Input.GetKey(KeyCode.W))
                input += 1f;
            if (Input.GetKey(KeyCode.S))
                input -= 1f;
            return input;
        }
    }
}
