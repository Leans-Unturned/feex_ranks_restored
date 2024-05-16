using Rocket.API;
using Rocket.Unturned.Chat;
using System.Collections.Generic;

namespace FeexRanks.Commands
{
    public class Rank : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "rank";

        public string Help => "Show your current rank";

        public string Syntax => "/rank";

        public List<string> Aliases => new();

        public List<string> Permissions => new();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            // Receive the rank
            string rank = FeexRanksPlugin.instance.Database.GetRank(caller.Id);
            // If the player doesn't exist, or something goes really wrong, simple show the rank 0
            if (rank == "") UnturnedChat.Say(caller, FeexRanksPlugin.instance.Configuration.Instance.Ranks[0].rankName);
            // Normal behaviour
            else UnturnedChat.Say(caller, rank);
        }
    }
}
