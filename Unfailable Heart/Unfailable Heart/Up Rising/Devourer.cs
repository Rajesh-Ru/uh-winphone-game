using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Unfailable_Heart.Up_Rising
{
    class Devourer : Monster
    {
        readonly int bottomLimitY = Level.PreferredBackBufferHeight;
        readonly int topLimitY = Level.PreferredBackBufferHeight - 200;
        
        bool isHitPlayer = false;
        public bool IsHitPlayer
        {
            get { return isHitPlayer; }
            set { isHitPlayer = value; }
        }

        public override Rectangle CollisionRect
        {
            get
            {
                return new Rectangle((int)(position.X + .078f * frameSizes[textureIndex].X * scale),
                    (int)(position.Y + .24f * frameSizes[textureIndex].Y * scale),
                    (int)((.83f * frameSizes[textureIndex].X) * scale),
                    (int)((.57f * frameSizes[textureIndex].Y) * scale));
            }
        }

        public Devourer(Level level, Vector2 position, Vector2 speed)
        {
            this.level = level;
            this.position = position;
            this.speed = new Vector2(Math.Abs(speed.X), Math.Abs(speed.Y));
            direction = new Vector2(0, -1);
            textures = new Texture2D[2];
            textures[0] = level.Content.Load<Texture2D>(@"Enemies\Devourer 1");
            textures[1] = level.Content.Load<Texture2D>(@"Enemies\Devourer 2");
            frameSizes = new Point[2];
            frameSizes[0] = new Point(640, 630);
            frameSizes[1] = new Point(640, 630);
            currentFrame = new Point(0, 0);
            sheetSizes = new Point[2];
            sheetSizes[0] = new Point(1, 1);
            sheetSizes[1] = new Point(1, 1);
            frameDurationMilliseconds = new int[2];
            frameDurationMilliseconds[0] = frameDurationMilliseconds[1] = 200;
            scale = 1f;
            collisionSound = null;
            score = 100;
        }

        public override void Update(GameTime gameTime)
        {
            DetectStatus();

            switch (currentStatus)
            {
                case Status.SURGING:
                    Surging(gameTime);
                    break;
                case Status.FALLING:
                    Falling(gameTime);
                    break;
            }
        }

        private void Surging(GameTime gameTime)
        {
            if (level.BackgroundSpeedY >= 2 && position.Y > topLimitY
                || level.BackgroundSpeedY < 2)
                position += direction * speed;

            if (!isHitPlayer)
                textureIndex = 0;
            else
                textureIndex = 1;
        }

        private void Falling(GameTime gameTime)
        {
            if (position.Y < bottomLimitY)
                position += direction * speed;
        }

        private void DetectStatus()
        {
            if (currentStatus != Status.SURGING &&
                level.BackgroundSpeedY < level.BackgroundThresholdSpeedY)
            {
                direction = new Vector2(0, -1);
                currentStatus = Status.SURGING;
            }
            else if (currentStatus != Status.FALLING &&
                level.BackgroundSpeedY >= level.BackgroundThresholdSpeedY)
            {
                direction = new Vector2(0, 1);
                currentStatus = Status.FALLING;
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(textures[textureIndex],
                position + new Vector2(level.BackgroundHorizontalOffset, level.BackgroundVirticalOffset),
                new Rectangle(currentFrame.X * frameSizes[textureIndex].X,
                    currentFrame.Y * frameSizes[textureIndex].Y,
                    frameSizes[textureIndex].X,
                    frameSizes[textureIndex].Y),
                Color.White, 0, Vector2.Zero, 1f,
                SpriteEffects.None, 0);
        }
    }
}
