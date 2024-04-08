using Reptile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CarJack.Common
{
    public class DrivableChopper : DrivableCar
    {
        private const int LandingLayerMask = 1 << Layers.Default;
        private const float LandingRayDistance = 0.1f;
        private const float NoseDownMultiplier = 1.5f;

        [Header("Helicopter")]
        public Transform[] LandingSensors;
        public float ThrottleSpeed = 1f;
        public float LiftAcceleration = 10f;
        public float LiftLerp = 5f;
        public float UprightForce = 1f;

        public float MovementAcceleration = 10f;
        public float IdleAcceleration = 1f;

        public float AirVerticalFriction = 1f;

        [NonSerialized]
        public float ThrottleAmount = 0f;
        [NonSerialized]
        public float LiftAmount = 0f;

        protected override void PollDrivingInputs()
        {
#if PLUGIN
            var gameInput = Core.Instance.GameInput;
            var controllerType = gameInput.GetCurrentControllerType(0);

            if (controllerType == Rewired.ControllerType.Joystick)
            {
                BrakeHeld = gameInput.GetButtonHeld(7, 0);
                PitchAxis = GetAxisDeadZone(gameInput, 6, ControllerRotationDeadZone);
                if (BrakeHeld)
                    RollAxis = GetAxisDeadZone(gameInput, 5, ControllerRotationDeadZone);
                else
                    YawAxis = GetAxisDeadZone(gameInput, 5, ControllerRotationDeadZone);
                ThrottleAxis += gameInput.GetAxis(8, 0);
                ThrottleAxis -= gameInput.GetAxis(18, 0);
            }
            else
            {
                YawAxis = gameInput.GetAxis(5, 0);
                ThrottleAxis = gameInput.GetAxis(6, 0);

                RollAxis += gameInput.GetButtonHeld(29, 0) ? 1f : 0f;
                RollAxis -= gameInput.GetButtonHeld(57, 0) ? 1f : 0f;

                PitchAxis += gameInput.GetButtonHeld(21, 0) ? 1f : 0f;
                PitchAxis -= gameInput.GetButtonHeld(56, 0) ? 1f : 0f;
            }
#else
            if (Input.GetKey(KeyCode.D))
                YawAxis += 1f;
            if (Input.GetKey(KeyCode.A))
                YawAxis -= 1f;

            if (Input.GetKey(KeyCode.W))
                ThrottleAxis += 1f;
            if (Input.GetKey(KeyCode.S))
                ThrottleAxis -= 1f;

            if (Input.GetKey(KeyCode.UpArrow))
                PitchAxis += 1f;
            if (Input.GetKey(KeyCode.DownArrow))
                PitchAxis -= 1f;

            if (Input.GetKey(KeyCode.RightArrow))
                RollAxis += 1f;
            if (Input.GetKey(KeyCode.LeftArrow))
                RollAxis -= 1f;
#endif
        }

        protected override bool CheckGrounded()
        {
            foreach(var sensor in LandingSensors)
            {
                if (!CheckSensorGrounded(sensor))
                    return false;
            }
            return true;
        }

        protected override void FixedUpdateCar()
        {
            base.FixedUpdateCar();

            var throttleAxis = ThrottleAxis;

            if (throttleAxis > 0f)
                throttleAxis = 1f;
            if (throttleAxis < 0f)
                throttleAxis = -1f;

            if (!Grounded)
                throttleAxis = Mathf.Max(0f, throttleAxis);

            if (ThrottleAmount != 0f && ThrottleAmount != 1f && ThrottleAxis == 0f)
            {
                throttleAxis = -1f;
            }

            if (ThrottleAmount >= 0.5f && Grounded && ThrottleAxis == 0f)
            {
                throttleAxis = 0f;

                if (0.9f > ThrottleAmount)
                    throttleAxis = 1f;

                if (ThrottleAmount > 0.99f)
                    ThrottleAmount = 0.99f;
            }

            ThrottleAmount += throttleAxis * ThrottleSpeed * Time.deltaTime;
            ThrottleAmount = Mathf.Clamp(ThrottleAmount, 0f, 1f);

            if (Grounded)
                LiftAmount = 0f;
            else
            {
                LiftAmount = Mathf.Lerp(LiftAmount, ThrottleAxis, LiftLerp * Time.deltaTime);
            }

            var lift = LiftAcceleration * ThrottleAmount;

            if (Grounded && ThrottleAmount < 1f)
                lift = 0f;

            var howMuchVerticalFrictionToApply = (-Mathf.Abs(Vector3.Dot(transform.up, Vector3.up)) + 1f) * NoseDownMultiplier;
            howMuchVerticalFrictionToApply = Mathf.Min(1f, howMuchVerticalFrictionToApply);

            howMuchVerticalFrictionToApply *= Mathf.Min(0f, ThrottleAxis) + 1f;
            //var howMuchVerticalFrictionToApply = Mathf.Abs(PitchAxis);
            //howMuchVerticalFrictionToApply = Mathf.Clamp(howMuchVerticalFrictionToApply, 0f, 1f);

            if (!Grounded)
                lift = LiftAmount * LiftAcceleration * (-howMuchVerticalFrictionToApply + 1f);

            Rigidbody.AddForce(Vector3.up * lift, ForceMode.Acceleration);

            if (ThrottleAmount >= 1f)
                Rigidbody.useGravity = false;
            else
                Rigidbody.useGravity = true;

            if (!Grounded && ThrottleAmount >= 1f)
            {
                var rollAngle = Vector3.SignedAngle(transform.up, Vector3.up, transform.forward);
                Rigidbody.AddTorque(rollAngle * transform.forward * UprightForce, ForceMode.Acceleration);

                var pitchAngle = Vector3.SignedAngle(transform.up, Vector3.up, transform.right);
                Rigidbody.AddTorque(pitchAngle * transform.right * UprightForce, ForceMode.Acceleration);

                var velWithoutUp = Rigidbody.velocity - Vector3.Project(Rigidbody.velocity, Vector3.up);
                Rigidbody.velocity = Vector3.Lerp(Rigidbody.velocity, velWithoutUp, AirVerticalFriction * howMuchVerticalFrictionToApply * Time.deltaTime);

                var facing = new Vector3(transform.up.x, 0f, transform.up.z).normalized;
                //var facing = transform.up;

                Rigidbody.AddForce(facing * (MovementAcceleration * Mathf.Max(0f, ThrottleAxis)) * howMuchVerticalFrictionToApply, ForceMode.Acceleration);
                Rigidbody.AddForce(facing * IdleAcceleration * howMuchVerticalFrictionToApply, ForceMode.Acceleration);

            }
        }

        private bool CheckSensorGrounded(Transform sensor)
        {
            var ray = new Ray(sensor.position, Vector3.down);
            return Physics.Raycast(ray, LandingRayDistance, LandingLayerMask);
        }
    }
}
