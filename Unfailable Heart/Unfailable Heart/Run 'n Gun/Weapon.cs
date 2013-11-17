using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Unfailable_Heart.Run_n_Gun
{
    class Weapon : Sprite
    {
        public Weapon(Texture2D[] textureImages, Vector2 position, Point[] frameSizes,
            int collisionOffset, Point currentFrame, Point[] sheetSizes, Vector2 speed,
            SoundEffect collisionSound, float scale, int score)
            : base(textureImages, position, frameSizes, collisionOffset, currentFrame,
            sheetSizes, speed, collisionSound, score, scale)
        {
        }

        public Weapon(Texture2D[] textureImages, Vector2 position, Point[] frameSizes,
            int collisionOffset, Point currentFrame, Point[] sheetSizes, Vector2 speed,
            SoundEffect collisionSound, float scale, int millisecondsPerFrame, int score)
            : base(textureImages, position, frameSizes, collisionOffset, currentFrame,
            sheetSizes, speed, millisecondsPerFrame, collisionSound, score)
        {
            this.scale = scale;
        }

        public override void Update(GameTime gameTime, Rectangle clientBounds,
            UnfailableHeartSpriteManager spriteManager)
        {
            if (speed.X != 0)
                position += new Vector2(speed.X - spriteManager.shiftingSpeed.X, speed.Y);

            base.Update(gameTime, clientBounds, spriteManager);
        }
    }
}
