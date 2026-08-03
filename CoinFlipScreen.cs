
using Golfbutsmaller;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Linq;


using System;

/**
* Manages coin flip animation and game start sequence.
* Controls turn order determination between player and AI.
*/
public class CoinFlipScreen : GameScreen
{
    // Visual and audio assets
    private Game1 _game;
    private SpriteFont _font;
    private Texture2D _coinHeadsTexture;
    private Texture2D _coinTailsTexture;
    private SoundEffect _coinFlipSound;
    private SoundEffect _coinLandSound;

    // Animation parameters
    private float _rotation = 0f;
    private float _scale = 1f;
    private float _flipTime = 0f;
    private bool _isFlipping = false;
    private bool _resultDetermined = false;
    private bool _isHeads;
    private const float FLIP_DURATION = 2.0f;
    private float _pulseTimer = 0f;

    // Error handling
    private string _errorMessage = "";
    private float _errorTimer = 0f;
    private const float ERROR_DISPLAY_TIME = 3f;

    private KeyboardState _previousKeyboardState;

    /**
     * Initialises coin flip screen and error handling
     */
    public CoinFlipScreen(Game game) : base(game)
    {
        try
        {
            _game = (Game1)game;
            Console.WriteLine("CoinFlipScreen initialised");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CoinFlipScreen initialisation error: {ex.Message}");
            _errorMessage = "Failed to initialise coin flip";
        }
    }

    /**
     * Loads required textures and sound effects
     */
    public override void LoadContent(ContentManager content)
    {
        try
        {
            _font = content.Load<SpriteFont>("GameFont");
            _coinHeadsTexture = content.Load<Texture2D>("Heads");
            _coinTailsTexture = content.Load<Texture2D>("Tails");
            _coinFlipSound = content.Load<SoundEffect>("coin-flip");
            _coinLandSound = content.Load<SoundEffect>("coin-land");
            Console.WriteLine("CoinFlipScreen content loaded");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CoinFlipScreen LoadContent error: {ex.Message}");
            _errorMessage = "Failed to load coin flip content";
        }
    }

