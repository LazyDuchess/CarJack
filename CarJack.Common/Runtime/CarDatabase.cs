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
        public static Dictionary<string, CarEntry> CarByInternalName;
        public static void Initialize()
        {
            CarByInternalName = new();
            var bundles = CarAssets.Instance.Bundles;
            foreach(var bundle in bundles)
            {
                LoadBundle(bundle);
            }
        }

        private static void LoadBundle(CarBundle bundle)
        {
            var cars = bundle.Bundle.LoadAllAssets<GameObject>().Where(x => x.GetComponent<DrivableCar>() != null).ToList();
            foreach (var car in cars)
            {
                var drivableCar = car.GetComponent<DrivableCar>();
                var entry = new CarEntry();
                entry.Bundle = bundle;
                entry.Prefab = car;
                CarByInternalName[drivableCar.InternalName] = entry;
            }
        }
    }
}
