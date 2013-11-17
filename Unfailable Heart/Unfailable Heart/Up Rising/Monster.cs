using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Unfailable_Heart.Up_Rising
{
    abstract class Monster
    {
        public enum Status
        {
            SURGING = 0,
            FLOATING = 1,
            FALLING = 2,
            CHARGING = 3,
            ATTACKING_MELEE = 4,
            ATTACKING_RANGE = 5,
            DYING = 6,
            DEAD = 7
        }

        protected Level level;
        protected Texture2D[] textures;
        protected int textureIndex = 0;
        protected Point currentFrame;
        protected Point[] frameSizes;
        protected Point[] sheetSizes;
        protected int timeSinceLastFrame = 0;
        protected int[] frameDurationMilliseconds;
        protected float scale;
        protected const float originalScale = 1;
        protected Vector2 speed;
        protected Vector2 direction = new Vector2(0, -1);

        protected Vector2 position;
        public Vector2 Position
        {
            get { return position; }
        }

        protected SoundEffect collisionSound;
        public SoundEffect CollisionSound
        {
            get { return collisionSound; }
        }

        protected int score;
        public int Score
        {
            get { return score; }
        }

        protected Status currentStatus = Status.SURGING;
        public Status CurrentStatus
        {
            get { return currentStatus; }
            set
            {
                if (value == Status.DYING)
                    currentStatus = value;
            }
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
