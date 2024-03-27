using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarJack.Common
{
    public class CarCamera : MonoBehaviour
    {
        public float LerpMultiplier = 10f;
        public float Distance = 1f;
        public float Height = 0.5f;
        public DrivableCar Target;

        private void Update()
        {
            var targetRotation = Quaternion.LookRotation(Target.Rigidbody.velocity.normalized, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, LerpMultiplier * Target.Rigidbody.velocity.magnitude * Time.deltaTime);
            transform.position = (Target.transform.position + (Height * Vector3.up)) - (transform.forward * Distance);
        }
    }
}
