using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarJack.Common
{
    public class CarAssets
    {
        public static CarAssets Instance { get; private set; }
        public CarBundle MainBundle;
        public List<CarBundle> Bundles;
        public string MainBundlePath;
        public string AddonBundlePath;
        public string PluginDirectoryName;
        public CarAssets()
        {
            Bundles = new();
            Instance = this;
        }

        public void UnloadAllBundles()
        {
            foreach(var bundle in Bundles)
            {
                bundle.Bundle.Unload(true);
            }
            Bundles = new();
        }

        public void LoadBundles()
        {
            MainBundle = new CarBundle(MainBundlePath);
            Bundles.Add(MainBundle);
            var carBundlePaths = Directory.GetFiles(AddonBundlePath, "*.carbundle", SearchOption.AllDirectories);
            foreach(var carBundlePath in carBundlePaths)
            {
                if (IsPathInsidePluginFolder(carBundlePath))
                {
                    Debug.LogWarning($"CarJack Warning: Skipped loading car bundle \"{carBundlePath}\" because it's in the same folder as the CarJack plugin. Car bundles should be placed in their own subfolder inside the plugins folder.");
                    continue;
                }
                try
                {
                    var bundle = new CarBundle(carBundlePath);
                    Bundles.Add(bundle);
                }
                catch(Exception e)
                {
                    Debug.LogError($"CarJack Error: Failed to load car bundle \"{carBundlePath}\".\nException:\n{e}");
                }
            }
        }

        private bool IsPathInsidePluginFolder(string path)
        {
            if (string.IsNullOrEmpty(PluginDirectoryName)) return false;
            var parsedPath = path.Replace('\\', '/');
            if (path.ToLowerInvariant().Contains($"/{PluginDirectoryName.ToLowerInvariant()}/")) return true;
            return false;
        }
    }
}
