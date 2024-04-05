using CarJack.Common;
using CommonAPI.Phone;
using Reptile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarJack.Plugin
{
    public class SpawnCarByBundleApp : CustomApp
    {
        public override bool Available => false;

        public override void OnAppInit()
        {
            base.OnAppInit();
            CreateIconlessTitleBar("Choose Bundle");
            ScrollView = PhoneScrollView.Create(this);
            ScrollView.RemoveAllButtons();
            // Temp workarounds for scrolling being messed up when coming back to the app. Should probably move this to CommonAPI itself but I'm lazy atm.
            ScrollView.ResetScroll();
            ScrollView.CancelAnimation();
            foreach (var bundle in CarAssets.Instance.Bundles)
            {
                var bundleButton = CreateBundleButton(bundle);
                ScrollView.AddButton(bundleButton);
            }
        }

        private SimplePhoneButton CreateBundleButton(CarBundle bundle)
        {
            var button = PhoneUIUtility.CreateSimpleButton(bundle.Name);
            button.OnConfirm += () =>
            {
                MyPhone.GetAppInstance<SpawnCarApp>().SetBundleFilter(bundle);
                MyPhone.OpenApp(typeof(SpawnCarApp));
            };
            return button;
        }
    }
}
