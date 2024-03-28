using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarJack.Common
{
    public class OneShotAudioSource : MonoBehaviour
    {
        private DrivableCar _car;
        private AudioSource[] _pooledAudioSources;
        private void Awake()
        {
            _pooledAudioSources = GetComponentsInChildren<AudioSource>();
            _car = GetComponentInParent<DrivableCar>();
        }

        private AudioSource GetPooledAudioSource()
        {
            foreach(var audioSource in _pooledAudioSources)
            {
                if (!audioSource.isPlaying)
                    return audioSource;
            }
            return _pooledAudioSources[0];
        }
        public void Play(AudioClip clip)
        {
            var audioSource = GetPooledAudioSource();
            if (_car.Driving)
                audioSource.spatialBlend = 0f;
            else
                audioSource.spatialBlend = 1f;
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
