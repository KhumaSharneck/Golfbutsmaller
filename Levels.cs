using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Golfbutsmaller
{
    /**
 * Manages level configuration and obstacle definitions
*/
    public static class LevelManager
    {
        public static int LevelCount => levels.Count;

        // Level definitions with increasing complexity
        private static List<Level> levels = new List<Level>
   {
       // Level 1: Basic introduction level
       new Level
       {
           Walls = new List<Rectangle>
           {
               new Rectangle(50, 100, 1180, 20),  // Top wall
               new Rectangle(50, 600, 1180, 20),  // Bottom wall
               new Rectangle(50, 100, 20, 520),   // Left wall
               new Rectangle(1210, 100, 20, 520)  // Right wall
           },
           BackgroundColour = Color.DarkCyan,
           RotatingObstacles = new List<RotatingObstacle>(),
           RiverForce = Vector2.Zero
       },
       
       // Level 2: Static square obstacle introduction
       new Level
       {
           Walls = new List<Rectangle>
           {
               new Rectangle(50, 100, 1180, 20),  // Top wall
               new Rectangle(50, 600, 1180, 20),  // Bottom wall
               new Rectangle(50, 100, 20, 520),   // Left wall
               new Rectangle(1210, 100, 20, 520)  // Right wall
           },
           BackgroundColour = Color.Coral,
           RotatingObstacles = new List<RotatingObstacle>
           {
               new RotatingObstacle
               {
                   Position = new Vector2(640, 350),
                   Size = new Vector2(160, 160),
                   RotationSpeed = 0f,
               }
           },
           RiverForce = Vector2.Zero
       },

       // Level 3: Dynamic rotating obstacle
       new Level
       {
           Walls = new List<Rectangle>
           {
               new Rectangle(50, 100, 1180, 20),  // Top wall
               new Rectangle(50, 600, 1180, 20),  // Bottom wall
               new Rectangle(50, 100, 20, 520),   // Left wall
               new Rectangle(1210, 100, 20, 520)  // Right wall
           },
           BackgroundColour = Color.Purple,
           RotatingObstacles = new List<RotatingObstacle>
           {
               new RotatingObstacle
               {
                   Position = new Vector2(640, 350),
                   Size = new Vector2(330, 110),
                   RotationSpeed = MathHelper.ToRadians(30)
               }
           },
           RiverForce = Vector2.Zero
       },

       // Level 4: River effect challenge
       new Level
       {
           Walls = new List<Rectangle>
           {
               new Rectangle(50, 100, 1180, 20),  // Top wall
               new Rectangle(50, 600, 1180, 20),  // Bottom wall
               new Rectangle(50, 100, 20, 520),   // Left wall
               new Rectangle(1210, 100, 20, 520)  // Right wall
           },
           BackgroundColour = Color.LightGreen,
           RotatingObstacles = new List<RotatingObstacle>(),
           RiverForce = new Vector2(0, -100)  // Upward river current
       }
   };

        /**
         * Returns level configuration for specified index
         */
        public static Level GetLevel(int index) => levels[index];
    }

    /**
    * Defines level properties and elements
*/
    public class Level
    {
        // Level boundaries and obstacles
        public List<Rectangle> Walls { get; set; } = new List<Rectangle>();
        public List<RotatingObstacle> RotatingObstacles { get; set; } = new List<RotatingObstacle>();

        // Visual and physics properties
        public Color BackgroundColour { get; set; } = Color.LightBlue;
        public Vector2 RiverForce { get; set; } = Vector2.Zero;
    }

    /**
    * Manages rotating obstacle behaviour and rendering
*/
    public class RotatingObstacle
    {
        // Obstacle properties
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public float Rotation { get; set; } = 0f;
        public float RotationSpeed { get; set; } = 0f;

        /**
         * Updates obstacle rotation based on time
         */
        public void Update(GameTime gameTime)
        {
            Rotation += RotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        /**
         * Renders obstacle with current rotation
         */
        public void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            spriteBatch.Draw(
                texture,
                Position,
                null,
                Color.White,
                Rotation,
                new Vector2(texture.Width / 2f, texture.Height / 2f),
                Size / new Vector2(texture.Width, texture.Height),
                SpriteEffects.None,
                0f
            );
        }
    }
}