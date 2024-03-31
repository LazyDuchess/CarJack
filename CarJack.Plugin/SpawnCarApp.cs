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

namespace CarJack.Plugin
{
    public class SpawnCarApp : CustomApp
    {
        public static void Initialize(string location)
        {
            var iconSprite = TextureUtility.LoadSprite(Path.Combine(location, "Phone-App-Icon.png"));
            PhoneAPI.RegisterApp<SpawnCarApp>("carjack", iconSprite);
        }

        public override void OnAppInit()
        {
            base.OnAppInit();
            CreateIconlessTitleBar("carjack");
            ScrollView = PhoneScrollView.Create(this);
        }

        public override void OnAppEnable()
        {
            base.OnAppEnable();
            ScrollView.RemoveAllButtons();
            // Temp workarounds for scrolling being messed up when coming back to the app. Should probably move this to CommonAPI itself but I'm lazy atm.
            ScrollView.ResetScroll();
            ScrollView.CancelAnimation();
            for (var i = 0; i < CarDatabase.Cars.Count; i++)
            {
                var carButton = CreateCarButton(i);
                ScrollView.AddButton(carButton);
            }
        }

        private SimplePhoneButton CreateCarButton(int carIndex)
        {
            var car = CarDatabase.Cars[carIndex];
            var button = PhoneUIUtility.CreateSimpleButton(car.name);
            button.OnConfirm += () =>
            {
                var player = WorldHandler.instance.GetCurrentPlayer();
                var carPrefab = CarDatabase.Cars[carIndex];
                var carGO = Instantiate(carPrefab);
                carGO.transform.position = player.transform.position;
                carGO.transform.rotation = player.transform.rotation;
                var car = carGO.GetComponent<DrivableCar>();
                car.Initialize();
                CarController.Instance.EnterCar(car);
            };
            return button;
        }
    }
}
