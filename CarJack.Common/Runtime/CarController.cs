using CarJack.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

#if PLUGIN
using Reptile;
#endif
using System.Runtime.ConstrainedExecution;

namespace CarJack.Common
{
    public class CarController : MonoBehaviour
    {
        public static CarController Instance { get; private set; }
        public static ICarConfig Config;
        public DrivableCar CurrentCar;
        public CarPassengerSeat CurrentSeat;
        public static void Initialize(ICarConfig config)
        {
            Config = config;
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
                if (CurrentSeat != null)
                    player.transform.position = CurrentSeat.transform.position;
                else
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

        public void EnterCarAsPassenger(DrivableCar car, int seatIndex)
        {
            if (CurrentCar != null)
            {
                CurrentCar.Driving = false;
                CurrentCar.InCar = false;
            }
            var seat = car.GetPassengerSeat(seatIndex);
            CurrentCar = car;
            CurrentSeat = seat;
            car.Driving = false;
            car.InCar = true;
            var player = WorldHandler.instance.GetCurrentPlayer();
            player.phone.TurnOff(false);
            player.StopHoldSpraycan();
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
            player.FlushInput();
            seat.PutInSeat(player);
        }

        public void EnterCar(DrivableCar car)
        {
            if (CurrentCar != null)
            {
                CurrentCar.Driving = false;
                CurrentCar.InCar = false;
            }
            CurrentCar = car;
            CurrentSeat = null;
            car.Driving = true;
            car.InCar = true;
            var player = WorldHandler.instance.GetCurrentPlayer();
            player.phone.TurnOff(false);
            player.StopHoldSpraycan();
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
            player.FlushInput();
            car.EnterCar(player);
        }

        public void ExitCar()
        {
            var car = CurrentCar;
            if (CurrentCar == null) return;
            var wasPassenger = false;
            if (CurrentCar.Driving)
                CurrentCar.ExitCar();
            else
            {
                wasPassenger = true;
                CurrentSeat.ExitSeat();
            }
            CurrentCar.Driving = false;
            CurrentCar.InCar = false;
            CurrentSeat = null;
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
            if (!wasPassenger)
                Destroy(car.gameObject);
        }
#endif
    }
}
