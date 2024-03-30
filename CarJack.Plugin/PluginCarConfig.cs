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
        private ConfigEntry<bool> _slopCrewIntegration;
        public PluginCarConfig(ConfigFile configFile)
        {
            _slopCrewIntegration = configFile.Bind(
                "General",
                "SlopCrewIntegration",
                true,
                "Synchronize cars in SlopCrew."
                );
        }
    }
}
