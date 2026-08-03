using Golfbutsmaller;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

/**
* Manages main menu interface and navigation
*/
public class MenuScreen : GameScreen
{
    // Core components
    private Game1 _game;
    private SpriteFont _font;
    private string endMessage = "";
    private KeyboardState _previousKeyboardState;
    private float _pulseTimer = 0f;

    // Error handling
    private string _errorMessage = "";
    private float _errorTimer = 0f;
    private const float ERROR_DISPLAY_TIME = 3f;

    /**
     * Initialises menu screen 
     */
    public MenuScreen(Game game) : base(game)
    {
        try
        {
            _game = (Game1)game;
            Console.WriteLine("MenuScreen initialised");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MenuScreen initialisation error: {ex.Message}");
            _errorMessage = "Failed to initialise menu";
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
            Console.WriteLine("MenuScreen content loaded");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MenuScreen LoadContent error: {ex.Message}");
            _errorMessage = "Failed to load menu content";
        }
    }

    /**
     * Sets end game message for display
     */
    public void SetEndMessage(string message)
    {
        endMessage = message;
    }

    /**
     * Updates menu state and handles input
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

            // Handle menu navigation
            if (currentKeyboardState.IsKeyDown(Keys.A) && !_previousKeyboardState.IsKeyDown(Keys.A))
            {
                _game.ChangeState(GameState.DifficultySelect);
            }
            else if (currentKeyboardState.IsKeyDown(Keys.L) && !_previousKeyboardState.IsKeyDown(Keys.L))
            {
                _game.ChangeState(GameState.Leaderboard);
            }
            else if (currentKeyboardState.IsKeyDown(Keys.C) && !_previousKeyboardState.IsKeyDown(Keys.C))
            {
                _game.ChangeState(GameState.Customisation);
            }
            else if (currentKeyboardState.IsKeyDown(Keys.T) && !_previousKeyboardState.IsKeyDown(Keys.T))
            {
                _game.ChangeState(GameState.Tutorial);
            }

            // Validate input keys
            Keys[] validKeys = new Keys[] { Keys.A, Keys.L, Keys.C, Keys.T, Keys.Enter, Keys.Space, Keys.M, Keys.Escape };
            if (currentKeyboardState.GetPressedKeys().Length > 0 &&
                !validKeys.Contains(currentKeyboardState.GetPressedKeys()[0]))
            {
                _errorMessage = "Invalid key pressed - use A, L, C, or T";
                Console.WriteLine("Error: Invalid key pressed");
                _errorTimer = 0f;
            }

            _previousKeyboardState = currentKeyboardState;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MenuScreen Update error: {ex.Message}");
            _errorMessage = "Menu navigation error";
        }
    }

    /**
     * Renders menu interface elements
     */
    public override void Draw(SpriteBatch spriteBatch)
    {
        try
        {
            float centerX = Game.GraphicsDevice.Viewport.Width / 2;
            float pulse = (float)Math.Sin(_pulseTimer * 4) * 0.2f + 0.8f;

            // Draw title with shadow effect
            string title = "GOLF BUT SMALLER";
            Vector2 titlePos = new Vector2(centerX, 100);
            float titleScale = 2.0f;
            Vector2 titleSize = _font.MeasureString(title) * titleScale;

            spriteBatch.DrawString(_font, title,
                new Vector2(titlePos.X - titleSize.X / 2 + 4, titlePos.Y + 4),
                Color.Black * 0.5f, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);

            spriteBatch.DrawString(_font, title,
                new Vector2(titlePos.X - titleSize.X / 2, titlePos.Y),
                Color.Gold, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);

            // Draw menu options with pulse effect
            spriteBatch.DrawString(_font, "Press A to Start",
                new Vector2(centerX - 150, 250), Color.White * pulse);

            spriteBatch.DrawString(_font, "Press L to View Leaderboard",
                new Vector2(centerX - 150, 350), Color.White * pulse);

            spriteBatch.DrawString(_font, "Press C for Customisation",
                new Vector2(centerX - 150, 450), Color.White * pulse);

            spriteBatch.DrawString(_font, "Press T for Tutorial",
                new Vector2(centerX - 150, 500), Color.White * pulse);

            // Draw end message if present
            if (!string.IsNullOrEmpty(endMessage))
            {
                spriteBatch.DrawString(_font, endMessage,
                    new Vector2(centerX - 150, 600), Color.Yellow);
            }

            // Draw error message if present
            if (_errorMessage != "")
            {
                Vector2 errorSize = _font.MeasureString(_errorMessage);
                spriteBatch.DrawString(_font, _errorMessage,
                    new Vector2(centerX - errorSize.X / 2, 650),
                    Color.Red * (1 - (_errorTimer / ERROR_DISPLAY_TIME)));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MenuScreen Draw error: {ex.Message}");
            _errorMessage = "Menu display error";
        }
    }
}
