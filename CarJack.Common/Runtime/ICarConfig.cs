using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarJack.Common
{
    public interface ICarConfig
    {
        public bool SlopCrewIntegration { get; set; }
        public bool ContinuousCollisionDetection { get; set; }
        public bool DeveloperMode { get; set; }
        public KeyCode ReloadBundlesKey { get; set; }
    }
}
