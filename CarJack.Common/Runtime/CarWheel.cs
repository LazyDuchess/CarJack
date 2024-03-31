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
        public float StartLength = 0f;
        public float MaxDistance = 0f;
        public float RestDistance = 0f;
        public float Mass = 10f;

        public float Traction = 0.5f;
        public float CurrentSpeed = 0f;

        private float _currentRoll = 0f;
        
        private float _currentSteerAngle = 0f;
        private DrivableCar _car;
        private float SlipSpeed = 5f;
        private const float MinimumSidewaysSpeedForSlip = 4f;
        private const float SidewaysSlipMultiplier = 0.05f;
        public float Slipping => _currentSlip;
        private float _currentSlip = 0f;

        private const float AutoSteer = 0.15f;
        private const float MinimumSpeedToAutoSteer = 1f;

        public void Initialize(DrivableCar car)
        {
            _car = car;
        }

        public void DoPhysics()
        {
            _currentSlip = Mathf.Lerp(_currentSlip, CalculateSlip(), SlipSpeed * Time.deltaTime);
            var tooSteep = false;
            if (_car.HasSurfaceAngleLimit)
            {
                var angle = Vector3.Angle(Vector3.up, transform.up);
                if (angle >= _car.SurfaceAngleLimit)
                    tooSteep = true;
            }
            var distance = MaxDistance;
            Grounded = false;
            var ray = new Ray(transform.position + (transform.up * StartLength), -transform.up);
            if (Physics.Raycast(ray, out var hit, MaxDistance + StartLength, _car.GroundMask))
            {
                distance = hit.distance - StartLength;
                Grounded = true;
                var offset = RestDistance - distance;
                var velocity = Vector3.Dot(_car.Rigidbody.GetPointVelocity(transform.position), transform.up);
                var force = (offset * Strength) - (velocity * Damping);
                if (tooSteep)
                    force = Mathf.Max(force, 0f);
                _car.Rigidbody.AddForceAtPosition(transform.up * force, transform.position);
            }

            if (Mesh != null)
            {
                Mesh.transform.position = transform.position - ((distance - MeshRadius) * transform.up);
            }

            if (Grounded && !tooSteep)
            {
                var traction = Traction;
                var wheelVelocity = _car.Rigidbody.GetPointVelocity(transform.position);
                var wheelVelocityWithoutUp = (wheelVelocity - Vector3.Project(wheelVelocity, transform.up)).magnitude;
                var tractionT = Mathf.Min(wheelVelocityWithoutUp, _car.TractionCurveMax) / _car.TractionCurveMax;
                var curve = _car.TractionCurve.Evaluate(tractionT);
                traction *= curve;
                traction *= (-_currentSlip) +1f;

                var slippingVelocity = Vector3.Dot(wheelVelocity, transform.right);
                var force = -slippingVelocity * traction;
                var acceleration = force / Time.fixedDeltaTime;
                _car.Rigidbody.AddForceAtPosition(transform.right * Mass * acceleration, transform.position);
            }
            DoInput(tooSteep);
        }

        private float CalculateSlip()
        {
            var wheelVelocity = _car.Rigidbody.GetPointVelocity(transform.position);
            var fwWheelVelocity = Vector3.Dot(wheelVelocity, transform.forward);
            var wheelVelocityDifference = Mathf.Abs(CurrentSpeed - fwWheelVelocity);
            var slip = 0f;
            if (wheelVelocityDifference >= 5f)
                slip = 0.5f;

            var sideWaysVelocity = Mathf.Abs(Vector3.Dot(wheelVelocity, transform.right));

            sideWaysVelocity = Mathf.Max(0f, sideWaysVelocity - MinimumSidewaysSpeedForSlip);
            slip += sideWaysVelocity * SidewaysSlipMultiplier * _car.SlipMultiplier;

            slip = Mathf.Clamp(slip, 0f, 0.9f);

            return slip;
        }

        private void DoInput(bool tooSteep)
        {
            var wheelVelocity = _car.Rigidbody.GetPointVelocity(transform.position);
            var wheelVelocityWithoutUp = (wheelVelocity - Vector3.Project(wheelVelocity, transform.up));
            var wheelVelocityWithoutUpMagnitude = wheelVelocityWithoutUp.magnitude;
            var wheelSidewaysVelocity = Mathf.Abs(Vector3.Dot(wheelVelocity, transform.right));

            var throttleAxis = _car.ThrottleAxis;
            var steerAxis = _car.SteerAxis;

            if (Grounded)
            {
                var deaccelerationAmount = 1f-Mathf.Abs(throttleAxis);
                var wheelForwardVelocity = Vector3.Dot(wheelVelocity, transform.forward);
                
                if (wheelForwardVelocity > 0f)
                {
                    _car.Rigidbody.AddForceAtPosition(-transform.forward * _car.Deacceleration * deaccelerationAmount, transform.position);
                }
                else
                {
                    _car.Rigidbody.AddForceAtPosition(transform.forward * _car.Deacceleration * deaccelerationAmount, transform.position);
                }
            }

            if (Throttle && (!Grounded || tooSteep))
            {
                if (Mathf.Abs(CurrentSpeed) < 100f)
                    CurrentSpeed += throttleAxis * RotationAcceleration * Time.deltaTime;
            }

            var forwardDot = Vector3.Dot(wheelVelocity, transform.forward);

            var braking = ((forwardDot > 0f && throttleAxis < 0f) || (forwardDot < 0f && throttleAxis > 0f)) && !_car.BrakeHeld;

            if ((Throttle || braking) && Grounded && !tooSteep)
            {
                

                var speed = Speed;
                var speedT = Mathf.Min(wheelVelocityWithoutUpMagnitude, _car.SpeedCurveMax) / _car.SpeedCurveMax;
                var curve = _car.SpeedCurve.Evaluate(speedT);
                speed *= curve;

                if (throttleAxis < 0f)
                {
                    speed = ReverseSpeed;
                    speedT = Mathf.Min(wheelVelocityWithoutUpMagnitude, _car.ReverseCurveMax) / _car.ReverseCurveMax;
                    curve = _car.ReverseCurve.Evaluate(speedT);
                    speed *= curve;
                }

                if (braking)
                {
                    speed = _car.BrakeForce;
                    if (_car.Rigidbody.velocity.magnitude <= DrivableCar.MaximumSpeedForStill || _car.Still)
                        speed = 0f;
                }

                var finalSpeed = throttleAxis * speed;

                if (_car.BrakeHeld)
                {
                    if (forwardDot > 0f)
                        finalSpeed = -_car.HandBrakeForce;
                    else if (forwardDot < 0f)
                        finalSpeed = _car.HandBrakeForce;
                    if (_car.Rigidbody.velocity.magnitude <= DrivableCar.MaximumSpeedForStill || _car.Still)
                        finalSpeed = 0f;
                }

                _car.Rigidbody.AddForceAtPosition(transform.forward * finalSpeed, transform.position);
            }
            if (Steer)
            {
                var steerAngle = SteerAngle;
                var steerT = Mathf.Min(Mathf.Abs(Vector3.Dot(wheelVelocity, transform.forward)), _car.SteerCurveMax) / _car.SteerCurveMax;
                var curve = _car.SteerCurve.Evaluate(steerT);
                steerAngle *= curve;

                var targetSteerAngle = steerAngle * steerAxis;
                var velForward = wheelVelocityWithoutUp.normalized;
                if (steerAxis == 0f && wheelSidewaysVelocity > MinimumSpeedToAutoSteer && Grounded)
                {
                    targetSteerAngle = (-Mathf.Clamp(Vector3.SignedAngle(velForward, _car.transform.forward, transform.up), -SteerAngle, SteerAngle)) * AutoSteer;
                }
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

                var wheelFwAbs = Mathf.Abs(wheelVelocityFw);
                var throttle = Mathf.Abs(_car.ThrottleAxis);
                CurrentSpeed = wheelVelocityFw;

                if (throttle >= 0.9f && wheelFwAbs <= throttle * 4f && Mathf.Sign(wheelVelocityFw) == Mathf.Sign(_car.ThrottleAxis) && Throttle)
                {
                    CurrentSpeed = 50f * Mathf.Sign(_car.ThrottleAxis);
                }
            }
            else
            {
                if (CurrentSpeed > 0f)
                    CurrentSpeed = Mathf.Max(CurrentSpeed - (RotationDeacceleration * Time.deltaTime), 0f);
                else
                    CurrentSpeed = Mathf.Min(CurrentSpeed + (RotationDeacceleration * Time.deltaTime), 0f);
            }

            if (_car.BrakeHeld && Throttle)
                CurrentSpeed = 0f;

            _currentRoll += CurrentSpeed * RotationMultiplier * Time.deltaTime;
            _currentRoll -= Mathf.Floor(_currentRoll / 360f) * 360f;
            Mesh.transform.localRotation = Quaternion.Euler(_currentRoll, 0f, 0f);
        }
    }
}
