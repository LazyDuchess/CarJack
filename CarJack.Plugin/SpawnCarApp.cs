﻿using CarJack.Common;
using CarJack.Common.WhipRemix;
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

        public void SetBundleFilter(CarBundle bundle = null)
        {
            ScrollView.RemoveAllButtons();
            // Temp workarounds for scrolling being messed up when coming back to the app. Should probably move this to CommonAPI itself but I'm lazy atm.
            ScrollView.ResetScroll();
            ScrollView.CancelAnimation();
            foreach (var car in CarDatabase.CarByInternalName)
            {
                if (bundle != null && car.Value.Bundle != bundle)
                    continue;
                var carButton = CreateCarButton(car.Key);
                ScrollView.AddButton(carButton);
            }
        }

        public override void OnAppInit()
        {
            base.OnAppInit();
            CreateIconlessTitleBar("Spawn Car");
            ScrollView = PhoneScrollView.Create(this);
        }

        public override void OnAppUpdate()
        {
            base.OnAppUpdate();
            var carController = CarController.Instance;
            if (carController == null) return;
            var currentCar = carController.CurrentCar;
            if (currentCar == null) return;
            if (currentCar.Driving) return;
            MyPhone.CloseCurrentApp();
        }

        private SimplePhoneButton CreateCarButton(string carInternalName)
        {
            var car = CarDatabase.CarByInternalName[carInternalName];
            var button = PhoneUIUtility.CreateSimpleButton(car.Prefab.name);
            button.OnConfirm += () =>
            {
                CarController.Instance.ExitCar();
                MyPhone.ReturnToHome();
                var player = WorldHandler.instance.GetCurrentPlayer();
                var carPrefab = CarDatabase.CarByInternalName[carInternalName].Prefab;
                var carGO = Instantiate(carPrefab);
                carGO.transform.position = player.transform.position;
                carGO.transform.rotation = player.transform.rotation;
                var car = carGO.GetComponent<DrivableCar>();
                car.Initialize();
                car.DoorsLocked = PlayerData.Instance.DoorsLocked;
                var recolorableCar = car.GetComponent<RecolorableCar>();
                if (recolorableCar != null)
                    recolorableCar.ApplySavedRecolor();
                CarController.Instance.EnterCar(car);
            };
            return button;
        }
    }
}
