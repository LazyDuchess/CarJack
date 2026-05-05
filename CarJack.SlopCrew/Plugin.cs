using CarJack.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarJack.SlopCrew
{
    public class Plugin
    {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public Plugin()
        {
            if (CarController.Config.AllCityNetworkIntegration)
            {
                BallController.Initialize();
                NetworkController.Initialize();
            }
        }
    }
}
