using Microsoft.Xna.Framework;

namespace Golfbutsmaller
{

    /**
* Defines available ball colour options
*/
    public enum BallColour { Normal, Blue, Red, Orange }

    /**
  * Static class managing global game settings and preferences
*/
    public static class GameSettings
    {
        // Audio and game state
        public static bool SoundEnabled { get; set; } = true;
        public static bool IsPaused { get; set; } = false;

        // Player settings 
        public static string PlayerName { get; set; } = "Player";
        public static BallColour CurrentBallColour { get; set; } = BallColour.Normal;
        public static bool HasCompletedTutorial { get; set; } = false;

        // Display settings
        public static bool ShowDetailedStats { get; set; } = true;
        public static bool DebugMode { get; set; } = false;

        // Leaderboard configuration
        public static bool AutoSaveStats { get; set; } = true;
        public static int MaxLeaderboardEntries { get; set; } = 10;
        public static bool ShowAIStats { get; set; } = true;

        /**
         * Resets all settings to default values
         */
        public static void ResetToDefaults()
        {
            // Reset audio and game state
            SoundEnabled = true;
            IsPaused = false;

            // Reset player preferences
            PlayerName = "Player";
            CurrentBallColour = BallColour.Normal;
            HasCompletedTutorial = false;

            // Reset display options
            ShowDetailedStats = true;
            DebugMode = false;

            // Reset leaderboard settings
            AutoSaveStats = true;
            MaxLeaderboardEntries = 10;
            ShowAIStats = true;
        }
    }
}