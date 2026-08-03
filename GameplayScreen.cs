using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using Golfbutsmaller;

namespace Golfbutsmaller
{
    public enum PlayerType { Human, AI }

    /**
     * Manages core gameplay mechanics and physics
     */
    public class GameplayScreen : GameScreen
    {
        // Core components
        private SpriteBatch _spriteBatch;
        private SpriteFont _scoreFont;
        private Texture2D _ballTexture, _holeTexture, _rotatedSquareTexture, _pixelTexture, _riverTexture; // Changed from windTexture
        private Vector2 _ballPosition, _holePosition, _ballVelocity, _pointerPosition, _aimDirection;
        private float _ballRadius, _holeRadius;
        private bool _isAiming = false;
        private float _maxPower = 30f, _currentPower = 0f, _powerChargeRate = 10f;
        private const float COLLISION_RADIUS = 15f;

        // UI elements and feedback
        private PauseScreen _pauseScreen;
        private List<ScorePopup> _scorePopups;
        private string _difficultyDisplay;
        private KeyboardState _previousKeyboardState;
        private float _turnIndicatorPulse = 0f;

        // Game state management
        private AIPlayer _aiPlayer;
        private PlayerType _currentPlayer = PlayerType.Human;
        private int _humanScore = 0;
        private int _aiScore = 0;
        private const int maxScore = 5;
        private bool _isAITakingShot = false;
        private float _aiShotDelay = 1f;
        private float _aiShotTimer = 0f;
        private Vector2 _aiTargetDirection;
        private float _aiTargetPower;

        private Level _currentLevel;
        private int _currentLevelIndex = 0;
        private int _currentShots = 0;

        /**
         * Initialises gameplay screen with AI opponent
         */
        public GameplayScreen(Game game) : base(game)
        {
            _aiPlayer = new AIPlayer(AIDifficulty.Hard);
            _pauseScreen = new PauseScreen((Game1)game);
            _scorePopups = new List<ScorePopup>();
            _difficultyDisplay = "AI Difficulty: Hard";
        }

        /**
         * Loads required textures and game content
         */

        public override void LoadContent(ContentManager content)
        {
            try
            {
                _spriteBatch = new SpriteBatch(Game.GraphicsDevice);
                _scoreFont = content.Load<SpriteFont>("GameFont");
                _ballTexture = content.Load<Texture2D>("GolfBall");
                _holeTexture = content.Load<Texture2D>("HoleButBetter");

                _rotatedSquareTexture = new Texture2D(Game.GraphicsDevice, 1, 1);
                _pixelTexture = new Texture2D(Game.GraphicsDevice, 1, 1);
                _riverTexture = new Texture2D(Game.GraphicsDevice, 1, 1); // Changed from windTexture

                _rotatedSquareTexture.SetData(new[] { Color.White });
                _pixelTexture.SetData(new[] { Color.White });
                _riverTexture.SetData(new[] { Color.LightBlue });

                _pauseScreen.LoadContent(content);
                LoadLevel(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GameplayScreen LoadContent error: {ex.Message}");
            }
        }

        /**
         * Updates human player input and shot mechanics
         */
        private void UpdateHumanPlayer(GameTime gameTime)
        {
            var mouseState = Mouse.GetState();
            _pointerPosition = new Vector2(mouseState.X, mouseState.Y);
            _aimDirection = _pointerPosition - _ballPosition;

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                _isAiming = true;
                _currentPower += _powerChargeRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
                _currentPower = MathHelper.Clamp(_currentPower, 0, _maxPower);
            }

            if (mouseState.LeftButton == ButtonState.Released && _isAiming)
            {
                _isAiming = false;
                _ballVelocity = Vector2.Normalize(_aimDirection) * _currentPower;
                _currentPower = 0f;
                _currentShots++;
            }
        }

