using BepInEx;
using HarmonyLib;
using System;
using System.IO;

namespace CarJack.Plugin
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    internal class Plugin : BaseUnityPlugin
    {
        private static Type ForceLoadCarJackCommonAssembly = typeof(CarJack.Common.DrivableCar);
        private void Awake()
        {
            new CarAssets(Path.Combine(Path.GetDirectoryName(Info.Location),"carjack"));
            CarController.Create();
            CarDebugController.Create();
            CarDatabase.Initialize();
        }
    }
}
