using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Golfbutsmaller
{
/**
 * Manages leaderboard functionality and player statistics display
 * Handles scoring, rankings, and player search capabilities
 */
public class EnhancedLeaderboardScreen : GameScreen
    {
        // Core components
        private Game1 _game;
        private SpriteFont _font;
        private LeaderboardManager _leaderboardManager;

        // View management
        private int _currentView = 0;
        private const int VIEWS_COUNT = 5;
        private KeyboardState _previousKeyboardState;
        private float _pulseTimer = 0f;

        // Search functionality
        private string _searchQuery = "";
        private PlayerStats _searchResult = null;

        // Error handling
        private string _errorMessage = "";
        private float _errorTimer = 0f;
        private const float ERROR_DISPLAY_TIME = 3f;

        /**
         * Initialises leaderboard screen and manager
         */
        public EnhancedLeaderboardScreen(Game game) : base(game)
        {
            try
            {
                _game = (Game1)game;
                _leaderboardManager = new LeaderboardManager();
                Console.WriteLine("EnhancedLeaderboardScreen initialised");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EnhancedLeaderboardScreen initialisation error: {ex.Message}");
                _errorMessage = "Failed to initialise leaderboard";
            }
        }

        public LeaderboardManager GetLeaderboardManager()
        {
            return _leaderboardManager;
        }

        /**
         * Loads required font resources
         */
        public override void LoadContent(ContentManager content)
        {
            try
            {
                _font = content.Load<SpriteFont>("GameFont");
                Console.WriteLine("LeaderboardScreen content loaded");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LeaderboardScreen LoadContent error: {ex.Message}");
                _errorMessage = "Failed to load leaderboard content";
            }
        }

        /**
         * Updates player statistics and rankings
         */
        public void UpdateStats(string playerName, bool isWin, int shots, int level, bool vsAI = true)
        {
            try
            {
                _leaderboardManager.UpdatePlayerStats(playerName, isWin, shots, level, vsAI);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Invalid stats update: {ex.Message}");
                _errorMessage = "Failed to update player stats";
            }
        }

        /**
         * Handles input and updates leaderboard state
         */
        public override void Update(GameTime gameTime)
        {
            try
            {
                KeyboardState currentKeyboardState = Keyboard.GetState();
                _pulseTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (_errorMessage != "")
                {
                    _errorTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (_errorTimer >= ERROR_DISPLAY_TIME)
                    {
                        _errorMessage = "";
                        _errorTimer = 0f;
                    }
                }

                Keys[] validKeys = new Keys[] { Keys.Enter, Keys.Right, Keys.Left, Keys.Back, Keys.L };
                if (currentKeyboardState.GetPressedKeys().Length > 0)
                {
                    Keys pressedKey = currentKeyboardState.GetPressedKeys()[0];
                    if (!validKeys.Contains(pressedKey) &&
                        !(pressedKey >= Keys.A && pressedKey <= Keys.Z))
                    {
                        _errorMessage = "Invalid key - use arrows, letters, or Enter";
                        Console.WriteLine("Error: Invalid key pressed in leaderboard");
                        _errorTimer = 0f;
                    }
                }

                UpdateInputHandling(currentKeyboardState);
                _previousKeyboardState = currentKeyboardState;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LeaderboardScreen Update error: {ex.Message}");
                _errorMessage = "Leaderboard update error";
            }
        }

        /**
         * Processes keyboard input for navigation and search
         */
        private void UpdateInputHandling(KeyboardState currentKeyboardState)
        {
            if (currentKeyboardState.IsKeyDown(Keys.Enter) && !_previousKeyboardState.IsKeyDown(Keys.Enter))
            {
                if (!string.IsNullOrWhiteSpace(_searchQuery))
                {
                    SearchPlayer(_searchQuery);
                    _searchQuery = "";
                }
                else
                {
                    _game.ChangeState(GameState.Menu);
                }
            }
            else if (currentKeyboardState.IsKeyDown(Keys.Right) && !_previousKeyboardState.IsKeyDown(Keys.Right))
            {
                _currentView = (_currentView + 1) % VIEWS_COUNT;
                _searchResult = null;
            }
            else if (currentKeyboardState.IsKeyDown(Keys.Left) && !_previousKeyboardState.IsKeyDown(Keys.Left))
            {
                _currentView = (_currentView - 1 + VIEWS_COUNT) % VIEWS_COUNT;
                _searchResult = null;
            }
            else
            {
                HandleTextInput(currentKeyboardState);
            }
        }

        /**
         * Processes text input for player search
         */
        private void HandleTextInput(KeyboardState currentKeyboardState)
        {
            Keys[] pressedKeys = currentKeyboardState.GetPressedKeys();
            foreach (Keys key in pressedKeys)
            {
                if (_previousKeyboardState.IsKeyUp(key))
                {
                    if (key >= Keys.A && key <= Keys.Z && _searchQuery.Length < 15)
                    {
                        _searchQuery += key.ToString();
                    }
                    else if (key == Keys.Back && _searchQuery.Length > 0)
                    {
                        _searchQuery = _searchQuery.Substring(0, _searchQuery.Length - 1);
                    }
                }
            }
        }

        /**
         * Searches for player by name using binary search
         */
        private void SearchPlayer(string name)
        {
            try
            {
                _searchResult = _leaderboardManager.BinarySearchByName(name);
                if (_searchResult == null)
                {
                    _errorMessage = "Player not found";
                    Console.WriteLine($"Player not found: {name}");
                    _errorTimer = 0f;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SearchPlayer error: {ex.Message}");
                _errorMessage = "Search failed";
                _errorTimer = 0f;
            }
        }

        /**
         * Renders leaderboard interface and statistics
         */
        public override void Draw(SpriteBatch spriteBatch)
        {
            try
            {
                float pulse = (float)Math.Sin(_pulseTimer * 4) * 0.2f + 0.8f;
                float centerX = Game.GraphicsDevice.Viewport.Width / 2;
                float startY = 50;
                float lineHeight = 30;

                string title = _currentView == 0 ? "Overall Leaderboard" : $"Level {_currentView} Best Scores";
                DrawCenteredText(spriteBatch, title, centerX, startY, Color.Gold);

                DrawCenteredText(spriteBatch, "← → to switch views | Enter to return",
                    centerX, startY + lineHeight, Color.Gray * pulse);
                DrawCenteredText(spriteBatch, $"Search: {_searchQuery}_",
                    centerX, startY + lineHeight * 2, Color.White * pulse);

                float contentStartY = startY + lineHeight * 3;

                if (_searchResult != null)
                {
                    DrawPlayerDetails(spriteBatch, _searchResult, centerX, contentStartY);
                }
                else
                {
                    if (_currentView == 0)
                    {
                        DrawOverallLeaderboard(spriteBatch, centerX, contentStartY, lineHeight);
                    }
                    else
                    {
                        DrawLevelLeaderboard(spriteBatch, centerX, contentStartY, lineHeight, _currentView);
                    }
                }

                DrawPlayerSummary(spriteBatch, centerX, Game.GraphicsDevice.Viewport.Height - 100);

                if (_errorMessage != "")
                {
                    Vector2 errorSize = _font.MeasureString(_errorMessage);
                    spriteBatch.DrawString(_font, _errorMessage,
                        new Vector2(centerX - errorSize.X / 2, Game.GraphicsDevice.Viewport.Height - 50),
                        Color.Red * (1 - (_errorTimer / ERROR_DISPLAY_TIME)));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LeaderboardScreen Draw error: {ex.Message}");
                _errorMessage = "Leaderboard display error";
                _errorTimer = 0f;
            }
        }

        /**
         * Displays detailed player statistics
         */
        private void DrawPlayerDetails(SpriteBatch spriteBatch, PlayerStats player, float centerX, float startY)
        {
            try
            {
                float lineHeight = 30;
                float x = centerX - 250;

                spriteBatch.DrawString(_font, $"Player: {player.Name}", new Vector2(x, startY), Color.Yellow);
                spriteBatch.DrawString(_font, $"Games Played: {player.GamesPlayed}", new Vector2(x, startY + lineHeight), Color.White);
                spriteBatch.DrawString(_font, $"Win Rate: {player.WinRate:F1}%", new Vector2(x, startY + lineHeight * 2), Color.White);
                spriteBatch.DrawString(_font, $"Best Score: {player.BestScore}", new Vector2(x, startY + lineHeight * 3), Color.White);
                spriteBatch.DrawString(_font, $"Current Streak: {player.CurrentWinStreak}", new Vector2(x, startY + lineHeight * 4), Color.White);
                spriteBatch.DrawString(_font, $"Best Streak: {player.BestWinStreak}", new Vector2(x, startY + lineHeight * 5), Color.White);
                spriteBatch.DrawString(_font, $"Holes in One: {player.TotalHolesInOne}", new Vector2(x, startY + lineHeight * 6), Color.White);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DrawPlayerDetails error: {ex.Message}");
                _errorMessage = "Error displaying player details";
                _errorTimer = 0f;
            }
        }

        /**
         * Renders overall leaderboard with player rankings
         */
        private void DrawOverallLeaderboard(SpriteBatch spriteBatch, float centerX, float startY, float lineHeight)
        {
            try
            {
                float tableX = centerX - 500;

                string header = String.Format("{0,-15} {1,8} {2,8} {3,12} {4,10} {5,12}",
                    "Player", "Wins", "Games", "Win Rate", "Avg Shots", "Win Streak");
                spriteBatch.DrawString(_font, header, new Vector2(tableX, startY), Color.LightBlue);

                float y = startY + lineHeight;
                foreach (var player in _leaderboardManager.GetTopPlayers())
                {
                    string stats = String.Format("{0,-15} {1,8} {2,8} {3,11:F1}% {4,10:F1} {5,12}",
                        player.Name, player.TotalWins, player.GamesPlayed,
                        player.WinRate, player.AverageShots, player.CurrentWinStreak);
                    spriteBatch.DrawString(_font, stats, new Vector2(tableX, y), Color.White);
                    y += lineHeight;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DrawOverallLeaderboard error: {ex.Message}");
                _errorMessage = "Error displaying leaderboard";
                _errorTimer = 0f;
            }
        }

        /**
         * Displays level-specific leaderboards
         */
        private void DrawLevelLeaderboard(SpriteBatch spriteBatch, float centerX, float startY, float lineHeight, int level)
        {
            try
            {
                string header = String.Format("{0,-20} {1,15}", "Player", "Best Shots");
                spriteBatch.DrawString(_font, header, new Vector2(centerX - 200, startY), Color.LightBlue);

                float y = startY + lineHeight;
                foreach (var player in _leaderboardManager.GetTopPlayersByLevel(level))
                {
                    string stats = String.Format("{0,-20} {1,15}",
                        player.Name, player.LevelBestShots[level]);
                    spriteBatch.DrawString(_font, stats, new Vector2(centerX - 200, y), Color.White);
                    y += lineHeight;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DrawLevelLeaderboard error: {ex.Message}");
                _errorMessage = "Error displaying level scores";
                _errorTimer = 0f;
            }
        }

        /**
         * Shows current player's statistics summary
         */
        private void DrawPlayerSummary(SpriteBatch spriteBatch, float centerX, float y)
        {
            try
            {
                var currentPlayerStats = _leaderboardManager.GetTopPlayers()
                    .FirstOrDefault(p => p.Name == GameSettings.PlayerName);

                if (currentPlayerStats != null)
                {
                    var scale = 0.8f;
                    string summary = $"Your Stats - Win Rate: {currentPlayerStats.WinRate:F1}% | " +
                                   $"Best Score: {currentPlayerStats.BestScore} | " +
                                   $"Current Streak: {currentPlayerStats.CurrentWinStreak} | " +
                                   $"Best Streak: {currentPlayerStats.BestWinStreak} | " +
                                   $"Holes-in-One: {currentPlayerStats.TotalHolesInOne}";

                    Vector2 size = _font.MeasureString(summary) * scale;
                    spriteBatch.DrawString(_font, summary,
                        new Vector2(centerX - size.X / 2, y),
                        Color.Yellow, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DrawPlayerSummary error: {ex.Message}");
                _errorMessage = "Error displaying player summary";
                _errorTimer = 0f;
            }
        }

        /**
         * Utility method for centered text rendering
         */
        private void DrawCenteredText(SpriteBatch spriteBatch, string text, float x, float y, Color color)
        {
            try
            {
                Vector2 size = _font.MeasureString(text);
                spriteBatch.DrawString(_font, text, new Vector2(x - size.X / 2, y), color);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DrawCenteredText error: {ex.Message}");
            }
        }
    }
}