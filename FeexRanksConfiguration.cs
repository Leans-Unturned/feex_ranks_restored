using Rocket.API;
using System.Collections.Generic;

namespace FeexRanks
{
    public class FeexRanksConfiguration : IRocketPluginConfiguration
    {
        public string DatabaseAddress = "127.0.0.1";
        public string DatabaseName = "unturned";
        public string DatabaseUsername = "admin";
        public string DatabasePassword = "root";
        public int DatabasePort = 3306;
        public string FeexRanksTableName = "ranks";
        public string UconomyTableName = "uconomy";
        public string UconomyCurrencyName = "Credits";
        public string RankPointsName = "experience";
        public bool RankGlobalNotify = true;
        public bool RankLocalNotify = false;
        public bool RankLoginGlobalNotify = true;
        public bool RankLoginLocalNotify = false;

        public int KillPlayersPoints = 10;
        public bool KillPlayerNotify = true;
        public int KillZombiePoints = 1;
        public bool KillZombieNotify = false;
        public int KillMegaZombiePoints = 50;
        public bool KillMegaZombieNotify = true;
        public List<Rank> Ranks = new();

        public void LoadDefaults()
        {
            Ranks = new List<Rank>()
            {
                new()
                {
                    points = 0,
                    rankName = "Rank 0",
                    groupReward = null,
                    groupNotify = false,
                    uconomyReward = 0,
                    uconomyNotify = true,
                },
                new()
                {
                    points = 100,
                    rankName = "Rank 1",
                    groupReward = "VIP",
                    groupNotify = true,
                    uconomyReward = 0,
                    uconomyNotify = true,
                }
            };
        }
    }

    public class Rank
    {
        public string rankName;
        public uint points;
        public string groupReward;
        public bool groupNotify;
        public uint uconomyReward;
        public bool uconomyNotify;
    }
}
