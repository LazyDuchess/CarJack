using System;
using Unity.Collections;
using UnityEngine;

namespace CarJack.Common
{
    public class CarWheel : MonoBehaviour
    {
        [NonSerialized]
        public bool Grounded = false;

        [Header("Suspension")]
        public float Damping = 0f;
        public float Strength = 0f;
        public float StartLength = 0f;
        public float MaxDistance = 0f;
        public float RestDistance = 0f;
        public float CurrentDistance { get; private set; }

        [Header("Visuals")]
        public GameObject Mesh;
        public float MeshRadius = 0.5f;
        public float RotationAcceleration = 100f;
        public float RotationDeacceleration = 1f;
        public float RotationMultiplier = 1f;

        [Header("Stats")]
        public float Mass = 10f;
        public float Traction = 0.5f;
        public float SteerAngle = 45f;
        public float SteerSpeed = 5f;
        public float ReverseSpeed = 400f;
        public float Speed = 10f;

        [Header("Functionality")]
        public bool Throttle = false;
        public bool Steer = false;
        public bool HandBrake = false;
        
        
        [NonSerialized]
        public float CurrentSpeed = 0f;

        private float _currentRoll = 0f;
        
        private float _currentSteerAngle = 0f;
        private DrivableCar _car;
        private float SlipSpeed = 5f;
        private const float MinimumSidewaysSpeedForSlip = 4f;
        private const float SidewaysSlipMultiplier = 0.05f;
        private const float MaximumSuspensionOffsetForRest = 0.5f;
        public float Slipping => _currentSlip;
        private float _currentSlip = 0f;

        private const float WheelSpinSlip = 0.35f;
        private const float WheelSpinSlipThreshold = 5f;
        private const float MaxSlipTractionLoss = 0.9f;

