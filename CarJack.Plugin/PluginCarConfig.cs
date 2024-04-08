using BepInEx.Configuration;
using CarJack.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarJack.Plugin
{
    public class PluginCarConfig : ICarConfig
    {
        public ChopperControlTypes ChopperControlType
        {
            get
            {
                return _chopperControlType.Value;
            }
            set
            {
                _chopperControlType.Value = value;
            }
        }
        public bool DeveloperMode
        {
            get
            {
                return _developerMode.Value;
            }
            set
            {
                _developerMode.Value = value;
            }
        }

        public KeyCode ReloadBundlesKey
        {
            get
            {
                return _reloadBundlesKey.Value;
            }
            set
            {
                _reloadBundlesKey.Value = value;
            }
        }

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

        private ConfigEntry<bool> _developerMode;
        private ConfigEntry<KeyCode> _reloadBundlesKey;

        private ConfigEntry<ChopperControlTypes> _chopperControlType;

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

            _developerMode = configFile.Bind(
                "Development",
                "DeveloperMode",
                false,
                "Enables development features."
                );

            _reloadBundlesKey = configFile.Bind(
                "Development",
                "ReloadBundlesKey",
                KeyCode.F8,
                "Key to reload all bundles when in developer mode."
                );

            _chopperControlType = configFile.Bind(
                "Controls",
                "ChopperControlType",
                ChopperControlTypes.A,
                @"Control type for helicopters on controller.
A: Left Stick to adjust pitch/roll, face buttons to adjust yaw.
B: Left Stick to adjust pitch/yaw, face buttons to adjust roll."
                );
        }
    }
}
