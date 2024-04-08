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
    public class RotorAudio : MonoBehaviour
    {
        public float MinimumPitch = -2f;
        public float MaximumPitch = 1f;
        public float PitchLerp = 5f;
        private DrivableChopper _chopper;
        private AudioSource _audioSource;
        private float _currentPitch = 1f;
        public float MaxVolumeThreshold = 0.5f;

        private void Awake()
        {
            _chopper = GetComponentInParent<DrivableChopper>();
            _audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
#if PLUGIN
            if (Core.Instance.IsCorePaused) return;
#endif
            var targetPitch = Mathf.Lerp(MinimumPitch, MaximumPitch, _chopper.ThrottleAmount);
            _currentPitch = Mathf.Lerp(_currentPitch, targetPitch, PitchLerp * Time.deltaTime);

            _audioSource.pitch = _currentPitch;
            _audioSource.volume = Mathf.Max(0f, _chopper.ThrottleAmount / MaxVolumeThreshold);
        }
    }
}
