using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reptile;
using HarmonyLib;
using UnityEngine;
using CarJack.Common;

namespace CarJack.Plugin.Patches
{
    [HarmonyPatch(typeof(AmbientTrigger))]
    internal static class AmbientTriggerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnTriggerEnter")]
        private static bool OnTriggerEnter(Collider trigger)
        {
            var leCar = trigger.GetComponentInParent<DrivableCar>();
            if (leCar == null)
                return true;
            else
                return leCar.InCar;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnTriggerExit")]
        private static bool OnTriggerExit(Collider trigger)
        {
            var leCar = trigger.GetComponentInParent<DrivableCar>();
            if (leCar == null)
                return true;
            else
                return leCar.InCar;
        }
    }
}
