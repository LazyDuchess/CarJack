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
        public static Dictionary<string, GameObject> CarByInternalName;
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
            Cars = bundle.Bundle.LoadAllAssets<GameObject>().Where(x => x.GetComponent<DrivableCar>() != null).ToList();
            foreach (var car in Cars)
            {
                var drivableCar = car.GetComponent<DrivableCar>();
                CarByInternalName[drivableCar.InternalName] = car;
            }
        }
    }
}
