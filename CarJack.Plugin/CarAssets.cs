using CarJack.Common;
using Reptile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarJack.Plugin
{
    public class CarAssets
    {
        public AssetBundle Bundle;
        public static CarAssets Instance { get; private set; }
        private string _path;
        public CarAssets(string path)
        {
            Instance = this;
            _path = path;
            ReloadAssets();
        }

        public void ReloadAssets()
        {
            if (Bundle != null)
                Bundle.Unload(true);
            Bundle = AssetBundle.LoadFromFile(_path);
            CarDatabase.Initialize();
        }
    }
}
