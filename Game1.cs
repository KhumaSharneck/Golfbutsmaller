using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using Golfbutsmaller;

namespace Golfbutsmaller
{
    public enum GameState { PlayerName, Menu, DifficultySelect, CoinFlip, Gameplay, Leaderboard, Tutorial, Customisation }

    /**
  * Main game class handling state management and screen transitions
*/
    public class Game1 : Game
    {
        // Core components
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Screen management 
        public MenuScreen menuScreen;
        public GameplayScreen gameplayScreen;
        public EnhancedLeaderboardScreen leaderboardScreen;
        private DifficultySelectScreen difficultySelectScreen;
        private PlayerNameScreen playerNameScreen;
        private CoinFlipScreen coinFlipScreen;
        private CustomisationScreen customisationScreen;
        private TutorialScreen tutorialScreen;
        private GameState currentState;

        /**
         * Initialises core game settings
         */
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();
        }

        /**
         * Initialises all game screens
         */
        protected override void Initialize()
        {
            try
            {
                menuScreen = new MenuScreen(this);
                gameplayScreen = new GameplayScreen(this);
                leaderboardScreen = new EnhancedLeaderboardScreen(this);
                difficultySelectScreen = new DifficultySelectScreen(this);
                playerNameScreen = new PlayerNameScreen(this);
                coinFlipScreen = new CoinFlipScreen(this);
                customisationScreen = new CustomisationScreen(this);
                tutorialScreen = new TutorialScreen(this);
                currentState = GameState.PlayerName;
                base.Initialize();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Initialisation error: {ex.Message}");
            }
        }

        /**
         * Loads content for all screens and sound manager
         */
        protected override void LoadContent()
        {
            try
            {
                _spriteBatch = new SpriteBatch(GraphicsDevice);
                menuScreen.LoadContent(Content);
                gameplayScreen.LoadContent(Content);
                leaderboardScreen.LoadContent(Content);
                difficultySelectScreen.LoadContent(Content);
                playerNameScreen.LoadContent(Content);
                coinFlipScreen.LoadContent(Content);
                customisationScreen.LoadContent(Content);
                tutorialScreen.LoadContent(Content);
                SoundManager.LoadContent(Content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Content loading error: {ex.Message}");
            }
        }

        /**
         * Updates current game state
         */
        protected override void Update(GameTime gameTime)
        {
            try
            {
                switch (currentState)
                {
                    case GameState.PlayerName:
                        playerNameScreen.Update(gameTime);
                        break;
                    case GameState.Menu:
                        menuScreen.Update(gameTime);
                        break;
                    case GameState.Customisation:
                        customisationScreen.Update(gameTime);
                        break;
                    case GameState.Tutorial:
                        tutorialScreen.Update(gameTime);
                        break;
                    case GameState.DifficultySelect:
                        difficultySelectScreen.Update(gameTime);
                        break;
                    case GameState.CoinFlip:
                        coinFlipScreen.Update(gameTime);
                        break;
                    case GameState.Gameplay:
                        gameplayScreen.Update(gameTime);
                        break;
                    case GameState.Leaderboard:
                        leaderboardScreen.Update(gameTime);
                        break;
                }
                base.Update(gameTime);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update error: {ex.Message}");
            }
        }

        /**
         * Renders current game state
         */
        protected override void Draw(GameTime gameTime)
        {
            try
            {
                GraphicsDevice.Clear(Color.CornflowerBlue);

                if (_spriteBatch == null)
                {
                    return;
                }

                _spriteBatch.Begin();

                switch (currentState)
                {
                    case GameState.PlayerName:
                        playerNameScreen.Draw(_spriteBatch);
                        break;
                    case GameState.Menu:
                        menuScreen.Draw(_spriteBatch);
                        break;
                    case GameState.Customisation:
                        customisationScreen.Draw(_spriteBatch);
                        break;
                    case GameState.Tutorial:
                        tutorialScreen.Draw(_spriteBatch);
                        break;
                    case GameState.DifficultySelect:
                        difficultySelectScreen.Draw(_spriteBatch);
                        break;
                    case GameState.CoinFlip:
                        coinFlipScreen.Draw(_spriteBatch);
                        break;
                    case GameState.Gameplay:
                        gameplayScreen.Draw(_spriteBatch);
                        break;
                    case GameState.Leaderboard:
                        leaderboardScreen.Draw(_spriteBatch);
                        break;
                }

                _spriteBatch.End();
                base.Draw(gameTime);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Draw error: {ex.Message}");
            }
        }

        /**
         * Handles state transitions and music changes
         */
        public void ChangeState(GameState newState)
        {
            try
            {
                currentState = newState;

                switch (newState)
                {
                    case GameState.Menu:
                    case GameState.DifficultySelect:
                    case GameState.Leaderboard:
                        SoundManager.PlayMusic("NFL");
                        break;
                    case GameState.Gameplay:
                        SoundManager.PlayMusic("background-music");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ChangeState error: {ex.Message}");
            }
        }

        /**
         * Starts new game with selected difficulty
         */
        public void StartGame(AIDifficulty difficulty)
        {
            try
            {
                gameplayScreen.SetAIDifficulty(difficulty);
                gameplayScreen.ResetGame();
                ChangeState(GameState.CoinFlip);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"StartGame error: {ex.Message}");
            }
        }

        public EnhancedLeaderboardScreen GetLeaderboardScreen()
        {
            return leaderboardScreen;
        }

        /**
         * Cleans up resources on game exit
         */
        protected override void UnloadContent()
        {
            try
            {
                SoundManager.UnloadContent();
                Content.Unload();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UnloadContent error: {ex.Message}");
            }
        }
    }
}