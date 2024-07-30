using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace CarJack.SlopCrew
{
    public class PlayerCarStatePacket
    {
        private const byte Version = 0;
        public const string GUID = "CarJack-PlayerCarState";
        public string CarInternalName = "";
        public int PassengerSeat = -1;
        public uint DriverPlayerID = uint.MaxValue;
        public bool DoorsLocked = false;
        public string RecolorGUID = string.Empty;

        public void Serialize(BinaryWriter writer)
        {
            //version
            writer.Write(Version);

            writer.Write(CarInternalName);
            writer.Write(PassengerSeat);
            writer.Write(DriverPlayerID);
            writer.Write(DoorsLocked);

            writer.Write(RecolorGUID);
        }

        public void Deserialize(BinaryReader reader)
        {
            var version = reader.ReadByte();

            CarInternalName = reader.ReadString();
            PassengerSeat = reader.ReadInt32();
            DriverPlayerID = reader.ReadUInt32();
            DoorsLocked = reader.ReadBoolean();

            RecolorGUID = reader.ReadString();
        }
    }
}
