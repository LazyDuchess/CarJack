using BepInEx;
using HarmonyLib;
using System;
using System.IO;
using CarJack.Common;
using System.Reflection;
using BepInEx.Bootstrap;
using System.Diagnostics;

namespace CarJack.Plugin
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("CommonAPI", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("SlopCrew.Plugin", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("BombRushCamera", BepInDependency.DependencyFlags.SoftDependency)]
    internal class Plugin : BaseUnityPlugin
    {
        private static Type ForceLoadCarJackCommonAssembly = typeof(DrivableCar);
        private void Awake()
        {
            Logger.LogInfo($"Loading {PluginInfo.PLUGIN_NAME} {PluginInfo.PLUGIN_VERSION}");
            try
            {
                var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
                harmony.PatchAll();
                var directory = Path.GetDirectoryName(Info.Location);
                new CarAssets(Path.Combine(directory, "carjack"));
                CarController.Initialize(new PluginCarConfig(Config));
#if DEBUG
                CarDebugController.Create();
#endif
                CarDatabase.Initialize();
                CarJackApp.Initialize(directory);
                var playerData = new PlayerData();
                playerData.LoadOrCreate();
                Logger.LogInfo($"Loaded {PluginInfo.PLUGIN_NAME} {PluginInfo.PLUGIN_VERSION}!");
            }
            catch(Exception e)
            {
                Logger.LogError($"Failed to load {PluginInfo.PLUGIN_NAME} {PluginInfo.PLUGIN_VERSION}!{Environment.NewLine}{e}");
            }
        }
    }
}
