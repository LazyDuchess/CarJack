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
        public float LerpMultiplier = 0.2f;
        public float Distance = 7f;
        public float Height = 2f;
        public DrivableCar Target;

        private void Update()
        {
            if (Target == null)
                return;
            var targetRotation = Quaternion.LookRotation(Target.Rigidbody.velocity.normalized, Vector3.up);
            var currentRotation = Quaternion.Lerp(transform.rotation, targetRotation, LerpMultiplier * Target.Rigidbody.velocity.magnitude * Time.deltaTime).eulerAngles;
            transform.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0f);
            transform.position = (Target.transform.position + (Height * Vector3.up)) - (transform.forward * Distance);
        }
    }
}
