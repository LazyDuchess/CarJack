#if PLUGIN
using Reptile;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace CarJack.Common
{
    public class CarDriverSeat : CarSeat
    {
        public int HonkLayerIndex = -1;
        public int ReverseLayerIndex = -1;
        public float ReverseAnimationLerp = 10f;
        public float HonkAnimationLerp = 20f;
        public float SteerAnimationLerp = 5f;
        private float _currentSteer = 0.5f;
        private float _currentHonk = 0f;
        private float _currentReverse = 0f;
#if PLUGIN
        protected override void Update()
        {
            base.Update();
            if (CurrentVisual == null) return;
            var targetSteer = (Car.SteerAxis*0.5f) + 0.5f;
            _currentSteer = Mathf.Lerp(_currentSteer, targetSteer, SteerAnimationLerp * Time.deltaTime);
            CurrentVisual.anim.SetFloat("Steer", _currentSteer);
            var targetHonk = 0f;
            var targetReverse = 0f;
            var fwVelocity = Vector3.Dot(Car.Rigidbody.velocity, Car.transform.forward);
            if (fwVelocity <= -1f && Car.ThrottleAxis < 0f && Car.Grounded)
            {
                targetReverse = 1f;
            }
            if (Car.HornHeld)
                targetHonk = 1f;
            _currentHonk = Mathf.Lerp(_currentHonk, targetHonk, HonkAnimationLerp * Time.deltaTime);
            _currentReverse = Mathf.Lerp(_currentReverse, targetReverse, ReverseAnimationLerp * Time.deltaTime);
            if (HonkLayerIndex != -1)
                CurrentVisual.anim.SetLayerWeight(HonkLayerIndex, _currentHonk);
            if (ReverseLayerIndex != -1)
                CurrentVisual.anim.SetLayerWeight(ReverseLayerIndex, _currentReverse);
        }
#endif
    }
}
