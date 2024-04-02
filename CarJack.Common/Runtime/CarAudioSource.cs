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
    public class CarAudioSource : MonoBehaviour
    {
        public enum AudioTypes
        {
            Master = 0,
            Music = 4,
            SFX = 1,
            UI = 2,
            Gameplay = 3,
            Voices = 5,
            Ambience = 6
        }

        public AudioTypes AudioType = AudioTypes.Gameplay;
        private DrivableCar _car;
        private AudioSource _audioSource;
        private void Awake()
        {
            _car = GetComponentInParent<DrivableCar>();
            _audioSource = GetComponent<AudioSource>();
#if PLUGIN
            _audioSource.outputAudioMixerGroup = Core.Instance.AudioManager.mixerGroups[(int)AudioType];
            Core.OnCoreUpdatePaused += OnPause;
            Core.OnCoreUpdateUnPaused += OnUnPause;
#endif
        }

        private void OnDestroy()
        {
#if PLUGIN
            Core.OnCoreUpdatePaused -= OnPause;
            Core.OnCoreUpdateUnPaused -= OnUnPause;
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

        private void Update()
        {
#if PLUGIN
            if (Core.Instance.IsCorePaused) return;
#endif
            var carCamera = CarCamera.Instance;
            if (carCamera == null || carCamera.Target != _car)
                _audioSource.spatialBlend = 1f;
            else
                _audioSource.spatialBlend = 0f;
        }
    }
}
