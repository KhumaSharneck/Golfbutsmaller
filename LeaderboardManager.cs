using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Golfbutsmaller
{
    /**
  * Manages leaderboard functionality and player statistics
  * Handles data persistence and sorting algorithms
*/
    public class LeaderboardManager : ILeaderboardManager
    {
        // Configuration constants
        private const int MAX_STORED_GAMES = 50;
        private const int MAX_BACKUPS = 3;
        private readonly object _saveLock = new object();
        private Dictionary<string, PlayerStats> playerStats;

        /**
         * Gets application save directory path
         */
        private static string GetSaveDirectory()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string saveDirectory = Path.Combine(appData, "Golfbutsmaller");

            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }

            return saveDirectory;
        }

        /**
         * Gets primary stats file path
         */
        private static string GetStatsFilePath()
        {
            return Path.Combine(GetSaveDirectory(), "PlayerStats.json");
        }

        /**
         * Gets backup file path for specified backup number
         */
        private static string GetBackupFilePath(int backupNumber)
        {
            return Path.Combine(GetSaveDirectory(), $"PlayerStats.backup{backupNumber}.json");
        }

        public LeaderboardManager()
        {
            playerStats = new Dictionary<string, PlayerStats>();
            LoadStats();
        }

        public Dictionary<string, PlayerStats> GetAllStats()
        {
            return new Dictionary<string, PlayerStats>(playerStats ?? new Dictionary<string, PlayerStats>());
        }

        public void LoadStats()
        {
            try
            {
                var loadedStats = new Dictionary<string, PlayerStats>();
                string filePath = GetStatsFilePath();

                // Attempt to load from primary file
                if (File.Exists(filePath))
                {
                    string jsonContent = File.ReadAllText(filePath);
                    loadedStats = JsonSerializer.Deserialize<Dictionary<string, PlayerStats>>(jsonContent);
                }
                else
                {
                    // Try backup files in reverse order
                    for (int i = MAX_BACKUPS - 1; i >= 0; i--)
                    {
                        string backupPath = GetBackupFilePath(i);
                        if (File.Exists(backupPath))
                        {
                            string backupContent = File.ReadAllText(backupPath);
                            loadedStats = JsonSerializer.Deserialize<Dictionary<string, PlayerStats>>(backupContent);
                            File.WriteAllText(filePath, backupContent);
                            break;
                        }
                    }
                }

                playerStats = loadedStats ?? new Dictionary<string, PlayerStats>();
                CleanupOldRecords();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading stats: {ex.Message}");
                playerStats = new Dictionary<string, PlayerStats>();
            }
        }

        /**
         * Attempts to load stats from backup file
         */
        private void LoadBackupFile(string filepath)
        {
            if (File.Exists(filepath))
            {
                string content = File.ReadAllText(filepath);
                var loadedData = JsonSerializer.Deserialize<Dictionary<string, PlayerStats>>(content);
                if (loadedData != null)
                {
                    playerStats = loadedData;
                    SaveStats(); // Restore main file from backup
                }
            }
        }

        /**
         * Attempts to recover data from backup files
         */
        public void AttemptDataRecovery()
        {
            var backupFiles = Directory.GetFiles(GetSaveDirectory(), "*.backup*");
            foreach (var file in backupFiles)
            {
                try
                {
                    LoadBackupFile(file);
                    return;
                }
                catch { continue; }
            }
        }

        public void SaveStats()
        {
            lock (_saveLock)
            {
                try
                {
                    // Serialise current stats
                    string jsonContent = JsonSerializer.Serialize(playerStats, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                    // Save to main file
                    File.WriteAllText(GetStatsFilePath(), jsonContent);

                    // Update backup files
                    for (int i = MAX_BACKUPS - 1; i > 0; i--)
                    {
                        string currentBackup = GetBackupFilePath(i - 1);
                        string nextBackup = GetBackupFilePath(i);
                        if (File.Exists(currentBackup))
                        {
                            File.Copy(currentBackup, nextBackup, true);
                        }
                    }

                    File.WriteAllText(GetBackupFilePath(0), jsonContent);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving stats: {ex.Message}");
                }
            }
        }

        /**
         * Performs binary search for player by name
         */
        public PlayerStats BinarySearchByName(string playerName)
        {
            try
            {
                // Sort players by name for binary search
                var sortedPlayers = playerStats.Values
                    .OrderBy(p => p.Name)
                    .ToList();

                int left = 0;
                int right = sortedPlayers.Count - 1;

                while (left <= right)
                {
                    int mid = (left + right) / 2;
                    int comparison = string.Compare(sortedPlayers[mid].Name, playerName);

                    if (comparison == 0)
                        return sortedPlayers[mid];
                    if (comparison < 0)
                        left = mid + 1;
                    else
                        right = mid - 1;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BinarySearchByName error: {ex.Message}");
                return null;
            }
        }

        /**
         * Sorts players by score using bubble sort
         */
        private List<PlayerStats> BubbleSortByScore(List<PlayerStats> players)
        {
            int n = players.Count;
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (players[j].BestScore > players[j + 1].BestScore)
                    {
                        var temp = players[j];
                        players[j] = players[j + 1];
                        players[j + 1] = temp;
                    }
                }
            }
            return players;
        }

        /**
         * Sorts players by win rate using insertion sort
         */
        private List<PlayerStats> InsertionSortByWinRate(List<PlayerStats> players)
        {
            for (int i = 1; i < players.Count; i++)
            {
                var key = players[i];
                var j = i - 1;

                while (j >= 0 && players[j].WinRate < key.WinRate)
                {
                    players[j + 1] = players[j];
                    j = j - 1;
                }
                players[j + 1] = key;
            }
            return players;
        }

        /**
         * Updates player statistics after game completion
         */
        public void UpdatePlayerStats(string playerName, bool isWin, int shots, int level, bool vsAI = true)
        {
            // Validate input parameters
            if (string.IsNullOrWhiteSpace(playerName))
                throw new ArgumentException("Player name required");
            if (shots < 1)
                throw new ArgumentException("Shots must be positive");
            if (level < 0 || level >= LevelManager.LevelCount)
                throw new ArgumentException("Invalid level");

            try
            {
                // Create new player stats if needed
                if (!playerStats.ContainsKey(playerName))
                {
                    playerStats[playerName] = new PlayerStats
                    {
                        Name = playerName,
                        LevelBestShots = new Dictionary<int, int>()
                    };
                }

                var stats = playerStats[playerName];

                // Update AI game statistics
                if (vsAI)
                {
                    if (isWin)
                    {
                        stats.TotalWins++;
                        stats.CurrentWinStreak++;
                        if (stats.CurrentWinStreak > stats.BestWinStreak)
                            stats.BestWinStreak = stats.CurrentWinStreak;
                    }
                    else
                    {
                        stats.TotalLosses++;
                        stats.CurrentWinStreak = 0;
                    }
                }

                // Update shot statistics
                if (stats.GamesPlayed < MAX_STORED_GAMES)
                {
                    stats.TotalShots += shots;
                    stats.GamesPlayed++;
                    stats.AverageShots = (float)stats.TotalShots / stats.GamesPlayed;
                }

                if (shots == 1)
                {
                    stats.TotalHolesInOne++;
                }

                stats.LastPlayed = DateTime.Now;

                // Update level best scores
                if (!stats.LevelBestShots.ContainsKey(level) || shots < stats.LevelBestShots[level])
                {
                    stats.LevelBestShots[level] = shots;
                }

                if (stats.BestScore == 0 || shots < stats.BestScore)
                {
                    stats.BestScore = shots;
                }

                SaveStats();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating player stats: {ex.Message}");
                throw;
            }
        }

        /**
         * Removes inactive player records
         */
        public void CleanupOldRecords()
        {
            var inactiveThreshold = DateTime.Now.AddMonths(-3);
            var inactivePlayers = playerStats.Where(p => p.Value.LastPlayed < inactiveThreshold).ToList();
            foreach (var player in inactivePlayers)
            {
                playerStats.Remove(player.Key);
            }
        }

        /**
         * Returns top players sorted by win rate
         */
        public IEnumerable<PlayerStats> GetTopPlayers(int count = 10)
        {
            var players = playerStats.Values.ToList();
            return InsertionSortByWinRate(players).Take(count);
        }

        /**
         * Returns top players for specific level
         */
        public IEnumerable<PlayerStats> GetTopPlayersByLevel(int level, int count = 10)
        {
            try
            {
                var levelPlayers = playerStats.Values
                    .Where(p => p.LevelBestShots.ContainsKey(level))
                    .ToList();

                return BubbleSortByScore(levelPlayers).Take(count);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetTopPlayersByLevel error: {ex.Message}");
                return Enumerable.Empty<PlayerStats>();
            }
        }
    }
}