        /**
         * Updates AI player shot calculations and execution
         */
        private void UpdateAIPlayer(GameTime gameTime)
        {
            if (!_isAITakingShot)
            {
                (_aiTargetDirection, _aiTargetPower) = _aiPlayer.CalculateShot(_ballPosition, _holePosition,
                    _currentLevel.RotatingObstacles, _currentLevel);
                _isAITakingShot = true;
                _aiShotTimer = _aiShotDelay;
            }
            else
            {
                _aiShotTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_aiShotTimer <= 0)
                {
                    _ballVelocity = _aiTargetDirection * _aiTargetPower;
                    _isAITakingShot = false;
                    _currentShots++;
                }
            }
        }

        /**
         * Updates game state, physics, and collisions
         */
        public override void Update(GameTime gameTime)
        {
            try
            {
                foreach (var obstacle in _currentLevel.RotatingObstacles)
                {
                    obstacle.Update(gameTime);
                }

                KeyboardState currentKeyboardState = Keyboard.GetState();

                if (currentKeyboardState.IsKeyDown(Keys.Escape) && _previousKeyboardState.IsKeyUp(Keys.Escape))
                {
                    GameSettings.IsPaused = !GameSettings.IsPaused;
                }

                if (GameSettings.IsPaused)
                {
                    _pauseScreen.Update(gameTime);
                    _previousKeyboardState = currentKeyboardState;
                    return;
                }

                for (int i = _scorePopups.Count - 1; i >= 0; i--)
                {
                    _scorePopups[i].Update(gameTime);
                    if (!_scorePopups[i].IsActive)
                    {
                        _scorePopups.RemoveAt(i);
                    }
                }

                _turnIndicatorPulse = (float)(Math.Sin(gameTime.TotalGameTime.TotalSeconds * 4) + 1) / 2;

                if (_ballVelocity == Vector2.Zero)
                {
                    if (_currentPlayer == PlayerType.Human)
                    {
                        UpdateHumanPlayer(gameTime);
                    }
                    else
                    {
                        UpdateAIPlayer(gameTime);
                    }
                }
                else
                {
                    UpdateBallPhysics(gameTime);
                    HandleWallCollisions();
                    HandleObstacleCollisions(gameTime);
                    CheckHoleCollision();
                }

                _previousKeyboardState = currentKeyboardState;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update error: {ex.Message}");
            }
        }

        /**
         * Updates ball physics including river effects
         */
        private void UpdateBallPhysics(GameTime gameTime)
        {
            bool wasInRiver = false; // Changed from wasInWind
            if (_currentLevel.RiverForce != Vector2.Zero) // Changed from WindForce
            {
                var riverRect = new Rectangle(600, 120, 200, 480); // Changed from windRect
                if (riverRect.Contains(_ballPosition))
                {
                    wasInRiver = true;
                    _ballVelocity += _currentLevel.RiverForce * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (_ballVelocity.Length() < 0.5f)
                    {
                        _ballVelocity = Vector2.Zero;
                        _currentPlayer = _currentPlayer == PlayerType.Human ? PlayerType.AI : PlayerType.Human;
                        return;
                    }
                }
            }

            _ballPosition += _ballVelocity;
            _ballVelocity *= wasInRiver ? 0.995f : 0.98f;

            if (_ballVelocity.Length() < 0.01f)
            {
                _ballVelocity = Vector2.Zero;
                _currentPlayer = _currentPlayer == PlayerType.Human ? PlayerType.AI : PlayerType.Human;
                CreateScorePopup(_ballPosition, "Turn End", Color.Gray);
            }
        }

