using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using Unfailable_Heart.Run_n_Gun;
using Unfailable_Heart.Up_Rising;

namespace Unfailable_Heart
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public GraphicsDeviceManager Graphics
        {
            get { return graphics; }
        }
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        UnfailableHeartSpriteManager unfailableHeartSpriteManager;
        UpRisingSpriteManager upRisingSpriteManger;

        public Random rnd { get; private set; }
        bool onceThrough = false;
        bool isOpening = true;

        // Button related stuff
        Rectangle startBottonRect = new Rectangle(530, 200, 160, 50);
        Rectangle spaceBottonRect = new Rectangle(530, 290, 160, 50);
        Rectangle planeBottonRect = new Rectangle(530, 375, 170, 50);
        int bottonIndex = 0;
        const int bottonNumOfEachKind = 2;
        int bottonTimer = 0;
        const int bottonTimeMilliseconds = 400;
        bool isTouched = false;
        Point touchPos;

        // Title related stuff
        int titleIndex = 0;
        const int titleNum = 7;
        int titleTimer = 0;
        const int titleTimeMilliseconds = 100;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            unfailableHeartSpriteManager = new UnfailableHeartSpriteManager(this);
            Components.Add(unfailableHeartSpriteManager);
            unfailableHeartSpriteManager.Enabled = false;
            unfailableHeartSpriteManager.Visible = false;
            upRisingSpriteManger = new UpRisingSpriteManager(this);
            rnd = new Random();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Preload background, title, and bottons
            Content.Load<Texture2D>(@"Background Images\Start Page");
            for (int i = 0; i < bottonNumOfEachKind; ++i)
            {
                Content.Load<Texture2D>(@"Bottons\plane" + (i + 1).ToString());
                Content.Load<Texture2D>(@"Bottons\space" + (i + 1).ToString());
                Content.Load<Texture2D>(@"Bottons\start" + (i + 1).ToString());
            }
            for (int i = 0; i < titleNum; ++i)
            {
                Content.Load<Texture2D>(@"Background Images\Title " + (i + 1).ToString());
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (isOpening)
                UpdateOpening(gameTime);

            // Switch to another game when the first is over
            if (!onceThrough && unfailableHeartSpriteManager.TheEnd)
                InvokeTheFirstGame();

            if (onceThrough && upRisingSpriteManger.GameStatus == UpRisingSpriteManager.Status.GAME_OVER)
                InvokeTheSecondGame();

            base.Update(gameTime);
        }

        private void UpdateOpening(GameTime gameTime)
        {
            // Update the animation of title and bottons
            bottonTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (bottonTimer > bottonTimeMilliseconds)
            {
                bottonTimer -= bottonTimeMilliseconds;
                bottonIndex = (bottonIndex + 1) % bottonNumOfEachKind;
            }
            titleTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (titleTimer > titleTimeMilliseconds)
            {
                titleTimer -= titleTimeMilliseconds;
                titleIndex = (titleIndex + 1) % titleNum;
            }

            TouchCollection touchCollection = TouchPanel.GetState();
            foreach (TouchLocation tl in touchCollection)
            {
                if (tl.State == TouchLocationState.Pressed || tl.State == TouchLocationState.Moved)
                {
                    touchPos = new Point((int)tl.Position.X, (int)tl.Position.Y);
                    isTouched = true;
                    break;
                }
                else if (tl.State == TouchLocationState.Released)
                {
                    Point releasedPos = new Point((int)tl.Position.X, (int)tl.Position.Y);
                    if (startBottonRect.Contains(releasedPos) && startBottonRect.Contains(touchPos)
                        || planeBottonRect.Contains(releasedPos) && planeBottonRect.Contains(touchPos))
                    {
                        unfailableHeartSpriteManager.Enabled = true;
                        unfailableHeartSpriteManager.Visible = true;
                        isOpening = false;
                        break;
                    }
                    else if (spaceBottonRect.Contains(releasedPos) && spaceBottonRect.Contains(touchPos))
                    {
                        unfailableHeartSpriteManager.TheEnd = true;
                        isOpening = false;
                        break;
                    }
                    isTouched = false;
                }
            }
        }

        private void InvokeTheFirstGame()
        {
            Content.Unload();
            Components.Remove(unfailableHeartSpriteManager);
            unfailableHeartSpriteManager = null;
            Components.Add(upRisingSpriteManger);
            graphics.SupportedOrientations = DisplayOrientation.Portrait;
            graphics.PreferredBackBufferWidth = Level.PreferredBackBufferWidth;
            graphics.PreferredBackBufferHeight = Level.PreferredBackBufferHeight;
            graphics.ApplyChanges();
            onceThrough = true;
        }

        private void InvokeTheSecondGame()
        {
            onceThrough = false;
            isOpening = true;
            isTouched = false;
            Content.Unload();
            Components.Remove(upRisingSpriteManger);
            upRisingSpriteManger = null;
            unfailableHeartSpriteManager = new UnfailableHeartSpriteManager(this);
            Components.Add(unfailableHeartSpriteManager);
            unfailableHeartSpriteManager.Enabled = false;
            unfailableHeartSpriteManager.Visible = false;
            upRisingSpriteManger = new UpRisingSpriteManager(this);
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft;
            graphics.PreferredBackBufferWidth = Level.PreferredBackBufferHeight;
            graphics.PreferredBackBufferHeight = Level.PreferredBackBufferWidth;
            graphics.ApplyChanges();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            if (isOpening)
            {
                spriteBatch.Begin();

                // Draw background
                spriteBatch.Draw(Content.Load<Texture2D>(@"Background Images\Start Page"),
                    Vector2.Zero, Color.White);

                // Draw title
                spriteBatch.Draw(Content.Load<Texture2D>(@"Background Images\Title " + (titleIndex + 1).ToString()),
                    Vector2.Zero, Color.White);

                DrawBottons();

                spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        private void DrawBottons()
        {
            // Start botton
            if (isTouched && startBottonRect.Contains(touchPos))
            {
                spriteBatch.Draw(Content.Load<Texture2D>(@"Bottons\start3"), Vector2.Zero, Color.White);
            }
            else
            {
                spriteBatch.Draw(Content.Load<Texture2D>(@"Bottons\start" + (bottonIndex + 1).ToString()),
                    Vector2.Zero, Color.White);
            }
            // Space mode botton
            if (isTouched && spaceBottonRect.Contains(touchPos))
            {
                spriteBatch.Draw(Content.Load<Texture2D>(@"Bottons\space3"), Vector2.Zero, Color.White);
            }
            else
            {
                spriteBatch.Draw(Content.Load<Texture2D>(@"Bottons\space" + (bottonIndex + 1).ToString()),
                    Vector2.Zero, Color.White);
            }
            // Plane mode botton
            if (isTouched && planeBottonRect.Contains(touchPos))
            {
                spriteBatch.Draw(Content.Load<Texture2D>(@"Bottons\plane3"), Vector2.Zero, Color.White);
            }
            else
            {
                spriteBatch.Draw(Content.Load<Texture2D>(@"Bottons\plane" + (bottonIndex + 1).ToString()),
                    Vector2.Zero, Color.White);
            }
        }
    }
}
