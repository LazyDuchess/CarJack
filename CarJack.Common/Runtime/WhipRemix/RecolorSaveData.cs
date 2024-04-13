#if PLUGIN
using CommonAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarJack.Common.WhipRemix
{
    public class RecolorSaveData : CustomSaveData
    {
        private const byte Version = 0;
        public static RecolorSaveData Instance { get; private set; }
        private Dictionary<string, string> _recolorGUIDByCarInternalName = new();
        public RecolorSaveData() : base("CarJack", "Slot{0}.cjs")
        {
            Instance = this;
        }

        public void SetRecolorForCar(string carInternalName, string recolorGUID)
        {
            if (string.IsNullOrEmpty(recolorGUID))
            {
                _recolorGUIDByCarInternalName.Remove(carInternalName);
            }
            else
            {
                _recolorGUIDByCarInternalName[carInternalName] = recolorGUID;
            }
        }

        public string GetRecolorGUIDForCar(string carInternalName)
        {
            if (_recolorGUIDByCarInternalName.TryGetValue(carInternalName, out var result))
                return result;
            return string.Empty;
        }

        public override void Initialize()
        {
            _recolorGUIDByCarInternalName = new();
        }

        public override void Read(BinaryReader reader)
        {
            var version = reader.ReadByte();
            var count = reader.ReadInt32();
            for(var i = 0; i < count; i++)
            {
                var carName = reader.ReadString();
                var recolorGUID = reader.ReadString();
                _recolorGUIDByCarInternalName[carName] = recolorGUID;
            }
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write(_recolorGUIDByCarInternalName.Count);
            foreach(var keyValue in _recolorGUIDByCarInternalName)
            {
                writer.Write(keyValue.Key);
                writer.Write(keyValue.Value);
            }
        }
    }
}
#endif