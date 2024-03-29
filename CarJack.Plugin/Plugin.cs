using BepInEx;
using HarmonyLib;
using System;
using System.IO;
using CarJack.Common;

namespace CarJack.Plugin
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    internal class Plugin : BaseUnityPlugin
    {
        private static Type ForceLoadCarJackCommonAssembly = typeof(DrivableCar);
        private void Awake()
        {
            new CarAssets(Path.Combine(Path.GetDirectoryName(Info.Location),"carjack"));
            CarController.Initialize();
            CarDebugController.Create();
            CarDatabase.Initialize();
            SpawnCarApp.Initialize();
        }
    }
}
