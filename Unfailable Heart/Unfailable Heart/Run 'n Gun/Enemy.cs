using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

//Enemy which color is red:
//            IDLE: move in its speed
//            ATTACKING: shot the player
//            DYING: change its image to dying, will change to DEAD soon
//            DEAD: clear out of the screen by manager

namespace Unfailable_Heart.Run_n_Gun
{
    class Enemy : Sprite
    {
        public enum Status { IDLE, ATTACKING, DYING, DEAD };
        public Status EnemyState
        {
            get { return currentGameState; }
        }
        Status currentGameState = Status.IDLE;
        double defensiveRange = 600.0;
        bool stateChanged = false;
        
        //for shooting
        int shotDelay = 4000;
        int shotCountdown = 0;

        //for dying to dead state change
        int dyingDelay = 1000;
        int dyingTimer = 0;

        //for restore from attacking image to idle image
        int restoreDelay = 1000;

        // Enemy Type
        public enum Type
        {
            RED = 0,
            GREEN = 1
        }
        public Type EnemyType
        {
            get { return enemyType; }
            set { enemyType = value; }
        }
        Type enemyType = Type.RED;

        Vector2 direction = new Vector2(-1, 0);
        Vector2 defaultSpeed;
        bool isReturning = false;

        public bool IsTransparent
        {
            get { return isTransparent; }
            set { isTransparent = value; }
        }
        bool isTransparent = false;
        const int transparentTimeMilliseconds = 1000;
        int transparentTimer = 0;

        public Enemy(Texture2D[] textureImages, Vector2 position, Point[] frameSizes,
            int collisionOffset, Point currentFrame, Point[] sheetSizes, Vector2 speed,
            SoundEffect collisionSound, int score)
            : base(textureImages, position, frameSizes, collisionOffset, currentFrame,
            sheetSizes, speed, collisionSound, score)
        {
            defaultSpeed = speed;
        }

        public Enemy(Texture2D[] textureImages, Vector2 position, Point[] frameSizes,
            int collisionOffset, Point currentFrame, Point[] sheetSizes, Vector2 speed,
            SoundEffect collisionSound, float scale, int score)
            : base(textureImages, position, frameSizes, collisionOffset, currentFrame,
            sheetSizes, speed, collisionSound, score, scale)
        {
            defaultSpeed = speed;
        }

        public Enemy(Texture2D[] textureImages, Vector2 position, Point[] frameSizes,
            int collisionOffset, Point currentFrame, Point[] sheetSizes, Vector2 speed,
            int millisecondsPerFrame, SoundEffect collisionSound, int score)
            : base(textureImages, position, frameSizes, collisionOffset, currentFrame,
            sheetSizes, speed, millisecondsPerFrame, collisionSound, score)
        {
            defaultSpeed = speed;
        }

        //it may attack the player when it is close enough
        //it may cast a weapon
        //it may be out of the visible world
        //it may get zapped by the player
        //refresh the state in time
        public override void Update(GameTime gameTime, Rectangle clientBounds, UnfailableHeartSpriteManager spriteManager)
        {
            if (enemyType == Type.RED)
            {
                Vector2 playerPos = spriteManager.GetPlayerPos();
                refreshStatusAccordingTo(playerPos);

                if (stateChanged == true)
                {
                    if (currentGameState == Status.IDLE)
                    {
                        //play walking movie
                        ChangeTextureAnimated(0);
                    }
                    else if (currentGameState == Status.ATTACKING)
                    {
                        //play attacking movie
                        //ChangeTextureAnimated(1);
                    }
                    else if (currentGameState == Status.DYING)
                    {
                        //play the attacked last frame
                        ChangeTextureAnimated(2);
                    }

                    stateChanged = false;
                }

                if (currentGameState == Status.ATTACKING)
                {
                    //some bullets will fly out
                    FireShots(gameTime, spriteManager, playerPos);
                    restoreToIdleImage(gameTime);
                }

                if (currentGameState == Status.DYING)
                {
                    //change to DEAD state after 2sec
                    DyingToDead(gameTime);
                }

                position += -spriteManager.shiftingSpeed;
            }
            else // Muddy Green
            {
                // Muddy green moves when it's alive
                if (currentGameState == Status.IDLE)
                {
                    if (isTransparent)
                    {
                        transparentTimer += gameTime.ElapsedGameTime.Milliseconds;
                        if (transparentTimer > transparentTimeMilliseconds)
                        {
                            transparentTimer = 0;
                            isTransparent = false;
                        }
                    }

                    position += (-spriteManager.shiftingSpeed + speed * direction);

                    if (!isReturning)
                    {
                        speed.X -= .1f;
                        if (speed.X <= 0)
                        {
                            ChangeTextureAnimated((textureIndex + 1) % 2);
                            direction = -direction;
                            isReturning = true;
                        }
                    }
                    else
                    {
                        speed.X += .1f;
                        if (speed.X > defaultSpeed.X)
                        {
                            speed = defaultSpeed;
                            isReturning = false;
                        }
                    }
                }
                else if (currentGameState == Status.DYING)
                {
                    if (stateChanged)
                    {
                        if (direction.X < 0)
                            ChangeTextureAnimated(2);
                        else
                            ChangeTextureAnimated(3);
                    }

                    // Make it static relative to the background
                    position += -spriteManager.shiftingSpeed;

                    dyingTimer += gameTime.ElapsedGameTime.Milliseconds;
                    if (dyingTimer > dyingDelay)
                    {
                        currentGameState = Status.DEAD;
                    }
                }
            }

            base.Update(gameTime, clientBounds, spriteManager);
        }

