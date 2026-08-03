
using Golfbutsmaller;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Linq;


using System;

/**
* Manages difficulty selection screen and AI opponent settings
* Controls visual presentation and input handling for difficulty options
*/
public class DifficultySelectScreen : GameScreen
{
    // Screen assets and state
    private Game1 _game;
    private SpriteFont _font;
    private KeyboardState _previousKeyboardState;
    private Color easyColor = Color.White;
    private Color hardColor = Color.White;
    private float pulseTimer = 0f;

    // Error handling
    private string _errorMessage = "";
    private float _errorTimer = 0f;
    private const float ERROR_DISPLAY_TIME = 3f;

    /**
     * Initialises difficulty selection screen and error handling
     */
    public DifficultySelectScreen(Game game) : base(game)
    {
        try
        {
            _game = (Game1)game;
            Console.WriteLine("DifficultySelectScreen initialised");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DifficultySelectScreen initialisation error: {ex.Message}");
            _errorMessage = "Failed to initialise difficulty select";
        }
    }

    /**
     * Loads required fonts and visual assets
     */
    public override void LoadContent(ContentManager content)
    {
        try
        {
            _font = content.Load<SpriteFont>("GameFont");
            Console.WriteLine("DifficultySelectScreen content loaded");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DifficultySelectScreen LoadContent error: {ex.Message}");
            _errorMessage = "Failed to load difficulty content";
        }
    }

    /**
     * Handles difficulty selection input and updates visual state
     */
    public override void Update(GameTime gameTime)
    {
        try
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();
            pulseTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_errorMessage != "")
            {
                _errorTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_errorTimer >= ERROR_DISPLAY_TIME)
                {
                    _errorMessage = "";
                    _errorTimer = 0f;
                }
            }

            Keys[] validKeys = new Keys[] { Keys.E, Keys.H, Keys.Escape, Keys.A };
            if (currentKeyboardState.GetPressedKeys().Length > 0 &&
                !validKeys.Contains(currentKeyboardState.GetPressedKeys()[0]))
            {
                _errorMessage = "Invalid key - use E, H, or ESC";
                Console.WriteLine("Error: Invalid key pressed in difficulty select");
                _errorTimer = 0f;
            }

            float pulse = (float)Math.Sin(pulseTimer * 4) * 0.2f + 0.8f;

            if (currentKeyboardState.IsKeyDown(Keys.E) && !_previousKeyboardState.IsKeyDown(Keys.E))
            {
                Console.WriteLine("Starting game in Easy mode");
                _game.StartGame(AIDifficulty.Easy);
            }
            else if (currentKeyboardState.IsKeyDown(Keys.H) && !_previousKeyboardState.IsKeyDown(Keys.H))
            {
                Console.WriteLine("Starting game in Hard mode");
                _game.StartGame(AIDifficulty.Hard);
            }
            else if (currentKeyboardState.IsKeyDown(Keys.Escape) && !_previousKeyboardState.IsKeyDown(Keys.Escape))
            {
                _game.ChangeState(GameState.Menu);
            }

            easyColor = currentKeyboardState.IsKeyDown(Keys.E) ? Color.Yellow * pulse : Color.White;
            hardColor = currentKeyboardState.IsKeyDown(Keys.H) ? Color.Yellow * pulse : Color.White;

            _previousKeyboardState = currentKeyboardState;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DifficultySelectScreen Update error: {ex.Message}");
            _errorMessage = "Difficulty select update error";
            _errorTimer = 0f;
        }
    }

    /**
     * Renders difficulty options and current selection state
     */
    public override void Draw(SpriteBatch spriteBatch)
    {
        try
        {
            Vector2 centerPos = new Vector2(Game.GraphicsDevice.Viewport.Width / 2, 0);

            DrawCenteredText(spriteBatch, "Select AI Difficulty", centerPos.X, 200, Color.White);
            DrawCenteredText(spriteBatch, "Press E for Easy Mode", centerPos.X, 300, easyColor);
            DrawCenteredText(spriteBatch, "Easy: AI makes more mistakes and is less accurate", centerPos.X, 350, Color.LightGray);
            DrawCenteredText(spriteBatch, "Press H for Hard Mode", centerPos.X, 450, hardColor);
            DrawCenteredText(spriteBatch, "Hard: AI plays more precisely and makes fewer mistakes", centerPos.X, 500, Color.LightGray);
            DrawCenteredText(spriteBatch, "Press ESC to return to Menu", centerPos.X, 600, Color.Yellow);

            if (_errorMessage != "")
            {
                Vector2 errorSize = _font.MeasureString(_errorMessage);
                spriteBatch.DrawString(_font, _errorMessage,
                    new Vector2(centerPos.X - errorSize.X / 2, 650),
                    Color.Red * (1 - (_errorTimer / ERROR_DISPLAY_TIME)));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DifficultySelectScreen Draw error: {ex.Message}");
            _errorMessage = "Difficulty select display error";
            _errorTimer = 0f;
        }
    }

    /**
     * Renders text centred at specified coordinates with error handling
     */
    private void DrawCenteredText(SpriteBatch spriteBatch, string text, float x, float y, Color color)
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
                spriteBatch.DrawString(_font, text, new Vector2(x - size.X / 2, y), color);
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
}