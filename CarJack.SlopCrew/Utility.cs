using Microsoft.Extensions.DependencyInjection;
using Reptile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slop = SlopCrew;

namespace CarJack.SlopCrew
{
    public static class Utility
    {
        public static Player GetPlayer(uint playerId)
        {
            var playerManager = Slop.Plugin.Plugin.Host.Services.GetRequiredService<Slop.Plugin.PlayerManager>();
            if (!playerManager.Players.TryGetValue(playerId, out var player))
                return null;
            return player.ReptilePlayer;
        }
    }
}
