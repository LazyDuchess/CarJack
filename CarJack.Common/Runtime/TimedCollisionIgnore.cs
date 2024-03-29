using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#if PLUGIN
using Reptile;
#endif

namespace CarJack.Common
{
    public class TimedCollisionIgnore : MonoBehaviour
    {
        private Collider _owner;
        private Collider _other;
        private float _time;
        public static TimedCollisionIgnore Create(Collider owner, Collider other, float duration)
        {
            var component = owner.gameObject.AddComponent<TimedCollisionIgnore>();
            component.Initialize(owner, other, duration);
            return component;
        }

        public void Initialize(Collider owner, Collider other, float duration)
        {
            _owner = owner;
            _other = other;
            _time = duration;
            Physics.IgnoreCollision(_owner, _other, true);
        }

        private void Update()
        {
#if PLUGIN
            if (Core.Instance.IsCorePaused) return;
#endif
            _time -= Time.deltaTime;
            if (_time <= 0f)
            {
                if (_owner != null && _other != null)
                    Physics.IgnoreCollision(_owner, _other, false);
                Destroy(this);
            }
        }
    }
}
