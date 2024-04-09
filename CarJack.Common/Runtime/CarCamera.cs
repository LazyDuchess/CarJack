#if PLUGIN
using Reptile;
using Rewired;

#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarJack.Common
{
    public class CarCamera : MonoBehaviour
    {
        public static bool Enabled = true;
        private const float ReferenceDeltaTime = 1f / 60f;
        public static CarCamera Instance { get; private set; }
        public float Radius = 0.1f;
        public float MaxLerpSpeed = 5f;
        public float MaxLerpSpeedJoystick = 2f;
        public float FreeCameraTimer = 1f;
        public LayerMask ObstructionMask;
        public float LerpMultiplier = 0.15f;
        public float Distance = 7f;
        public float Height = 2f;
        public DrivableCar Target;
        private bool _controller = false;
        private float _xAxis = 0f;
        private float _yAxis = 0f;
        private bool _wasLookingBehind = false;
        private bool _lookBehind = false;
        private float _currentFreeCameraTimer = 0f;

        private void Awake()
        {
            Instance = this;
        }

        private void ResetInputs()
        {
            _controller = false;
            _xAxis = 0f;
            _yAxis = 0f;
            _lookBehind = false;
        }

        private void PollInputs()
        {
            ResetInputs();
#if PLUGIN
            var gameInput = Core.Instance.GameInput;

            /*
            inputBuffer.trick1ButtonHeld = this.gameInput.GetButtonHeld(15, 0);
			inputBuffer.trick2ButtonHeld = this.gameInput.GetButtonHeld(12, 0);
			inputBuffer.trick3ButtonHeld = this.gameInput.GetButtonHeld(65, 0);
            */

            _xAxis = gameInput.GetAxis(13, 0);
            _yAxis = gameInput.GetAxis(14, 0);
            _lookBehind = gameInput.GetButtonHeld(12, 0);

            if (gameInput.GetCurrentControllerType(0) == ControllerType.Joystick)
            {
                var dt = ReferenceDeltaTime / Time.deltaTime;
                _xAxis *= dt;
                _yAxis *= dt;

                _currentFreeCameraTimer = 0f;
                _controller = true;
            }

#else
            _xAxis = Input.GetAxisRaw("Mouse X");
            _yAxis = Input.GetAxisRaw("Mouse Y");
            _lookBehind = Input.GetKey(KeyCode.Mouse0);
#endif
            if ((_xAxis != 0f || _yAxis != 0f) && !_controller)
                _currentFreeCameraTimer = FreeCameraTimer;
        }

        private void Update()
        {
            if (!Enabled) return;
#if PLUGIN
            if (Core.Instance.IsCorePaused) return;
#endif
            if (Target == null)
                return;
            PollInputs();
#if PLUGIN
            var aimSensitivity = Core.Instance.SaveManager.Settings.gameplaySettings.aimSensitivity;
            var invertY = Core.Instance.SaveManager.Settings.gameplaySettings.invertY;
            var sensitivity = Mathf.Lerp(0.75f, 1.8f, aimSensitivity);
#else
            var sensitivity = Mathf.Lerp(0.75f, 1.8f, 0.5f);
            var invertY = false;
#endif
            var maxLerp = MaxLerpSpeed;
            _currentFreeCameraTimer = Mathf.Max(_currentFreeCameraTimer - Time.deltaTime, 0f);

            if (_controller || _currentFreeCameraTimer > 0f)
            {
                if (_controller)
                {
                    if (_xAxis != 0f || _yAxis != 0f)
                        maxLerp = MaxLerpSpeedJoystick;
                }

                var euler = transform.rotation.eulerAngles;
                euler.y += _xAxis * sensitivity;
                euler.x += _yAxis * sensitivity * (invertY ? 1 : -1);
                euler.z = 0f;

                euler.x = ConvertTo180Rotation(euler.x);

                euler.x = Mathf.Max(-80f, euler.x);
                euler.x = Mathf.Min(80f, euler.x);

                transform.rotation = Quaternion.Euler(euler);
            }

            var vel = Target.Rigidbody.velocity;
            if (Target is DrivableChopper)
                vel.y = 0f;

            var normalizedVelocity = vel.normalized;


            var targetRotation = transform.rotation;

            if (normalizedVelocity.magnitude > float.Epsilon && !Target.Still)
            {
                targetRotation = Quaternion.LookRotation(normalizedVelocity, Vector3.up);
                var euler = targetRotation.eulerAngles;
                euler.x += Target.ExtraPitch;
                targetRotation = Quaternion.Euler(euler);
            }

            var currentRotation = Quaternion.Lerp(transform.rotation, targetRotation, Mathf.Min(maxLerp, LerpMultiplier * vel.magnitude) * Time.deltaTime).eulerAngles;

            if (_currentFreeCameraTimer <= 0f)
            {
                transform.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0f);
            }

            if (_lookBehind)
            {
                transform.rotation = Quaternion.LookRotation(-Target.transform.forward, Vector3.up);
                var euler = transform.rotation.eulerAngles;
                euler.x += Target.ExtraPitch;
                transform.rotation = Quaternion.Euler(euler);
                _wasLookingBehind = true;
            }
            else if (_wasLookingBehind)
            {
                transform.rotation = Quaternion.LookRotation(Target.transform.forward, Vector3.up);
                var euler = transform.rotation.eulerAngles;
                euler.x += Target.ExtraPitch;
                transform.rotation = Quaternion.Euler(euler);
                _wasLookingBehind = false;
            }

            var distance = Distance + Target.ExtraDistance;
            var height = Height + Target.ExtraHeight;

            var target = Target.transform.position + (height * Vector3.up);
            var origin = target - (transform.forward * distance);

            var ray = new Ray(target, -transform.forward);
            if (Physics.Raycast(ray, out var hit, distance + Radius, ObstructionMask))
            {
                origin = target - (transform.forward * (hit.distance - Radius));
            }

            transform.position = origin;
        }

        private float ConvertTo180Rotation(float rotation)
        {
            if (rotation > 180f)
            {
                rotation = rotation - 360f;
            }
            return rotation;
        }

        public void SetTarget(DrivableCar target)
        {
            Target = target;
        }
    }
}
