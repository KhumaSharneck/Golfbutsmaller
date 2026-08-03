using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
namespace Golfbutsmaller

{
    /**
    * Base class for all game screens providing core functionality
*/
    public abstract class GameScreen
    {
        // Protected game instance accessible to derived screens
        protected Game Game { get; private set; }

        /**
         * Initialises screen with game reference
         */
        public GameScreen(Game game)
        {
            Game = game;
        }

        /**
         * Virtual method for loading screen content
         * Overridden by derived screens to load specific assets
         */
        public virtual void LoadContent(ContentManager content) { }

        /**
         * Virtual update method for screen logic
         * Overridden to implement screen-specific updates
         */
        public virtual void Update(GameTime gameTime) { }

        /**
         * Virtual draw method for screen rendering
         * Overridden to implement screen-specific drawing
         */
        public virtual void Draw(SpriteBatch spriteBatch) { }
    }
}