using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarJack.Common
{
    [RequireComponent(typeof(AudioSource))]
    public class OneShotAudioSource : MonoBehaviour
    {
        private DrivableCar _car;
        private AudioSource _audioSource;
        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _car = GetComponentInParent<DrivableCar>();
        }
        public void Play(AudioClip clip)
        {
            if (_car.Driving)
                _audioSource.spatialBlend = 0f;
            else
                _audioSource.spatialBlend = 1f;
            _audioSource.Stop();
            _audioSource.clip = clip;
            _audioSource.Play();
        }
    }
}
