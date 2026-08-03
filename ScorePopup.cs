using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System.Collections.Generic;
using System;
using System.Drawing;
using Color = Microsoft.Xna.Framework.Color;

namespace Golfbutsmaller
{
    /**
    * Manages floating score popup animations
*/
    public class ScorePopup
    {
        // Visual properties
        private Vector2 _position;
        private string _text;
        private float _alpha = 1.0f;
        private float _scale = 1.0f;
        private Color _colour;

        // Active state tracking
        public bool IsActive => _alpha > 0;

        /**
         * Initialises popup with position and text
         */
        public ScorePopup(Vector2 position, string text, Color colour)
        {
            _position = position;
            _text = text;
            _colour = colour;
        }

        /**
         * Updates popup animation state
         */
        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _alpha -= elapsed;          // Fade out
            _scale += elapsed;          // Grow larger
            _position.Y -= 60f * elapsed; // Float upwards
        }

        /**
         * Renders popup if active 
         */
        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            if (!IsActive) return;

            // Apply fade effect to colour
            Color currentColour = _colour * _alpha;

            // Centre text at position
            Vector2 origin = font.MeasureString(_text) / 2;

            spriteBatch.DrawString(font, _text, _position, currentColour, 0f, origin, _scale, SpriteEffects.None, 0f);
        }
    }
}