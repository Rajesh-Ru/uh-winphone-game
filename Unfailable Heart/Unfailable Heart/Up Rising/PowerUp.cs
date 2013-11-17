using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

using Unfailable_Heart.Up_Rising;

namespace Unfailable_Heart
{
    /*
     * What constructor needs:
     *      The position;
     *      The type of power up;
     *      
     * When collised, the state of power up go to consuming
     * Check variable isConsumed, if it is true, it need to be removed
     */
    class PowerUp
    {
        public enum State { moving, floating, consuming, disappearing };
        protected State currentState;
        public State CurrentState
        {
            get { return currentState; }
            set
            {
                if (value == State.consuming)
                    currentState = value;
            }
        }

        private bool isConsumed;         // if isConsumed == true, it should be remove from screen
        public bool IsConsumed
        {
            get { return isConsumed; }
        }

        private int movingTime;         // you can change it in function intializeData           
        private int floatingTime;       
        private int consumingTime;
        private int noCollideTime;

        private int speedOfMove = 16;
        private int speedOfDisapper = 2;    
        private int speedOfUp = -4;         // 字“life+1”上升的速度

        Random rnd;

        public enum PowerUpType { Heart, DogFood }
        public PowerUpType powerUpType;
        public PowerUpType Type
        {
            get { return powerUpType; }
        }

        Texture2D[] textureImages;
        protected Point[] frameSizes;
        
        protected Vector2 speed;
        protected Vector2 position;
        public Vector2 Position
        {
            get { return position; }
        }

        protected int textureIndex;

        protected SoundEffect collisionSound;
        public SoundEffect CollisionSound
        {
            get { return collisionSound; }
        }
        private bool canCollide;
        public bool CanCollide
        {
            get { return canCollide; }
        }

        private float radius = (float)0.223;

        Level level;

        public PowerUp( Level spriteManager, Vector2 startPosition , PowerUpType powerUpType)
        {
            level = spriteManager;
            intializeData(spriteManager);
            this.isConsumed = false;
            this.position = startPosition;
            this.position.X += (Math.Abs(speed.X) / speed.X) * 70;
            this.powerUpType = powerUpType;

            switch (powerUpType)
            {
                case PowerUpType.Heart:
                    textureIndex = 0;
                    break;
                case PowerUpType.DogFood:
                    textureIndex = 2;
                    break;
                default:
                    textureIndex = 1;
                    break;
            }
        }

        private void intializeData( Level spriteManager )
        {
            textureImages = new Texture2D[3];
            textureImages[0] = spriteManager.Content.Load<Texture2D>(@"PowerUp/Heart");
            textureImages[1] = spriteManager.Content.Load<Texture2D>(@"PowerUp/LifePlus1");
            textureImages[2] = spriteManager.Content.Load<Texture2D>(@"PowerUp/DogFood");

            this.frameSizes = new Point[3];
            this.frameSizes[0] = new Point(150, 150);
            this.frameSizes[1] = new Point(150, 150);
            this.frameSizes[2] = new Point(150, 150);

            rnd = new Random();
            this.speed.Y = 0;
            this.speed.X = speedOfMove * ( (rnd.NextDouble() < 0.5)?1:(-1) );

            this.currentState = State.moving;

            this.movingTime = 1300;
            this.floatingTime = 3000;
            this.consumingTime = 1000;
            this.noCollideTime = 1000;

            this.canCollide = false;

            this.collisionSound = null;
        }
 

        public void Update(GameTime gameTime)//, Rectangle clientBounds)
        {
            Vector2 prePos = position;

            switch (currentState)
            {
                case State.moving:
                    movingToFloat( gameTime );
                    break;
                case State.floating:
                    floating( gameTime );
                    break;
                case State.disappearing:
                    position += speed;
                    speed.Y = speedOfDisapper;
                    break;
                case State.consuming:
                    consumingPowerUp( gameTime );
                    break;
            }

            if (IsOutOfBounds(new Rectangle(0,0,480, 800)))
                speed.X = -speed.X;

            if (level.IsTurbo)
                position.Y = prePos.Y + 30;
        }

        private void movingToFloat( GameTime gameTime)
        {
            movingTime -= gameTime.ElapsedGameTime.Milliseconds;
            position += speed;
            if (position.X + 42 < 0 || position.X + 109 > Level.PreferredBackBufferWidth)
            {
                speed.X = -speed.X;
            }

            if (Math.Abs(speed.X) <= 2)
                speed.X = (Math.Abs(speed.X) / speed.X) * 2;
            else
                speed.X = (Math.Abs(speed.X) / speed.X) * (Math.Abs(speed.X) - 2);
            
            if (movingTime <= 0 || speed.X == 0)
            {
                currentState = State.floating;
                speed.X = 0;
            }

            noCollideTime -= gameTime.ElapsedGameTime.Milliseconds;
            if (noCollideTime <= 0)
                canCollide = true;
        }

        private void floating(GameTime gameTime)
        {
            floatingTime -= gameTime.ElapsedGameTime.Milliseconds;
            
            if (floatingTime <= 0)
            { 
                currentState = State.disappearing;
                speed.Y = speedOfDisapper;
            }

        }

        public void consumingPowerUp(GameTime gameTime)
        {
            if (powerUpType == PowerUpType.DogFood)
            {
                isConsumed = true;
                return;
            }

            textureIndex = 1;
            consumingTime -= gameTime.ElapsedGameTime.Milliseconds;
            canCollide = false;
            speed.X = 0;
            speed.Y = speedOfUp;
            position += speed;

            if (consumingTime <= 0)
            {
                isConsumed = true;
                return;
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(textureImages[textureIndex],
               position,
               new Rectangle( 0, 0, frameSizes[textureIndex].X, 
                   frameSizes[textureIndex].Y ),
                Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
            
        }

        private Rectangle GetCollistionRect()
        {
            if (!canCollide)
                return new Rectangle(0, 0, 0, 0);

            return new Rectangle((int)(position.X + frameSizes[textureIndex].X * (0.5 - radius)),
                (int)(position.Y + frameSizes[textureIndex].Y * (0.5 - radius)),
                (int)(frameSizes[textureIndex].X * (1 - 2 * radius)),
                (int)(frameSizes[textureIndex].Y * (1 - 2 * radius)));
        }

        public Rectangle CollisionRect
        {
            get { return GetCollistionRect(); }
        }

        //public Rectangle GetCollistionRect()
        //{
        //    return new Rectangle((int)(frameSizes[textureIndex].X * (0.5 - radius)),
        //        (int)(frameSizes[textureIndex].Y * (0.5 - radius)),
        //        (int)(frameSizes[textureIndex].X * (2 * radius)),
        //        (int)(frameSizes[textureIndex].Y * (2 * radius)));
        //}

        public bool IsOutOfBounds(Rectangle boundRect)      // check if the power up is out of bounds given
        {
            if (position.X < -frameSizes[textureIndex].X*( 0.5 + radius ) ||
                position.X > boundRect.Width ||
                position.Y < -frameSizes[textureIndex].Y*( 0.5 + radius ) ||
                position.Y > boundRect.Height)
                return true;
            else
                return false;
        }
    }
}
