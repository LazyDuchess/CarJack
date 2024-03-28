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
    public class ScrapeAudio : MonoBehaviour
    {
        public float SpeedMultiplier = 0.1f;
        public float MinimumSpeed = 5f;
        public float LerpSpeed = 10f;
        private AudioSource _audioSource;
        private DrivableCar _car;
        private float _targetVolume = 0f;
        private void Awake()
        {
            _car = GetComponentInParent<DrivableCar>();
            _audioSource = GetComponent<AudioSource>();
            _audioSource.volume = 0f;
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
        public void OnScrape(Collision other)
        {
            var normal = other.contacts[0].normal;
            var velocityAway = (_car.Rigidbody.velocity - Vector3.Project(_car.Rigidbody.velocity, normal)).magnitude;
            if (velocityAway > MinimumSpeed)
            {
                velocityAway -= MinimumSpeed;
                velocityAway = Mathf.Min(1f, velocityAway * SpeedMultiplier);
                _targetVolume = velocityAway;
            }
        }

        private void FixedUpdate()
        {
            _targetVolume = 0f;
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
            _audioSource.volume = Mathf.Lerp(_audioSource.volume, _targetVolume, LerpSpeed * Time.deltaTime);
        }
    }
}
