using Golfbutsmaller;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

namespace Golfbutsmaller
{
    /**
 * Manages pause menu functionality and display
*/
    public class PauseScreen
    {
        private Game1 _game;
        private Texture2D _pixel;
        private SpriteFont _font;
        private KeyboardState _previousKeyboardState;
        private readonly Color _overlayColour = new Color(0, 0, 0, 180);
        private Rectangle _fullScreen;
        private float _pulseTimer = 0f;
        private string _errorMessage = "";
        private float _errorTimer = 0f;
        private const float ERROR_DISPLAY_TIME = 3f;

        /**
         * Initialises pause screen components
         */
        public PauseScreen(Game1 game)
        {
            try
            {
                _game = game;
                _fullScreen = new Rectangle(0, 0, 1280, 720);
                Console.WriteLine("PauseScreen initialised");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PauseScreen initialisation error: {ex.Message}");
                _errorMessage = "Failed to initialise pause screen";
            }
        }

        /**
         * Loads required visual assets and fonts
         */
        public void LoadContent(ContentManager content)
        {
            try
            {
                _font = content.Load<SpriteFont>("GameFont");
                _pixel = new Texture2D(_game.GraphicsDevice, 1, 1);
                _pixel.SetData(new[] { Color.White });
                Console.WriteLine("PauseScreen content loaded");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PauseScreen LoadContent error: {ex.Message}");
                _errorMessage = "Failed to load pause content";
            }
        }

        /**
         * Updates pause menu state and handles input
         */
        public void Update(GameTime gameTime)
        {
            try
            {
                KeyboardState currentKeyboardState = Keyboard.GetState();
                _pulseTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                // Handle error message timing
                if (_errorMessage != "")
                {
                    _errorTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (_errorTimer >= ERROR_DISPLAY_TIME)
                    {
                        _errorMessage = "";
                        _errorTimer = 0f;
                    }
                }

                // Validate input keys
                Keys[] validKeys = new Keys[] { Keys.Escape, Keys.R, Keys.M, Keys.S, Keys.L };
                if (currentKeyboardState.GetPressedKeys().Length > 0 &&
                    !validKeys.Contains(currentKeyboardState.GetPressedKeys()[0]))
                {
                    _errorMessage = "Invalid key - use ESC, R, M, S, or L";
                    Console.WriteLine("Error: Invalid key pressed in pause menu");
                    _errorTimer = 0f;
                }

                // Toggle pause state
                if (currentKeyboardState.IsKeyDown(Keys.Escape) &&
                    _previousKeyboardState.IsKeyUp(Keys.Escape))
                {
                    GameSettings.IsPaused = !GameSettings.IsPaused;
                }

                // Handle pause menu options
                if (GameSettings.IsPaused)
                {
                    if (currentKeyboardState.IsKeyDown(Keys.R) && _previousKeyboardState.IsKeyUp(Keys.R))
                        Resume();
                    else if (currentKeyboardState.IsKeyDown(Keys.M) && _previousKeyboardState.IsKeyUp(Keys.M))
                        ReturnToMenu();
                    else if (currentKeyboardState.IsKeyDown(Keys.S) && _previousKeyboardState.IsKeyUp(Keys.S))
                        ToggleSound();
                    else if (currentKeyboardState.IsKeyDown(Keys.L) && _previousKeyboardState.IsKeyUp(Keys.L))
                        ViewLeaderboard();
                }

                _previousKeyboardState = currentKeyboardState;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PauseScreen Update error: {ex.Message}");
                _errorMessage = "Pause screen update error";
                _errorTimer = 0f;
            }
        }

