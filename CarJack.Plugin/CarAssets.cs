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
            Bundle = AssetBundle.LoadFromFile(path);
            _path = path;
        }

        public void ReloadAssets()
        {
            if (Bundle != null)
                Bundle.Unload(true);
            var stageManager = Core.Instance.BaseModule.StageManager;
            stageManager.ExitCurrentStage(Utility.GetCurrentStage());
            Bundle = AssetBundle.LoadFromFile(_path);
        }
    }
}
