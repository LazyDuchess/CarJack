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
        public static void Initialize(string location)
        {
            Icon = TextureUtility.LoadSprite(Path.Combine(location, "Phone-App-Icon.png"));
            PhoneAPI.RegisterApp<CarJackApp>("carjack", Icon);
            PhoneAPI.RegisterApp<SpawnCarApp>("spawn car", Icon);
        }

        public override void OnAppInit()
        {
            base.OnAppInit();
            CreateTitleBar("CarJack", Icon);
            ScrollView = PhoneScrollView.Create(this);

            var button = PhoneUIUtility.CreateSimpleButton("Spawn Car");
            button.OnConfirm += () =>
            {
                MyPhone.OpenApp(typeof(SpawnCarApp));
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

            UpdateDoorsLockedLabel();
        }

        public override void OnAppUpdate()
        {
            base.OnAppUpdate();
            UpdateDoorsLockedLabel();
            UpdateMutePlayersLabel();
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
