using CarJack.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#if PLUGIN
using Reptile;
#endif
using System.Runtime.ConstrainedExecution;

namespace CarJack.Common
{
    public class CarController : MonoBehaviour
    {
        public static CarController Instance { get; private set; }
        public DrivableCar CurrentCar;
        public static void Initialize()
        {
#if PLUGIN
            StageManager.OnStageInitialized += StageManager_OnStageInitialized;
#endif
        }

#if PLUGIN
        private static void StageManager_OnStageInitialized()
        {
            Create();
            CreateResources();
        }

        private static void CreateResources()
        {
            var assets = CarAssets.Instance;
            var resources = assets.Bundle.LoadAsset<GameObject>("Car Resources");
            Instantiate(resources);
        }

        public static CarController Create()
        {
            var gameObject = new GameObject("Car Controller");
            return gameObject.AddComponent<CarController>();
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (CurrentCar != null)
            {
                var player = WorldHandler.instance.GetCurrentPlayer();
                player.transform.position = CurrentCar.transform.position;
                var flatForward = (CurrentCar.transform.forward - Vector3.Project(CurrentCar.transform.forward, Vector3.up)).normalized;
                player.SetRotHard(Quaternion.LookRotation(flatForward, Vector3.up));
            }
        }

        private CarCamera MakeCamera(GameObject go)
        {
            var cam = go.AddComponent<CarCamera>();
            cam.ObstructionMask = Layers.Junk | Layers.VertSurface | Layers.Default | Layers.NonStableSurface | Layers.Wallrun;
            return cam;
        }

        public void EnterCar(DrivableCar car)
        {
            if (CurrentCar != null)
                CurrentCar.Driving = false;
            CurrentCar = car;
            car.Driving = true;
            var player = WorldHandler.instance.GetCurrentPlayer();
            player.phone.TurnOff(false);
            player.characterVisual.SetPhone(false);
            car.GroundMask = player.motor.groundDetection.groundMask;
            player.DisablePlayer();
            player.CompletelyStop();
            player.gameObject.SetActive(false);
            var gameplayCamera = GameplayCamera.instance;
            gameplayCamera.enabled = false;
            var cameraComponent = gameplayCamera.GetComponent<CarCamera>();
            if (cameraComponent == null)
                cameraComponent = MakeCamera(gameplayCamera.gameObject);
            cameraComponent.SetTarget(car);
            car.EnterCar(player);
        }

        public void ExitCar()
        {
            if (CurrentCar == null) return;
            CurrentCar.ExitCar();
            CurrentCar.Driving = false;
            CurrentCar = null;
            var gameplayCamera = GameplayCamera.instance;
            gameplayCamera.enabled = true;
            var cameraComponent = gameplayCamera.GetComponent<CarCamera>();
            if (cameraComponent != null)
                Destroy(cameraComponent);
            var player = WorldHandler.instance.GetCurrentPlayer();
            player.gameObject.SetActive(true);
            player.EnablePlayer();
            gameplayCamera.ResetCameraPositionRotation();
        }
#endif
    }
}
