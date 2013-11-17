using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Unfailable_Heart.Run_n_Gun
{
    abstract class Sprite
    {
        Texture2D[] textureImages;
        protected Point[] frameSizes;
        Point currentFrame;
        Point[] sheetSizes;
        protected Vector2 speed;
        protected Vector2 position;
        int timeSinceLastFrame = 0;
        int millisecondsPerFrame;
        protected int collisionOffset;
        const int defaultMillisecondsPerFrame = 33;
        protected float scale = 1;
        protected float originalScale = 1;
        Vector2 originalSpeed;
        protected int textureIndex = 0;
        public SoundEffect collisionSound { get; private set; }
        public int score { get; protected set; }

        public Sprite(Texture2D[] textureImages, Vector2 position, Point[] frameSizes,
            int collisionOffset, Point currentFrame, Point[] sheetSizes, Vector2 speed,
            SoundEffect collisionSound, int score)
            : this(textureImages, position, frameSizes, collisionOffset, currentFrame,
            sheetSizes, speed, defaultMillisecondsPerFrame, collisionSound, score)
        {
        }

        public Sprite(Texture2D[] textureImages, Vector2 position, Point[] frameSizes,
            int collisionOffset, Point currentFrame, Point[] sheetSizes, Vector2 speed,
            SoundEffect collisionSound,int score, float scale)
            : this(textureImages, position, frameSizes, collisionOffset, currentFrame,
            sheetSizes, speed, defaultMillisecondsPerFrame, collisionSound, score)
        {
            this.scale = scale;
        }

        public Sprite(Texture2D[] textureImages, Vector2 position, Point[] frameSizes,
            int collisionOffset, Point currentFrame, Point[] sheetSizes, Vector2 speed,
            int millisecondsPerFrame, SoundEffect collisionSound, int score)
        {
            this.textureImages = textureImages;
            this.position = position;
            this.frameSizes = frameSizes;
            this.collisionOffset = collisionOffset;
            this.currentFrame = currentFrame;
            this.sheetSizes = sheetSizes;
            this.speed = speed;
            originalSpeed = speed;
            this.millisecondsPerFrame = millisecondsPerFrame;
            this.collisionSound = collisionSound;
            this.score = score;
        }

        public virtual void Update(GameTime gameTime, Rectangle clientBounds,
            UnfailableHeartSpriteManager spriteManager)
        {
            timeSinceLastFrame += gameTime.ElapsedGameTime.Milliseconds;
            if (timeSinceLastFrame > millisecondsPerFrame)
            {
                timeSinceLastFrame -= millisecondsPerFrame;
                currentFrame.X++;
                if (currentFrame.X >= sheetSizes[textureIndex].X)
                {
                    currentFrame.X = 0;
                    currentFrame.Y++;
                    if (currentFrame.Y >= sheetSizes[textureIndex].Y)
                        currentFrame.Y = 0;
                }
            }
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(textureImages[textureIndex], position,
                new Rectangle(currentFrame.X * frameSizes[textureIndex].X,
                    currentFrame.Y * frameSizes[textureIndex].Y,
                    frameSizes[textureIndex].X, frameSizes[textureIndex].Y),
                Color.White, 0, Vector2.Zero,
                scale, SpriteEffects.None, 0);
            //spriteBatch.Draw(textureImages[textureIndex], position,
            //    new Rectangle((int)(currentFrame.X * frameSizes[textureIndex].X
            //        + 3.3f * collisionOffset * scale), (int)(currentFrame.Y * frameSizes[textureIndex].Y
            //        + collisionOffset * scale), (int)((frameSizes[textureIndex].X - 5f * collisionOffset)
            //         * scale), (int)((frameSizes[textureIndex].Y - 2 * collisionOffset) * scale)),
            //    Color.White, 0, Vector2.Zero,
            //    scale, SpriteEffects.None, 0);
        }

        public bool IsOutOfBounds(Rectangle boundRect)
        {
            if (position.X < -frameSizes[textureIndex].X ||
                position.X > boundRect.Width ||
                position.Y < -frameSizes[textureIndex].Y ||
                position.Y > boundRect.Height)
                return true;
            else
                return false;
        }

        public void ModifyScale(float modifier)
        {
            scale *= modifier;
        }

        public void ResetScale()
        {
            scale = originalScale;
        }

        public void SetSpeed(Vector2 newSpeed)
        {
            speed = newSpeed;
        }

        public void ResetSpeed()
        {
            speed = originalSpeed;
        }

        public Vector2 GetPosition()
        {
            return position;
        }

        protected void ChangeTextureAnimated(int newTextureIndex)
        {
            if (newTextureIndex != textureIndex)
            {
                textureIndex = newTextureIndex;
                currentFrame = new Point(0, 0);
                timeSinceLastFrame = 0;
            }
        }

        public virtual Rectangle GetCollisionRect()
        {
            return new Rectangle((int)(position.X + collisionOffset * scale),
                (int)(position.Y + collisionOffset * scale),
                (int)((frameSizes[textureIndex].X - 2 * collisionOffset) * scale),
                (int)((frameSizes[textureIndex].Y - 2 * collisionOffset) * scale));
        }
    }
}
