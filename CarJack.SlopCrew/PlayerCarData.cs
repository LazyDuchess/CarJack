using CarJack.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarJack.SlopCrew
{
    public class PlayerCarData
    {
        public uint PlayerID;
        public DrivableCar Car;
        public PlayerCarTransformPacket TransformPacket;
        public PlayerCarStatePacket StatePacket;
        // SlopCrew april fools stuff.
        public GameObject Polo;
        public CarSeat Seat;
    }
}
