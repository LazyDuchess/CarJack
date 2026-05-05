using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reptile;
using HarmonyLib;
using CarJack.Common;

namespace CarJack.Plugin.Patches
{
    [HarmonyPatch(typeof(Player))]
    internal static class PlayerPatch
    {
        private static bool _updatingVisuals = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Player.InitVisual))]
        private static void InitVisual_Prefix(Player __instance)
        {
            _updatingVisuals = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Player.InitVisual))]
        private static void InitVisual_Postfix(Player __instance)
        {
            _updatingVisuals = false;
            UpdatePlayer(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Player.SetOutfit))]
        private static void SetOutfit_Postfix(Player __instance)
        {
            if (_updatingVisuals) return;
            UpdatePlayer(__instance);
        }

        private static void UpdatePlayer(Player player)
        {
            CarController.OnPlayerVisualUpdated?.Invoke(player);
            if (player.isAI) return;
            var carController = CarController.Instance;
            if (carController == null) return;
            if (carController.CurrentCar != null && carController.CurrentSeat == null)
            {
                carController.CurrentCar.DriverSeat.UpdateVisual();
            }
            else if (carController.CurrentCar != null && carController.CurrentSeat != null)
            {
                carController.CurrentSeat.UpdateVisual();
            }
        }
    }
}
