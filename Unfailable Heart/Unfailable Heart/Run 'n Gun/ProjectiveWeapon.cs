using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Unfailable_Heart.Run_n_Gun
{
    class ProjectiveWeapon : Weapon
    {
        public ProjectiveWeapon(Texture2D[] textureImages, Vector2 position, Point[] frameSizes,
            int collisionOffset, Point currentFrame, Point[] sheetSizes, Vector2 speed,
            SoundEffect collisionSound, float scale, int score)
            : base(textureImages, position, frameSizes, collisionOffset, currentFrame,
            sheetSizes, speed, collisionSound, scale, score)
        {
        }

        public override void Update(GameTime gameTime, Rectangle clientBounds,
            UnfailableHeartSpriteManager spriteManager)
        {
            speed.Y += 1.5f;

            base.Update(gameTime, clientBounds, spriteManager);
        }
    }
}
