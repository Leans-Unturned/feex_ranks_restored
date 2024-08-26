extern alias UnityEngineCoreModule;

using Rocket.API.Collections;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityCoreModule = UnityEngineCoreModule.UnityEngine;

namespace FeexRanks
{
    public class FeexRanksPlugin : RocketPlugin<FeexRanksConfiguration>
    {
        public static FeexRanksPlugin instance;
        private FeexRanksTickrate tickrate;
        public DatabaseMgr Database;
        public override void LoadPlugin()
        {
            Database = new(this);
            base.LoadPlugin();
            // Instanciating events
            Rocket.Unturned.U.Events.OnPlayerConnected += OnPlayerConnected;
            Rocket.Unturned.U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
            UnturnedPlayerEvents.OnPlayerUpdateStat += OnPlayerStatsUpdate;
            if (Configuration.Instance.PointsEarnPerTime > 0)
                tickrate = gameObject.AddComponent<FeexRanksTickrate>();
            instance = this;

            Logger.Log("FeexRanks instanciated, restored by LeandroTheDev");
        }

        private void OnPlayerStatsUpdate(UnturnedPlayer player, EPlayerStat stat)
        {
            switch (stat)
            {
                case EPlayerStat.KILLS_PLAYERS:
                    if (Configuration.Instance.KillPlayerNotify)
                    {
                        int[] colors = Configuration.Instance.KillPlayerRGBColorNotify.Split(',').Select(int.Parse).ToArray();
                        ChatManager.serverSendMessage(
                            Translate("killed_player", Configuration.Instance.KillPlayersPoints, Configuration.Instance.RankPointsName),
                            new UnityCoreModule.Color(colors[0], colors[1], colors[2]),
                            null,
                            player.SteamPlayer(),
                            EChatMode.SAY,
                            Configuration.Instance.ChatIconURL,
                            true
                        );
                    }
                    Database.AddPoints(player.Id, Configuration.Instance.KillPlayersPoints);
                    PlayerNotifyRankSystem(player, Database.GetPoints(player.Id), Database.GetRank(player.Id));
                    break;
                case EPlayerStat.KILLS_ZOMBIES_NORMAL:
                    if (Configuration.Instance.KillZombieNotify)
                    {
                        int[] colors = Configuration.Instance.KillZombieRGBColorNotify.Split(',').Select(int.Parse).ToArray();
                        ChatManager.serverSendMessage(
                            Translate("killed_zombie", Configuration.Instance.KillZombiePoints, Configuration.Instance.RankPointsName),
                            new UnityCoreModule.Color(colors[0], colors[1], colors[2]),
                            null,
                            player.SteamPlayer(),
                            EChatMode.SAY,
                            Configuration.Instance.ChatIconURL,
                            true
                        );
                    }
                    Database.AddPoints(player.Id, Configuration.Instance.KillZombiePoints);
                    PlayerNotifyRankSystem(player, Database.GetPoints(player.Id), Database.GetRank(player.Id));
                    break;
                case EPlayerStat.KILLS_ZOMBIES_MEGA:
                    if (Configuration.Instance.KillMegaZombieNotify)
                    {
                        int[] colors = Configuration.Instance.KillZombieRGBColorNotify.Split(',').Select(int.Parse).ToArray();
                        ChatManager.serverSendMessage(
                            Translate("killed_mega_zombie", Configuration.Instance.KillMegaZombiePoints, Configuration.Instance.RankPointsName),
                            new UnityCoreModule.Color(colors[0], colors[1], colors[2]),
                            null,
                            player.SteamPlayer(),
                            EChatMode.SAY,
                            Configuration.Instance.ChatIconURL,
                            true
                        );
                    }
                    Database.AddPoints(player.Id, Configuration.Instance.KillMegaZombiePoints);
                    PlayerNotifyRankSystem(player, Database.GetPoints(player.Id), Database.GetRank(player.Id));
                    break;
            }
        }

