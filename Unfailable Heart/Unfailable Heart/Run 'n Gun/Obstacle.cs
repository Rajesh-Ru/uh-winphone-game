using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

/* 
	Notice: the constructor is different from the base, it add a  parameter Tpye( the obstacle type ), position: first parameter
 *  It only override the function Update and GetCollisionRect
 *  Need member int textureIndex of sprite to be protected
 *  The size of images ( frameSize )for testing: Bar.png 150*143, Peel.png 150*150, Shelf.png 150*150, all with white background    
 */

namespace Unfailable_Heart.Run_n_Gun
{
	class Obstacle : Sprite
	{
        public enum Type { BAR, SHELF, PEEL, BEAUTY };
		public Type type;
        public Vector2 speedOfLastUpdate { get; private set; }
        public bool STEPED = false;

        private double barCollisionOffsetX = 0.160;
        private double barCollisionOffsetY = 0.331;
        private double shelfCollisionOffsetX = 0.1;
        private double shelfCollisionOffsetY = 0.41;
        private double peelCollisionOffsetX = 0.376;
        private double peelCollisionOffsetY = 0.425;

		 public Obstacle(Type obstacleType, Texture2D[] textureImages, Vector2 position, Point[] frameSizes,
            int collisionOffset, Point currentFrame, Point[] sheetSizes, Vector2 speed,
            SoundEffect collisionSound, int score)
            : base(textureImages, position, frameSizes, collisionOffset, currentFrame,
            sheetSizes, speed, collisionSound, score)
        {
			this.type = obstacleType;
        }

        public Obstacle(Type obstacleType, Texture2D[] textureImages, Vector2 position, Point[] frameSizes,
            int collisionOffset, Point currentFrame, Point[] sheetSizes, Vector2 speed,
            SoundEffect collisionSound, float scale, int score)
            : base(textureImages, position, frameSizes, collisionOffset, currentFrame,
            sheetSizes, speed, collisionSound, score, scale)
        {
			this.type = obstacleType;
        }

        public Obstacle(Type obstacleType, Texture2D[] textureImages, Vector2 position, Point[] frameSizes,
            int collisionOffset, Point currentFrame, Point[] sheetSizes, Vector2 speed,
            int millisecondsPerFrame, SoundEffect collisionSound, int score)
            : base(textureImages, position, frameSizes, collisionOffset, currentFrame,
            sheetSizes, speed, millisecondsPerFrame, collisionSound, score)
        {
			this.type = obstacleType;
        }

		// the obstacles are static relative to the background
        public override void Update(GameTime gameTime, Rectangle clientBounds, UnfailableHeartSpriteManager spriteManager)
        {
            speedOfLastUpdate = speed = -spriteManager.shiftingSpeed;
            position += speed;

            base.Update(gameTime, clientBounds, spriteManager);
        }

        //public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        //{
        //    spriteBatch.Draw(textureImages[textureIndex], new Vector2(
        //        (float)((position.X + barCollisionOffsetX * frameSizes[textureIndex].X) * scale),
        //        (float)((position.Y + barCollisionOffsetY * frameSizes[textureIndex].Y) * scale)),
        //        this.GetCollisionRect(),
        //        Color.White, 0, Vector2.Zero,
        //        scale, SpriteEffects.None, 0);
        //    //spriteBatch.Draw(textureImages[textureIndex], position,
        //    //    new Rectangle((int)(currentFrame.X * frameSizes[textureIndex].X
        //    //        + 3.3f * collisionOffset * scale), (int)(currentFrame.Y * frameSizes[textureIndex].Y
        //    //        + collisionOffset * scale), (int)((frameSizes[textureIndex].X - 5f * collisionOffset)
        //    //         * scale), (int)((frameSizes[textureIndex].Y - 2 * collisionOffset) * scale)),
        //    //    Color.White, 0, Vector2.Zero,
        //    //    scale, SpriteEffects.None, 0);
        //}

		// unfinished, it need the graph to decide its collision rectangle
		public override Rectangle GetCollisionRect()
		{
			switch(type){
				case Type.BAR:
					return rectangleOfBar();
					
				case Type.SHELF:
					return rectangleOfShelf();
					
				case Type.PEEL:
                    return rectangleOfPeel();

                case Type.BEAUTY:
                    return rectangleOfBeauty();

            };
			return base .GetCollisionRect();
		}

        private Rectangle rectangleOfBar()
        {
            return new Rectangle((int)(position.X + (barCollisionOffsetX * frameSizes[textureIndex].X) * scale),
                (int)(position.Y + (barCollisionOffsetY * frameSizes[textureIndex].Y) * scale),
                (int)(((0.72) * frameSizes[textureIndex].X) * scale),
                (int)(((0.5) * frameSizes[textureIndex].Y) * scale));
        }

        private Rectangle rectangleOfShelf()
        {
            return new Rectangle((int)(position.X + (shelfCollisionOffsetX * frameSizes[textureIndex].X) * scale),
                (int)(position.Y + (shelfCollisionOffsetY * frameSizes[textureIndex].Y) * scale),
                (int)(((0.775) * frameSizes[textureIndex].X) * scale),
                (int)(((0.349) * frameSizes[textureIndex].Y) * scale));
        }

        private Rectangle rectangleOfPeel()
        {
            return new Rectangle((int)(position.X + (peelCollisionOffsetX * frameSizes[textureIndex].X) * scale),
                (int)(position.Y + (peelCollisionOffsetY * frameSizes[textureIndex].Y) * scale),
                (int)(((0.472) * frameSizes[textureIndex].X) * scale),
                (int)(((0.315) * frameSizes[textureIndex].Y) * scale));
        }

        private Rectangle rectangleOfBeauty()
        {
            return new Rectangle((int)(position.X + (0.355 * frameSizes[textureIndex].X) * scale),
                (int)(position.Y + (0.257 * frameSizes[textureIndex].Y) * scale),
                (int)(((0.263) * frameSizes[textureIndex].X) * scale),
                (int)(((0.686) * frameSizes[textureIndex].Y) * scale));
        }

        //private Rectangle rectangleOfBar()
        //{
        //    return new Rectangle((int)((barCollisionOffsetX * frameSizes[textureIndex].X) * scale),
        //        (int)((barCollisionOffsetY * frameSizes[textureIndex].Y) * scale),
        //        (int)(((0.72) * frameSizes[textureIndex].X) * scale),
        //        (int)(((0.5) * frameSizes[textureIndex].Y) * scale));
        //}

        //private Rectangle rectangleOfShelf()
        //{
        //    return new Rectangle((int)((shelfCollisionOffsetX * frameSizes[textureIndex].X) * scale),
        //        (int)((shelfCollisionOffsetY * frameSizes[textureIndex].Y) * scale),
        //        (int)(((0.755) * frameSizes[textureIndex].X) * scale),
        //        (int)(((0.349) * frameSizes[textureIndex].Y) * scale));
        //}

        //private Rectangle rectangleOfPeel()
        //{
        //    return new Rectangle((int)((peelCollisionOffsetX * frameSizes[textureIndex].X) * scale),
        //        (int)((peelCollisionOffsetY * frameSizes[textureIndex].Y) * scale),
        //        (int)(((0.472) * frameSizes[textureIndex].X) * scale),
        //        (int)(((0.315) * frameSizes[textureIndex].Y) * scale));
        //}

        public Type getType()
        {
            return this.type;
        }

	}
}
