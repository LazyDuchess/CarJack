using CarJack.Common;
using CarJack.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarJack.SlopCrew
{
    [CarJackPlugin]
    public class Plugin
    {
        public Plugin()
        {
            if (CarController.Config.SlopCrewIntegration)
            {
                BallController.Initialize();
                NetworkController.Initialize();
            }
        }
    }
}
