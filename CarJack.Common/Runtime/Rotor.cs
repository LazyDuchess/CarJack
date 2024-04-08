using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarJack.Common
{
    public class Rotor : MonoBehaviour
    {
        public float Speed = 1f;
        private DrivableChopper _chopper;
        private void Awake()
        {
            _chopper = GetComponentInParent<DrivableChopper>();
        }

        private void Update()
        {
            transform.Rotate(Vector3.up, Speed * _chopper.ThrottleAmount * Time.deltaTime, Space.Self);
        }
    }
}