        /**
         * Renders pause menu interface
         */
        public void Draw(SpriteBatch spriteBatch)
        {
            try
            {
                if (!GameSettings.IsPaused) return;

                // Draw semi-transparent overlay
                spriteBatch.Draw(_pixel, _fullScreen, _overlayColour);

                float centerX = _game.GraphicsDevice.Viewport.Width / 2;
                float baseY = 200;
                float lineSpacing = 50;
                float pulse = (float)Math.Sin(_pulseTimer * 4) * 0.2f + 0.8f;

                // Draw menu options
                DrawCenteredText(spriteBatch, "PAUSED", centerX, baseY, Color.White);
                DrawCenteredText(spriteBatch, "Press R to Resume", centerX, baseY + lineSpacing, Color.White * pulse);
                DrawCenteredText(spriteBatch, "Press M for Main Menu", centerX, baseY + lineSpacing * 2, Color.White);
                DrawCenteredText(spriteBatch, $"Press S for Sound: {(GameSettings.SoundEnabled ? "ON" : "OFF")}",
                    centerX, baseY + lineSpacing * 3, Color.White);
                DrawCenteredText(spriteBatch, "Press L to View Leaderboard", centerX, baseY + lineSpacing * 4, Color.White);

                // Draw stats if enabled
                if (GameSettings.ShowDetailedStats)
                {
                    DrawGameStats(spriteBatch, centerX, baseY + lineSpacing * 6);
                }

                // Draw error message if present
                if (_errorMessage != "")
                {
                    DrawCenteredText(spriteBatch, _errorMessage, centerX, baseY + lineSpacing * 7,
                        Color.Red * (1 - (_errorTimer / ERROR_DISPLAY_TIME)));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PauseScreen Draw error: {ex.Message}");
                _errorMessage = "Pause screen display error";
                _errorTimer = 0f;
            }
        }

        /**
         * Displays player statistics
         */
        private void DrawGameStats(SpriteBatch spriteBatch, float centerX, float baseY)
        {
            try
            {
                var stats = _game.GetLeaderboardScreen().GetLeaderboardManager()
                    .GetTopPlayers(1)
                    .FirstOrDefault();

                if (stats != null)
                {
                    DrawCenteredText(spriteBatch, $"Best Score: {stats.BestScore}", centerX, baseY, Color.Yellow);
                    DrawCenteredText(spriteBatch, $"Win Rate: {stats.WinRate:F1}%", centerX, baseY + 30, Color.Yellow);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DrawGameStats error: {ex.Message}");
                _errorMessage = "Error displaying game stats";
                _errorTimer = 0f;
            }
        }

        /**
         * Utility method for centered text rendering
         */
        private void DrawCenteredText(SpriteBatch spriteBatch, string text, float x, float y, Color colour)
        {
            try
            {
                if (spriteBatch == null || _font == null || string.IsNullOrEmpty(text))
                {
                    throw new ArgumentException("Invalid drawing parameters");
                }
                Vector2 size = _font.MeasureString(text);
                try
                {
                    spriteBatch.DrawString(_font, text, new Vector2(x - size.X / 2, y), colour);
                }
                catch
                {
                    _errorMessage = "Failed to render text";
                    _errorTimer = 0f;
                    Console.WriteLine($"Failed to render text: {text}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DrawCenteredText error: {ex.Message}");
                _errorMessage = "Error displaying text";
                _errorTimer = 0f;
            }
        }

        /**
         * Resumes gameplay from pause state
         */
        private void Resume()
        {
            try
            {
                GameSettings.IsPaused = false;
                Console.WriteLine("Game resumed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Resume error: {ex.Message}");
                _errorMessage = "Error resuming game";
                _errorTimer = 0f;
            }
        }

        /**
         * Returns to main menu
         */
        private void ReturnToMenu()
        {
            try
            {
                GameSettings.IsPaused = false;
                _game.ChangeState(GameState.Menu);
                Console.WriteLine("Returned to main menu");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ReturnToMenu error: {ex.Message}");
                _errorMessage = "Error returning to menu";
                _errorTimer = 0f;
            }
        }

        /**
         * Toggles sound settings
         */
        private void ToggleSound()
        {
            try
            {
                GameSettings.SoundEnabled = !GameSettings.SoundEnabled;
                Console.WriteLine($"Sound toggled: {GameSettings.SoundEnabled}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ToggleSound error: {ex.Message}");
                _errorMessage = "Error toggling sound";
                _errorTimer = 0f;
            }
        }

        /**
         * Shows leaderboard screen
         */
        private void ViewLeaderboard()
        {
            try
            {
                GameSettings.IsPaused = false;
                _game.ChangeState(GameState.Leaderboard);
                Console.WriteLine("Viewing leaderboard from pause menu");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ViewLeaderboard error: {ex.Message}");
                _errorMessage = "Error viewing leaderboard";
                _errorTimer = 0f;
            }
        }
    }
}