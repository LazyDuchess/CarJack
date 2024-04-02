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
        private AudioSource[] _pooledAudioSources;
        private void Awake()
        {
            _pooledAudioSources = GetComponentsInChildren<AudioSource>();
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
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
