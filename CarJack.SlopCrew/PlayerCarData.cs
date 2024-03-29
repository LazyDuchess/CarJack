using CarJack.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarJack.SlopCrew
{
    public class PlayerCarData
    {
        public uint PlayerID;
        public DrivableCar Car;
        public PlayerCarPacket LastPacket;
    }
}
