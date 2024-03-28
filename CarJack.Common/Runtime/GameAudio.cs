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
    public class GameAudio : MonoBehaviour
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
        private void Awake()
        {
#if PLUGIN
            GetComponent<AudioSource>().outputAudioMixerGroup = Core.Instance.AudioManager.mixerGroups[(int)AudioType];
#endif
        }
    }
}
