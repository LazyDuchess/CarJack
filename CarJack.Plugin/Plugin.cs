using BepInEx;
using HarmonyLib;
using System;
using System.IO;
using CarJack.Common;
using System.Reflection;
using BepInEx.Bootstrap;

namespace CarJack.Plugin
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    internal class Plugin : BaseUnityPlugin
    {
        private static Type ForceLoadCarJackCommonAssembly = typeof(DrivableCar);
        private void Awake()
        {
            Logger.LogInfo($"Loading {PluginInfo.PLUGIN_NAME} {PluginInfo.PLUGIN_VERSION}");
            try
            {
                new CarAssets(Path.Combine(Path.GetDirectoryName(Info.Location), "carjack"));
                CarController.Initialize();
                CarDebugController.Create();
                CarDatabase.Initialize();
                SpawnCarApp.Initialize();
                if (Chainloader.PluginInfos.ContainsKey("SlopCrew.Plugin"))
                {
                    Logger.LogInfo("Loading CarJack SlopCrew Plugin!");
                    try
                    {
                        var slopCrewAssemblyLocation = Path.Combine(Path.GetDirectoryName(Info.Location), "CarJack.SlopCrew");
                        LoadPlugin(slopCrewAssemblyLocation);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError($"Failed to load CarJack SlopCrew Plugin!{Environment.NewLine}{e}");
                    }
                }
                Logger.LogInfo($"Loaded {PluginInfo.PLUGIN_NAME} {PluginInfo.PLUGIN_VERSION}!");
            }
            catch(Exception e)
            {
                Logger.LogError($"Failed to load {PluginInfo.PLUGIN_NAME} {PluginInfo.PLUGIN_VERSION}!{Environment.NewLine}{e}");
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
