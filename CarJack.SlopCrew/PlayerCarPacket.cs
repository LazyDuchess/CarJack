using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarJack.SlopCrew
{
    public class PlayerCarPacket
    {
        private const byte Version = 1;
        public const string GUID = "CarJack-PlayerCar";
        public string CarInternalName = "";
        public Vector3 Position = Vector3.zero;
        public Quaternion Rotation = Quaternion.identity;
        public Vector3 Velocity = Vector3.zero;
        public Vector3 AngularVelocity = Vector3.zero;

        public float ThrottleAxis = 0f;
        public float SteerAxis = 0f;
        public bool HornHeld = false;
        public bool BrakeHeld = false;
        public float PitchAxis = 0f;
        public float YawAxis = 0f;
        public float RollAxis = 0f;

        public int PassengerSeat = -1;
        public uint DriverPlayerID = uint.MaxValue;

        public void Serialize(BinaryWriter writer)
        {
            //version
            writer.Write(Version);

            writer.Write(CarInternalName);

            writer.Write(Position.x);
            writer.Write(Position.y);
            writer.Write(Position.z);

            writer.Write(Rotation.x);
            writer.Write(Rotation.y);
            writer.Write(Rotation.z);
            writer.Write(Rotation.w);

            writer.Write(Velocity.x);
            writer.Write(Velocity.y);
            writer.Write(Velocity.z);

            writer.Write(AngularVelocity.x);
            writer.Write(AngularVelocity.y);
            writer.Write(AngularVelocity.z);

            writer.Write(ThrottleAxis);
            writer.Write(SteerAxis);
            writer.Write(HornHeld);

            writer.Write(BrakeHeld);
            writer.Write(PitchAxis);
            writer.Write(YawAxis);
            writer.Write(RollAxis);

            writer.Write(PassengerSeat);
            writer.Write(DriverPlayerID);
        }

        public void Deserialize(BinaryReader reader)
        {
            var version = reader.ReadByte();

            CarInternalName = reader.ReadString();

            var posX = reader.ReadSingle();
            var posY = reader.ReadSingle();
            var posZ = reader.ReadSingle();

            var rotX = reader.ReadSingle();
            var rotY = reader.ReadSingle();
            var rotZ = reader.ReadSingle();
            var rotW = reader.ReadSingle();

            var velX = reader.ReadSingle();
            var velY = reader.ReadSingle();
            var velZ = reader.ReadSingle();

            var aVelX = reader.ReadSingle();
            var aVelY = reader.ReadSingle();
            var aVelZ = reader.ReadSingle();

            ThrottleAxis = reader.ReadSingle();
            SteerAxis = reader.ReadSingle();
            HornHeld = reader.ReadBoolean();
            /*
            writer.Write(BrakeHeld);
            writer.Write(PitchAxis);
            writer.Write(YawAxis);
            writer.Write(RollAxis);
            */
            BrakeHeld = reader.ReadBoolean();
            PitchAxis = reader.ReadSingle();
            YawAxis = reader.ReadSingle();
            RollAxis = reader.ReadSingle();

            if (version >= 1)
            {
                PassengerSeat = reader.ReadInt32();
                DriverPlayerID = reader.ReadUInt32();
            }

            Position = new Vector3(posX, posY, posZ);
            Rotation = new Quaternion(rotX, rotY, rotZ, rotW);
            Velocity = new Vector3(velX, velY, velZ);
            AngularVelocity = new Vector3(aVelX, aVelY, aVelZ);
        }
    }
}
