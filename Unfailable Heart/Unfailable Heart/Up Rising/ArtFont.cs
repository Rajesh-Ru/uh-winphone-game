using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Unfailable_Heart.Up_Rising
{
    class ArtFont
    {
        Level level;
        Texture2D[] textures;
        int textureIndex = 0;
        Vector2 position;
        Vector2 speed;
        bool isScaleEnabled = false;
        bool isDisposable = true;
        float scale = 1f;
        const float scaleMax = 1f;
        const int timeToLiveMilliseconds = 1000;
        int aliveTimer = 0;
        Vector2 origin;
        const int timePerTextureMilliseconds = 100;
        int textureTimer = 0;
        string name;

        public enum Status
        {
            DISPLAYING = 0,
            DONE = 1
        }

        Status currentStatus = Status.DISPLAYING;
        public Status CurrentStatus
        {
            get { return currentStatus; }
        }

        public ArtFont(Level level, string assetName, Vector2 position, Vector2 speed,
            bool isScaleEnabled, bool isDisposable)
            : this(level, assetName, position, speed)
        {
            this.isScaleEnabled = isScaleEnabled;
            this.isDisposable = isDisposable;
            if (isScaleEnabled)
                scale = .1f;
        }

        public ArtFont(Level level, string assetName, Vector2 position, Vector2 speed)
        {
            this.level = level;
            name = assetName;
            switch (assetName)
            {
                case @"Art Font\Plus One":
                case @"Art Font\Turbo":
                    textures = new Texture2D[1];
                    textures[0] = level.Content.Load<Texture2D>(assetName);
                    this.origin = new Vector2(textures[0].Width / 2.0f, textures[0].Height / 2.0f);
                    break;
                case @"Art Font\fail":
                    textures = new Texture2D[3];
                    for (int i = 0; i < textures.Length; ++i)
                        textures[i] = level.Content.Load<Texture2D>(assetName + (i + 1).ToString());
                    this.origin = new Vector2(textures[0].Width * .5f, textures[0].Height * .4f);
                    break;
            }
            this.position = position;
            this.speed = speed;
        }

        public void Update(GameTime gameTime)
        {
            position += speed;

            if (isScaleEnabled && scale < scaleMax)
                scale += .1f;

            if (isDisposable)
            {
                aliveTimer += gameTime.ElapsedGameTime.Milliseconds;
                if (aliveTimer > timeToLiveMilliseconds)
                {
                    aliveTimer = 0;
                    currentStatus = Status.DONE;
                }
            }

            textureTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (textureTimer > timePerTextureMilliseconds)
            {
                textureTimer = 0;
                textureIndex = (textureIndex + 1) % textures.Length;
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (currentStatus == Status.DISPLAYING)
            {
                switch (name)
                {
                    case @"Art Font\Plus One":
                    case @"Art Font\Turbo":
                        spriteBatch.Draw(textures[textureIndex],
                            position + new Vector2(level.BackgroundHorizontalOffset, level.BackgroundVirticalOffset),
                            null, Color.White, 0, origin, scale, SpriteEffects.None, 0);
                        break;
                    case @"Art Font\fail":
                        spriteBatch.Draw(textures[textureIndex],
                            position + origin
                            + new Vector2(level.BackgroundHorizontalOffset, level.BackgroundVirticalOffset),
                            null, Color.White, 0, origin, scale, SpriteEffects.None, 0);
                        break;
                }
            }
        }
    }
}
