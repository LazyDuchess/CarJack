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
        private float _currentFreeCameraTimer = 0f;

        private void ResetInputs()
        {
            _controller = false;
            _xAxis = 0f;
            _yAxis = 0f;
        }

        private void PollInputs()
        {
            ResetInputs();
#if PLUGIN
            var gameInput = Core.Instance.GameInput;

            _xAxis = gameInput.GetAxis(13, 0);
            _yAxis = gameInput.GetAxis(14, 0);

            if (gameInput.GetCurrentControllerType(0) == ControllerType.Joystick)
            {
                _currentFreeCameraTimer = 0f;
                _controller = true;
            }

#else
            _xAxis = Input.GetAxisRaw("Mouse X");
            _yAxis = Input.GetAxisRaw("Mouse Y");
#endif
            if ((_xAxis != 0f || _yAxis != 0f) && !_controller)
                _currentFreeCameraTimer = FreeCameraTimer;
        }

        private void Update()
        {
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

            
            var targetRotation = Quaternion.LookRotation(Target.Rigidbody.velocity.normalized, Vector3.up);
            var currentRotation = Quaternion.Lerp(transform.rotation, targetRotation, Mathf.Min(maxLerp, LerpMultiplier * Target.Rigidbody.velocity.magnitude) * Time.deltaTime).eulerAngles;
            
            if (_currentFreeCameraTimer <= 0f)
                transform.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0f);

            var target = Target.transform.position + (Height * Vector3.up);
            var origin = target - (transform.forward * Distance);

            var ray = new Ray(target, -transform.forward);
            if (Physics.Raycast(ray, out var hit, Distance + Radius, ObstructionMask))
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
