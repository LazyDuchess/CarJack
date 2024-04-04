using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarJack.Common
{
    public class PlayerData
    {
        public static PlayerData Instance { get; private set; }
        public bool DoorsLocked
        {
            get
            {
                return _data.DoorsLocked;
            }
            set
            {
                _data.DoorsLocked = value;
            }
        }
        private SerializedData _data;

        public PlayerData()
        {
            Instance = this;
        }
#if PLUGIN
        public void LoadOrCreate()
        {
            try
            {
                _data = JsonUtility.FromJson<SerializedData>(GetSaveLocation());
            }
            catch(Exception e)
            {
                _data = new SerializedData();
            }
        }

        public void Save()
        {
            var jsonData = JsonUtility.ToJson(_data, true);
            var saveDirectory = Path.GetDirectoryName(GetSaveLocation());
            Directory.CreateDirectory(saveDirectory);
            File.WriteAllText(GetSaveLocation(), jsonData);
        }

        private string GetSaveLocation()
        {
            return Path.Combine(BepInEx.Paths.ConfigPath, "CarJack", "playerdata.json");
        }
#endif

        [Serializable]
        public class SerializedData
        {
            public bool DoorsLocked = false;
        }
    }
}
