using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarJack.Common;
using HarmonyLib;
using Reptile;

namespace CarJack.Plugin.Patches
{
    [HarmonyPatch(typeof(CharacterSelect))]
    internal static class CharacterSelectPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("Init")]
        private static void Init_Prefix(Player p, CharacterSelect __instance)
        {
            if (p.isAI) return;
            var carController = CarController.Instance;
            if (carController.CurrentCar == null) return;
            carController.ExitCar();
        }
    }
}
