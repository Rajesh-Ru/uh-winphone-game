
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

/* 
 * Constructor needs:
 *      Level;
 *      StartPosition, under the screen will be better
 * When collised, change the currentStatus to DYING
 * FloatingPosY can control the position.Y of BigMouth stop
 */
namespace Unfailable_Heart.Up_Rising
{
    class BigMouth : Monster 
    {
        private int highestPosY = 500;              // 怪物上升的最大高度
        public int FloatingPosY
        {
            get { return highestPosY; }
            set
            {
                if (value >= 400 && value <= 600)
                    highestPosY = value;
            }
        }

        private float surgingAcceleraton = 2;       // 怪物上升的加速度
        private int surgingSpeedY = 16;             // 怪物上升的初速度

        private int floatingTime = 4000;            
        private int chargingTime = 1000;
        private int attackingTime = 1500;
        private int normalTime = 1500;              // 怪物保持正常形态的时间
        private int dyingTime = 400;

        private int blinkDelay = 100;               // 怪物眨眼延迟
        private int fallingAcceleration = 3;        // 下落的加速度
        Random rnd = new Random();

        private float collisionOffsetX = (float)0.216;  
        private float collisionOffsetY = (float)0.283;
        
        public BigMouth(Level level, Vector2 startPosition)
        {
            intializeData(level);
            this.position = startPosition;
        }

        public BigMouth(Level level, Vector2 startPosition, int highestPosY)
        {
            intializeData(level);
            this.position = startPosition;
            //this.highestPosY = highestPosY;
            this.FloatingPosY = highestPosY;
        }

        private void intializeData(Level level)
        {
            this.level = level;

            this.textures = new Texture2D[4];
            textures[0] = level.Content.Load<Texture2D>(@"Monster/BigMouth/Normal");
            textures[1] = level.Content.Load<Texture2D>(@"Monster/BigMouth/Charging");
            textures[2] = level.Content.Load<Texture2D>(@"Monster/BigMouth/Attacking");
            textures[3] = level.Content.Load<Texture2D>(@"Monster/BigMouth/Dying");

            this.textureIndex = 0;
            this.currentFrame = new Point(0, 0);

            this.frameSizes = new Point[4];
            frameSizes[0] = new Point(150, 150);
            frameSizes[1] = new Point(150, 150);
            frameSizes[2] = new Point(150, 150);
            frameSizes[3] = new Point(150, 150);

            this.sheetSizes = new Point[4];         // It seems with no use
            sheetSizes[0] = new Point(150, 150);
            sheetSizes[1] = new Point(300, 150);
            sheetSizes[2] = new Point(150, 150);
            sheetSizes[3] = new Point(150, 150);

            this.frameDurationMilliseconds = new int[4];
            frameDurationMilliseconds[0] = 400;
            frameDurationMilliseconds[1] = 400;
            frameDurationMilliseconds[2] = 400;
            frameDurationMilliseconds[3] = 400;

            this.speed.Y = surgingSpeedY;
            this.speed.X = (rnd.NextDouble() < 0.5) ? 3 : (-3);     // random direction of x coordinate axis

            scale = 1f;
            collisionSound = null;

            score = 20;
        }

        public override void Update(GameTime gameTime)
        {
            Vector2 prePos = position;

            switch (this.currentStatus)
            {
                case Status.SURGING:
                    surging( gameTime );
                    break;
                case Status.FLOATING:
                    floating(gameTime);
                    break;
                case Status.FALLING:
                    falling(gameTime);
                    break;
                case Status.ATTACKING_MELEE:
                    attacking_melee(gameTime);
                    break;
                case Status.DYING:
                    dyingAnimation(gameTime);
                    break;
                case Status.CHARGING:
                    charging(gameTime);
                    break;
            }

            if (level.IsTurbo)
                position.Y = prePos.Y + 30;
        }

        private void surging(GameTime gameTime)
        {
            position.X += (speed.X);
            position.Y += (speed.Y * direction.Y);
            if (speed.Y <= 4)   // min speed to surging
                speed.Y = 4;
            else 
                speed.Y -= surgingAcceleraton;
            // Bounce back when hitting boundary
            if (position.X + 10 < 0 || position.X + 140 > Level.PreferredBackBufferWidth)
            {
                speed.X = -speed.X;
            }

            if (position.Y <= highestPosY)
            {
                speed.Y = 0;
                currentStatus = Status.FLOATING;
                textureIndex = 0;
                timeSinceLastFrame = 0;
                return;
            }
        }

