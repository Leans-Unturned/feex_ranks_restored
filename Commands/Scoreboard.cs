using Rocket.API;
using System.Collections.Generic;

namespace FeexRanks.Commands
{
    public class Scoreboard : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "scoreboard";

        public string Help => "Shows the top 5 players";

        public string Syntax => "/scoreaboard";

        public List<string> Aliases => new();

        public List<string> Permissions => new();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            // To Do
        }
    }
}
