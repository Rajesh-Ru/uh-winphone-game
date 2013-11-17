using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Unfailable_Heart.Up_Rising
{
    class Vestigial
    {
        Level level;
        Texture2D texture;
        Vector2 speed;

        Vector2 position;
        public Vector2 Position
        {
            get { return position; }
        }

        public Vestigial(Level level, string assetName, Vector2 position, Vector2 speed)
        {
            this.level = level;
            texture = level.Content.Load<Texture2D>(assetName);
            this.position = position;
            this.speed = speed;
        }

        public void Update(GameTime gameTime)
        {
            position += speed;
            ++speed.Y;

            if (level.IsTurbo)
                position.Y += 30;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture,
                position + new Vector2(level.BackgroundHorizontalOffset, level.BackgroundVirticalOffset),
                Color.White);
        }
    }
}
