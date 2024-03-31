using CarJack.Plugin;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarJack.BombRushCamera
{
    [CarJackPlugin]
    public class Plugin
    {
        public Plugin()
        {
            var go = new GameObject("CarJack BombRushCamera controller");
            go.AddComponent<CameraController>();
            Object.DontDestroyOnLoad(go);
        }
    }
}
