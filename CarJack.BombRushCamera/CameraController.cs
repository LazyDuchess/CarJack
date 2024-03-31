using CarJack.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BRC = BombRushCamera;

namespace CarJack.BombRushCamera
{
    public class CameraController : MonoBehaviour
    {
        private void Update()
        {
            CarCamera.Enabled = !BRC.Plugin.Active;
        }
    }
}
