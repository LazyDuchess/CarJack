using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarJack.Common
{
    public static class CarDatabase
    {
        public static List<GameObject> Cars;
        public static void Initialize()
        {
            CarAssets.OnReload -= Initialize;
            CarAssets.OnReload += Initialize;
            var bundle = CarAssets.Instance.Bundle;
            Cars = bundle.LoadAllAssets<GameObject>().Where(x => x.GetComponent<DrivableCar>() != null).ToList();
        }
    }
}
