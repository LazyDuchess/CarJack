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
    public class CameraBlocker
    {
        public bool Enabled = false;
    }
    public class CarCamera : MonoBehaviour
    {
        public static List<CameraBlocker> Blockers = new();
        public static bool Enabled => Blockers.Where((blocker) => { return blocker.Enabled; }).Count() == 0;
        public static CarCamera Instance { get; private set; }
        public float Radius = 0.1f;
        public float MaxLerpSpeed = 5f;
        public float MaxLerpSpeedJoystick = 2f;
        public float FreeCameraTimer = 1f;
        public LayerMask ObstructionMask;
        public float LerpMultiplier = 0.15f;
        public float Distance = 7f;
        public float Height = 2f;
        public float Fov = 60f;
        public DrivableCar Target;
        private bool _controller = false;
        private float _xAxis = 0f;
        private float _yAxis = 0f;
        private bool _wasLookingBehind = false;
        private bool _lookBehind = false;
        private float _currentFreeCameraTimer = 0f;

        private float _currentJumpAnimation = 0f;
        private bool _inJumpAnimation = false;

        public float JumpAnimationDistance = 1f;
        public float JumpAnimationHeight = 0.5f;
        public float JumpAnimationMinSpeed = 10f;
        public float JumpAnimationBeginSpeed = 1f;
        public float JumpAnimationLandSpeed = 5f;
        public float JumpAnimationFov = 2f;

        private float _currentSpeedAnimation = 0f;

        public float SpeedAnimationDistance = 3f;
        public float SpeedAnimationFov = 10f;
        public float SpeedAnimationMinSpeed = 20f;
        public float SpeedAnimationMaxSpeed = 80f;
        public float SpeedAnimationSpeed = 5f;

        private float _currentBrakeAnimation = 0f;
        private bool _onBrakeAnimation = false;

        public float BrakeAnimationThreshold = 0.5f;
        public float BrakeAnimationSpeed = 1f;
        public float BrakeAnimationStopSpeed = 0.2f;
        public float BrakeAnimationDistance = -1f;
        public float BrakeAnimationFov = -2f;


        private Camera _camera;

        private void Awake()
        {
            Instance = this;
            _camera = GetComponent<Camera>();
        }

        private void OnDestroy()
        {
            _camera.fieldOfView = 64f;
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
                if (!CarController.Config.MouseCameraControlsOnController)
                    _currentFreeCameraTimer = 0f;
                _controller = true;
            }

#else
            _xAxis = Input.GetAxisRaw("Mouse X");
            _yAxis = Input.GetAxisRaw("Mouse Y");
            _lookBehind = Input.GetKey(KeyCode.Mouse0);
#endif
            if ((_xAxis != 0f || _yAxis != 0f) && (!_controller || CarController.Config.MouseCameraControlsOnController))
                _currentFreeCameraTimer = FreeCameraTimer;
        }

        private float _lastFwSpeed = 0f;

        private void UpdateBrakeAnimation()
        {
            var fwSpeed = Target.Rigidbody.velocity.magnitude;
            var targetBrakeAnimation = 0f;

            var speedDiff = (fwSpeed - _lastFwSpeed);

            var negSpeedDiff = -speedDiff;
            var posSpeedDiff = speedDiff;

            if (posSpeedDiff <= 0f)
                posSpeedDiff = 0f;

            if (negSpeedDiff <= 0f)
                negSpeedDiff = 0f;

            if (negSpeedDiff >= BrakeAnimationThreshold)
                _onBrakeAnimation = true;
            else if (posSpeedDiff >= 0f)
                _onBrakeAnimation = false;

            if (fwSpeed > 10f && (Target.BrakeHeld || Target.ThrottleAxis < -0.5f))
                _onBrakeAnimation = true;

            if (Target.AllWheelsOffGround)
                _onBrakeAnimation = false;

            if (_onBrakeAnimation)
                targetBrakeAnimation = 1f;


            if (targetBrakeAnimation == 1f)
                _currentBrakeAnimation = Mathf.Lerp(_currentBrakeAnimation, targetBrakeAnimation, BrakeAnimationSpeed * Time.deltaTime);
            else
                _currentBrakeAnimation = Mathf.Lerp(_currentBrakeAnimation, targetBrakeAnimation, (BrakeAnimationStopSpeed + posSpeedDiff) * Time.deltaTime);
            _lastFwSpeed = fwSpeed;
        }

        private void UpdateSpeedAnimation()
        {
            var targetSpeedAnimation = 0f;
            var camVel = Vector3.Dot(Target.Rigidbody.velocity, transform.forward);
            if (camVel >= SpeedAnimationMinSpeed)
            {
                var maxSp = SpeedAnimationMaxSpeed - SpeedAnimationMinSpeed;
                var vel = camVel - SpeedAnimationMinSpeed;
                targetSpeedAnimation = Mathf.Min(1f, vel / maxSp);
            }

            _currentSpeedAnimation = Mathf.Lerp(_currentSpeedAnimation, targetSpeedAnimation, SpeedAnimationSpeed * Time.deltaTime);

            if (_currentSpeedAnimation <= 0f)
                _currentSpeedAnimation = 0f;
        }

        private void UpdateJumpAnimation()
        {
            if (Target is DrivableChopper) return;

            if (Target.AllWheelsOffGround)
            {
                if (Target.Rigidbody.velocity.y >= JumpAnimationMinSpeed)
                {
                    _inJumpAnimation = true;
                }
            }
            else
            {
                _inJumpAnimation = false;
            }

            if (_inJumpAnimation)
                _currentJumpAnimation = Mathf.Lerp(_currentJumpAnimation, 1f, JumpAnimationBeginSpeed * Time.deltaTime);
            else
                _currentJumpAnimation = Mathf.Lerp(_currentJumpAnimation, 0f, JumpAnimationLandSpeed * Time.deltaTime);
        }

        private void Update()
        {
            if (!Enabled) return;
#if PLUGIN
            if (Core.Instance.IsCorePaused) return;
#endif
            if (Target == null)
                return;

            UpdateJumpAnimation();
            UpdateBrakeAnimation();
            UpdateSpeedAnimation();

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

            if (_controller)
            {
                _xAxis *= Time.deltaTime * 100f;
                _yAxis *= Time.deltaTime * 100f;
            }

            if ((_controller && !CarController.Config.MouseCameraControlsOnController) || _currentFreeCameraTimer > 0f)
            {
                if (_controller && !CarController.Config.MouseCameraControlsOnController)
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
            var velFw = Vector3.Dot(Target.Rigidbody.velocity, Target.transform.forward);
            if (Target is DrivableChopper)
                vel.y = 0f;

            var normalizedVelocity = vel.normalized;


            var targetRotation = transform.rotation;

            if (velFw > 1f)
                normalizedVelocity = Vector3.Lerp(normalizedVelocity, Target.transform.forward, 0.5f).normalized;

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
            var fov = Fov;

            distance += _currentJumpAnimation * JumpAnimationDistance;
            height += _currentJumpAnimation * JumpAnimationHeight;
            fov += _currentJumpAnimation * JumpAnimationFov;

            distance += _currentSpeedAnimation * SpeedAnimationDistance;
            fov += _currentSpeedAnimation * SpeedAnimationFov;

            distance += _currentBrakeAnimation * BrakeAnimationDistance;
            fov += _currentBrakeAnimation * BrakeAnimationFov;

            var target = Target.transform.position + (height * Vector3.up);
            var origin = target - (transform.forward * distance);

            var ray = new Ray(target, -transform.forward);
            if (Physics.Raycast(ray, out var hit, distance + Radius, ObstructionMask))
            {
                origin = target - (transform.forward * (hit.distance - Radius));
            }

            transform.position = origin;
            _camera.fieldOfView = fov;
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
