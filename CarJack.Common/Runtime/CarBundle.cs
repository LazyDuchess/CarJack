using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarJack.Common
{
    public class CarBundle
    {
        public string Name;
        public AssetBundle Bundle;

        public CarBundle(string path)
        {
            Name = Path.GetFileNameWithoutExtension(path);
            Bundle = AssetBundle.LoadFromFile(path);
            if (Bundle == null)
                throw new IOException("AssetBundle.LoadFromFile returned null!");
        }
    }
}
