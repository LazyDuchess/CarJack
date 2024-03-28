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
                var carController = CarController.Instance;
                if (carController.CurrentCar != null)
                {
                    var car = carController.CurrentCar;
                    carController.ExitCar();
                    Destroy(car.gameObject);
                }
                else
                    SpawnCar();
            }

            if (Input.GetKeyDown(KeyCode.F6))
            {
                var stageManager = Core.Instance.BaseModule.StageManager;
                stageManager.ExitCurrentStage(Utility.GetCurrentStage());
                CarAssets.Instance.ReloadAssets();
            }
        }

        public void SpawnCar()
        {
            var worldHandler = WorldHandler.instance;
            if (worldHandler == null) return;
            var player = worldHandler.GetCurrentPlayer();
            if (player == null) return;
            var carPrefab = CarAssets.Instance.Bundle.LoadAsset<GameObject>("Blue Car");
            var car = Instantiate(carPrefab);
            car.transform.position = player.transform.position + (player.transform.forward * 2f);
            car.transform.rotation = Quaternion.Euler(0f, player.transform.rotation.eulerAngles.y + 90f, 0f);
            CarController.Instance.EnterCar(car.GetComponent<DrivableCar>());
        }
    }
}
