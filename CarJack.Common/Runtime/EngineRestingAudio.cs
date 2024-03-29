#if PLUGIN
using Reptile;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarJack.Common
{
    [RequireComponent(typeof(AudioSource))]
    public class EngineRestingAudio : MonoBehaviour
    {
        public float MaximumSpeed = 1f;
        private DrivableCar _car;
        private AudioSource _audioSource;
        private void Awake()
        {
            _car = GetComponentInParent<DrivableCar>();
            _audioSource = GetComponent<AudioSource>();
#if PLUGIN
            Core.OnCoreUpdatePaused += OnPause;
            Core.OnCoreUpdateUnPaused += OnUnPause;
#endif
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
            var highestSpeed = 0f;
            foreach (var wheel in _car.Wheels)
            {
                if (!wheel.Throttle) continue;
                var speed = Mathf.Abs(wheel.CurrentSpeed);
                if (speed > highestSpeed)
                    highestSpeed = speed;
            }
            var volumeMultiplier = 0f;
            if (MaximumSpeed > 0f)
                volumeMultiplier = -(Mathf.Min(MaximumSpeed, highestSpeed) / MaximumSpeed) + 1f;

            _audioSource.volume = volumeMultiplier;
        }
    }
}
