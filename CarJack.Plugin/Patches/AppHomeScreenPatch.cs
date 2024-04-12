using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarJack.Common;
using HarmonyLib;
using Reptile;
using Reptile.Phone;

namespace CarJack.Plugin.Patches
{
    [HarmonyPatch(typeof(AppHomeScreen))]
    internal static class AppHomeScreenPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(AppHomeScreen.OnPressRight))]
        private static bool OnPressRight_Prefix(AppHomeScreen __instance)
        {
            if (CarController.Instance == null) return true;
            var inVehicle = CarController.Instance.CurrentCar != null;
            if (!inVehicle) return true;
            var appToOpen = __instance.m_ScrollView.SelectedButtton as HomescreenButton;
            if (appToOpen.AssignedApp.appType != Reptile.HomeScreenApp.HomeScreenAppType.CAMERA) return true;
            __instance.m_AudioManager.PlaySfxGameplay(SfxCollectionID.PhoneSfx, AudioClipID.FlipPhone_Back);
            return false;
        }
    }
}
