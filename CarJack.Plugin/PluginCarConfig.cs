using BepInEx.Configuration;
using CarJack.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarJack.Plugin
{
    public class PluginCarConfig : ICarConfig
    {
        public bool SlopCrewIntegration
        {
            get
            {
                return _slopCrewIntegration.Value;
            }
            set
            {
                _slopCrewIntegration.Value = value;
            }
        }

        public bool ContinuousCollisionDetection
        {
            get
            {
                return _continuousCollisionDetection.Value;
            }
            set
            {
                _continuousCollisionDetection.Value = value;
            }
        }

        private ConfigEntry<bool> _slopCrewIntegration;
        private ConfigEntry<bool> _continuousCollisionDetection;
        public PluginCarConfig(ConfigFile configFile)
        {
            _continuousCollisionDetection = configFile.Bind(
                "General",
                "ContinuousCollisionDetection",
                true,
                "Prevents cars from going through geometry at high speeds."
                );

            _slopCrewIntegration = configFile.Bind(
                "General",
                "SlopCrewIntegration",
                true,
                "Synchronize cars in SlopCrew."
                );
        }
    }
}