        public void Initialize(DrivableCar car)
        {
            _car = car;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, MeshRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position + transform.up * StartLength, transform.position - transform.up * MaxDistance);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position - transform.up * RestDistance);
        }

        public void DoPhysics(ref bool resting)
        {
            CalculateSlip();
            var tooSteep = false;
            if (_car.HasSurfaceAngleLimit)
            {
                var angle = Vector3.Angle(Vector3.up, transform.up);
                if (angle >= _car.SurfaceAngleLimit)
                {
                    tooSteep = true;
                    resting = false;
                }
            }
            CurrentDistance = MaxDistance;
            Grounded = false;
            var ray = new Ray(transform.position + (transform.up * StartLength), -transform.up);
            if (Physics.Raycast(ray, out var hit, MaxDistance + StartLength, _car.GroundMask))
            {
                CurrentDistance = hit.distance - StartLength;
                Grounded = true;
                var offset = RestDistance - CurrentDistance;
                var velocity = Vector3.Dot(_car.Rigidbody.GetPointVelocity(transform.position), transform.up);
                var force = (offset * Strength) - (velocity * Damping);
                if (tooSteep)
                    force = Mathf.Max(force, 0f);
                else
                {
                    if (Mathf.Abs(offset) > MaximumSuspensionOffsetForRest)
                        resting = false;
                }
                if (!_car.Still)
                    _car.Rigidbody.AddForceAtPosition(transform.up * force, transform.position);
            }

            if (Mesh != null)
            {
                Mesh.transform.position = transform.position - ((CurrentDistance - MeshRadius) * transform.up);
            }

            if (Grounded && !tooSteep)
            {
                var traction = Traction;
                var wheelVelocity = _car.Rigidbody.GetPointVelocity(transform.position);
                var wheelVelocityWithoutUp = (wheelVelocity - Vector3.Project(wheelVelocity, transform.up)).magnitude;
                var curve = Evaluate(_car.TractionCurve, wheelVelocityWithoutUp, _car.TractionCurveMax);
                traction *= curve;
                traction *= (-_currentSlip) +1f;

                traction = Mathf.Lerp(traction, DrivableCar.DriftTraction, _car.DriftingAmount);

                var slippingVelocity = Vector3.Dot(wheelVelocity, transform.right);
                var force = -slippingVelocity * traction;
                var acceleration = force / Time.fixedDeltaTime;
                _car.Rigidbody.AddForceAtPosition(transform.right * Mass * acceleration, transform.position);
            }
            DoInput(tooSteep);
        }

        private float Evaluate(AnimationCurve curve, float value, float maxValue)
        {
            value = Mathf.Abs(value);
            var t = Mathf.Min(value, maxValue) / maxValue;
            return curve.Evaluate(t);
        }

        private void CalculateSlip()
        {
            var wheelVelocity = _car.Rigidbody.GetPointVelocity(transform.position);
            var fwWheelVelocity = Vector3.Dot(wheelVelocity, transform.forward);
            var wheelVelocityDifference = Mathf.Abs(CurrentSpeed - fwWheelVelocity);
            var slip = 0f;
            if (wheelVelocityDifference >= WheelSpinSlipThreshold)
            {
                slip = WheelSpinSlip;
            }                

            var sideWaysVelocity = Mathf.Abs(Vector3.Dot(wheelVelocity, transform.right));

            sideWaysVelocity = Mathf.Max(0f, sideWaysVelocity - MinimumSidewaysSpeedForSlip);
            slip += sideWaysVelocity * SidewaysSlipMultiplier * _car.SlipMultiplier;

            slip = Mathf.Clamp(slip, 0f, MaxSlipTractionLoss);

            if (_currentSlip < slip)
                _currentSlip = slip;

            _currentSlip = Mathf.Lerp(_currentSlip, slip, SlipSpeed * Time.deltaTime);
        }

        private void DoInput(bool tooSteep)
        {
            var wheelVelocity = _car.Rigidbody.GetPointVelocity(transform.position);
            var wheelVelocityWithoutUp = (wheelVelocity - Vector3.Project(wheelVelocity, transform.up));
            var wheelVelocityWithoutUpMagnitude = wheelVelocityWithoutUp.magnitude;
            var wheelForwardVelocity = Vector3.Dot(wheelVelocity, transform.forward);

            var throttleAxis = _car.ThrottleAxis;
            var steerAxis = _car.SteerAxis;

            if (Grounded)
            {
                var deaccelerationAmount = 1f-Mathf.Abs(throttleAxis);
                
                
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

            var braking = ((forwardDot > DrivableCar.MaximumSpeedForStill && throttleAxis < 0f) || (forwardDot < -DrivableCar.MaximumSpeedForStill && throttleAxis > 0f)) && !_car.Still;

            if (Grounded && !tooSteep)
            {
                var addForce = false;

                var speed = Speed;
                var speedT = Mathf.Min(Mathf.Abs(wheelForwardVelocity), _car.SpeedCurveMax) / _car.SpeedCurveMax;
                var curve = _car.SpeedCurve.Evaluate(speedT);
                speed *= curve;

                if (throttleAxis < 0f)
                {
                    speed = ReverseSpeed;
                    speedT = Mathf.Min(Mathf.Abs(wheelForwardVelocity), _car.ReverseCurveMax) / _car.ReverseCurveMax;
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

                if (Throttle)
                    addForce = true;

                if (braking)
                    addForce = true;

                if (_car.BrakeHeld && HandBrake) {
                    if (forwardDot > 0f)
                        finalSpeed = -_car.HandBrakeForce;
                    else if (forwardDot < 0f)
                        finalSpeed = _car.HandBrakeForce;
                    if (_car.Rigidbody.velocity.magnitude <= DrivableCar.MaximumSpeedForStill || _car.Still)
                        finalSpeed = 0f;
                    addForce = true;
                }

                if (addForce)
                    _car.Rigidbody.AddForceAtPosition(transform.forward * finalSpeed, transform.position);
            }
            if (Steer)
            {
                var steerAngle = SteerAngle;
                var curve = Evaluate(_car.SteerCurve, wheelForwardVelocity, _car.SteerCurveMax);
                steerAngle *= curve;

                var targetSteerAngle = steerAngle * steerAxis;

                _currentSteerAngle = Mathf.Lerp(_currentSteerAngle, targetSteerAngle, SteerSpeed * Time.deltaTime);
                var counterSteer = 0f;
                if (Grounded)
                    counterSteer = _car.CounterSteering;
                transform.localRotation = Quaternion.Euler(0f, _currentSteerAngle + counterSteer, 0f);
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

            if (_car.BrakeHeld && HandBrake)
                CurrentSpeed = 0f;

            _currentRoll += CurrentSpeed * RotationMultiplier * Time.deltaTime;
            _currentRoll -= Mathf.Floor(_currentRoll / 360f) * 360f;
            Mesh.transform.localRotation = Quaternion.Euler(_currentRoll, 0f, 0f);
        }
    }
}
