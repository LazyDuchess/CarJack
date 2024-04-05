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
                var bundle = new CarBundle(carBundlePath);
                Bundles.Add(bundle);
            }
        }
    }
}
