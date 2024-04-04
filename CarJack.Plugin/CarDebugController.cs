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
            if (Input.GetKeyDown(KeyCode.F8))
            {
                var stageManager = Core.Instance.BaseModule.StageManager;
                stageManager.ExitCurrentStage(Utility.GetCurrentStage());
                CarAssets.Instance.ReloadAssets();
            }

            if (Input.GetKeyDown(KeyCode.F9))
            {
                var player = WorldHandler.instance.GetCurrentPlayer();
                var carPrefab = CarDatabase.CarByInternalName["carjack.benni"];
                var carGO = Instantiate(carPrefab);
                var car = carGO.GetComponent<DrivableCar>();

                car.transform.position = player.transform.position;
                car.transform.rotation = player.transform.rotation;

                car.Initialize();
                car.DoorsLocked = PlayerData.Instance.DoorsLocked;
            }

            if (Input.GetKeyDown(KeyCode.F10))
            {
                var currentCar = CarController.Instance.CurrentCar;
                if (currentCar == null) return;
                Destroy(currentCar.gameObject);
            }
        }
    }
}
