using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Unfailable_Heart.Up_Rising
{
    abstract class Ammo
    {
        protected Level level;
        protected Texture2D texture;
        protected Vector2 speed;

        protected int score;
        public int Score
        {
            get { return score; }
        }

        protected Vector2 position;
        public Vector2 Position
        {
            get { return position; }
        }

        public virtual Rectangle CollisionRect
        {
            get;
            set;
        }

        public virtual void Update(GameTime gameTime)
        {
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
        }
    }
}
