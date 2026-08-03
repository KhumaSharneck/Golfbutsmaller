using System.Collections.Generic;

namespace Golfbutsmaller
{
    /**
    * Interface defining leaderboard management functionality
    * Handles player statistics tracking and persistence
*/
    public interface ILeaderboardManager
    {
        /**
         * Retrieves all player statistics
         * Returns dictionary of player names and their stats
         */
        Dictionary<string, PlayerStats> GetAllStats();

        /**
         * Loads player statistics from storage
         */
        void LoadStats();

        /**
         * Persists current player statistics to storage
         */
        void SaveStats();

        /**
         * Removes outdated or invalid records
         */
        void CleanupOldRecords();

        /**
         * Retrieves top players sorted by overall performance
         */
        IEnumerable<PlayerStats> GetTopPlayers(int count = 10);

        /**
         * Retrieves top players for specific level
         */
        IEnumerable<PlayerStats> GetTopPlayersByLevel(int level, int count = 10);

        /**
         * Updates player statistics after game completion
         */
        void UpdatePlayerStats(string playerName, bool isWin, int shots, int level, bool vsAI = true);
    }
}