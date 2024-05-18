using Rocket.API.Collections;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;

namespace FeexRanks
{
    public class FeexRanksPlugin : RocketPlugin<FeexRanksConfiguration>
    {
        public static FeexRanksPlugin instance;
        public DatabaseMgr Database;
        public override void LoadPlugin()
        {
            Database = new(this);
            base.LoadPlugin();
            // Instanciating events
            Rocket.Unturned.U.Events.OnPlayerConnected += OnPlayerConnected;
            Rocket.Unturned.U.Events.OnPlayerDisconnected += OnPlayerDisconnected;            
            UnturnedPlayerEvents.OnPlayerUpdateStat += OnPlayerStatsUpdate;
            instance = this;
            Logger.Log("FeexRanks instanciated, restored by LeandroTheDev");
        }

        private void OnPlayerStatsUpdate(UnturnedPlayer player, EPlayerStat stat)
        {            
            switch (stat)
            {
                case EPlayerStat.KILLS_PLAYERS:
                    if (Configuration.Instance.KillPlayerNotify)
                        UnturnedChat.Say(player, Translate("killed_player", Configuration.Instance.KillPlayersPoints, Configuration.Instance.RankPointsName));
                    Database.AddPoints(player.Id, Configuration.Instance.KillPlayersPoints);
                    PlayerNotifyRankSystem(player, Database.GetPoints(player.Id), Database.GetRank(player.Id));
                    break;
                case EPlayerStat.KILLS_ZOMBIES_NORMAL:
                    if (Configuration.Instance.KillZombieNotify)
                        UnturnedChat.Say(player, Translate("killed_zombie", Configuration.Instance.KillZombiePoints, Configuration.Instance.RankPointsName));
                    Database.AddPoints(player.Id, Configuration.Instance.KillZombiePoints);
                    PlayerNotifyRankSystem(player, Database.GetPoints(player.Id), Database.GetRank(player.Id));
                    break;
                case EPlayerStat.KILLS_ZOMBIES_MEGA:
                    if (Configuration.Instance.KillMegaZombieNotify)
                        UnturnedChat.Say(player, Translate("killed_mega_zombie", Configuration.Instance.KillMegaZombiePoints, Configuration.Instance.RankPointsName));
                    Database.AddPoints(player.Id, Configuration.Instance.KillMegaZombiePoints);
                    PlayerNotifyRankSystem(player, Database.GetPoints(player.Id), Database.GetRank(player.Id));
                    break;
            }
        }

        private void PlayerNotifyRankSystem(UnturnedPlayer player, decimal currentPoints, string currentRank)
        {
            Rank calculatedRank = null;
            // Swipe all levels to get the current level
            foreach (Rank forRank in Configuration.Instance.Ranks)
            {
                // Check the level
                if (currentPoints >= forRank.points)
                {
                    calculatedRank = forRank;
                }
                else break;
            }
            // Simple verify if the actual rank is different from the points
            if (calculatedRank != null && currentRank != calculatedRank.rankName)
            {
                // Update in database the player rank
                Database.UpdateRank(player.Id, calculatedRank.rankName);

                // Notify
                if (Configuration.Instance.RankLocalNotify) UnturnedChat.Say(player, Translate("rank_up_notify", calculatedRank.rankName));
                if (Configuration.Instance.RankGlobalNotify) UnturnedChat.Say(Translate("rank_up_notify_global", player.DisplayName, calculatedRank.rankName));

                // Group Reward
                if (calculatedRank.groupReward != null)
                {
                    // Adding group to the player
                    Rocket.Core.R.Permissions.AddPlayerToGroup(calculatedRank.groupReward, player);
                    // Notify
                    if (calculatedRank.groupNotify) UnturnedChat.Say(player, Translate("rank_up_group_reward_notify", calculatedRank.groupReward));
                }
                // Uconomy Reward
                if (calculatedRank.uconomyReward > 0)
                {
                    // Adding points
                    Database.AddPoints(player.Id, (int)calculatedRank.uconomyReward);
                    // Notify
                    if (calculatedRank.uconomyNotify) UnturnedChat.Say(player, Translate("rank_up_uconomy_reward_notify", calculatedRank.uconomyReward, Configuration.Instance.UconomyCurrencyName));
                }
            }
        }

        private void OnPlayerConnected(UnturnedPlayer player)
        {
            Database.AddNewPlayer(player.Id);
            if (Configuration.Instance.RankLoginGlobalNotify)
                UnturnedChat.Say(Translate("player_connected_global", Database.GetRank(player.Id), player.DisplayName));
            if (Configuration.Instance.RankLoginLocalNotify)
                UnturnedChat.Say(Translate("player_connected", player.DisplayName, Database.GetRank(player.Id)));
        }

        private void OnPlayerDisconnected(UnturnedPlayer player)
        {
            if (Configuration.Instance.RankLogoutGlobalNotify)
                UnturnedChat.Say(Translate("player_disconnected_global", Database.GetRank(player.Id), player.DisplayName));
        }

        public override TranslationList DefaultTranslations => new()
        {
            {"killed_player", "Player killed you earned: {0} {1}"},
            {"killed_zombie", "Zombie killed you earned: {0} {1}"},
            {"killed_mega_zombie", "Mega Zombie killed you earned: {0} {1}"},
            {"rank_up_notify", "You have reached {0}" },
            {"rank_up_notify_global", "{0} have reached {1}" },
            {"rank_up_group_reward_notify", "You have earned the group permission {0}" },
            {"rank_up_uconomy_reward_notify", "You have earned {0} {1}" },
            {"player_connected_global", "[{0}] {1} connected" },
            {"player_disconnected_global", "[{0}] {1} disconnected" },
            {"player_connected", "Welcome {0} Your current rank is {1}" },
        };
    }
}