#if PLUGIN
using Reptile;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarJack.Common.Runtime
{
    public class CarAudioLOD : MonoBehaviour
    {
        private const float LODDistance = 80f;
        private DrivableCar _car;
#if PLUGIN
        private void Awake()
        {
            _car = GetComponentInParent<DrivableCar>();
            Core.OnUpdate += CoreUpdate;
        }

        private void OnDestroy()
        {
            Core.OnUpdate -= CoreUpdate;
        }

        private void SetActive(bool set)
        {
            if (gameObject.activeSelf && !set)
                gameObject.SetActive(false);
            else if (!gameObject.activeSelf && set)
                gameObject.SetActive(true);
        }

        private void CoreUpdate()
        {

            if (_car.Driving)
            {
                SetActive(true);
                return;
            }
            var cam = WorldHandler.instance.CurrentCamera;
            if (cam == null) return;
            var dist = Vector3.Distance(cam.transform.position, transform.position);
            if (dist > LODDistance)
                SetActive(false);
            else
                SetActive(true);

        }
#endif
    }
}
