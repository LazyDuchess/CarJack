using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#if PLUGIN
using Reptile;
#endif

namespace CarJack.Common
{
    [RequireComponent(typeof(AudioSource))]
    public class EngineAudio : MonoBehaviour
    {
        public float PitchLerp = 5f;
        public AnimationCurve PitchCurve;
        public float PitchCurveMax = 100f;
        public float AddPitch = 1f;
        private DrivableCar _car;
        private AudioSource _audioSource;
        private float _currentPitch = 1f;

        private void Awake()
        {
            _car = GetComponentInParent<DrivableCar>();
            _audioSource = GetComponent<AudioSource>();
#if PLUGIN
            Core.OnCoreUpdatePaused += OnPause;
            Core.OnCoreUpdateUnPaused += OnUnPause;
#endif
        }

        private float EvaluatePitchCurve(float value)
        {
            var val = Mathf.Min(PitchCurveMax, Mathf.Abs(value)) / PitchCurveMax;
            return PitchCurve.Evaluate(val);
        }

        private void OnPause()
        {
            _audioSource.mute = true;
        }

        private void OnUnPause()
        {
            _audioSource.mute = false;
        }

        private void OnDestroy()
        {
#if PLUGIN
            Core.OnCoreUpdatePaused -= OnPause;
            Core.OnCoreUpdateUnPaused -= OnUnPause;
#endif
        }

        private void Update()
        {
#if PLUGIN
            if (Core.Instance.IsCorePaused) return;
#endif
            if (_car.Driving)
                _audioSource.spatialBlend = 0f;
            else
                _audioSource.spatialBlend = 1f;
            var targetPitch = 1f;
            var highestSpeed = 0f;
            foreach(var wheel in _car.Wheels)
            {
                if (!wheel.Throttle) continue;
                var speed = Mathf.Abs(wheel.CurrentSpeed);
                if (speed > highestSpeed)
                    highestSpeed = speed;
            }
            var evaluatedPitch = EvaluatePitchCurve(highestSpeed) * AddPitch;
            targetPitch += evaluatedPitch;

            _currentPitch = Mathf.Lerp(_currentPitch, targetPitch, PitchLerp * Time.deltaTime);
            _audioSource.pitch = _currentPitch;
        }
    }
}
