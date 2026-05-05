using BombRushMP.Plugin;
using Reptile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarJack.SlopCrew
{
    public static class Utility
    {
        public static Player GetPlayer(ushort playerId)
        {
            if (ClientController.Instance.Players.TryGetValue(playerId, out var player))
                return player.Player;
            return null;
        }
    }
}
