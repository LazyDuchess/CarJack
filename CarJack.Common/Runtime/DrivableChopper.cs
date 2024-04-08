using Reptile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarJack.Common
{
    public class DrivableChopper : DrivableCar
    {
        private const int LandingLayerMask = 1 << Layers.Default;
        private const float LandingRayDistance = 0.2f;
        [Header("Helicopter")]
        public Transform[] _landingSensors;

        protected override bool CheckGrounded()
        {
            foreach(var sensor in _landingSensors)
            {
                if (!CheckSensorGrounded(sensor))
                    return false;
            }
            return true;
        }

        private bool CheckSensorGrounded(Transform sensor)
        {
            var ray = new Ray(sensor.position, Vector3.down);
            return Physics.Raycast(ray, LandingRayDistance, LandingLayerMask);
        }
    }
}
