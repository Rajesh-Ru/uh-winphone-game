using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;


//TextureIndex = 0:毒药
//TextureIndex = 1:消失状态
namespace Unfailable_Heart.Run_n_Gun
{
    class powerUp:Sprite
    {
       public enum State { available, consuming, consume };
       public State currentState = State.available;

       public enum Type { poison, bread, DOG_FOOD, LIFE };
       public Type type;

       protected int duration = 1000;
        public powerUp(Texture2D[] textureImages, Vector2 position, Point[] frameSizes,
            int collisionOffset, Point currentFrame, Point[] sheetSizes, Vector2 speed,
            SoundEffect collisionSound, int millisecondsPerFrame,Type type, int score)
            : base(textureImages, position, frameSizes, collisionOffset, currentFrame,
            sheetSizes, speed, millisecondsPerFrame, collisionSound, score)
        {
            this.type = type;
            if (type == Type.DOG_FOOD || type == Type.LIFE)
            {
                duration = 0;
            }
        }

        public powerUp(Texture2D[] textureImages, Vector2 position, Point[] frameSizes,
            int collisionOffset, Point currentFrame, Point[] sheetSizes, Vector2 speed,
            SoundEffect collisionSound, float scale, int millisecondsPerFrame,Type type,
            int score)
            : base(textureImages, position, frameSizes, collisionOffset, currentFrame,
            sheetSizes, speed, millisecondsPerFrame, collisionSound, score)
        {
            this.scale = scale;
            this.type = type;
            if (type == Type.DOG_FOOD || type == Type.LIFE)
            {
                duration = 0;
            }
        }

        public override void Update(GameTime gameTime, Rectangle clientBounds, UnfailableHeartSpriteManager spriteManager)
        {
            position += -spriteManager.shiftingSpeed;

            if (currentState == State.consuming) 
            {
                ChangeTextureAnimated(1);
                duration -= gameTime.ElapsedGameTime.Milliseconds;
                if (duration <= 0)
                    currentState = State.consume;
            }
            base.Update(gameTime, clientBounds, spriteManager);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (currentState == State.consume) return;
            //spriteBatch.Draw(textureImages[textureIndex], position, GetCollisionRect(),
            //    Color.White, 0, Vector2.Zero,
            //    scale, SpriteEffects.None, 0);
            base.Draw(gameTime, spriteBatch);
        }

        public override Rectangle GetCollisionRect()
        {
            if (this.type == Type.poison) 
            {

                return new Rectangle((int)(position.X + 0.30 * frameSizes[textureIndex].X * scale),
                    (int)(position.Y + 0.265 * frameSizes[textureIndex].Y * scale),
                    (int)((frameSizes[textureIndex].X*0.41) * scale),
                    (int)((frameSizes[textureIndex].Y*0.735) * scale));
            }
            else if (this.type == Type.bread)
            {
                return new Rectangle((int)(position.X + 0.1 * frameSizes[textureIndex].X * scale),
                        (int)(position.Y + 0.46 * frameSizes[textureIndex].Y * scale),
                        (int)((frameSizes[textureIndex].X * 0.69) * scale),
                        (int)((frameSizes[textureIndex].Y * 0.54) * scale));
            }
            else if (this.type == Type.DOG_FOOD)
            {
                return new Rectangle((int)(position.X + .25f * frameSizes[textureIndex].X * scale),
                    (int)(position.Y + .485f * frameSizes[textureIndex].Y * scale),
                    (int)((frameSizes[textureIndex].X * .726f) * scale),
                    (int)((frameSizes[textureIndex].Y * .466f) * scale));
            }
            else if (this.type == Type.LIFE)
            {
                return new Rectangle((int)(position.X + .1f * frameSizes[textureIndex].X * scale),
                    (int)(position.Y + .1f * frameSizes[textureIndex].Y * scale),
                    (int)((frameSizes[textureIndex].X * .8f) * scale),
                    (int)((frameSizes[textureIndex].Y * .8f) * scale));
            }
            return base.GetCollisionRect();
        }

        public bool IsAvailable()
        {
            return currentState == State.available;
        }

        public void Consume()
        {
            currentState = State.consuming;
        }
    }
}
