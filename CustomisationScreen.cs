
using Golfbutsmaller;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Linq;


using System;

/**
* Manages ball customisation screen and colour selection
* Controls visual presentation and input handling for ball options
*/
public class CustomisationScreen : GameScreen
{
    // Screen assets and state
    private Game1 _game;
    private SpriteFont _font;
    private KeyboardState _previousKeyboardState;
    private Texture2D _ballTexture;
    private float _pulseTimer = 0f;

    // Error handling
    private string _errorMessage = "";
    private float _errorTimer = 0f;
    private const float ERROR_DISPLAY_TIME = 3f;

    /**
     * Initialises customisation screen and error handling
     */
    public CustomisationScreen(Game game) : base(game)
    {
        try
        {
            _game = (Game1)game;
            Console.WriteLine("CustomisationScreen initialised");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CustomisationScreen initialisation error: {ex.Message}");
            _errorMessage = "Failed to initialise customisation";
        }
    }

    /**
     * Loads visual assets for ball customisation
     */
    public override void LoadContent(ContentManager content)
    {
        try
        {
            _font = content.Load<SpriteFont>("GameFont");
            _ballTexture = content.Load<Texture2D>("GolfBall");
            Console.WriteLine("CustomisationScreen content loaded");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CustomisationScreen LoadContent error: {ex.Message}");
            _errorMessage = "Failed to load customisation content";
        }
    }

    /**
     * Handles colour selection inputs and state updates
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

            Keys[] validKeys = new Keys[] { Keys.N, Keys.B, Keys.R, Keys.O, Keys.Escape, Keys.C };
            if (currentKeyboardState.GetPressedKeys().Length > 0 &&
                !validKeys.Contains(currentKeyboardState.GetPressedKeys()[0]))
            {
                _errorMessage = "Invalid key - use N, B, R, O, or ESC";
                Console.WriteLine("Error: Invalid key pressed in customisation");
                _errorTimer = 0f;
            }

            if (currentKeyboardState.IsKeyDown(Keys.N) && !_previousKeyboardState.IsKeyDown(Keys.N))
                GameSettings.CurrentBallColour = BallColour.Normal;
            else if (currentKeyboardState.IsKeyDown(Keys.B) && !_previousKeyboardState.IsKeyDown(Keys.B))
                GameSettings.CurrentBallColour = BallColour.Blue;
            else if (currentKeyboardState.IsKeyDown(Keys.R) && !_previousKeyboardState.IsKeyDown(Keys.R))
                GameSettings.CurrentBallColour = BallColour.Red;
            else if (currentKeyboardState.IsKeyDown(Keys.O) && !_previousKeyboardState.IsKeyDown(Keys.O))
                GameSettings.CurrentBallColour = BallColour.Orange;
            else if (currentKeyboardState.IsKeyDown(Keys.Escape) && !_previousKeyboardState.IsKeyDown(Keys.Escape))
                _game.ChangeState(GameState.Menu);

            _previousKeyboardState = currentKeyboardState;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CustomisationScreen Update error: {ex.Message}");
            _errorMessage = "Customisation update error";
        }
    }

    /**
     * Renders ball options and current selection
     */
    public override void Draw(SpriteBatch spriteBatch)
    {
        try
        {
            float centerX = Game.GraphicsDevice.Viewport.Width / 2;
            float startY = 200;
            float spacing = 50;

            spriteBatch.DrawString(_font, "Ball Customisation",
                new Vector2(centerX - 100, startY), Color.White);

            DrawBallOption(spriteBatch, "Normal (N)", Color.White, startY + spacing);
            DrawBallOption(spriteBatch, "Blue (B)", Color.RoyalBlue, startY + spacing * 2);
            DrawBallOption(spriteBatch, "Red (R)", Color.Crimson, startY + spacing * 3);
            DrawBallOption(spriteBatch, "Orange (O)", Color.Orange, startY + spacing * 4);

            spriteBatch.DrawString(_font, "Press ESC to return",
                new Vector2(centerX - 100, startY + spacing * 6), Color.Yellow);

            string current = $"Current: {GameSettings.CurrentBallColour}";
            spriteBatch.DrawString(_font, current,
                new Vector2(centerX - 100, startY + spacing * 5), Color.Green);

            if (_errorMessage != "")
            {
                Vector2 errorSize = _font.MeasureString(_errorMessage);
                spriteBatch.DrawString(_font, _errorMessage,
                    new Vector2(centerX - errorSize.X / 2, startY + spacing * 7),
                    Color.Red * (1 - (_errorTimer / ERROR_DISPLAY_TIME)));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CustomisationScreen Draw error: {ex.Message}");
            _errorMessage = "Customisation display error";
        }
    }

    /**
     * Renders individual ball colour option
     */
    private void DrawBallOption(SpriteBatch spriteBatch, string text, Color color, float y)
    {
        try
        {
            float centerX = Game.GraphicsDevice.Viewport.Width / 2;
            spriteBatch.Draw(_ballTexture,
                new Vector2(centerX - 150, y),
                null,
                color,
                0f,
                Vector2.Zero,
                0.5f,
                SpriteEffects.None,
                0f);
            spriteBatch.DrawString(_font, text, new Vector2(centerX - 100, y), color);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DrawBallOption error: {ex.Message}");
            _errorMessage = "Error drawing ball options";
        }
    }
}