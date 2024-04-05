using CarJack.Common;
using CommonAPI;
using CommonAPI.Phone;
using Reptile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarJack.Plugin
{
    public class SpawnCarApp : CustomApp
    {
        public override bool Available => false;

        public override void OnAppInit()
        {
            base.OnAppInit();
            CreateIconlessTitleBar("Spawn Car");
            ScrollView = PhoneScrollView.Create(this);
        }

        public override void OnAppEnable()
        {
            base.OnAppEnable();
            ScrollView.RemoveAllButtons();
            // Temp workarounds for scrolling being messed up when coming back to the app. Should probably move this to CommonAPI itself but I'm lazy atm.
            ScrollView.ResetScroll();
            ScrollView.CancelAnimation();
            foreach(var car in CarDatabase.CarByInternalName)
            {
                var carButton = CreateCarButton(car.Key);
                ScrollView.AddButton(carButton);
            }
        }

        private SimplePhoneButton CreateCarButton(string carInternalName)
        {
            var car = CarDatabase.CarByInternalName[carInternalName];
            var button = PhoneUIUtility.CreateSimpleButton(car.name);
            button.OnConfirm += () =>
            {
                var player = WorldHandler.instance.GetCurrentPlayer();
                var carPrefab = CarDatabase.CarByInternalName[carInternalName];
                var carGO = Instantiate(carPrefab);
                carGO.transform.position = player.transform.position;
                carGO.transform.rotation = player.transform.rotation;
                var car = carGO.GetComponent<DrivableCar>();
                car.Initialize();
                car.DoorsLocked = PlayerData.Instance.DoorsLocked;
                CarController.Instance.EnterCar(car);
            };
            return button;
        }
    }
}
