using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Unfailable_Heart.Up_Rising
{
    class Octopuscs : Monster
    {
        // Floating related stuff
        const float defaultSpeedY = 5;
        bool isReturning = false;
        const int floatingDurationMilliseconds = 5000;
        int floatingTimer = 0;

        public override Rectangle CollisionRect
        {
            get
            {
                return new Rectangle((int)(position.X + .23f * frameSizes[textureIndex].X * scale),
                    (int)(position.Y + .33f * frameSizes[textureIndex].Y * scale),
                    (int)((.47f * frameSizes[textureIndex].X) * scale),
                    (int)((.37f * frameSizes[textureIndex].Y) * scale));
            }
        }

        public Octopuscs(Level level, Vector2 position, Vector2 speed)
        {
            this.level = level;
            this.position = position;
            this.speed = new Vector2(Math.Abs(speed.X), Math.Abs(speed.Y));
            direction = new Vector2(speed.X / this.speed.X, speed.Y / this.speed.Y);
            textures = new Texture2D[1];
            textures[0] = level.Content.Load<Texture2D>(@"Enemies\Octopus 1");
            frameSizes = new Point[1];
            frameSizes[0] = new Point(150, 150);
            currentFrame = new Point(0, 0);
            sheetSizes = new Point[1];
            sheetSizes[0] = new Point(2, 1);
            frameDurationMilliseconds = new int[1];
            frameDurationMilliseconds[0] = 200;
            scale = 1f;
            collisionSound = null;
            score = 10;
        }

        public override void Update(GameTime gameTime)
        {
            Vector2 prePos = position;

            switch (currentStatus)
            {
                case Status.SURGING:
                    Surging(gameTime);
                    break;
                case Status.FLOATING:
                    Floating(gameTime);
                    break;
                case Status.DYING:
                    Dying(gameTime);
                    break;
                case Status.FALLING:
                    Falling(gameTime);
                    break;
            }

            if (level.IsTurbo)
                position.Y = prePos.Y + 30;
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
                speed.Y = 0;
                direction.Y = 1;
                currentStatus = Status.FALLING;
            }

            UpdateAnimation(gameTime);
        }

        private void Dying(GameTime gameTime)
        {
            // Spawn vestigial
            level.VestigialList.Add(new Vestigial(level, @"Enemies\Octopus 2",
                new Vector2(position.X, position.Y + 10),
                new Vector2(-5, -5)));
            level.VestigialList.Add(new Vestigial(level, @"Enemies\Octopus 3",
                new Vector2(position.X + 60, position.Y + 10),
                new Vector2(5, -5)));
            level.FontList.Add(new ArtFont(level, @"Art Font\Plus One",
                new Vector2(position.X + 50, position.Y + 20), Vector2.Zero,
                true, true));

            currentStatus = Status.DEAD;
        }

        private void Falling(GameTime gameTime)
        {
            ++speed.Y;
            position += direction * speed;

            // Bounce back when hitting boundary
            if (position.X + 10 < 0 || position.X + 140 > Level.PreferredBackBufferWidth)
            {
                direction.X = -direction.X;
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
