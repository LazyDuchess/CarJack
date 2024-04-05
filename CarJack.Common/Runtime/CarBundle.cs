using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarJack.Common
{
    public class CarBundle
    {
        public AssetBundle Bundle;

        public CarBundle(string path)
        {
            Bundle = AssetBundle.LoadFromFile(path);
        }
    }
}
