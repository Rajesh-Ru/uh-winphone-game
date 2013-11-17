using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Unfailable_Heart.Up_Rising
{
    class BloodSquirter : Monster
    {
        int residenceInterval;
        int residenceDelay = 0;

        string orientation;
        Vector2 floatingSpeed;

        int vibrateInterval = 20;
        int vibrateDelay = 0;
        Vector2 vibrateSpeed = new Vector2(0, 2);

        int screenWidth;
        int attackCounter = 0;

        public BloodSquirter(Level level, Vector2 initPosition, Vector2 initSpeed, Vector2 initDirection,
            int residenceTime, Vector2 floatingSpeed, Vector2 vibrateSpeed)
        {
            this.level = level;
            position = initPosition;
            speed = initSpeed;
            direction = initDirection;

            residenceInterval = residenceTime;

            if (initDirection.X > 0)
                orientation = "right";
            else
                orientation = "left";

            this.floatingSpeed = floatingSpeed * initDirection;
            this.vibrateSpeed = vibrateSpeed;
            screenWidth = Level.PreferredBackBufferWidth;

            LoadContent(level);

            score = 20;//
        }

        public void LoadContent(Level level)
        {
            frameDurationMilliseconds = new int[2];
            frameDurationMilliseconds[0] = 200;
            frameDurationMilliseconds[1] = 40;

            textures = new Texture2D[2];
            textures[0] = level.Content.Load<Texture2D>(@"BloodSquirter/floating");
            textures[1] = level.Content.Load<Texture2D>(@"BloodSquirter/attack");

            frameSizes = new Point[2];
            frameSizes[0] = new Point(125, 150);
            frameSizes[1] = new Point(125, 150);

            sheetSizes = new Point[2];
            sheetSizes[0] = new Point(1, 1);
            sheetSizes[1] = new Point(5, 1);
        }

        public override void Update(GameTime gameTime)
        {
            Vector2 prePos = position;//

            switch (currentStatus)
            {
                case Status.SURGING:
                    surging();
                    break;
                case Status.FLOATING:
                    floating();
                    break;
                case Status.DYING:
                    attackMelee(gameTime);
                    break;
                case Status.FALLING:
                    falling();
                    break;
                case Status.DEAD:
                    break;
                default:
                    break;
            }

            if (level.IsTurbo)//
                position.Y = prePos.Y + 30;//

            timeSinceLastFrame += gameTime.ElapsedGameTime.Milliseconds;
            if (timeSinceLastFrame > frameDurationMilliseconds[textureIndex])
            {
                timeSinceLastFrame -= frameDurationMilliseconds[textureIndex];
                currentFrame.X++;
                if (currentFrame.X >= sheetSizes[textureIndex].X)
                {
                    currentFrame.X = 0;
                    currentFrame.Y++;
                    if (currentFrame.Y >= sheetSizes[textureIndex].Y)
                        currentFrame.Y = 0;
                }
            }

            base.Update(gameTime);
        }

        private void surging()
        {
            ChangeTextureAnimated(0);

            if (speed.Y > 0)
            {
                speed.Y--;
                position += speed * direction + floatingSpeed;
            }
            else
                currentStatus = Status.FLOATING;

            if (IsOutOfBounds())
            {
                floatingSpeed *= -1;

                if (orientation == "left")
                    orientation = "right";
                else
                    orientation = "left";
                direction *= new Vector2(-1, 1);
            }
        }

        private void floating()
        {
            ChangeTextureAnimated(0);

            if (++residenceDelay > residenceInterval)
            {
                currentStatus = Status.FALLING;
                direction.Y *= -1;
                residenceDelay = 0;
            }
            else
                position += floatingSpeed;

            if (++vibrateDelay > vibrateInterval)
            {
                vibrateDelay = 0;
                vibrateSpeed *= -1;
            }
            else
                position += vibrateSpeed;


            if (IsOutOfBounds())
            {
                floatingSpeed *= -1;

                if (orientation == "left")
                    orientation = "right";
                else
                    orientation = "left";
                direction *= new Vector2(-1, 1);
            }
        }

        private void attackMelee(GameTime gameTime)
        {
            ChangeTextureAnimated(1);

            if (timeSinceLastFrame + gameTime.ElapsedGameTime.Milliseconds > frameDurationMilliseconds[textureIndex])
                attackCounter++;
            if (attackCounter >= 4)
            {
                attackCounter = 0;
                currentStatus = Status.DEAD;
            }
        }

        public void falling()
        {
            ChangeTextureAnimated(0);

            speed.Y++;
            //if (floatingSpeed.X > 0 )
            //    floatingSpeed.X += 0.25f;
            //else
            //    floatingSpeed.X -= 0.25f;

            position += speed * direction;
        }

        private bool IsOutOfBounds()
        {
            if (position.X < -90 ||
                position.X > screenWidth - 40)
                return true;
            else
                return false;
        }

        private void ChangeTextureAnimated(int newTextureIndex)
        {
            if (newTextureIndex != textureIndex)
            {
                textureIndex = newTextureIndex;
                currentFrame = new Point(0, 0);
                timeSinceLastFrame = 0;
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            SpriteEffects effect = SpriteEffects.None;
            if (orientation == "right")
                effect = SpriteEffects.FlipHorizontally;

            spriteBatch.Draw(
                textures[textureIndex], position,
                new Rectangle(currentFrame.X * frameSizes[textureIndex].X,
                    currentFrame.Y * frameSizes[textureIndex].Y,
                    frameSizes[textureIndex].X, frameSizes[textureIndex].Y),
                Color.White, 0, Vector2.Zero,
                1.0f, effect, 0);
        }

        public override Rectangle CollisionRect
        {
            get
            {
                return new Rectangle(
                    (int)( position.X + frameSizes[textureIndex].X * 0.32),
                    (int)( position.Y + frameSizes[textureIndex].Y * 0.3),
                    (int)(frameSizes[textureIndex].X * 0.4),
                    (int)(frameSizes[textureIndex].Y * 0.5));
            }
            //return new Rectangle( 40,45,50,75);
        }

        public Status GetStates()
        {
            return currentStatus;
        }

        public void SetStates(Status newState)
        {
            if (currentStatus != newState)
                currentStatus = newState;
        }

        //public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        //{
        //    spriteBatch.Draw(
        //        textures[textureIndex], position,
        //        GetCollisionRect(),
        //        Color.White, 0, Vector2.Zero,
        //        1.0f, SpriteEffects.None, 0);
        //}
    }
}