    /**
     * Updates animation state and handles input
     * Manages error message timing
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

            Keys[] validKeys = new Keys[] { Keys.Space, Keys.E, Keys.H };
            if (currentKeyboardState.GetPressedKeys().Length > 0 &&
                !validKeys.Contains(currentKeyboardState.GetPressedKeys()[0]))
            {
                _errorMessage = "Invalid key - use Space to flip";
                Console.WriteLine("Error: Invalid key pressed in coin flip");
                _errorTimer = 0f;
            }

            if (!_isFlipping && !_resultDetermined &&
                currentKeyboardState.IsKeyDown(Keys.Space) &&
                !_previousKeyboardState.IsKeyDown(Keys.Space))
            {
                StartFlip();
            }

            if (_isFlipping)
            {
                UpdateFlip(gameTime);
            }

            _previousKeyboardState = currentKeyboardState;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CoinFlipScreen Update error: {ex.Message}");
            _errorMessage = "Coin flip update error";
            _errorTimer = 0f;
        }
    }

    /**
     * Initialises coin flip animation and randomises result
     */
    private void StartFlip()
    {
        try
        {
            _isFlipping = true;
            _flipTime = 0f;
            _resultDetermined = false;
            _isHeads = Random.Shared.Next(2) == 0;

            if (GameSettings.SoundEnabled)
            {
                _coinFlipSound.Play();
            }
            Console.WriteLine("Coin flip started");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"StartFlip error: {ex.Message}");
            _errorMessage = "Failed to start coin flip";
            _errorTimer = 0f;
        }
    }

    /**
     * Updates flip animation parameters
     */
    private void UpdateFlip(GameTime gameTime)
    {
        try
        {
            _flipTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            _rotation += 15f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            float progress = _flipTime / FLIP_DURATION;
            _scale = 1f + (float)Math.Sin(progress * Math.PI) * 0.5f;

            if (_flipTime >= FLIP_DURATION)
            {
                CompleteFlip();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UpdateFlip error: {ex.Message}");
            _errorMessage = "Error during coin flip";
            _errorTimer = 0f;
        }
    }

    /**
     * Handles flip completion and determines first player
     */
    private void CompleteFlip()
    {
        try
        {
            _isFlipping = false;
            _resultDetermined = true;
            _rotation = 0f;
            _scale = 1f;

            if (GameSettings.SoundEnabled)
            {
                _coinLandSound.Play();
            }

            _game.gameplayScreen.SetFirstPlayer(_isHeads ? PlayerType.Human : PlayerType.AI);
            DelayedStateChange();
            Console.WriteLine($"Coin flip complete: {(_isHeads ? "Heads" : "Tails")}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CompleteFlip error: {ex.Message}");
            _errorMessage = "Error completing coin flip";
            _errorTimer = 0f;
        }
    }

    /**
     * Handles delayed transition to gameplay state
     */
    private async void DelayedStateChange()
    {
        try
        {
            await System.Threading.Tasks.Task.Delay(1500);
            _game.ChangeState(GameState.Gameplay);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DelayedStateChange error: {ex.Message}");
            _errorMessage = "Error transitioning game state";
            _errorTimer = 0f;
        }
    }

    /**
     * Renders coin flip animation and UI elements
     */
    public override void Draw(SpriteBatch spriteBatch)
    {
        try
        {
            float centerX = Game.GraphicsDevice.Viewport.Width / 2;
            float centerY = Game.GraphicsDevice.Viewport.Height / 2;
            float pulse = (float)Math.Sin(_pulseTimer * 4) * 0.2f + 0.8f;

            DrawCenteredText(spriteBatch, !_isFlipping && !_resultDetermined ?
                "Press SPACE to flip coin" : "", centerX, centerY + 100, Color.White * pulse);

            DrawCoin(spriteBatch, centerX, centerY);

            if (_resultDetermined)
            {
                DrawCenteredText(spriteBatch, $"Result: {(_isHeads ? "Heads" : "Tails")}!",
                    centerX, centerY + 100, Color.Yellow);
                DrawCenteredText(spriteBatch, $"{(_isHeads ? "Player" : "AI")} goes first!",
                    centerX, centerY + 150, Color.White);
            }

            if (_errorMessage != "")
            {
                DrawCenteredText(spriteBatch, _errorMessage, centerX, centerY + 200,
                    Color.Red * (1 - (_errorTimer / ERROR_DISPLAY_TIME)));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Draw error: {ex.Message}");
            _errorMessage = "Error displaying coin flip";
            _errorTimer = 0f;
        }
    }

    /**
     * Renders coin texture with current animation state
     */
    private void DrawCoin(SpriteBatch spriteBatch, float centerX, float centerY)
    {
        try
        {
            Texture2D currentTexture = _isHeads ? _coinHeadsTexture : _coinTailsTexture;
            Rectangle coinRect = new Rectangle(
                (int)(centerX - currentTexture.Width / 2 * _scale),
                (int)(centerY - currentTexture.Height / 2 * _scale),
                (int)(currentTexture.Width * _scale),
                (int)(currentTexture.Height * _scale)
            );

            spriteBatch.Draw(currentTexture, coinRect, null, Color.White,
                _rotation, new Vector2(currentTexture.Width / 2, currentTexture.Height / 2),
                SpriteEffects.None, 0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DrawCoin error: {ex.Message}");
            _errorMessage = "Error drawing coin";
            _errorTimer = 0f;
        }
    }

    /**
     * Renders centered text with error handling
     */
    private void DrawCenteredText(SpriteBatch spriteBatch, string text, float x, float y, Color color)
    {
        try
        {
            if (!string.IsNullOrEmpty(text))
            {
                Vector2 size = _font.MeasureString(text);
                spriteBatch.DrawString(_font, text, new Vector2(x - size.X / 2, y), color);
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