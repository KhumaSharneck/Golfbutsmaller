using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Golfbutsmaller
{

    /**
* Manages tutorial screen and step progression
*/
    public class TutorialScreen : GameScreen
    {
        // Core components
        private Game1 _game;
        private SpriteFont _font;
        private KeyboardState _previousKeyboardState;
        private float _pulseTimer = 0f;

        // Error handling
        private string _errorMessage = "";
        private float _errorTimer = 0f;
        private const float ERROR_DISPLAY_TIME = 3f;

        // Tutorial content
        private List<string> _tutorialSteps = new List<string>
   {
       "Click and drag to aim",
       "Hold to increase power",
       "Release to shoot",
       "ESC to pause",
       "Space to skip tutorial"
   };
        private int _currentStep = 0;

        /**
         * Initialises tutorial screen
         */
        public TutorialScreen(Game game) : base(game)
        {
            try
            {
                _game = (Game1)game;
                Console.WriteLine("TutorialScreen initialised");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TutorialScreen initialisation error: {ex.Message}");
                _errorMessage = "Failed to initialise tutorial";
            }
        }

        /**
         * Loads required font resources
         */
        public override void LoadContent(ContentManager content)
        {
            try
            {
                _font = content.Load<SpriteFont>("GameFont");
                Console.WriteLine("TutorialScreen content loaded");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TutorialScreen LoadContent error: {ex.Message}");
                _errorMessage = "Failed to load tutorial content";
            }
        }

        /**
         * Updates tutorial state and handles input
         */
        public override void Update(GameTime gameTime)
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
                Keys[] validKeys = new Keys[] { Keys.Space, Keys.Enter, Keys.T };
                if (currentKeyboardState.GetPressedKeys().Length > 0 &&
                    !validKeys.Contains(currentKeyboardState.GetPressedKeys()[0]))
                {
                    _errorMessage = "Invalid key pressed - use Space or Enter";
                    Console.WriteLine("Error: Invalid key pressed in tutorial");
                    _errorTimer = 0f;
                }

                // Handle skip tutorial
                if (currentKeyboardState.IsKeyDown(Keys.Space) && !_previousKeyboardState.IsKeyDown(Keys.Space))
                {
                    GameSettings.HasCompletedTutorial = true;
                    _game.ChangeState(GameState.Menu);
                }

                // Handle next step
                if (currentKeyboardState.IsKeyDown(Keys.Enter) && !_previousKeyboardState.IsKeyDown(Keys.Enter))
                {
                    NextStep();
                }

                _previousKeyboardState = currentKeyboardState;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TutorialScreen Update error: {ex.Message}");
                _errorMessage = "Tutorial navigation error";
            }
        }

        /**
         * Advances to next tutorial step
         */
        public void NextStep()
        {
            try
            {
                _currentStep++;
                if (_currentStep >= _tutorialSteps.Count)
                {
                    GameSettings.HasCompletedTutorial = true;
                    _game.ChangeState(GameState.Menu);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TutorialScreen NextStep error: {ex.Message}");
                _errorMessage = "Error advancing tutorial";
            }
        }

        /**
         * Renders tutorial interface
         */
        public override void Draw(SpriteBatch spriteBatch)
        {
            try
            {
                float pulse = (float)System.Math.Sin(_pulseTimer * 4) * 0.2f + 0.8f;
                Vector2 centre = new Vector2(Game.GraphicsDevice.Viewport.Width / 2, 300);

                // Draw current tutorial step
                string currentText = _tutorialSteps[_currentStep];
                Vector2 textSize = _font.MeasureString(currentText);
                spriteBatch.DrawString(_font, currentText,
                    new Vector2(centre.X - textSize.X / 2, centre.Y),
                    Color.White * pulse);

                // Draw instruction text
                string instruction = "Press Enter to continue or Space to skip";
                Vector2 instructSize = _font.MeasureString(instruction);
                spriteBatch.DrawString(_font, instruction,
                    new Vector2(centre.X - instructSize.X / 2, centre.Y + 50),
                    Color.Yellow);

                // Draw error message if present
                if (_errorMessage != "")
                {
                    Vector2 errorSize = _font.MeasureString(_errorMessage);
                    spriteBatch.DrawString(_font, _errorMessage,
                        new Vector2(centre.X - errorSize.X / 2, centre.Y + 100),
                        Color.Red * (1 - (_errorTimer / ERROR_DISPLAY_TIME)));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TutorialScreen Draw error: {ex.Message}");
                _errorMessage = "Tutorial display error";
            }
        }
    }
}