        private void floating(GameTime gameTime)
        {
            position += speed;

            // Bounce back when hitting boundary
            if (position.X + 10 < 0 || position.X + 140 > Level.PreferredBackBufferWidth)
            {
                speed.X = -speed.X;
            }

            textureIndex = 0;
            floatingTime -= gameTime.ElapsedGameTime.Milliseconds;
            if (floatingTime <= 0)
            {
                currentStatus = Status.FALLING;
                textureIndex = 0;
                direction.Y = 1;
                timeSinceLastFrame = 0;
                return;
            }

            timeSinceLastFrame += gameTime.ElapsedGameTime.Milliseconds;
            if (timeSinceLastFrame >= normalTime)
            {
                currentStatus = Status.CHARGING;
                timeSinceLastFrame = 0;
                return;
            }
        }

        private void charging(GameTime gameTime)
        {
            textureIndex = 1;
          
            if (currentFrame.X == 0)
                currentFrame.X = 1;
            else 
                currentFrame.X = 0;

            timeSinceLastFrame += gameTime.ElapsedGameTime.Milliseconds;
            if (timeSinceLastFrame >= chargingTime)
            {
                currentStatus = Status.ATTACKING_MELEE;
                timeSinceLastFrame = 0;
                currentFrame.X = 0;
                return;
            }
        }

        private void attacking_melee(GameTime gameTime)
        {
            textureIndex = 2;

            timeSinceLastFrame += gameTime.ElapsedGameTime.Milliseconds;
            if (timeSinceLastFrame >= attackingTime)
            {
                currentStatus = Status.FLOATING;
                timeSinceLastFrame = 0;
                return;
            }
        }

        private void falling(GameTime gameTime)
        {
            position.X += (speed.X);
            position.Y += (speed.Y * direction.Y);

            if (speed.Y < 0)
                speed.Y = 0;

            speed.Y += fallingAcceleration;
            if (speed.Y >= 10)
                speed.Y = 10;

        }

        private void dyingAnimation(GameTime gameTime)
        {
            textureIndex = 3;
            blinkDelay -= gameTime.ElapsedGameTime.Milliseconds;
            if (blinkDelay <= 0)
            {
                if (currentFrame.X >= 3)
                    currentFrame.X = 0;
                else
                    currentFrame.X++;
                blinkDelay = 100;
            }

            if (dyingTime < 0)
            {
                currentStatus = Status.DEAD;
                currentFrame.X = 0;
                level.VestigialList.Add(new Vestigial(level, @"Monster\BigMouth\Dead",
                position, new Vector2(-1, -5)));
                return;
            }
            dyingTime -= gameTime.ElapsedGameTime.Milliseconds;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(textures[textureIndex],
               position,
               new Rectangle(currentFrame.X * frameSizes[textureIndex].X,
                    currentFrame.Y * frameSizes[textureIndex].Y,
                    frameSizes[textureIndex].X, frameSizes[textureIndex].Y),
                Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);

        }

        //public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        //{
        //    spriteBatch.Draw(textures[textureIndex],
        //       position,
        //      GetCollisionRect(),
        //        Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
        //}



        private Rectangle GetCollisionRect()
        {
            return new Rectangle((int)(position.X + frameSizes[textureIndex].X * (currentFrame.X + collisionOffsetX)),
                (int)(position.Y + frameSizes[textureIndex].Y * (currentFrame.Y + collisionOffsetY)),
                (int)(frameSizes[textureIndex].X * (1 - 2 * collisionOffsetX)),
                (int)(frameSizes[textureIndex].Y * (1 - 2 * collisionOffsetY)));
            
        }

        public override Rectangle CollisionRect
        {
            get
            {
                return GetCollisionRect();
            }
            set
            {
                base.CollisionRect = value;
            }
        }

        public bool IsOutOfBounds(Rectangle boundRect)      // check if the power up is out of bounds given
        {
            if (position.X < -frameSizes[textureIndex].X * collisionOffsetX ||
                position.X > boundRect.Width ||
                position.Y < -frameSizes[textureIndex].Y * collisionOffsetY ||
                position.Y > boundRect.Height)
                return true;
            else
                return false;
        }
    }
}
