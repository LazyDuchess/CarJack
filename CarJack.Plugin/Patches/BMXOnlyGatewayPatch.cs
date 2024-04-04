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
    [HarmonyPatch(typeof(BMXOnlyGateway))]
    internal static class BMXOnlyGatewayPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnTriggerStay")]
        private static bool OnTriggerStay(Collider other)
        {
            var leCar = other.GetComponentInParent<DrivableCar>();
            if (leCar != null)
                return false;
            return true;
        }
    }
}
