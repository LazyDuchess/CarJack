using CarJack.Common;
using CommonAPI;
using CommonAPI.Phone;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarJack.Plugin
{
    public class CarJackApp : CustomApp
    {
        private static Sprite Icon;
        private SimplePhoneButton _doorButton;
        private SimplePhoneButton _muteButton;
        private SimplePhoneButton _autoRecoverButton;
        public static void Initialize(string location)
        {
            Icon = TextureUtility.LoadSprite(Path.Combine(location, "Phone-App-Icon.png"));
            PhoneAPI.RegisterApp<CarJackApp>("carjack", Icon);
            PhoneAPI.RegisterApp<SpawnCarApp>("spawn car", Icon);
            PhoneAPI.RegisterApp<SpawnCarByBundleApp>("choose bundle", Icon);
            PhoneAPI.RegisterApp<RecolorApp>("whipremix", Icon);
        }

        public override void OnAppInit()
        {
            base.OnAppInit();
            CreateTitleBar("CarJack", Icon);
            ScrollView = PhoneScrollView.Create(this);

            var button = PhoneUIUtility.CreateSimpleButton("Spawn Car");
            button.OnConfirm += () =>
            {
                if (CarAssets.Instance.Bundles.Count == 1)
                {
                    MyPhone.GetAppInstance<SpawnCarApp>().SetBundleFilter(null);
                    MyPhone.OpenApp(typeof(SpawnCarApp));
                }
                else
                {
                    MyPhone.OpenApp(typeof(SpawnCarByBundleApp));
                }
            };

            ScrollView.AddButton(button);
            _doorButton = PhoneUIUtility.CreateSimpleButton("Doors: Unlocked");
            _doorButton.OnConfirm += () =>
            {
                ToggleDoorsLocked();
            };
            ScrollView.AddButton(_doorButton);

            _muteButton = PhoneUIUtility.CreateSimpleButton("Mute Players: OFF");
            _muteButton.OnConfirm += () =>
            {
                ToggleMutePlayers();
            };
            ScrollView.AddButton(_muteButton);

            _autoRecoverButton = PhoneUIUtility.CreateSimpleButton("Auto-Recovery: ON");
            _autoRecoverButton.OnConfirm += () =>
            {
                ToggleAutoRecover();
            };
            ScrollView.AddButton(_autoRecoverButton);

            var whipRemixButton = PhoneUIUtility.CreateSimpleButton("WhipRemix");
            whipRemixButton.OnConfirm += () =>
            {
                MyPhone.OpenApp(typeof(RecolorApp));
            };
            ScrollView.AddButton(whipRemixButton);

            UpdateDoorsLockedLabel();
        }

        public override void OnAppUpdate()
        {
            base.OnAppUpdate();
            UpdateDoorsLockedLabel();
            UpdateMutePlayersLabel();
            UpdateAutoRecoverLabel();
        }

        private void ToggleAutoRecover()
        {
            PlayerData.Instance.AutoRecover = !PlayerData.Instance.AutoRecover;
            UpdateAutoRecoverLabel();
            PlayerData.Instance.Save();
        }

        private void ToggleDoorsLocked()
        {
            PlayerData.Instance.DoorsLocked = !PlayerData.Instance.DoorsLocked;
            UpdateDoorsLockedLabel();
            PlayerData.Instance.Save();
        }

        private void ToggleMutePlayers()
        {

            PlayerData.Instance.MutePlayers = !PlayerData.Instance.MutePlayers;
            UpdateMutePlayersLabel();
            PlayerData.Instance.Save();
        }

        private void UpdateAutoRecoverLabel()
        {
            _autoRecoverButton.Label.text = "Auto-Recovery: " + (PlayerData.Instance.AutoRecover ? "ON" : "OFF");
        }

        private void UpdateMutePlayersLabel()
        {
            _muteButton.Label.text = "Mute Players: " + (PlayerData.Instance.MutePlayers ? "ON" : "OFF");
        }
        private void UpdateDoorsLockedLabel()
        {
            _doorButton.Label.text = "Doors: " + (PlayerData.Instance.DoorsLocked ? "Locked" : "Unlocked");
        }
    }
}
