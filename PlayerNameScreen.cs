using Golfbutsmaller;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Linq;
using System;

/**
* Manages player name input and validation screen
*/
public class PlayerNameScreen : GameScreen
{
    // Core components
    private Game1 _game;
    private SpriteFont _font;
    private string _playerName = "";
    private KeyboardState _previousState;
    private float _pulseTimer = 0f;

    // Error handling
    private string _errorMessage = "";
    private float _errorTimer = 0f;
    private const float ERROR_DISPLAY_TIME = 3f;

    /**
     * Initialises player name screen
     */
    public PlayerNameScreen(Game game) : base(game)
    {
        try
        {
            _game = (Game1)game;
            Console.WriteLine("PlayerNameScreen initialised");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PlayerNameScreen initialisation error: {ex.Message}");
        }
    }

    /**
     * Validates player name against requirements
     */
    private bool ValidatePlayerName(string name)
    {
        // Check for empty name
        if (string.IsNullOrWhiteSpace(name))
        {
            _errorMessage = "Name cannot be empty";
            Console.WriteLine("Error: Name cannot be empty");
            return false;
        }
        // Check minimum length
        if (name.Length < 3)
        {
            _errorMessage = "Name must be at least 3 characters";
            Console.WriteLine("Error: Name must be at least 3 characters");
            return false;
        }
        // Check valid characters
        if (!name.All(c => char.IsLetterOrDigit(c)))
        {
            _errorMessage = "Name can only contain letters and numbers";
            Console.WriteLine("Error: Name can only contain letters and numbers");
            return false;
        }
        return true;
    }

    /**
     * Loads required font resources
     */
    public override void LoadContent(ContentManager content)
    {
        try
        {
            _font = content.Load<SpriteFont>("GameFont");
            Console.WriteLine("PlayerNameScreen content loaded");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PlayerNameScreen LoadContent error: {ex.Message}");
        }
    }

    /**
     * Updates name input and validation state
     */
    public override void Update(GameTime gameTime)
    {
        try
        {
            KeyboardState currentState = Keyboard.GetState();
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

            // Handle backspace
            if (currentState.IsKeyDown(Keys.Back) && !_previousState.IsKeyDown(Keys.Back) && _playerName.Length > 0)
                _playerName = _playerName.Substring(0, _playerName.Length - 1);

            // Validate and submit name
            if (currentState.IsKeyDown(Keys.Enter) && ValidatePlayerName(_playerName))
            {
                GameSettings.PlayerName = _playerName;
                _game.ChangeState(GameState.Menu);
            }

            // Handle character input
            Keys[] pressedKeys = currentState.GetPressedKeys();
            foreach (Keys key in pressedKeys)
            {
                if (_previousState.IsKeyUp(key))
                {
                    if (key >= Keys.A && key <= Keys.Z && _playerName.Length < 15)
                        _playerName += key.ToString();
                    else if (key >= Keys.D0 && key <= Keys.D9 && _playerName.Length < 15)
                        _playerName += (key - Keys.D0).ToString();
                }
            }

            _previousState = currentState;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PlayerNameScreen Update error: {ex.Message}");
        }
    }

    /**
     * Renders name input interface
     */
    public override void Draw(SpriteBatch spriteBatch)
    {
        try
        {
            float pulse = (float)Math.Sin(_pulseTimer * 4) * 0.2f + 0.8f;
            Vector2 centre = new Vector2(Game.GraphicsDevice.Viewport.Width / 2, 300);

            // Draw title
            string title = "Enter Your Name:";
            Vector2 titleSize = _font.MeasureString(title);
            spriteBatch.DrawString(_font, title,
                new Vector2(centre.X - titleSize.X / 2, centre.Y - 50), Color.White);

            // Draw input field
            string displayText = _playerName + "_";
            Vector2 textSize = _font.MeasureString(displayText);
            spriteBatch.DrawString(_font, displayText,
                new Vector2(centre.X - textSize.X / 2, centre.Y), Color.Yellow * pulse);

            // Draw instruction
            string instruction = "Press Enter to continue";
            Vector2 instructSize = _font.MeasureString(instruction);
            spriteBatch.DrawString(_font, instruction,
                new Vector2(centre.X - instructSize.X / 2, centre.Y + 50),
                _playerName.Length > 0 ? Color.White : Color.Gray);

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
            Console.WriteLine($"PlayerNameScreen Draw error: {ex.Message}");
        }
    }
}