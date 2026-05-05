using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarJack.BombRushCamera
{
    public class Plugin
    {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public Plugin()
        {
            var go = new GameObject("CarJack BombRushCamera controller");
            go.AddComponent<CameraController>();
            Object.DontDestroyOnLoad(go);
        }
    }
}
