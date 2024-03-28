using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarJack.Common
{
    public class CarResources : MonoBehaviour
    {
        public static CarResources Instance { get; private set; }
        public AudioClip[] CrashSFX;
        public AudioClip[] ScrapeSFX;

        private void Awake()
        {
            Instance = this;
        }

        public AudioClip GetCrashSFX()
        {
            return CrashSFX[Random.Range(0, CrashSFX.Length)];
        }
    }
}
