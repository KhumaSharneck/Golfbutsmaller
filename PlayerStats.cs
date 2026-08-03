using System;
using System.Collections.Generic;

namespace Golfbutsmaller
{
    /**
    * Manages player statistics and performance tracking
*/
    public class PlayerStats
    {
        // Player identification
        public string Name { get; set; }

        // Match statistics
        public int TotalWins { get; set; }
        public int TotalLosses { get; set; }
        public int GamesPlayed { get; set; }
        public DateTime LastPlayed { get; set; }

        // Shot tracking
        public int TotalShots { get; set; }
        public float AverageShots { get; set; }
        public int BestScore { get; set; }
        public int TotalHolesInOne { get; set; }

        // Streak tracking
        public int CurrentWinStreak { get; set; }
        public int BestWinStreak { get; set; }

        // Level progression
        public Dictionary<int, int> LevelBestShots { get; set; } = new Dictionary<int, int>();

        // Calculated properties
        public int TotalGames => TotalWins + TotalLosses;
        public float WinRate => TotalGames > 0 ? (float)TotalWins / TotalGames * 100 : 0;
    }
}