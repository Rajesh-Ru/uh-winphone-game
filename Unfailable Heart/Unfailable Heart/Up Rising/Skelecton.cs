using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Unfailable_Heart.Up_Rising
{
    class Skelecton : Monster
    {
        // Floating related stuff
        float defaultSpeedY;
        bool isReturning = false;
        const int floatingDurationMilliseconds = 5000;
        int floatingTimer = 0;

        public override Rectangle CollisionRect
        {
            get
            {
                return new Rectangle((int)(position.X + .33f * frameSizes[textureIndex].X * scale),
                    (int)(position.Y + .33f * frameSizes[textureIndex].Y * scale),
                    (int)(.34f * frameSizes[textureIndex].X * scale),
                    (int)(.61f * frameSizes[textureIndex].Y * scale));
            }
        }

        public Skelecton(Level level, Vector2 position, Vector2 speed)
        {
            this.level = level;
            this.position = position;
            this.speed = new Vector2(Math.Abs(speed.X), Math.Abs(speed.Y));
            direction = new Vector2(speed.X / this.speed.X, speed.Y / this.speed.Y);
            textures = new Texture2D[2];
            frameSizes = new Point[2];
            sheetSizes = new Point[2];
            frameDurationMilliseconds = new int[2];
            textures[0] = level.Content.Load<Texture2D>(@"Enemies\Skelecton 1");
            frameSizes[0] = new Point(150, 150);
            sheetSizes[0] = new Point(2, 1);
            frameDurationMilliseconds[0] = 200;
            textures[1] = level.Content.Load<Texture2D>(@"Enemies\Skelecton 2");
            frameSizes[1] = new Point(150, 150);
            sheetSizes[1] = new Point(3, 1);
            frameDurationMilliseconds[1] = 200;
            currentFrame = new Point(0, 0);
            scale = 1f;
            collisionSound = null;
            score = 20;
            defaultSpeedY = level.RandomNum.Next(2, 10);
        }

        public override void Update(GameTime gameTime)
        {
            Vector2 prePos = position;

            switch (currentStatus)
            {
                case Status.FLOATING:
                    Floating(gameTime);
                    break;
                case Status.CHARGING:
                    Charging(gameTime);
                    break;
                case Status.ATTACKING_RANGE:
                    AttackingRange(gameTime);
                    break;
                case Status.DYING:
                    Dying(gameTime);
                    break;
                case Status.SURGING:
                    Surging(gameTime);
                    break;
            }

            if (level.IsTurbo)
                position.Y = prePos.Y + 30;
        }

        private void Floating(GameTime gameTime)
        {
            position += direction * speed;

            // Bounce back when hitting boundary
            if (position.X + 10 < 0 || position.X + 140 > Level.PreferredBackBufferWidth)
            {
                direction.X = -direction.X;
            }

            // Generate wave-like movement
            if (!isReturning)
            {
                --speed.Y;
                if (speed.Y <= 0)
                {
                    direction.Y = -direction.Y;
                    isReturning = true;
                }
            }
            else
            {
                ++speed.Y;
                if (speed.Y > defaultSpeedY)
                {
                    --speed.Y;
                    isReturning = false;
                }
            }

            floatingTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (floatingTimer > floatingDurationMilliseconds)
            {
                floatingTimer = 0;
                // Divert to animate attacking actions
                ChangeTextureAnimated(1);
                currentStatus = Status.CHARGING;
            }

            UpdateAnimation(gameTime);
        }

        private void Charging(GameTime gameTime)
        {
            if (currentFrame.X == 2)
                currentStatus = Status.ATTACKING_RANGE;

            UpdateAnimation(gameTime);
        }

        private void AttackingRange(GameTime gameTime)
        {
            if (direction.X < 0)
            {
                level.AmmoList.Add(new SkelectonAmmo(level, @"Weapons\Skull",
                    position, new Vector2(-5, -5)));
            }
            else
            {
                level.AmmoList.Add(new SkelectonAmmo(level, @"Weapons\Skull",
                    new Vector2(position.X + 150, position.Y), new Vector2(5, -5)));
            }

            ChangeTextureAnimated(0);
            currentStatus = Status.FLOATING;
        }

        private void Dying(GameTime gameTime)
        {
            level.VestigialList.Add(new Vestigial(level, @"Enemies\Skelecton 3",
                position, new Vector2(-1, -5)));
            level.VestigialList.Add(new Vestigial(level, @"Enemies\Skelecton 4",
                position, new Vector2(-5, -5)));
            level.VestigialList.Add(new Vestigial(level, @"Enemies\Skelecton 5",
                position, new Vector2(3, -7)));
            level.VestigialList.Add(new Vestigial(level, @"Enemies\Skelecton 6",
                position, new Vector2(5, -5)));
            level.VestigialList.Add(new Vestigial(level, @"Enemies\Skelecton 7",
                position, new Vector2(1, -5)));
            level.FontList.Add(new ArtFont(level, @"Art Font\Plus One",
                new Vector2(position.X + 50, position.Y + 20), Vector2.Zero,
                true, true));

            int r = level.RandomNum.Next(100);
            if (r < 30)
            {
                level.PowerUpsList.Add(new PowerUp(level, position, PowerUp.PowerUpType.Heart));
            }
            else if (r < 40)
            {
                level.PowerUpsList.Add(new PowerUp(level, position, PowerUp.PowerUpType.DogFood));
            }

            currentStatus = Status.DEAD;
        }

        private void Surging(GameTime gameTime)
        {
            position += direction * speed;

            // Bounce back when hitting boundary
            if (position.X + 10 < 0 || position.X + 140 > Level.PreferredBackBufferWidth)
            {
                direction.X = -direction.X;
            }

            --speed.Y;
            if (speed.Y == 0)
            {
                speed.Y = defaultSpeedY;
                currentStatus = Status.FLOATING;
            }

            UpdateAnimation(gameTime);
        }

        private void UpdateAnimation(GameTime gameTime)
        {
            timeSinceLastFrame += gameTime.ElapsedGameTime.Milliseconds;
            if (timeSinceLastFrame > frameDurationMilliseconds[textureIndex])
            {
                timeSinceLastFrame -= frameDurationMilliseconds[textureIndex];
                ++currentFrame.X;
                if (currentFrame.X >= sheetSizes[textureIndex].X)
                {
                    currentFrame.X = 0;
                    ++currentFrame.Y;
                    if (currentFrame.Y >= sheetSizes[textureIndex].Y)
                        currentFrame.Y = 0;
                }
            }
        }

        private void ChangeTextureAnimated(int textureIndex)
        {
            if (textureIndex != this.textureIndex)
            {
                this.textureIndex = textureIndex;
                currentFrame = new Point(0, 0);
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (direction.X < 0)
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
            else
            {
                spriteBatch.Draw(textures[textureIndex], position,
                    new Rectangle(currentFrame.X * frameSizes[textureIndex].X,
                        currentFrame.Y * frameSizes[textureIndex].Y,
                        frameSizes[textureIndex].X,
                        frameSizes[textureIndex].Y),
                    Color.White, 0, Vector2.Zero, 1f,
                    SpriteEffects.FlipHorizontally, 0);
            }
        }
    }
}
