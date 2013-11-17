using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Unfailable_Heart.Up_Rising
{
    class SkelectonAmmo : Ammo
    {
        Vector2 origin;
        const int collisionOffset = 10;
        const float degreePerRotation = MathHelper.PiOver4;
        float degreeToRotate = 0;
        bool isDirectionChanged = false;

        public override Rectangle CollisionRect
        {
            get
            {
                return new Rectangle((int)(position.X - texture.Width / 2.0f + collisionOffset),
                    (int)(position.Y - texture.Height / 2.0f + collisionOffset),
                    texture.Width - 2 * collisionOffset,
                    texture.Height - 2 * collisionOffset);
            }
        }

        public SkelectonAmmo(Level level, string assetName, Vector2 position, Vector2 speed)
        {
            this.level = level;
            this.position = position;
            this.speed = speed;
            texture = level.Content.Load<Texture2D>(assetName);
            origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
            score = 10;
        }

        public override void Update(GameTime gameTime)
        {
            position += speed;

            if (level.IsTurbo)
                position.Y += 30;

            // Bouncing back and forth
            if (!isDirectionChanged
                && (position.X - 25 < 0 || position.X + 25 > Level.PreferredBackBufferWidth))
            {
                speed.X = -speed.X;
                isDirectionChanged = true;
            }
            else if (isDirectionChanged
                && position.X  - 30 > 0 && position.X + 30 < Level.PreferredBackBufferWidth)
            {
                isDirectionChanged = false;
            }

            // Rotate the skull
            degreeToRotate = (degreeToRotate + degreePerRotation) % (2 * MathHelper.Pi);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture,
                position + new Vector2(level.BackgroundHorizontalOffset, level.BackgroundVirticalOffset),
                null, Color.White, degreeToRotate, origin, 1f, SpriteEffects.None, 0);
        }
    }
}
