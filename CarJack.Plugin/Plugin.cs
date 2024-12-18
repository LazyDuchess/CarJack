﻿using BepInEx;
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
            if (Chainloader.PluginInfos.ContainsKey("SlopCrew.Plugin"))
            {
                Logger.LogInfo("Loading CarJack SlopCrew Plugin!");
                try
                {
                    var assemblyLocation = Path.Combine(Path.GetDirectoryName(Info.Location), "CarJack.SlopCrew.dll");
                    LoadPlugin(assemblyLocation);
                }
                catch (Exception e)
                {
                    Logger.LogError($"Failed to load CarJack SlopCrew Plugin!{Environment.NewLine}{e}");
                }
            }

            if (Chainloader.PluginInfos.ContainsKey("BombRushCamera"))
            {
                Logger.LogInfo("Loading CarJack BombRushCamera Plugin!");
                try
                {
                    var assemblyLocation = Path.Combine(Path.GetDirectoryName(Info.Location), "CarJack.BombRushCamera.dll");
                    LoadPlugin(assemblyLocation);
                }
                catch (Exception e)
                {
                    Logger.LogError($"Failed to load CarJack BombRushCamera Plugin!{Environment.NewLine}{e}");
                }
            }
        }

        private void LoadPlugin(string assemblyPath)
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            var typesInAssembly = assembly.GetTypes();
            foreach(var type in typesInAssembly)
            {
                if (type.GetCustomAttribute<CarJackPluginAttribute>() != null)
                {
                    var instance = Activator.CreateInstance(type);
                }
            }
        }
    }
}
