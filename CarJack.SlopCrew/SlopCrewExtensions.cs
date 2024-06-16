using SlopCrew.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlopCrew.Plugin;
using Slop = SlopCrew;
using Microsoft.Extensions.DependencyInjection;
using Google.Protobuf;
using SlopCrew.Common.Proto;

namespace CarJack.SlopCrew
{
    public static class SlopCrewExtensions
    {
        public static void SendCustomPacket(string id, byte[] data, SendFlags flags = SendFlags.Reliable)
        {
            id = SetPacketIDFlags(id, flags);
            var connectionManager = Slop.Plugin.Plugin.Host.Services.GetService<ConnectionManager>();
            connectionManager.SendMessage(new ServerboundMessage
            {
                CustomPacket = new ServerboundCustomPacket
                {
                    Packet = new CustomPacket
                    {
                        Id = id,
                        Data = ByteString.CopyFrom(data)
                    }
                }
            }, flags);
        }

        public static string GetPacketID(string packetIdWithFlags)
        {
            if (!packetIdWithFlags.Contains("|"))
                return packetIdWithFlags;

            var split = packetIdWithFlags.Split('|');
            return split[0];
        }

        private static string SetPacketIDFlags(string id, SendFlags flags)
        {
            if (!id.Contains("|"))
                id += "|";
            id += $"<f={(int)flags}>";
            return id;
        }
    }
}
