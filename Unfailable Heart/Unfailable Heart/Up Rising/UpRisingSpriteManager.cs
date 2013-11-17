using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;


namespace Unfailable_Heart.Up_Rising
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class UpRisingSpriteManager : Microsoft.Xna.Framework.DrawableGameComponent
    {
        SpriteBatch spriteBatch;

        // Current level
        Level level;

        public enum Status
        {
            BEGINNING = 0,
            IN_GAME = 1,
            GAME_OVER = 2
        }

        Status gameStatus = Status.BEGINNING;
        public Status GameStatus
        {
            get { return gameStatus; }
        }

        // Game openning effect
        Veil veil;

        public UpRisingSpriteManager(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            level = new Level(Game.Services, ((Game1)Game).rnd, 0);

            // Use veil as openning
            Texture2D[] veilTextures = new Texture2D[6];
            int[] veilPerTextureTimeMilliseconds = new int[6];
            for (int i = 0; i < veilTextures.Length; ++i)
            {
                veilTextures[i] = Game.Content.Load<Texture2D>(@"Veils\Veil " + (12 - i).ToString());
                veilPerTextureTimeMilliseconds[i] = 200;
            }
            veilPerTextureTimeMilliseconds[0] = 600;
            veil = new Veil(veilTextures, veilPerTextureTimeMilliseconds);

            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            switch (gameStatus)
            {
                case Status.BEGINNING:
                    veil.Update(gameTime);

                    if (veil.TheEnd)
                    {
                        gameStatus = Status.IN_GAME;
                    }
                    break;

                case Status.IN_GAME:
                    level.Update(gameTime);

                    // Touch the screen to go back to the openning page
                    if (level.TheEnd)
                    {
                        TouchCollection touchCollection = TouchPanel.GetState();

                        foreach (TouchLocation tl in touchCollection)
                        {
                            if (tl.State == TouchLocationState.Pressed)
                            {
                                gameStatus = Status.GAME_OVER;
                                level.Dispose();
                            }
                        }
                    }
                    break;

                case Status.GAME_OVER:
                    break;
            }
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (gameStatus != Status.GAME_OVER)
            {
                spriteBatch.Begin();

                level.Draw(gameTime, spriteBatch);

                if (gameStatus == Status.BEGINNING)
                {
                    veil.Draw(gameTime, spriteBatch);
                }

                spriteBatch.End();

                base.Draw(gameTime);
            }
        }
    }
}
