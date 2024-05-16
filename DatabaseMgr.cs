using System;
using MySql.Data.MySqlClient;
using Rocket.Core.Logging;

namespace FeexRanks
{
    public class DatabaseMgr
    {
        private readonly FeexRanksPlugin _feexRanks;

        internal DatabaseMgr(FeexRanksPlugin feexRanks)
        {
            _feexRanks = feexRanks;
            CheckSchema();
        }

        private void CheckSchema()
        {
            try
            {
                MySqlConnection mySqlConnection = CreateConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlConnection.Open();
                mySqlCommand.CommandText = string.Concat(
                "CREATE TABLE IF NOT EXISTS `",
                _feexRanks.Configuration.Instance.FeexRanksTableName,
                "` (",
                "`steamId` VARCHAR(32) NOT NULL,",
                "`points` DOUBLE NOT NULL,",
                "`currentRank` INT NOT NULL,",
                "`lastUpdated` VARCHAR(32) NOT NULL,",
                "PRIMARY KEY (`steamId`)",
                ");"
            );
                mySqlCommand.ExecuteNonQuery();
                mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogError($"[FeexRanks] Database Crashed by Console when trying to create or check existing table {_feexRanks.Configuration.Instance.FeexRanksTableName}, reason: {exception.Message}");
            }
        }

        public MySqlConnection CreateConnection()
        {
            MySqlConnection mySqlConnection = null;
            try
            {
                mySqlConnection = new MySqlConnection(string.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};PORT={4};", _feexRanks.Configuration.Instance.DatabaseAddress, _feexRanks.Configuration.Instance.DatabaseName, _feexRanks.Configuration.Instance.DatabaseUsername, _feexRanks.Configuration.Instance.DatabasePassword, _feexRanks.Configuration.Instance.DatabasePort));
            }
            catch (Exception exception)
            {
                Logger.LogError($"[FeexRanks] Instance Connection Database Crashed, reason: {exception.Message}");
            }
            return mySqlConnection;
        }

        /// <summary>
        /// Add a new player to the feexranks table if not exist
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="balance"></param>
        public void AddNewPlayer(string playerId)
        {
            try
            {
                // Instanciate connection
                MySqlConnection mySqlConnection = CreateConnection();
                // Instanciate command
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                // Command: Insert new player only if not exist the same steamId
                mySqlCommand.CommandText = string.Concat("Insert ignore into `", _feexRanks.Configuration.Instance.FeexRanksTableName, "` (`steamId`, `points`, `currentRank` `lastUpdated`) VALUES ('", playerId, "', '", 0, "', '", _feexRanks.Configuration.Instance.Ranks[0].rankName, "', '", DateTime.Now.ToShortDateString(), "');");
                // Try to connect
                mySqlConnection.Open();
                // Execute the command
                mySqlCommand.ExecuteNonQuery();
                // Close connection
                mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogError($"[FeexRanks] Database Crashed by {playerId} from function AddNewPlayer, reason: {exception.Message}");
            }
        }

        /// <summary>
        /// Returns the player points from the table feexranks
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public int GetPoints(string playerId)
        {
            int num = 0;
            try
            {
                MySqlConnection mySqlConnection = CreateConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat("select `points` from `", _feexRanks.Configuration.Instance.FeexRanksTableName, "` where `steamId` = '", playerId, "';");
                mySqlConnection.Open();
                object obj = mySqlCommand.ExecuteScalar();
                if (obj != null)
                {
                    int.TryParse(obj.ToString(), out num);
                }
                mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogError($"[FeexRanks] Database Crashed by {playerId} from function GetPoints, reason: {exception.Message}");
            }
            return num;
        }

        /// <summary>
        /// Returns the player actual rank
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public string GetRank(string playerId)
        {
            string rank = "";
            try
            {
                MySqlConnection mySqlConnection = CreateConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat("select `currentRank` from `", _feexRanks.Configuration.Instance.FeexRanksTableName, "` where `steamId` = '", playerId, "';");
                mySqlConnection.Open();
                object obj = mySqlCommand.ExecuteScalar();
                if (obj != null)
                {
                    rank = obj.ToString();
                }
                mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogError($"[FeexRanks] Database Crashed by {playerId} from function GetRank, reason: {exception.Message}");
            }
            return rank;
        }

        /// <summary>
        /// Add points to the player rank
        /// </summary>
        /// <param name="id"></param>
        /// <param name="quantity"></param>
        public void AddPoints(string id, int quantity)
        {
            // Simple cancel the operation if not enabled
            if (quantity == 0) return;
            try
            {
                MySqlConnection mySqlConnection = CreateConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = $"update `{_feexRanks.Configuration.Instance.FeexRanksTableName}` set `points` = `points` + {quantity} where `steamId` = {id};";
                mySqlConnection.Open();
                mySqlCommand.ExecuteNonQuery();
                mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogError($"[FeexRanks] Database Crashed by {id} from function AddPoints, reason: {exception.Message}");
            }
        }

        /// <summary>
        /// Update the rank for the player
        /// </summary>
        /// <param name="id"></param>
        /// <param name="quantity"></param>
        public void UpdateRank(string id, string rank)
        {
            try
            {
                MySqlConnection mySqlConnection = CreateConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = $"update `{_feexRanks.Configuration.Instance.FeexRanksTableName}` set `currentRank` = {rank} where `steamId` = {id};";
                mySqlConnection.Open();
                mySqlCommand.ExecuteNonQuery();
                mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogError($"[FeexRanks] Database Crashed by {id} from function UpdateRank, reason: {exception.Message}");
            }
        }

        /// <summary>
        /// Add more balance to the player
        /// </summary>
        /// <param name="id"></param>
        /// <param name="quantity"></param>
        public void AddBalance(string id, decimal quantity)
        {
            try
            {
                MySqlConnection mySqlConnection = CreateConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = $"update `{_feexRanks.Configuration.Instance.UconomyTableName}` set `balance` = `balance` + {quantity} where `steamId` = {id};";
                mySqlConnection.Open();
                mySqlCommand.ExecuteNonQuery();
                mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogError($"[FeexRanks] Database Crashed by {id} from function AddBalance, reason: {exception.Message}");
            }
        }
    }
}