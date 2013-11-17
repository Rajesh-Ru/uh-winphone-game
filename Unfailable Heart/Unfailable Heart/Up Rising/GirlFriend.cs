using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Unfailable_Heart.Up_Rising
{
    class GirlFriend
    {
        Level level;
        Vector2 position;
        Vector2 speed;
        Vector2 direction;
        Texture2D texture;

        public Rectangle CollisionRect
        {
            get
            {
                return new Rectangle((int)position.X + 20, (int)position.Y + 20,
                    texture.Width - 40, texture.Height - 40);
            }
        }

        public GirlFriend(Level level, Vector2 position, Vector2 speed)
        {
            this.level = level;
            this.position = position;
            this.speed = new Vector2(Math.Abs(speed.X), Math.Abs(speed.Y));
            direction = new Vector2(speed.X / this.speed.X, speed.Y / this.speed.Y);
            texture = level.Content.Load<Texture2D>(@"Player\Girl Friend");
        }

        public void Update(GameTime gameTime)
        {
            --speed.Y;
            if (speed.Y < 0)
                speed.Y = 0;
            
            position += direction * speed;

            if (position.X + 10 < 0 || position.X + 140 > Level.PreferredBackBufferWidth)
            {
                direction.X = -direction.X;
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, Color.White);
        }
    }
}
