using CarJack.Common;
using Reptile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarJack.Plugin
{
    public static class CarDatabase
    {
        public static List<GameObject> Cars;
        public static void Initialize()
        {
            var bundle = CarAssets.Instance.Bundle;
            Cars = bundle.LoadAllAssets<GameObject>().Where(x => x.GetComponent<DrivableCar> != null).ToList();
        }
    }
}
