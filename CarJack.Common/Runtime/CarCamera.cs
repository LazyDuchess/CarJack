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
        public LayerMask ObstructionMask;
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

            var target = Target.transform.position + (Height * Vector3.up);
            var origin = target - (transform.forward * Distance);

            var ray = new Ray(target, -transform.forward);
            if (Physics.Raycast(ray, out var hit, Distance, ObstructionMask))
            {
                origin = target - (transform.forward * hit.distance);
            }

            transform.position = origin;
        }
    }
}