        //refresh the enemy's status according to player's position
        //change to ATTACKING status if the player is closed enough
        private void refreshStatusAccordingTo(Vector2 playerPos)
        {
            //it is not currently in dying and dead status
            if (currentGameState != Status.DYING && currentGameState != Status.DEAD)
            {
                //the enemy is at the right of the player
                if (position.X >= playerPos.X + 100)
                {
                    double distanceBetween = Math.Sqrt(
                        Math.Pow(position.X - playerPos.X, 2) +
                        Math.Pow(position.Y - playerPos.Y, 2));

                    //player is into the defensive range
                    if (distanceBetween < defensiveRange)
                    {
                        currentGameState = Status.ATTACKING;
                        stateChanged = true;
                    }
                }
                    //the enemy is at the left of the player and it is still alive
                else if (currentGameState == Status.ATTACKING)
                {
                    currentGameState = Status.IDLE;
                    stateChanged = true;
                }
            }

        }

        //bullet will fly out every shotDelay time
        private void FireShots(GameTime gameTime, UnfailableHeartSpriteManager spriteManager, Vector2 playerPos)
        {
            if (shotCountdown <= 0)
            {              
                ChangeTextureAnimated(1);
                //the bullet's speed set to be the enemy's speed*8
                Vector2 shotSpeed = - (13 * Vector2.UnitX + 20 * Vector2.UnitY);
                spriteManager.SpawnEnemyShot(position, shotSpeed);
                shotCountdown = shotDelay;
            }
            else
                shotCountdown -= gameTime.ElapsedGameTime.Milliseconds;

        }

        //restore from the attacking image to idle image
        private void restoreToIdleImage(GameTime gameTime)
        {
            if (restoreDelay >= 0)
                restoreDelay -= gameTime.ElapsedGameTime.Milliseconds;
            else
            {
                restoreDelay = 2000;
                if (currentGameState == Status.ATTACKING)
                    currentGameState = Status.IDLE;

                ChangeTextureAnimated(0);
            }
                     
        }

        //the state dying will change to dead
        private void DyingToDead(GameTime gameTime)
        {
            if (dyingDelay >= 0)
                dyingDelay -= gameTime.ElapsedGameTime.Milliseconds;
            else
                currentGameState = Status.DEAD;
        }

        //attack player using melee weapon or range weapon
        //design rectangle for the collision check of their weapon
        public Rectangle GetAttackRect()
        {
                return GetCollisionRect();
        }

        //return enemy's collision rectangle
        public override Rectangle GetCollisionRect()
        {
            return new Rectangle((int)(position.X + 0.3 * frameSizes[textureIndex].X * scale),
                                 (int)(position.Y + 0.2 * frameSizes[textureIndex].Y * scale),
                    (int)((frameSizes[textureIndex].X * 0.5) * scale),
                    (int)((frameSizes[textureIndex].Y * 0.7) * scale));
        }


        //public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        //{
        //    spriteBatch.Draw(textureImages[textureIndex], position,
        //                GetCollisionRect(),
        //                Color.White, 0, Vector2.Zero,
        //                scale, SpriteEffects.None, 0);
        //}

        //allow manager to reset its status
        public void SetStatus(Status status)
        {
            if (status != currentGameState)
            {
                currentGameState = status;
                stateChanged = true;
            }
        }

        public Status GetCurrentStatus()
        {
            return currentGameState;
        }

    }
}