        /**
    * Loads and initialises level configuration
    */
        private void LoadLevel(int index)
        {
            try
            {
                if (index < LevelManager.LevelCount)
                {
                    _currentLevel = LevelManager.GetLevel(index);
                    _ballPosition = new Vector2(200, 350);
                    _holePosition = new Vector2(1100, 350);
                    _ballVelocity = Vector2.Zero;
                    _isAiming = false;
                    _currentPower = 0f;
                    _isAITakingShot = false;
                    _aiShotTimer = 0f;
                    _currentShots = 0;
                    _ballRadius = _ballTexture.Width / 2f;
                    _holeRadius = 15f;

                    CreateScorePopup(_holePosition, $"Level {_currentLevelIndex + 1}", Color.Yellow);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadLevel error: {ex.Message}");
            }
        }

        /**
         * Handles ball collisions with course boundaries
         */
        private void HandleWallCollisions()
        {
            try
            {
                int leftBoundary = 30;
                int rightBoundary = 1258;
                int topBoundary = 70;
                int bottomBoundary = 635;
                bool collided = false;

                if (_ballPosition.X - _ballRadius < leftBoundary || _ballPosition.X + _ballRadius > rightBoundary)
                {
                    _ballVelocity.X *= -1;
                    _ballPosition.X = MathHelper.Clamp(_ballPosition.X, leftBoundary + _ballRadius, rightBoundary - _ballRadius);
                    collided = true;
                }

                if (_ballPosition.Y - _ballRadius < topBoundary || _ballPosition.Y + _ballRadius > bottomBoundary)
                {
                    _ballVelocity.Y *= -1;
                    _ballPosition.Y = MathHelper.Clamp(_ballPosition.Y, topBoundary + _ballRadius, bottomBoundary - _ballRadius);
                    collided = true;
                }

                if (collided && GameSettings.SoundEnabled)
                {
                    SoundManager.PlaySound("wall_hit");
                    CreateScorePopup(_ballPosition, "Bounce!", Color.Orange);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HandleWallCollisions error: {ex.Message}");
            }
        }

        /**
         * Manages ball collisions with rotating obstacles
         */
        private void HandleObstacleCollisions(GameTime gameTime)
        {
            try
            {
                foreach (var obstacle in _currentLevel.RotatingObstacles)
                {
                    obstacle.Update(gameTime);

                    if (CheckRotatingObstacleCollision(_ballPosition, COLLISION_RADIUS, obstacle))
                    {
                        Vector2 normal = Vector2.Normalize(_ballPosition - obstacle.Position);
                        _ballVelocity = Vector2.Reflect(_ballVelocity, normal);
                        _ballPosition += normal * 2f;

                        if (GameSettings.SoundEnabled)
                        {
                            SoundManager.PlaySound("obstacle_hit");
                            CreateScorePopup(_ballPosition, "Bonk!", Color.Red);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HandleObstacleCollisions error: {ex.Message}");
            }
        }

        private void CheckHoleCollision()
        {
            float distance = Vector2.Distance(_ballPosition, _holePosition);
            if (distance < _holeRadius)
            {
                // Update scores based on current player
                if (_currentPlayer == PlayerType.Human)
                    _humanScore++;
                else
                    _aiScore++;

                // Play scoring sound effect
                if (GameSettings.SoundEnabled)
                    SoundManager.PlaySound("score");

                CreateScorePopup(_holePosition, "Goal!", Color.Green);

                // Check for game win condition and handle end game
                if (_humanScore >= maxScore || _aiScore >= maxScore)
                {
                    bool isPlayerWin = _humanScore >= maxScore;
                    string winner = isPlayerWin ? "Player" : "AI";

                    // Update leaderboard with game results
                    ((Game1)Game).GetLeaderboardScreen().UpdateStats(
                        GameSettings.PlayerName,
                        isPlayerWin,
                        _currentShots,
                        _currentLevelIndex,
                        true
                    );

                    // Return to menu with winner message
                    ((Game1)Game).menuScreen.SetEndMessage($"{winner} wins!");
                    ((Game1)Game).ChangeState(GameState.Menu);
                    ResetGame();
                    return;
                }

                // Advance to next level if game hasn't ended
                _currentLevelIndex = (_currentLevelIndex + 1) % LevelManager.LevelCount;
                LoadLevel(_currentLevelIndex);
            }
        }

        /**
 * Returns colour for ball based on player settings
 */
        public Color GetBallColor()
        {
            // Convert ball colour setting to actual colour value
            return GameSettings.CurrentBallColour switch
            {
                BallColour.Blue => Color.RoyalBlue,
                BallColour.Red => Color.Crimson,
                BallColour.Orange => Color.Orange,
                _ => Color.White
            };
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (spriteBatch == null) return;

            try
            {
                // Set background and draw course elements
                Game.GraphicsDevice.Clear(_currentLevel.BackgroundColour);

                // Draw walls with collision boundaries
                foreach (var wall in _currentLevel.Walls)
                {
                    spriteBatch.Draw(_pixelTexture, wall, Color.BlanchedAlmond);
#if DEBUG
                    spriteBatch.Draw(_pixelTexture, wall, Color.Transparent * 0.3f);
#endif
                }

                // Draw ball with collision boundary
                var ballRect = new Rectangle(
                    (int)(_ballPosition.X - _ballRadius),
                    (int)(_ballPosition.Y - _ballRadius),
                    (int)(_ballRadius * 2),
                    (int)(_ballRadius * 2)
                );
                spriteBatch.Draw(_ballTexture, ballRect, GetBallColor());
#if DEBUG
                spriteBatch.Draw(_pixelTexture, ballRect, Color.Transparent * 0.3f);
#endif

                // Draw hole target
                var holeTextureSize = _holeTexture.Width;
                spriteBatch.Draw(_holeTexture,
                    new Rectangle(
                        (int)(_holePosition.X - holeTextureSize / 2),
                        (int)(_holePosition.Y - holeTextureSize / 2),
                        holeTextureSize,
                        holeTextureSize
                    ),
                    Color.White);

                // Draw river area if present
                if (_currentLevel.RiverForce != Vector2.Zero)
                {
                    var riverRect = new Rectangle(600, 120, 200, 480);
                    spriteBatch.Draw(_riverTexture, riverRect, Color.LightBlue * 0.5f);
                    DrawRiverArrow(spriteBatch, riverRect);
                }

                // Draw obstacles and UI elements
                foreach (var obstacle in _currentLevel.RotatingObstacles)
                {
                    obstacle.Draw(spriteBatch, _rotatedSquareTexture);
                }

                DrawUI(spriteBatch);

                // Draw score popups and pause screen
                foreach (var popup in _scorePopups)
                {
                    popup.Draw(spriteBatch, _scoreFont);
                }

                if (GameSettings.IsPaused)
                {
                    _pauseScreen.Draw(spriteBatch);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Draw error: {ex.Message}");
            }
        }

        private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color)
        {
            try
            {
                // Calculate angle and length for line drawing
                Vector2 edge = end - start;
                float angle = (float)Math.Atan2(edge.Y, edge.X);
                float length = edge.Length();

                spriteBatch.Draw(
                    _pixelTexture,
                    start,
                    null,
                    color,
                    angle,
                    Vector2.Zero,
                    new Vector2(length, 2),
                    SpriteEffects.None,
                    0
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DrawLine error: {ex.Message}");
            }
        }

        private void DrawUI(SpriteBatch spriteBatch)
        {
            try
            {
                // Base positions for UI elements
                float baseX = 20;
                float baseY = 20;
                float lineSpacing = 30;

                // Draw semi-transparent UI background
                DrawUIBackground(spriteBatch, new Vector2(baseX - 10, baseY - 5), new Vector2(300, 150));

                // Verify font loaded before drawing text
                if (_scoreFont == null)
                {
                    Console.WriteLine("ERROR: Score font is null in DrawUI");
                    return;
                }

                // Draw scores with shadow effects
                DrawShadowedText(spriteBatch, $"Player: {_humanScore}", new Vector2(baseX, baseY), Color.White);
                DrawShadowedText(spriteBatch, $"AI: {_aiScore}", new Vector2(baseX + 200, baseY), Color.White);

                // Draw pulsing turn indicator
                string turnText = $"Current Turn: {(_currentPlayer == PlayerType.Human ? "Player" : "AI")}";
                Color turnColor = _currentPlayer == PlayerType.Human ? Color.Yellow : Color.Orange;
                turnColor = Color.Lerp(turnColor, Color.White, _turnIndicatorPulse);
                DrawShadowedText(spriteBatch, turnText, new Vector2(baseX, baseY + lineSpacing), turnColor);

                // Draw game information
                DrawShadowedText(spriteBatch, _difficultyDisplay, new Vector2(baseX, baseY + lineSpacing * 2), Color.White);
                DrawShadowedText(spriteBatch, $"Level: {_currentLevelIndex + 1}/4", new Vector2(baseX, baseY + lineSpacing * 3), Color.White);
                DrawShadowedText(spriteBatch, $"Shots: {_currentShots}", new Vector2(baseX, baseY + lineSpacing * 4), Color.White);
                DrawShadowedText(spriteBatch, $"Sound: {(GameSettings.SoundEnabled ? "ON" : "OFF")}",
                                new Vector2(baseX, baseY + lineSpacing * 5), Color.White);

                // Draw current player UI elements
                if (_currentPlayer == PlayerType.Human && _isAiming)
                {
                    DrawPowerMeter(spriteBatch);
                    DrawAimLine(spriteBatch);
                }
                else if (_currentPlayer == PlayerType.AI && _isAITakingShot)
                {
                    DrawAIAimLine(spriteBatch);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DrawUI error: {ex.Message}");
            }
        }

        private void DrawRiverArrow(SpriteBatch spriteBatch, Rectangle riverRect)
        {
            try
            {
                // Calculate arrow positions
                Vector2 center = new Vector2(riverRect.Center.X, riverRect.Center.Y);
                Vector2 direction = Vector2.Normalize(_currentLevel.RiverForce);
                Vector2 arrowEnd = center + direction * 40;

                // Draw main arrow line
                DrawLine(spriteBatch, center, arrowEnd, Color.White * 0.7f);

                // Draw arrow head
                float arrowHeadLength = 10f;
                float arrowHeadAngle = 0.5f;
                Vector2 arrowTip1 = arrowEnd -
                    Vector2.Transform(direction * arrowHeadLength,
                    Matrix.CreateRotationZ(arrowHeadAngle));
                Vector2 arrowTip2 = arrowEnd -
                    Vector2.Transform(direction * arrowHeadLength,
                    Matrix.CreateRotationZ(-arrowHeadAngle));

                DrawLine(spriteBatch, arrowEnd, arrowTip1, Color.White * 0.7f);
                DrawLine(spriteBatch, arrowEnd, arrowTip2, Color.White * 0.7f);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DrawRiverArrow error: {ex.Message}");
            }
        }

        private void DrawShadowedText(SpriteBatch spriteBatch, string text, Vector2 position, Color color)
        {
            try
            {
                // Check if font is loaded
                if (_scoreFont == null)
                {
                    Console.WriteLine("ERROR: Score font is null in DrawShadowedText");
                    return;
                }

                // Draw text shadow slightly offset
                spriteBatch.DrawString(_scoreFont, text, position + new Vector2(2, 2), Color.Black * 0.5f);
                // Draw main text
                spriteBatch.DrawString(_scoreFont, text, position, color);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DrawShadowedText error: {ex.Message}");
            }
        }

        private void DrawUIBackground(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
        {
            try
            {
                // Create semi-transparent background rectangle
                var rect = new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);
                spriteBatch.Draw(_pixelTexture, rect, Color.Black * 0.3f);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DrawUIBackground error: {ex.Message}");
            }
        }

        private void DrawPowerMeter(SpriteBatch spriteBatch)
        {
            try
            {
                // Draw power meter background
                var powerMeterBg = new Rectangle(
                    (int)_ballPosition.X - 50,
                    (int)_ballPosition.Y - 20,
                    100,
                    10
                );
                spriteBatch.Draw(_pixelTexture, powerMeterBg, Color.Gray * 0.5f);

                // Draw power meter fill based on current power
                var powerMeterFill = new Rectangle(
                    (int)_ballPosition.X - 50,
                    (int)_ballPosition.Y - 20,
                    (int)(_currentPower / _maxPower * 100),
                    10
                );
                // Interpolate colour from green to red based on power
                Color powerColor = Color.Lerp(Color.Green, Color.Red, _currentPower / _maxPower);
                spriteBatch.Draw(_pixelTexture, powerMeterFill, powerColor);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DrawPowerMeter error: {ex.Message}");
            }
        }
        private void DrawAimLine(SpriteBatch spriteBatch)
        {
            try
            {
                // Calculate aim direction and display projected trajectory
                Vector2 direction = Vector2.Normalize(_aimDirection);
                Vector2 lineEnd = _ballPosition + direction * (_currentPower * 2);
                DrawLine(spriteBatch, _ballPosition, lineEnd, Color.White * 0.5f);

                // Draw mouse pointer location for aim targeting
                spriteBatch.Draw(
                    _pixelTexture,
                    new Rectangle((int)_pointerPosition.X - 5, (int)_pointerPosition.Y - 5, 10, 10),
                    Color.Red
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DrawAimLine error: {ex.Message}");
            }
        }

        private void DrawAIAimLine(SpriteBatch spriteBatch)
        {
            try
            {
                // Display AI's calculated shot trajectory and power
                Vector2 aimEnd = _ballPosition + _aiTargetDirection * (_aiTargetPower * 2);
                // Use yellow with transparency for AI shot preview
                DrawLine(spriteBatch, _ballPosition, aimEnd, Color.Yellow * 0.5f);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DrawAIAimLine error: {ex.Message}");
            }
        }

        private bool CheckRotatingObstacleCollision(Vector2 ballPosition, float collisionRadius, RotatingObstacle obstacle)
        {
            try
            {
                // Calculate ball position relative to obstacle center
                Vector2 relativePosition = ballPosition - obstacle.Position;

                // Transform ball position based on obstacle rotation
                float cosAngle = (float)Math.Cos(-obstacle.Rotation);
                float sinAngle = (float)Math.Sin(-obstacle.Rotation);
                Vector2 rotatedPosition = new Vector2(
                    relativePosition.X * cosAngle - relativePosition.Y * sinAngle,
                    relativePosition.X * sinAngle + relativePosition.Y * cosAngle
                );

                // Get obstacle dimensions for boundary check
                float halfWidth = obstacle.Size.X / 2;
                float halfHeight = obstacle.Size.Y / 2;

                // Check if ball is within rotated obstacle bounds plus collision radius
                return Math.Abs(rotatedPosition.X) < halfWidth + collisionRadius &&
                       Math.Abs(rotatedPosition.Y) < halfHeight + collisionRadius;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CheckRotatingObstacleCollision error: {ex.Message}");
                return false;
            }
        }

        private void CreateScorePopup(Vector2 position, string text, Color color)
        {
            // Create new floating score popup at specified position
            _scorePopups.Add(new ScorePopup(position, text, color));
        }

        public void SetFirstPlayer(PlayerType playerType)
        {
            try
            {
                // Set starting player after coin flip
                _currentPlayer = playerType;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SetFirstPlayer error: {ex.Message}");
            }
        }

        public void SetAIDifficulty(AIDifficulty difficulty)
        {
            try
            {
                // Initialize AI with selected difficulty level
                _aiPlayer = new AIPlayer(difficulty);
                // Update UI display with current difficulty
                _difficultyDisplay = $"AI Difficulty: {difficulty}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SetAIDifficulty error: {ex.Message}");
            }
        }

        public void ResetGame()
        {
            try
            {
                // Reset all game state to initial values
                _humanScore = 0;        // Clear player score
                _aiScore = 0;           // Clear AI score
                _currentLevelIndex = 0;  // Return to first level
                _currentPlayer = PlayerType.Human;  // Reset to human first
                _currentShots = 0;      // Reset shot counter
                _scorePopups.Clear();   // Clear any active popups
                LoadLevel(_currentLevelIndex);  // Load initial level
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ResetGame error: {ex.Message}");
            }
        }
    }
}