        public void PlayerNotifyRankSystem(UnturnedPlayer player, decimal currentPoints, string currentRank)
        {
            Rank calculatedRank = null;
            Rank actualRank = null;
            // Swipe all levels to get the current level
            bool currentPointsFinished = false;
            bool currentRankFinished = false;
            foreach (Rank forRank in Configuration.Instance.Ranks)
            {
                // Check the level points
                if (currentPoints >= forRank.points)
                    // Update current rank
                    calculatedRank = forRank;
                else currentPointsFinished = true;

                // Check the level rank
                if (currentRank == forRank.rankName)
                    // Update current rank
                    actualRank = forRank;
                else currentRankFinished = true;

                // Check if swipe finished
                if (currentPointsFinished && currentRankFinished) break;
            }
            // Null check
            if (calculatedRank == null || actualRank == null)
                Logger.LogError($"{player.Id} have any invalid rank: {currentRank}, types: {calculatedRank} : {actualRank}, this 2 types needs to be different from null and thats not the case");
            // Simple verify if the calculatedRank is bigger than actual rank
            if (calculatedRank.points > actualRank.points)
            {
                // Update in database the player rank
                Database.UpdateRank(player.Id, calculatedRank.rankName);

                // Notify
                int[] rankColors = Configuration.Instance.RankRGBColorNotify.Split(',').Select(int.Parse).ToArray();
                if (Configuration.Instance.RankLocalNotify)
                    ChatManager.serverSendMessage(
                        Translate("rank_up_notify", calculatedRank.rankName),
                        new UnityCoreModule.Color(rankColors[0], rankColors[1], rankColors[2]),
                        null,
                        player.SteamPlayer(),
                        EChatMode.SAY,
                        Configuration.Instance.ChatIconURL,
                        true
                    );
                if (Configuration.Instance.RankGlobalNotify)
                    ChatManager.serverSendMessage(
                        Translate("rank_up_notify_global", player.DisplayName, calculatedRank.rankName),
                        new UnityCoreModule.Color(rankColors[0], rankColors[1], rankColors[2]),
                        null,
                        null,
                        EChatMode.SAY,
                        Configuration.Instance.ChatIconURL,
                        true
                    );
                // Group Reward
                if (calculatedRank.groupReward != null)
                {
                    // Adding group to the player
                    Rocket.Core.R.Permissions.AddPlayerToGroup(calculatedRank.groupReward, player);

                    // Notify
                    int[] groupRewardColors = calculatedRank.groupRGBColorNotify.Split(',').Select(int.Parse).ToArray();
                    if (calculatedRank.groupNotify)
                        ChatManager.serverSendMessage(
                            Translate("rank_up_group_reward_notify", calculatedRank.groupReward),
                            new UnityCoreModule.Color(groupRewardColors[0], groupRewardColors[1], groupRewardColors[2]),
                            null,
                            player.SteamPlayer(),
                            EChatMode.SAY,
                            Configuration.Instance.ChatIconURL,
                            true
                        );
                }
                // Uconomy Reward
                if (calculatedRank.uconomyReward > 0)
                {
                    // Adding points
                    Database.AddPoints(player.Id, (int)calculatedRank.uconomyReward);
                    // Notify
                    int[] uconomyRewardColors = calculatedRank.uconomyRGBColorNotify.Split(',').Select(int.Parse).ToArray();
                    if (calculatedRank.uconomyNotify)
                        ChatManager.serverSendMessage(
                            Translate("rank_up_uconomy_reward_notify", calculatedRank.uconomyReward, Configuration.Instance.UconomyCurrencyName),
                            new UnityCoreModule.Color(uconomyRewardColors[0], uconomyRewardColors[1], uconomyRewardColors[2]),
                            null,
                            player.SteamPlayer(),
                            EChatMode.SAY,
                            Configuration.Instance.ChatIconURL,
                            true
                        );
                }
            }
        }

        private void OnPlayerConnected(UnturnedPlayer player)
        {
            Database.AddNewPlayer(player.Id);
            if (Configuration.Instance.RankLoginGlobalNotify)
            {
                int[] loginColors = Configuration.Instance.RankLoginRGBColorNotify.Split(',').Select(int.Parse).ToArray();
                ChatManager.serverSendMessage(
                    Translate("player_connected_global",
                    Database.GetRank(player.Id), player.DisplayName),
                    new UnityCoreModule.Color(loginColors[0], loginColors[1], loginColors[2]),
                    null,
                    null,
                    EChatMode.SAY,
                    Configuration.Instance.ChatIconURL,
                    true
                );
            }
            if (Configuration.Instance.RankLoginLocalNotify)
            {
                int[] loginColors = Configuration.Instance.RankLogoutRGBColorNotify.Split(',').Select(int.Parse).ToArray();
                ChatManager.serverSendMessage(
                    Translate("player_connected",
                    player.DisplayName, Database.GetRank(player.Id)),
                    new UnityCoreModule.Color(loginColors[0], loginColors[1], loginColors[2]),
                    null,
                    null,
                    EChatMode.SAY,
                    Configuration.Instance.ChatIconURL,
                    true
                );
            }

            if (Configuration.Instance.PointsLoseWhenDie > 0)
                player.Events.OnDead += OnPlayerDied;

            tickrate?.AddPlayer(player);
        }

