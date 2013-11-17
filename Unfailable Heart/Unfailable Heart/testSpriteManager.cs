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


namespace Unfailable_Heart
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class testSpriteManager : Microsoft.Xna.Framework.DrawableGameComponent
    {
        SpriteBatch spriteBatch;
        UserControlledSprite player;

        public testSpriteManager(Game game)
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

            // Load player
            Texture2D[] playerTextures = new Texture2D[5];
            playerTextures[0] = Game.Content.Load<Texture2D>(@"Test Images\bolt");
            playerTextures[1] = Game.Content.Load<Texture2D>(@"Test Images\plus");
            playerTextures[2] = Game.Content.Load<Texture2D>(@"Test Images\skullball");
            playerTextures[3] = Game.Content.Load<Texture2D>(@"Test Images\threeblades");
            playerTextures[4] = Game.Content.Load<Texture2D>(@"Test Images\threerings");
            Point[] playerFrameSizes = new Point[5];
            playerFrameSizes[0] = new Point(75, 75);
            playerFrameSizes[1] = new Point(75, 75);
            playerFrameSizes[2] = new Point(75, 75);
            playerFrameSizes[3] = new Point(75, 75);
            playerFrameSizes[4] = new Point(75, 75);
            Point[] playerSheetSizes = new Point[5];
            playerSheetSizes[0] = new Point(6, 8);
            playerSheetSizes[1] = new Point(6, 4);
            playerSheetSizes[2] = new Point(6, 8);
            playerSheetSizes[3] = new Point(6, 8);
            playerSheetSizes[4] = new Point(6, 8);
            player = new UserControlledSprite(playerTextures, new Vector2(200, 350),
                playerFrameSizes, 10, new Point(0, 0), playerSheetSizes,
                new Vector2(5, 0), null);

            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
            player.Update(gameTime, Game.Window.ClientBounds, new List<Sprite>());

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            player.Draw(gameTime, spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
