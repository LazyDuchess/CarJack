using CarJack.Common;
using CarJack.Common.WhipRemix;
using CommonAPI.Phone;
using Reptile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace CarJack.Plugin
{
    public class RecolorApp : CustomApp
    {
        private const string NewRecolorFolder = "WhipRemix";
        public override bool Available => false;

        public static void Initialize()
        {
            CarController.OnPlayerEnteredCar += OnEnterCar;
        }

        private static void OnEnterCar()
        {
            var player = WorldHandler.instance.GetCurrentPlayer();
            if (player == null) return;
            var phone = player.phone;
            if (phone == null) return;
            var app = phone.GetAppInstance<RecolorApp>();
            if (app == null) return;
            app.SetForCurrentCar();
        }

        public static bool AvailableForCurrentCar()
        {
            var carController = CarController.Instance;
            if (carController == null) return false;
            var car = carController.CurrentCar;
            if (car == null) return false;
            if (!car.Driving) return false;
            if (car.GetComponent<RecolorableCar>() == null) return false;
            return true;
        }

        public void SetForCurrentCar()
        {
            ScrollView.RemoveAllButtons();
            // Temp workarounds for scrolling being messed up when coming back to the app. Should probably move this to CommonAPI itself but I'm lazy atm.
            ScrollView.ResetScroll();
            ScrollView.CancelAnimation();
            var carController = CarController.Instance;
            if (carController == null) return;
            var car = carController.CurrentCar;
            if (car == null) return;

            var button = PhoneUIUtility.CreateSimpleButton("Stock");
            button.OnConfirm += () =>
            {
                car.GetComponent<RecolorableCar>().ApplyDefaultColor();
            };
            ScrollView.AddButton(button);

            foreach (var recolor in RecolorManager.RecolorsByGUID)
            {
                if (recolor.Value.Properties.CarInternalName != car.InternalName) continue;
                var rbutton = PhoneUIUtility.CreateSimpleButton(recolor.Value.Properties.RecolorDisplayName);
                rbutton.OnConfirm += () =>
                {
                    car.GetComponent<RecolorableCar>().ApplyRecolor(recolor.Value);
                };
                ScrollView.AddButton(rbutton);
            }

            var newRecolorButton = PhoneUIUtility.CreateSimpleButton("Create New Recolor");
            newRecolorButton.OnConfirm += () =>
            {
                var carController = CarController.Instance;
                var recolor = new Recolor();
                recolor.CreateDefault(carController.CurrentCar.GetComponent<RecolorableCar>());

                var recolorDirectory = Path.Combine(RecolorManager.RecolorFolder, NewRecolorFolder);

                if (!Directory.Exists(recolorDirectory))
                    Directory.CreateDirectory(recolorDirectory);

                var path = GetUniquePath(Path.Combine(recolorDirectory, $"{recolor.Properties.RecolorDisplayName}.whipremix"));
                recolor.Properties.RecolorDisplayName = Path.GetFileNameWithoutExtension(path);
                recolor.Save(path);

                Core.Instance.UIManager.ShowNotification($"New recolor ZIP saved to BepInEx/plugins/{NewRecolorFolder}/{recolor.Properties.RecolorDisplayName}.whipremix");
            };
            ScrollView.AddButton(newRecolorButton);
        }

        public override void OnAppInit()
        {
            base.OnAppInit();
            CreateIconlessTitleBar("WhipRemix");
            ScrollView = PhoneScrollView.Create(this);
        }

        public override void OnAppUpdate()
        {
            base.OnAppUpdate();
            if (!AvailableForCurrentCar())
                MyPhone.CloseCurrentApp();
        }

        private string GetUniquePath(string path)
        {
            if (!File.Exists(path))
                return path;
            var pathIndex = 2;
            var extension = Path.GetExtension(path);
            var noExtension = Path.GetFileNameWithoutExtension(path);
            var directory = Path.GetDirectoryName(path);
            while (File.Exists(Path.Combine(directory, $"{noExtension} {pathIndex}{extension}")))
                pathIndex++;
            return Path.Combine(directory, $"{noExtension} {pathIndex}{extension}");
        }
    }
}
