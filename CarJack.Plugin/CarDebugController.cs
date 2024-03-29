using CarJack.Common;
using Reptile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarJack.Plugin
{
    public class CarDebugController : MonoBehaviour
    {
        public static CarDebugController Create()
        {
            var gameObject = new GameObject("Car Debug Controller");
            DontDestroyOnLoad(gameObject);
            return gameObject.AddComponent<CarDebugController>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                var stageManager = Core.Instance.BaseModule.StageManager;
                stageManager.ExitCurrentStage(Utility.GetCurrentStage());
                CarAssets.Instance.ReloadAssets();
            }
        }
    }
}
