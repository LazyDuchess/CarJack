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
        private CameraBlocker _blocker = new();
        private void Awake()
        {
            CarCamera.Blockers.Add(_blocker);
        }
        private void Update()
        {
            _blocker.Enabled = BRC.Plugin.Active;
        }
    }
}
