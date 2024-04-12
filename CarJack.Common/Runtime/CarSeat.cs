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
    public abstract class CarSeat : MonoBehaviour
    {
        public bool PlayerVisible = true;
        public RuntimeAnimatorController controller;
        [NonSerialized]
        public DrivableCar Car;
        private float _blinkTimer = 0f;
        private const float BlinkDuration = 0.1f;
#if PLUGIN
        public Player Player;
        private Characters _cachedCharacter;
#endif
        protected virtual void Awake()
        {
            Car = GetComponentInParent<DrivableCar>();
            ResetBlinkTimer();
        }

        private void ResetBlinkTimer()
        {
            _blinkTimer = UnityEngine.Random.Range(2, 4);
        }

#if PLUGIN
        protected CharacterVisual CurrentVisual;
        public void PutInSeat(Player player)
        {
            _cachedCharacter = player.character;
            Player = player;
            if (PlayerVisible)
            {
                CurrentVisual = VisualFromPlayer(player, controller);
                var animator = CurrentVisual.GetComponentInChildren<Animator>();
                animator.runtimeAnimatorController = controller;
                CurrentVisual.transform.SetParent(transform);
                CurrentVisual.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
        }
        public void ExitSeat()
        {
            Player = null;
            if (CurrentVisual != null)
            {
                StopAllCoroutines();
                Destroy(CurrentVisual.gameObject);
            }
        }

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
            visual.characterObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            OpenEyes(visual);
            return visual;
        }

        private void LateUpdate()
        {
            if (Core.Instance.IsCorePaused) return;
            if (CurrentVisual == null) return;
            if (!CurrentVisual.canBlink) return;
            _blinkTimer -= Time.deltaTime;
            if (_blinkTimer <= 0f)
            {
                ResetBlinkTimer();
                StartCoroutine(DoBlink());
            }
        }

        private IEnumerator DoBlink()
        {
            CloseEyes(CurrentVisual);
            yield return new WaitForSeconds(BlinkDuration);
            OpenEyes(CurrentVisual);
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

        protected virtual void Update()
        {
            if (Core.Instance.IsCorePaused) return;
            if (Player != null && CurrentVisual != null)
            {
                if (Player.character != _cachedCharacter)
                {
                    var player = Player;
                    ExitSeat();
                    PutInSeat(player);
                }
            }
        }
#endif
    }
}
