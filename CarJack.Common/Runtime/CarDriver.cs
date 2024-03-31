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
    public class CarDriver : MonoBehaviour
    {
        public int HonkLayerIndex = -1;
        public int ReverseLayerIndex = -1;
        public float ReverseAnimationLerp = 10f;
        public float HonkAnimationLerp = 20f;
        public float SteerAnimationLerp = 5f;
        public RuntimeAnimatorController controller;
        private DrivableCar _car;
        private float _currentSteer = 0.5f;
        private float _currentHonk = 0f;
        private float _currentReverse = 0f;
        private float _blinkTimer = 0f;
        private const float BlinkDuration = 0.1f;
        private void Awake()
        {
            _car = GetComponentInParent<DrivableCar>();
            ResetBlinkTimer();
        }

        private void ResetBlinkTimer()
        {
            _blinkTimer = UnityEngine.Random.Range(2, 4);
        }

#if PLUGIN
        private CharacterVisual _currentVisual;
        public void PutInCar(Player player)
        {
            _currentSteer = 0.5f;
            _currentVisual = VisualFromPlayer(player, controller);
            var animator = _currentVisual.GetComponentInChildren<Animator>();
            animator.runtimeAnimatorController = controller;
            _currentVisual.transform.SetParent(transform);
            _currentVisual.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        // Copy the players visuals for driving.
        private CharacterVisual VisualFromPlayer(Player player, RuntimeAnimatorController controller)
        {
            var visualObject = Instantiate(player.characterVisual.gameObject);
            visualObject.SetActive(true);
            var visual = visualObject.GetComponent<CharacterVisual>();
            visual.mainRenderer.enabled = true;
            visual.SetMoveStyleVisualAnim(null, MoveStyle.ON_FOOT, null);
            visual.SetMoveStyleVisualProps(null, MoveStyle.ON_FOOT, false);
            visual.SetSpraycan(false);
            visual.SetPhone(false);
            visual.SetBoostpackEffect(BoostpackEffectMode.OFF);
            visual.VFX.boostpackTrail.SetActive(false);
            visual.Init(Characters.NONE, controller, false, 0f);
            visual.canBlink = player.characterVisual.canBlink;
            OpenEyes(visual);
            return visual;
        }

        private void LateUpdate()
        {
            if (Core.Instance.IsCorePaused) return;
            if (!_currentVisual.canBlink) return;
            _blinkTimer -= Time.deltaTime;
            if (_blinkTimer <= 0f)
            {
                ResetBlinkTimer();
                StartCoroutine(DoBlink());
            }
        }

        private IEnumerator DoBlink()
        {
            CloseEyes(_currentVisual);
            yield return new WaitForSeconds(BlinkDuration);
            OpenEyes(_currentVisual);
        }

        private void CloseEyes(CharacterVisual visual)
        {
            if (!visual.canBlink) return;
            if (visual.mainRenderer.sharedMesh.blendShapeCount <= 0) return;
            visual.mainRenderer.SetBlendShapeWeight(0, 100f);
        }

        private void OpenEyes(CharacterVisual visual)
        {
            if (!visual.canBlink) return;
            if (visual.mainRenderer.sharedMesh.blendShapeCount <= 0) return;
            visual.mainRenderer.SetBlendShapeWeight(0, 0f);
        }

        public void ExitCar()
        {
            if (_currentVisual != null)
            {
                StopAllCoroutines();
                Destroy(_currentVisual.gameObject);
            }
        }

        private void Update()
        {
            if (Core.Instance.IsCorePaused) return;
            if (_currentVisual == null) return;
            var targetSteer = (_car.SteerAxis*0.5f) + 0.5f;
            _currentSteer = Mathf.Lerp(_currentSteer, targetSteer, SteerAnimationLerp * Time.deltaTime);
            _currentVisual.anim.SetFloat("Steer", _currentSteer);
            var targetHonk = 0f;
            var targetReverse = 0f;
            var fwVelocity = Vector3.Dot(_car.Rigidbody.velocity, _car.transform.forward);
            if (fwVelocity <= 1f && _car.ThrottleAxis < 0f && _car.Grounded)
            {
                targetReverse = 1f;
            }
            if (_car.HornHeld)
                targetHonk = 1f;
            _currentHonk = Mathf.Lerp(_currentHonk, targetHonk, HonkAnimationLerp * Time.deltaTime);
            _currentReverse = Mathf.Lerp(_currentReverse, targetReverse, ReverseAnimationLerp * Time.deltaTime);
            if (HonkLayerIndex != -1)
                _currentVisual.anim.SetLayerWeight(HonkLayerIndex, _currentHonk);
            if (ReverseLayerIndex != -1)
                _currentVisual.anim.SetLayerWeight(ReverseLayerIndex, _currentReverse);
        }
#endif
    }
}