        private void OnPlayerDied(UnturnedPlayer player, UnityCoreModule.Vector3 position)
        {
            if (player == null) return;
            uint pointsToLose = Configuration.Instance.PointsLoseWhenDie;
            uint currentPoints = (uint)Database.GetPoints(player.Id);
            string currentRank = Database.GetRank(player.Id);
            Rank actualRank = null;
            // Swipe all levels to get the current level
            foreach (Rank forRank in Configuration.Instance.Ranks)
            {
                // Check the level rank
                if (currentRank == forRank.rankName)
                {
                    // Update current rank
                    actualRank = forRank;
                    break;
                }
            }
            // If can't find the rank cancel it
            if (actualRank == null) return;

            // If rank points is bigger than player points cancel the function
            if (actualRank.points >= currentPoints) return;

            // This is hard to understand but this will calculate the max points to lose
            // if player only have 5 points and he will lose 10, in this case he will lose only 5 points instead of 10 points
            if (((currentPoints - actualRank.points) - pointsToLose) <= 0)
                pointsToLose = ((currentPoints - actualRank.points) - pointsToLose) + pointsToLose;

            if (pointsToLose == 0) return;

            // Reduce in database
            Database.ReducePoints(player.Id, pointsToLose);
            // Inform player
            int[] loseColors = Configuration.Instance.PointsLoseRGBColorNotify.Split(',').Select(int.Parse).ToArray();
            ChatManager.serverSendMessage(
                Translate("points_lost", pointsToLose, Configuration.Instance.RankPointsName),
                new UnityCoreModule.Color(loseColors[0], loseColors[1], loseColors[2]),
                null,
                player.SteamPlayer(),
                EChatMode.SAY,
                Configuration.Instance.ChatIconURL,
                true
            );
        }

        private void OnPlayerDisconnected(UnturnedPlayer player)
        {
            if (Configuration.Instance.RankLogoutGlobalNotify)
            {
                int[] logoutColors = Configuration.Instance.RankLogoutRGBColorNotify.Split(',').Select(int.Parse).ToArray();
                ChatManager.serverSendMessage(
                    Translate("player_disconnected_global", Database.GetRank(player.Id), player.DisplayName),
                    new UnityCoreModule.Color(logoutColors[0], logoutColors[1], logoutColors[2]),
                    null,
                    null,
                    EChatMode.SAY,
                    Configuration.Instance.ChatIconURL,
                    true
                );
            }

            if (Configuration.Instance.PointsLoseWhenDie > 0)
                player.Events.OnDead -= OnPlayerDied;

            tickrate?.RemovePlayer(player);
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
            {"rank_command", "Your current rank is {0} with {1} {2}" },
            {"points_lost", "You lost {0} {1}" },
            {"points_earned_time", "You earned {0} {1} for playing" },
        };
    }

    class FeexRanksTickrate : MonoBehaviour
    {
        readonly List<UnturnedPlayer> playersToEarnPoints = new();
        uint actualTick = 0;

        public void Start()
        {
            actualTick = FeexRanksPlugin.instance.Configuration.Instance.TickratePointsEarnPerTime;
        }

        public void Update()
        {
            if (actualTick <= 0)
            {
                actualTick = FeexRanksPlugin.instance.Configuration.Instance.TickratePointsEarnPerTime;
                foreach (UnturnedPlayer player in playersToEarnPoints)
                {
                    // Add points into database
                    FeexRanksPlugin.instance.Database.AddPoints(player.Id, (int)FeexRanksPlugin.instance.Configuration.Instance.PointsEarnPerTime);
                    int[] pointsEarn = FeexRanksPlugin.instance.Configuration.Instance.PointsEarnRGBColorNotify.Split(',').Select(int.Parse).ToArray();
                    // Notify poitns earned
                    ChatManager.serverSendMessage(
                        FeexRanksPlugin.instance.Translate("points_earned_time", FeexRanksPlugin.instance.Configuration.Instance.PointsEarnPerTime, FeexRanksPlugin.instance.Configuration.Instance.RankPointsName),
                        new UnityCoreModule.Color(pointsEarn[0], pointsEarn[1], pointsEarn[2]),
                        null,
                        player.SteamPlayer(),
                        EChatMode.SAY,
                        FeexRanksPlugin.instance.Configuration.Instance.ChatIconURL,
                        true
                    );
                    UnturnedChat.Say(FeexRanksPlugin.instance.Translate("points_earned_time", FeexRanksPlugin.instance.Configuration.Instance.PointsEarnPerTime, FeexRanksPlugin.instance.Configuration.Instance.RankPointsName));
                    // Notify if he rank up
                    FeexRanksPlugin.instance.PlayerNotifyRankSystem(player, FeexRanksPlugin.instance.Database.GetPoints(player.Id), FeexRanksPlugin.instance.Database.GetRank(player.Id));
                }
            }
            actualTick--;
        }

        public void AddPlayer(UnturnedPlayer player) => playersToEarnPoints.Add(player);

        public void RemovePlayer(UnturnedPlayer player) => playersToEarnPoints.Remove(player);
    }
}