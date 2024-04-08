using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarJack.Common
{
    [RequireComponent(typeof(DrivableCar))]
    public class HelicopterCarType : MonoBehaviour
    {
        private DrivableCar _car;
        private void Awake()
        {
            _car = GetComponent<DrivableCar>();
        }
    }
}
