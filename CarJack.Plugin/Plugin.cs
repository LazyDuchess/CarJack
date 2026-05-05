using BepInEx;
using HarmonyLib;
using System;
using System.IO;
using CarJack.Common;
using System.Reflection;
using BepInEx.Bootstrap;
using System.Diagnostics;
using CarJack.Common.WhipRemix;

namespace CarJack.Plugin
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("CommonAPI", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("BombRushMP.Plugin", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("BombRushCamera", BepInDependency.DependencyFlags.SoftDependency)]
    internal class Plugin : BaseUnityPlugin
    {
        // I believe this is for the Unity types to be loaded. Been a while.
        private static Type ForceLoadCarJackCommonAssembly = typeof(DrivableCar);
        private void Awake()
        {
            Logger.LogInfo($"Loading {PluginInfo.PLUGIN_NAME} {PluginInfo.PLUGIN_VERSION}");
            try
            {
                var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
                harmony.PatchAll();
                var directory = Path.GetDirectoryName(Info.Location);

                var carAssets = new CarAssets();
                carAssets.MainBundlePath = Path.Combine(directory, "carjack");
                carAssets.AddonBundlePath = Paths.PluginPath;
                carAssets.PluginDirectoryName = Path.GetFileName(Path.GetDirectoryName(Info.Location));
                carAssets.LoadBundles();

                new RecolorSaveData();
                RecolorManager.Initialize(Paths.PluginPath);
                RecolorManager.LoadRecolors();
                RecolorApp.Initialize();

                CarController.Initialize(new PluginCarConfig(Config));

                if (CarController.Config.DeveloperMode)
                    CarDebugController.Create();

                CarDatabase.Initialize();
                CarJackApp.Initialize(directory);
                LoadCompatibilityPlugins();
                var playerData = new PlayerData();
                playerData.LoadOrCreate();
                Logger.LogInfo($"Loaded {PluginInfo.PLUGIN_NAME} {PluginInfo.PLUGIN_VERSION}!");
            }
            catch(Exception e)
            {
                Logger.LogError($"Failed to load {PluginInfo.PLUGIN_NAME} {PluginInfo.PLUGIN_VERSION}!{Environment.NewLine}{e}");
            }
        }

        private void LoadCompatibilityPlugins()
        {
            if (Chainloader.PluginInfos.ContainsKey("BombRushMP.Plugin"))
            {
                Logger.LogInfo("Loading CarJack All City Network Plugin!");
                try
                {
                    var slopPlugin = new CarJack.SlopCrew.Plugin();
                }
                catch (Exception e)
                {
                    Logger.LogError($"Failed to load CarJack All City Network Plugin!{Environment.NewLine}{e}");
                }
            }

            if (Chainloader.PluginInfos.ContainsKey("BombRushCamera"))
            {
                Logger.LogInfo("Loading CarJack BombRushCamera Plugin!");
                try
                {
                    var brcPlugin = new CarJack.BombRushCamera.Plugin();
                }
                catch (Exception e)
                {
                    Logger.LogError($"Failed to load CarJack BombRushCamera Plugin!{Environment.NewLine}{e}");
                }
            }
        }
    }
}
