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
    public class TyreAudio : MonoBehaviour
    {
        public float LerpSpeed = 5f;
        private DrivableCar _car;
        private AudioSource _audioSource;
        private float _currentVolume = 0f;
        private void Awake()
        {
            _car = GetComponentInParent<DrivableCar>();
            _audioSource = GetComponent<AudioSource>();
        }
        private void Update()
        {
#if PLUGIN
            if (Core.Instance.IsCorePaused) return;
#endif
            var targetVolume = 0f;
            foreach(var wheel in _car.Wheels)
            {
                if (wheel.Slipping >= 0.3f && wheel.Grounded)
                {
                    targetVolume = 1f;
                    break;
                }
            }
            _currentVolume = Mathf.Lerp(_currentVolume, targetVolume, LerpSpeed * Time.deltaTime);
            _audioSource.volume = _currentVolume;
        }
    }
}
