using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Unfailable_Heart
{
    class Veil
    {
        Texture2D[] textures;
        int[] textureTimeMilliseconds;
        int currentTextureIndex = 0;
        int timer = 0;

        public bool IsRepeated
        {
            get { return isRepeated; }
            set { isRepeated = value; }
        }
        bool isRepeated = false;

        public bool TheEnd
        {
            get { return theEnd; }
        }
        bool theEnd = false;

        public Veil(Texture2D[] textures, int textureTimeMilliseconds)
        {
            this.textures = textures;
            this.textureTimeMilliseconds = new int[textures.Length];
            for (int i = 0; i < this.textureTimeMilliseconds.Length; ++i)
            {
                this.textureTimeMilliseconds[i] = textureTimeMilliseconds;
            }
        }

        public Veil(Texture2D[] textures, int[] textureTimeMilliseconds)
        {
            this.textures = textures;
            this.textureTimeMilliseconds = textureTimeMilliseconds;
        }

        public void Update(GameTime gameTime)
        {
            if (!TheEnd)
            {
                timer += gameTime.ElapsedGameTime.Milliseconds;
                if (timer > textureTimeMilliseconds[currentTextureIndex])
                {
                    if (currentTextureIndex + 1 == textures.Length && !IsRepeated)
                    {
                        theEnd = true;
                        --currentTextureIndex;
                    }
                    currentTextureIndex = (currentTextureIndex + 1) % textures.Length;
                    timer = 0;
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(textures[currentTextureIndex], Vector2.Zero, Color.White);
        }

        public void Play()
        {
            theEnd = false;
        }
    }
}
