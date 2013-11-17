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

using System.Diagnostics;


namespace Unfailable_Heart.Run_n_Gun
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class UnfailableHeartSpriteManager : Microsoft.Xna.Framework.DrawableGameComponent
    {
        SpriteBatch spriteBatch;
        Texture2D[] backgroundTextureList;
        int currentBackgroundTextureIndex;
        const int BACKGROUND_IMAGE_NUM = 2;
        Vector2 windowPosition;
        public Vector2 shiftingSpeed;
        const float MAX_SHIFTING_SPEED = 15;
        const int PHASE_ONE_SPEED_INCREASE_INTERVAL = 500;
        int backgroundSpeedTimer = 0;
        float nextTextureLength = 0;
        const float screenWidth = 800;
        const float screenHeight = 480;
        Rectangle screenBound = new Rectangle(0, 0, (int)screenWidth, (int)screenHeight);
        // Pattern spawn time stuff in Milliseconds
        int nextSpawnTime;
        int minSpawnTime = 4000;
        int maxSpawnTime = 6000;
        // Pattern is a prearrangement of enemies, obstacles, and power-ups
        const int PATTERN_NUM = 10;
        List<Sprite> livesList;
        int maxNumOfLives = 5;
        int maxShotNum = 3;
        SpriteFont scoreFont;
        int score = 0;
        int currentSpeedLevel = 0;
        int currentPatternNum;

        UserControlledSprite player;
        List<Sprite> shotList;
        List<Sprite> spriteList; // Enemies, obstacles, etc.
        List<Sprite> shotIndicationList;

        //SoundEffect trackSound;
        //SoundEffectInstance trackSoundInstance;
        Song trackSong;

        // Game state
        public enum Status
        {
            BEGINNING = 0,
            IN_GAME = 1,
            GAME_OVER = 2
        }
        public Status GameStatus
        {
            get { return gameStatus; }
            set
            {
                if (value == Status.IN_GAME)
                    gameStatus = value;
            }
        }
        Status gameStatus = Status.BEGINNING;
        
        // Ending animation
        Veil veil;

        public bool TheEnd
        {
            get { return theEnd; }
            set { theEnd = value; }
        }
        bool theEnd = false;

        // Turbo mode related stuff
        readonly Vector2 turboSpeed = new Vector2(40, 0);
        const int turboTimeMilliseconds = 3000;
        int turboTimer = 0;
        Vector2 preShiftingSpeed;
        bool onceThrough = false;

        public UnfailableHeartSpriteManager(Game game)
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
            backgroundTextureList = new Texture2D[BACKGROUND_IMAGE_NUM];
            currentBackgroundTextureIndex = 0;
            windowPosition = Vector2.Zero;
            shiftingSpeed = new Vector2(0, 0);
            shotList = new List<Sprite>();
            spriteList = new List<Sprite>();
            livesList = new List<Sprite>();
            shotIndicationList = new List<Sprite>();
            nextSpawnTime = ((Game1)Game).rnd.Next(minSpawnTime, maxSpawnTime);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Get a proprietory spriteBatch
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);

            // Load background images
            backgroundTextureList[0] = Game.Content.Load<Texture2D>(@"Background Images\Background 1");
            backgroundTextureList[1] = Game.Content.Load<Texture2D>(@"Background Images\Background 2");

            // Load player, enemies, obstacles, and so forth
            // Load player
            Texture2D[] playerTextures = new Texture2D[8];
            playerTextures[0] = Game.Content.Load<Texture2D>(@"Player\run");
            playerTextures[1] = Game.Content.Load<Texture2D>(@"Player\run");
            playerTextures[2] = Game.Content.Load<Texture2D>(@"Player\charge");
            playerTextures[3] = Game.Content.Load<Texture2D>(@"Player\jump");
            playerTextures[4] = Game.Content.Load<Texture2D>(@"Player\spit");
            playerTextures[5] = Game.Content.Load<Texture2D>(@"Player\beaten");
            playerTextures[6] = Game.Content.Load<Texture2D>(@"Player\fallover");
            playerTextures[7] = Game.Content.Load<Texture2D>(@"Player\turbo");
            Point[] playerFrameSizes = new Point[8];
            playerFrameSizes[0] = new Point(150, 150);
            playerFrameSizes[1] = new Point(150, 150);
            playerFrameSizes[2] = new Point(150, 150);
            playerFrameSizes[3] = new Point(150, 150);
            playerFrameSizes[4] = new Point(150, 150);
            playerFrameSizes[5] = new Point(150, 150);
            playerFrameSizes[6] = new Point(150, 150);
            playerFrameSizes[7] = new Point(250, 250);
            Point[] playerSheetSizes = new Point[8];
            playerSheetSizes[0] = new Point(3, 1);
            playerSheetSizes[1] = new Point(3, 1);
            playerSheetSizes[2] = new Point(1, 1);
            playerSheetSizes[3] = new Point(1, 1);
            playerSheetSizes[4] = new Point(1, 1);
            playerSheetSizes[5] = new Point(1, 1);
            playerSheetSizes[6] = new Point(1, 1);
            playerSheetSizes[7] = new Point(2, 1);
            SoundEffect playerSoundEffect = Game.Content.Load<SoundEffect>(@"Music\jump");
            player = new UserControlledSprite(playerTextures,
                new Vector2(200, UserControlledSprite.groundLineY),
                playerFrameSizes, 20, new Point(0, 0),
                playerSheetSizes, Vector2.Zero, playerSoundEffect, 133);

            // Pre-load textures for later usage
            Game.Content.Load<Texture2D>(@"Obstacles\bracket");
            Game.Content.Load<Texture2D>(@"Obstacles\peel");
            Game.Content.Load<Texture2D>(@"Obstacles\beauty");
            Game.Content.Load<Texture2D>(@"PowerUps\bane available");
            Game.Content.Load<Texture2D>(@"PowerUps\bane consumed");
            Game.Content.Load<Texture2D>(@"PowerUps\bread available");
            Game.Content.Load<Texture2D>(@"PowerUps\bread consumed");
            Game.Content.Load<Texture2D>(@"PowerUps\Heart");
            Game.Content.Load<Texture2D>(@"PowerUps\Dog Food 1");
            Game.Content.Load<Texture2D>(@"PowerUps\Dog Food 2");
            Game.Content.Load<Texture2D>(@"Weapons\food vestigial");
            Game.Content.Load<Texture2D>(@"Enemy Weapons\mud");
            Game.Content.Load<Texture2D>(@"Enemies/muddy red idle");
            Game.Content.Load<Texture2D>(@"Enemies/muddy red dying");
            Game.Content.Load<Texture2D>(@"Enemies/muddy red attacking");
            Game.Content.Load<SoundEffect>(@"Music\bane hurt");
            Game.Content.Load<SoundEffect>(@"Music\burp");
            Game.Content.Load<SoundEffect>(@"Music\eat");
            Game.Content.Load<SoundEffect>(@"Music\hit");
            Game.Content.Load<SoundEffect>(@"Music\seduce");
            Game.Content.Load<SoundEffect>(@"Music\slip");
            Game.Content.Load<SoundEffect>(@"Music\dog food");
            Game.Content.Load<SoundEffect>(@"Music\heart");
            for (int i = 0; i < 4; ++i)
            {
                Game.Content.Load<Texture2D>(@"Enemies\Muddy Green " + (i + 1).ToString());
            }

            // Load veil
            Texture2D[] veilTextures = new Texture2D[6];
            for (int i = 0; i < veilTextures.Length; ++i)
            {
                veilTextures[i] = Game.Content.Load<Texture2D>(@"Veils\Veil " + (i + 1).ToString());
            }
            veil = new Veil(veilTextures, 200);

            // Load font for score display
            scoreFont = Game.Content.Load<SpriteFont>(@"Fonts\score");

            // Load maxNumOfLives of lives into livesList
            Texture2D[] lifeTextures = new Texture2D[1];
            lifeTextures[0] = Game.Content.Load<Texture2D>(@"PowerUps\Heart");
            Point[] lifeFrameSizes = new Point[1];
            lifeFrameSizes[0] = new Point(75, 75);
            Point[] lifeSheetSizes = new Point[1];
            lifeSheetSizes[0] = new Point(1, 1);

            // Initial number of lives is three
            for (int i = 0; i < 3; i++)
            {
                livesList.Add(new Weapon(lifeTextures, new Vector2(
                    i * 50 + 10, 10), lifeFrameSizes, 10, new Point(0, 0),
                    lifeSheetSizes, Vector2.Zero, null, .6f, 0));
            }

            //trackSound = Game.Content.Load<SoundEffect>(@"Music\background music");
            //trackSoundInstance = trackSound.CreateInstance();
            //trackSoundInstance.IsLooped = true;
            //trackSoundInstance.Play();
            trackSong = Game.Content.Load<Song>(@"Music\town");
            MediaPlayer.IsRepeating = true;

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
                    // start playing background music
                    MediaPlayer.Play(trackSong);
                    // Flush accrued gestures
                    while (TouchPanel.IsGestureAvailable)
                    {
                        TouchPanel.ReadGesture();
                    }
                    gameStatus = Status.IN_GAME;
                    break;

                case Status.IN_GAME:
                    if (player.status != UserControlledSprite.Status.TURBO)
                    {
                        AdjustDifficulty(gameTime);
                        // Update score
                        score += 2;
                    }
                    else
                    {
                        // When the player just enters turbo mode
                        if (!onceThrough)
                        {
                            preShiftingSpeed = shiftingSpeed;
                            shiftingSpeed = turboSpeed;
                            onceThrough = true;
                        }

                        turboTimer += gameTime.ElapsedGameTime.Milliseconds;
                        if (turboTimer > turboTimeMilliseconds)
                        {
                            --shiftingSpeed.X;
                            if (shiftingSpeed == preShiftingSpeed)
                            {
                                turboTimer = 0;
                                // Flush gestures captured in turbo mode
                                while (TouchPanel.IsGestureAvailable)
                                {
                                    TouchPanel.ReadGesture();
                                }
                                player.ChangeStatus(UserControlledSprite.Status.FORWARDING);
                                onceThrough = false;
                            }
                        }

                        // Update score
                        score += 10;
                    }

                    // Change background window position to scroll the background
                    windowPosition.X += shiftingSpeed.X * gameTime.ElapsedGameTime.Milliseconds / 33;
                    if (windowPosition.X + screenWidth >
                        backgroundTextureList[currentBackgroundTextureIndex].Width)
                    {
                        if (windowPosition.X >=
                            backgroundTextureList[currentBackgroundTextureIndex].Width)
                        {
                            windowPosition.X %= backgroundTextureList[currentBackgroundTextureIndex].Width;
                            currentBackgroundTextureIndex++;
                            currentBackgroundTextureIndex %= BACKGROUND_IMAGE_NUM;
                            nextTextureLength = 0;
                        }
                        else
                        {
                            nextTextureLength = windowPosition.X
                                + screenWidth
                                - backgroundTextureList[currentBackgroundTextureIndex].Width;
                        }
                    }

                    // Spawn Enemy only when player is not in turbo mode
                    if (player.status != UserControlledSprite.Status.TURBO)
                    {
                        // Spawn patterns. Pattern = Enemies + Obstacles + Power-ups.
                        nextSpawnTime -= gameTime.ElapsedGameTime.Milliseconds;
                        if (nextSpawnTime < 0)
                        {
                            SpawnPattern();
                            ResetSpawnTime();
                        }
                    }

                    // Updates sprites, checks for collisions, and responds to collisions
                    UpdateSprite(gameTime);

                    if (score > 30000)
                    {
                        gameStatus = Status.GAME_OVER;
                    }
                    break;

                case Status.GAME_OVER:
                    // Lose focus gradually
                    veil.Update(gameTime);
                    // Capture the end of this game
                    theEnd = veil.TheEnd && MediaPlayer.Volume < .001f;
                    // Lower background music gradually
                    MediaPlayer.Volume /= 1.1f;
                    if (MediaPlayer.Volume < .001f)
                    {
                        MediaPlayer.Stop();
                    }
                    break;
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            // Draw background
            spriteBatch.Draw(backgroundTextureList[currentBackgroundTextureIndex],
                Vector2.Zero,
                new Rectangle((int)windowPosition.X, (int)windowPosition.Y,
                    (int)(screenWidth - nextTextureLength),
                    backgroundTextureList[currentBackgroundTextureIndex].Height
                    - (int)windowPosition.Y),
                Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            

            spriteBatch.Draw(backgroundTextureList[(currentBackgroundTextureIndex + 1)
                % BACKGROUND_IMAGE_NUM],
                new Vector2(screenWidth - nextTextureLength, windowPosition.Y),
                new Rectangle(0, 0, (int)nextTextureLength,
                    backgroundTextureList[currentBackgroundTextureIndex].Height),
                Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

            // Draw other sprites
            foreach (Sprite s in spriteList)
                s.Draw(gameTime, spriteBatch);

            // Draw shots
            foreach (Sprite s in shotList)
                s.Draw(gameTime, spriteBatch);

            // Draw player
            player.Draw(gameTime, spriteBatch);

            // Draw vitality
            foreach (Sprite s in livesList)
                s.Draw(gameTime, spriteBatch);

            // Draw shot-indications
            foreach (Sprite s in shotIndicationList)
                s.Draw(gameTime, spriteBatch);

            // Draw score
            spriteBatch.DrawString(scoreFont, "SCORE: " + score,
                new Vector2(screenWidth / 2 - 70, 15), Color.Gold, 0,
                Vector2.Zero, 1.4f, SpriteEffects.None, 0);

            if (gameStatus == Status.GAME_OVER)
            {
                veil.Draw(gameTime, spriteBatch);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void AdjustDifficulty(GameTime gameTime)
        {
            // Update background scrolling speed
            if (shiftingSpeed.X < MAX_SHIFTING_SPEED)
            {
                if (shiftingSpeed.X < 5)
                {
                    backgroundSpeedTimer += gameTime.ElapsedGameTime.Milliseconds;
                    if (backgroundSpeedTimer > PHASE_ONE_SPEED_INCREASE_INTERVAL)
                    {
                        backgroundSpeedTimer -= PHASE_ONE_SPEED_INCREASE_INTERVAL;
                        shiftingSpeed.X++;
                    }
                }
                else if (shiftingSpeed.X < 10)
                {
                    if ((score - 5000) / 2000 > currentSpeedLevel)
                    {
                        currentSpeedLevel++;
                        shiftingSpeed.X++;
                    }
                }
                else
                {
                    if ((score - 10000) / 2000 > currentSpeedLevel)
                    {
                        currentSpeedLevel++;
                        shiftingSpeed.X++;
                    }
                }
            }
        }

        // Spawn a player's shot
        public void SpawnShot(Vector2 playerPos, Vector2 shotSpeed)
        {
            SoundEffect burpSound = Game.Content.Load<SoundEffect>(@"Music\burp");
            burpSound.Play();

            Texture2D[] shotTextures = new Texture2D[1];
            shotTextures[0] = Game.Content.Load<Texture2D>(@"Weapons\food vestigial");
            Point[] shotFrameSizes = new Point[1];
            shotFrameSizes[0] = new Point(75, 75);
            Point[] shotSheetSizes = new Point[1];
            shotSheetSizes[0] = new Point(1, 1);
            SoundEffect shotSoundEffect = Game.Content.Load<SoundEffect>(@"Music\hit");
            shotList.Add(new Weapon(shotTextures, new Vector2(
                playerPos.X + 20, playerPos.Y + 30), shotFrameSizes,
                10, new Point(0, 0), shotSheetSizes, shotSpeed,
                shotSoundEffect, 1f, 0));

            player.shotCount--;
            shotIndicationList.RemoveAt(shotIndicationList.Count - 1);
        }

        // Spawn an enemy's shot
        public void SpawnEnemyShot(Vector2 enemyPos, Vector2 enemyShotSpeed)
        {
            Texture2D[] enemyShotTextures = new Texture2D[1];
            enemyShotTextures[0] = Game.Content.Load<Texture2D>(@"Enemy Weapons\mud");
            Point[] enemyShotFrameSizes = new Point[1];
            enemyShotFrameSizes[0] = new Point(150, 95);
            Point[] enemyShotSheetSizes = new Point[1];
            enemyShotSheetSizes[0] = new Point(1, 1);
            SoundEffect shotSoundEffect = Game.Content.Load<SoundEffect>(@"Music\hit");

            spriteList.Add(new ProjectiveWeapon(enemyShotTextures, new Vector2(
                enemyPos.X - 20, enemyPos.Y + 30), enemyShotFrameSizes,
                10, new Point(0, 0), enemyShotSheetSizes, enemyShotSpeed,
                shotSoundEffect, .5f, 100));
        }

        public Vector2 GetPlayerPos()
        {
            return new Vector2(player.GetPosition().X, player.GetPosition().Y);
        }

        private void ResetSpawnTime()
        {
            nextSpawnTime = ((Game1)Game).rnd.Next(
                minSpawnTime, maxSpawnTime);
        }

        public void AddAvailableShot()
        {
            // Add icon to indicate a gain of shots
            Texture2D[] hamburgerTextures = new Texture2D[1];
            hamburgerTextures[0] = Game.Content.Load<Texture2D>(@"PowerUps\bread available");
            Point[] hamburgerFrameSizes = new Point[1];
            hamburgerFrameSizes[0] = new Point(100, 100);
            Point[] hamburgerSheetSizes = new Point[1];
            hamburgerSheetSizes[0] = new Point(2, 1);
            shotIndicationList.Add(new Weapon(hamburgerTextures, new Vector2(
                screenWidth - player.shotCount * 50 - 65, 0),
                hamburgerFrameSizes, 10, new Point(0, 0), hamburgerSheetSizes,
                Vector2.Zero, null, .6f, 200, 0));
            
            player.shotCount++;
        }

        private void AddLife()
        {
            Texture2D[] lifeTextures = new Texture2D[1];
            lifeTextures[0] = Game.Content.Load<Texture2D>(@"PowerUps\Heart");
            Point[] lifeFrameSizes = new Point[1];
            lifeFrameSizes[0] = new Point(75, 75);
            Point[] lifeSheetSizes = new Point[1];
            lifeSheetSizes[0] = new Point(1, 1);

            livesList.Add(new Weapon(lifeTextures, new Vector2(
                    livesList.Count * 50 + 10, 10), lifeFrameSizes, 10, new Point(0, 0),
                    lifeSheetSizes, Vector2.Zero, null, .6f, 0));
        }

        private void SpawnPattern()
        {
            // Choose a pattern to spawn
            int patternNum;
            while (true)
            {
                patternNum = ((Game1)Game).rnd.Next(PATTERN_NUM);
                if (patternNum != currentPatternNum)
                {
                    currentPatternNum = patternNum;
                    break;
                }
            }

            switch (currentPatternNum)
            {
                //default:
                //    SpawnSprite("Bracket", new Vector2(screenWidth + 100,
                //        UserControlledSprite.groundLineY - 20), -shiftingSpeed);
                //    SpawnSprite("Bracket", new Vector2(screenWidth + 120,
                //        UserControlledSprite.groundLineY - 80), -shiftingSpeed);
                //    SpawnSprite("Bracket", new Vector2(screenWidth + 310,
                //        UserControlledSprite.groundLineY - 20), -shiftingSpeed);
                //    SpawnSprite("Bracket", new Vector2(screenWidth + 300,
                //        UserControlledSprite.groundLineY - 80), -shiftingSpeed);
                //    SpawnSprite("Bracket", new Vector2(screenWidth + 140,
                //        UserControlledSprite.groundLineY - 140), -shiftingSpeed);
                //    SpawnSprite("Dog Food", new Vector2(screenWidth + 520,
                //        UserControlledSprite.groundLineY + 45), -shiftingSpeed);
                //    break;
                case 0:
                    SpawnSprite("Bracket", new Vector2(screenWidth + 100,
                        UserControlledSprite.groundLineY - 20), -shiftingSpeed);
                    SpawnSprite("Bracket", new Vector2(screenWidth + 280,
                        UserControlledSprite.groundLineY - 20), -shiftingSpeed);
                    SpawnSprite("Muddy Green", new Vector2(screenWidth + 245,
                        UserControlledSprite.groundLineY - 15), new Vector2(5, 0));
                    break;
                case 1:
                    SpawnSprite("Bracket", new Vector2(screenWidth + 100,
                        UserControlledSprite.groundLineY - 20), -shiftingSpeed);
                    SpawnSprite("Bracket", new Vector2(screenWidth + 310,
                        UserControlledSprite.groundLineY - 20), -shiftingSpeed);
                    SpawnSprite("Bracket", new Vector2(screenWidth + 220,
                        UserControlledSprite.groundLineY - 80), -shiftingSpeed);
                    if (shiftingSpeed.X < 10)
                    {
                        SpawnSprite("Bane", new Vector2(screenWidth + 290,
                            UserControlledSprite.groundLineY - 60), -shiftingSpeed);
                    }
                    else
                    {
                        SpawnSprite("Bane", new Vector2(screenWidth + 400,
                            UserControlledSprite.groundLineY), -shiftingSpeed);
                    }
                    break;
                case 2:
                    SpawnSprite("Bracket", new Vector2(screenWidth + 100,
                        UserControlledSprite.groundLineY - 20), -shiftingSpeed);
                    SpawnSprite("Bracket", new Vector2(screenWidth + 120,
                        UserControlledSprite.groundLineY - 80), -shiftingSpeed);
                    SpawnSprite("Bracket", new Vector2(screenWidth + 310,
                        UserControlledSprite.groundLineY - 20), -shiftingSpeed);
                    SpawnSprite("Bracket", new Vector2(screenWidth + 300,
                        UserControlledSprite.groundLineY - 80), -shiftingSpeed);
                    SpawnSprite("Beauty", new Vector2(screenWidth + 500,
                        UserControlledSprite.groundLineY), -shiftingSpeed);
                    break;
                case 3:
                    SpawnSprite("Bracket", new Vector2(screenWidth + 100,
                        UserControlledSprite.groundLineY - 20), -shiftingSpeed);
                    SpawnSprite("Bracket", new Vector2(screenWidth + 120,
                        UserControlledSprite.groundLineY - 80), -shiftingSpeed);
                    SpawnSprite("Bracket", new Vector2(screenWidth + 310,
                        UserControlledSprite.groundLineY - 20), -shiftingSpeed);
                    SpawnSprite("Bracket", new Vector2(screenWidth + 300,
                        UserControlledSprite.groundLineY - 80), -shiftingSpeed);
                    SpawnSprite("Banana", new Vector2(screenWidth + 40,
                         UserControlledSprite.groundLineY + 80), -shiftingSpeed);
                    break;
                case 4:
                    SpawnSprite("Bracket", new Vector2(screenWidth + 100,
                        UserControlledSprite.groundLineY - 20), -shiftingSpeed);
                    SpawnSprite("Bracket", new Vector2(screenWidth + 120,
                        UserControlledSprite.groundLineY - 80), -shiftingSpeed);
                    SpawnSprite("Bracket", new Vector2(screenWidth + 310,
                        UserControlledSprite.groundLineY - 20), -shiftingSpeed);
                    SpawnSprite("Bracket", new Vector2(screenWidth + 300,
                        UserControlledSprite.groundLineY - 80), -shiftingSpeed);
                    SpawnSprite("Bracket", new Vector2(screenWidth + 140,
                        UserControlledSprite.groundLineY - 140), -shiftingSpeed);
                    SpawnSprite("Dog Food", new Vector2(screenWidth + 520,
                        UserControlledSprite.groundLineY + 45), -shiftingSpeed);
                    break;
                case 5:
                    SpawnSprite("Bracket", new Vector2(screenWidth + 100,
                        UserControlledSprite.groundLineY - 20), -shiftingSpeed);
                    SpawnSprite("Bracket", new Vector2(screenWidth + 120,
                        UserControlledSprite.groundLineY - 80), -shiftingSpeed);
                    SpawnSprite("Bracket", new Vector2(screenWidth + 310,
                        UserControlledSprite.groundLineY - 20), -shiftingSpeed);
                    SpawnSprite("Bracket", new Vector2(screenWidth + 300,
                        UserControlledSprite.groundLineY - 80), -shiftingSpeed);
                    SpawnSprite("Bracket", new Vector2(screenWidth + 140,
                        UserControlledSprite.groundLineY - 140), -shiftingSpeed);
                    SpawnSprite("Muddy Red", new Vector2(screenWidth + 350,
                        UserControlledSprite.groundLineY - 120), -shiftingSpeed);
                    SpawnSprite("Hamburger", new Vector2(screenWidth + 40,
                        UserControlledSprite.groundLineY + 60), -shiftingSpeed);
                    break;
                case 6:
                    SpawnSprite("Bracket", new Vector2(screenWidth + 100,
                        UserControlledSprite.groundLineY - 20), -shiftingSpeed);
                    SpawnSprite("Bracket", new Vector2(screenWidth + 120,
                        UserControlledSprite.groundLineY - 80), -shiftingSpeed);
                    SpawnSprite("Bracket", new Vector2(screenWidth + 310,
                        UserControlledSprite.groundLineY - 20), -shiftingSpeed);
                    SpawnSprite("Bracket", new Vector2(screenWidth + 300,
                        UserControlledSprite.groundLineY - 80), -shiftingSpeed);
                    SpawnSprite("Bracket", new Vector2(screenWidth + 140,
                        UserControlledSprite.groundLineY - 140), -shiftingSpeed);
                    SpawnSprite("Muddy Red", new Vector2(screenWidth + 500,
                        UserControlledSprite.groundLineY), -shiftingSpeed);
                    SpawnSprite("Bane", new Vector2(screenWidth + 40,
                          UserControlledSprite.groundLineY + 60), -shiftingSpeed);
                    break;
                case 7:
                    SpawnSprite("Bane", new Vector2(screenWidth + 100,
                          UserControlledSprite.groundLineY + 60), -shiftingSpeed);
                    SpawnSprite("Banana", new Vector2(screenWidth + 60,
                          UserControlledSprite.groundLineY + 80), -shiftingSpeed);
                    break;
                case 8:
                    SpawnSprite("Bane", new Vector2(screenWidth + 100,
                          UserControlledSprite.groundLineY + 60), -shiftingSpeed);
                    SpawnSprite("Hamburger", new Vector2(screenWidth + 60,
                          UserControlledSprite.groundLineY + 60), -shiftingSpeed);
                    break;
                case 9:
                    SpawnSprite("Bracket", new Vector2(screenWidth + 100,
                        UserControlledSprite.groundLineY - 20), -shiftingSpeed);
                    SpawnSprite("Banana", new Vector2(screenWidth + 450,
                        UserControlledSprite.groundLineY + 80), -shiftingSpeed);
                    break;
                //case 1:
                //    SpawnSprite("Bane", new Vector2(screenWidth + 100,
                //        UserControlledSprite.groundLineY + 60), -shiftingSpeed);
                //    break;
                //case 2:
                //    SpawnSprite("Hamburger", new Vector2(screenWidth + 100,
                //        UserControlledSprite.groundLineY + 60), -shiftingSpeed);
                //    break;
                //case 3:
                //    SpawnSprite("Bracket", new Vector2(screenWidth + 100,
                //        UserControlledSprite.groundLineY - 20), -shiftingSpeed);
                //    break;
                //case 4:
                //    SpawnSprite("Banana", new Vector2(screenWidth + 100,
                //        UserControlledSprite.groundLineY + 80), -shiftingSpeed);
                //    break;
                //case 5:
                //    SpawnSprite("Beauty", new Vector2(screenWidth + 100,
                //        UserControlledSprite.groundLineY), -shiftingSpeed);
                //    break;
                //case 6:
                //    SpawnSprite("Muddy Red", new Vector2(screenWidth + 100,
                //        UserControlledSprite.groundLineY), -shiftingSpeed);
                //    break;
                // Additional patterns are attached here
            }
        }

        private void SpawnSprite(string name, Vector2 position, Vector2 speed)
        {
            switch (name)
            {
                case "Bane":
                    Texture2D[] baneTextures = new Texture2D[2];
                    baneTextures[0] = Game.Content.Load<Texture2D>(@"PowerUps\bane available");
                    baneTextures[1] = Game.Content.Load<Texture2D>(@"PowerUps\bane consumed");
                    Point[] baneFrameSizes = new Point[2];
                    baneFrameSizes[0] = new Point(75, 75);
                    baneFrameSizes[1] = new Point(75, 75);
                    Point[] baneSheetSizes = new Point[2];
                    baneSheetSizes[0] = new Point(2, 1);
                    baneSheetSizes[1] = new Point(2, 1);
                    SoundEffect baneSoundEffect = Game.Content.Load<SoundEffect>(@"Music\bane hurt");
                    spriteList.Add(new powerUp(baneTextures, position,
                        baneFrameSizes, 10, new Point(0, 0), baneSheetSizes,
                        speed, baneSoundEffect, 200, powerUp.Type.poison, 100));
                    break;
                case "Hamburger":
                    Texture2D[] hamburgerTextures = new Texture2D[2];
                    hamburgerTextures[0] = Game.Content.Load<Texture2D>(@"PowerUps\bread available");
                    hamburgerTextures[1] = Game.Content.Load<Texture2D>(@"PowerUps\bread consumed");
                    Point[] hamburgerFrameSizes = new Point[2];
                    hamburgerFrameSizes[0] = new Point(100, 100);
                    hamburgerFrameSizes[1] = new Point(100, 100);
                    Point[] hamburgerSheetSizes = new Point[2];
                    hamburgerSheetSizes[0] = new Point(2, 1);
                    hamburgerSheetSizes[1] = new Point(2, 1);
                    SoundEffect hamburgerSoundEffect = Game.Content.Load<SoundEffect>(@"Music\eat");
                    spriteList.Add(new powerUp(hamburgerTextures, position,
                        hamburgerFrameSizes, 10, new Point(0, 0), hamburgerSheetSizes,
                        speed, hamburgerSoundEffect, .75f, 200, powerUp.Type.bread, 0));
                    break;
                case "Bracket":
                    Texture2D[] bracketTextures = new Texture2D[1];
                    bracketTextures[0] = Game.Content.Load<Texture2D>(@"Obstacles\bracket");
                    Point[] bracketFrameSizes = new Point[1];
                    bracketFrameSizes[0] = new Point(200, 200);
                    Point[] bracketSheetSizes = new Point[1];
                    bracketSheetSizes[0] = new Point(1, 1);
                    spriteList.Add(new Obstacle(Obstacle.Type.SHELF, bracketTextures,
                        position, bracketFrameSizes, 10, new Point(0, 0),
                        bracketSheetSizes, speed, null, 0));
                    break;
                case "Banana":
                    Texture2D[] bananaTextures = new Texture2D[1];
                    bananaTextures[0] = Game.Content.Load<Texture2D>(@"Obstacles\peel");
                    Point[] bananaFrameSizes = new Point[1];
                    bananaFrameSizes[0] = new Point(150, 150);
                    Point[] bananaSheetSizes = new Point[1];
                    bananaSheetSizes[0] = new Point(1, 1);
                    SoundEffect bananaSoundEffect = Game.Content.Load<SoundEffect>(@"Music\slip");
                    spriteList.Add(new Obstacle(Obstacle.Type.PEEL, bananaTextures,
                        position, bananaFrameSizes, 10, new Point(0, 0),
                        bananaSheetSizes, speed, bananaSoundEffect, .5f, 100));
                    break;
                case "Beauty":
                    Texture2D[] beautyTextures = new Texture2D[1];
                    beautyTextures[0] = Game.Content.Load<Texture2D>(@"Obstacles\beauty");
                    Point[] beautyFrameSizes = new Point[1];
                    beautyFrameSizes[0] = new Point(150, 150);
                    Point[] beautySheetSizes = new Point[1];
                    beautySheetSizes[0] = new Point(1, 1);
                    SoundEffect beautySoundEffect = Game.Content.Load<SoundEffect>(@"Music\seduce");
                    spriteList.Add(new Obstacle(Obstacle.Type.BEAUTY, beautyTextures,
                        position, beautyFrameSizes, 10, new Point(0, 0),
                        beautySheetSizes, speed, beautySoundEffect, 200));
                    break;
                case "Muddy Red":
                    Texture2D[] muddyRedTextures = new Texture2D[3];
                    muddyRedTextures[0] = Game.Content.Load<Texture2D>(@"Enemies/muddy red idle");
                    muddyRedTextures[1] = Game.Content.Load<Texture2D>(@"Enemies/muddy red attacking");
                    muddyRedTextures[2] = Game.Content.Load<Texture2D>(@"Enemies/muddy red dying");
                    Point[] muddyRedFrameSizes = new Point[3];
                    muddyRedFrameSizes[0] = new Point(150, 150);
                    muddyRedFrameSizes[1] = new Point(150, 150);
                    muddyRedFrameSizes[2] = new Point(150, 150);
                    Point[] muddyRedSheetSizes = new Point[3];
                    muddyRedSheetSizes[0] = new Point(1, 1);
                    muddyRedSheetSizes[0] = new Point(1, 1);
                    muddyRedSheetSizes[0] = new Point(1, 1);
                    spriteList.Add(new Enemy(muddyRedTextures, position,
                        muddyRedFrameSizes, 10, new Point(0, 0),
                        muddyRedSheetSizes, speed, null, 100));
                    break;
                case "Muddy Green":
                    Texture2D[] muddyGreenTextures = new Texture2D[4];
                    Point[] muddyGreenFrameSizes = new Point[4];
                    Point[] muddyGreenSheetSizes = new Point[4];
                    for (int i = 0; i < 4; ++i)
                    {
                        muddyGreenTextures[i] =
                            Game.Content.Load<Texture2D>(@"Enemies\Muddy Green " + (i + 1).ToString());
                        muddyGreenFrameSizes[i] = new Point(100, 100);
                        muddyGreenSheetSizes[i] = new Point(
                            muddyGreenTextures[i].Width / muddyGreenFrameSizes[i].X,
                            muddyGreenTextures[i].Height / muddyGreenFrameSizes[i].Y);
                    }
                    SoundEffect muddyGreenSoundEffect =
                        Game.Content.Load<SoundEffect>(@"Music\bane hurt");
                    Enemy muddyGreen = new Enemy(muddyGreenTextures, position,
                        muddyGreenFrameSizes, 10, new Point(0, 0),
                        muddyGreenSheetSizes, speed, 300, muddyGreenSoundEffect, 100);
                    muddyGreen.EnemyType = Enemy.Type.GREEN;
                    spriteList.Add(muddyGreen);
                    break;
                case "Dog Food":
                    Texture2D[] dogFoodTextures = new Texture2D[2];
                    Point[] dogFoodFrameSizes = new Point[2];
                    Point[] dogFoodSheetSizes = new Point[2];
                    for (int i = 0; i < 2; ++i )
                    {
                        dogFoodTextures[i] = Game.Content.Load<Texture2D>(@"PowerUps\Dog Food " + (i + 1).ToString());
                        dogFoodFrameSizes[i] = new Point(75, 94);
                        dogFoodSheetSizes[i] = new Point(2, 1);
                    }
                    SoundEffect dogFoodSoundEffect = Game.Content.Load<SoundEffect>(@"Music\dog food");
                    spriteList.Add(new powerUp(dogFoodTextures, position, dogFoodFrameSizes, 10,
                        new Point(0, 0), dogFoodSheetSizes, speed, dogFoodSoundEffect, 200,
                        powerUp.Type.DOG_FOOD, 0));
                    break;
                case "Heart":
                    Texture2D[] heartTextures = new Texture2D[2];
                    Point[] heartFrameSizes = new Point[2];
                    Point[] heartSheetSizes = new Point[2];
                    for (int i = 0; i < 2; ++i)
                    {
                        heartTextures[i] = Game.Content.Load<Texture2D>(@"PowerUps\Heart");
                        heartFrameSizes[i] = new Point(75, 75);
                        heartSheetSizes[i] = new Point(1, 1);
                    }
                    SoundEffect heartSoundEffect = Game.Content.Load<SoundEffect>(@"Music\heart");
                    spriteList.Add(new powerUp(heartTextures, position, heartFrameSizes, 10,
                        new Point(0, 0), heartSheetSizes, speed, heartSoundEffect, .75f,
                        200, powerUp.Type.LIFE, 0));
                    break;
            }
        }

        private void ResolvePlayerObstacleCollision(Obstacle obstacle)
        {
            Rectangle playerCollisionRect = player.GetCollisionRect();
            Rectangle obstacleCollisionRect = obstacle.GetCollisionRect();

            if (playerCollisionRect.Bottom - player.speedOfLastUpdate.Y
                > obstacleCollisionRect.Top - obstacle.speedOfLastUpdate.Y
                && playerCollisionRect.Bottom - player.speedOfLastUpdate.Y
                < obstacleCollisionRect.Center.Y)
            {
                // Collide with the top of the obstacle
                player.BlockFromBottom(obstacleCollisionRect.Top - 127);
            }
            else if (playerCollisionRect.Right - player.speedOfLastUpdate.X
                < obstacleCollisionRect.Left - obstacle.speedOfLastUpdate.X + 20)
            {
                // Collide with the left side of the obstacle
                player.BlockFromRight();
            }
            else if (playerCollisionRect.Left - player.speedOfLastUpdate.X
                > obstacleCollisionRect.Right - obstacle.speedOfLastUpdate.X - 5)
            {
                // Collide with the right side of the obstacle
                player.BlockFromLeft();
            }
            else if (playerCollisionRect.Bottom - player.speedOfLastUpdate.Y
                > obstacleCollisionRect.Top - obstacle.speedOfLastUpdate.Y
                && playerCollisionRect.Bottom > obstacleCollisionRect.Center.Y)
            {
                // Collide with the bottom of the obstacle
                player.BlockFromTop();
            }
            else
            {
                player.BlockFromRight();
            }
        }

        private void UpdateSprite(GameTime gameTime)
        {
            // Update player
            player.Update(gameTime, screenBound, this);

            // Update shots
            foreach (Sprite s in shotList)
                s.Update(gameTime, screenBound, this);

            // Update lives
            foreach (Sprite s in livesList)
                s.Update(gameTime, screenBound, this);

            // Update shot-indications
            foreach (Sprite s in shotIndicationList)
                s.Update(gameTime, screenBound, this);

            player.ClearBlock();
            // Update other sprites and check collision
            for (int i = 0; i < spriteList.Count; i++)
            {
                Sprite s = spriteList[i];

                s.Update(gameTime, screenBound, this);

                if (player.status != UserControlledSprite.Status.TURBO)
                {
                    // Check collision with player
                    if (player.GetCollisionRect().Intersects(s.GetCollisionRect()))
                    {
                        if (s is powerUp)
                        {
                            // s.collisionSound.Play();

                            switch (((powerUp)s).type)
                            {
                                case powerUp.Type.poison:
                                    if (((powerUp)s).IsAvailable())
                                    {
                                        // Decrement game score
                                        score -= s.score;
                                        // Play sound since player hurt by the toxicant
                                        s.collisionSound.Play();
                                        // The bane can affect only once
                                        ((powerUp)s).Consume();
                                        // Remove one life
                                        if (livesList.Count > 0)
                                            livesList.RemoveAt(livesList.Count - 1);
                                        player.ChangeStatus(UserControlledSprite.Status.BEATEN);
                                    }
                                    break;
                                case powerUp.Type.bread:
                                    if (((powerUp)s).IsAvailable())
                                    {
                                        s.collisionSound.Play();
                                        ((powerUp)s).Consume();
                                        if (player.shotCount < maxShotNum)
                                            AddAvailableShot();
                                    }
                                    break;
                                case powerUp.Type.DOG_FOOD:
                                    if (((powerUp)s).IsAvailable())
                                    {
                                        s.collisionSound.Play();
                                        ((powerUp)s).Consume();
                                        player.ChangeStatus(UserControlledSprite.Status.TURBO);
                                    }
                                    break;
                                case powerUp.Type.LIFE:
                                    if (((powerUp)s).IsAvailable())
                                    {
                                        s.collisionSound.Play();
                                        ((powerUp)s).Consume();
                                        if (livesList.Count < maxNumOfLives)
                                        {
                                            AddLife();
                                        }
                                    }
                                    break;
                            }
                        }
                        else if (s is Obstacle)
                        {
                            switch (((Obstacle)s).type)
                            {
                                case Obstacle.Type.SHELF:
                                    ResolvePlayerObstacleCollision((Obstacle)s);
                                    break;
                                case Obstacle.Type.PEEL:
                                    if (!((Obstacle)s).STEPED && player.status
                                        != UserControlledSprite.Status.JUMPING
                                        && player.status != UserControlledSprite.Status.BEATEN)
                                    {
                                        score -= s.score;
                                        s.collisionSound.Play();
                                        player.ChangeStatus(UserControlledSprite.Status.FALLING);
                                        ((Obstacle)s).STEPED = true;
                                    }
                                    break;
                                case Obstacle.Type.BEAUTY:
                                    if (!((Obstacle)s).STEPED && !player.FACINATED)
                                    {
                                        score -= s.score;
                                        s.collisionSound.Play();
                                        player.FACINATED = true;
                                    }
                                    break;
                                // Some other cases
                            }
                        }
                        else if (s is Weapon)
                        {
                            score -= s.score;
                            s.collisionSound.Play();

                            if (livesList.Count > 0)
                                livesList.RemoveAt(livesList.Count - 1);
                            player.ChangeStatus(UserControlledSprite.Status.BEATEN);

                            spriteList.RemoveAt(i);
                            i--;
                        }
                        else // s is Enemy
                        {
                            if (((Enemy)s).EnemyType == Enemy.Type.GREEN
                                && !((Enemy)s).IsTransparent
                                && ((Enemy)s).EnemyState == Enemy.Status.IDLE
                                && player.status != UserControlledSprite.Status.BEATEN)
                            {
                                score -= s.score;
                                s.collisionSound.Play();

                                // Make the enemy do no harm for a little while
                                ((Enemy)s).IsTransparent = true;

                                if (livesList.Count > 0)
                                    livesList.RemoveAt(livesList.Count - 1);
                                player.ChangeStatus(UserControlledSprite.Status.BEATEN);
                            }
                        }
                    }
                }

                // Check collision with player's shots
                for (int j = 0; j < shotList.Count; j++)
                {
                    Sprite shot = shotList[j];

                    if (shot.GetCollisionRect().Intersects(s.GetCollisionRect()))
                    {
                        if (s is Enemy)
                        {
                            score += s.score;
                            // Enemy enters the DYING state
                            ((Enemy)s).SetStatus(Enemy.Status.DYING);
                            shot.collisionSound.Play();
                            shotList.RemoveAt(j);
                            j--;
                        }
                        //else
                        //{
                        //    s.collisionSound.Play();
                        //    shotList.RemoveAt(j);
                        //    j--;
                        //}
                    }

                    if (shot.IsOutOfBounds(screenBound))
                    {
                        shotList.RemoveAt(j);
                        j--;
                    }
                }

                if ((s.IsOutOfBounds(screenBound) && (s.GetPosition().X < 0
                    || s.GetPosition().Y > screenHeight)) || (s is Enemy
                    && ((Enemy)s).GetCurrentStatus() == Enemy.Status.DEAD))
                {
                    spriteList.RemoveAt(i);
                    i--;
                    // Enemy spawn power-ups with some probability 
                    if (s is Enemy)
                    {
                        int r = ((Game1)Game).rnd.Next(100);
                        if (r < 30)
                        {
                            if (((Enemy)s).EnemyType == Enemy.Type.RED)
                            {
                                SpawnSprite("Hamburger", new Vector2(s.GetPosition().X + 50,
                                    s.GetPosition().Y + 55), -shiftingSpeed);
                            }
                            else
                            {
                                SpawnSprite("Hamburger", new Vector2(s.GetPosition().X + 25,
                                    s.GetPosition().Y + 10), -shiftingSpeed);
                            }
                        }
                        else if (r >= 30 && r < 60)
                        {
                            if (((Enemy)s).EnemyType == Enemy.Type.RED)
                            {
                                SpawnSprite("Heart", new Vector2(s.GetPosition().X + 50,
                                    s.GetPosition().Y + 70), -shiftingSpeed);
                            }
                            else
                            {
                                SpawnSprite("Heart", new Vector2(s.GetPosition().X + 25,
                                    s.GetPosition().Y + 25), -shiftingSpeed);
                            }
                        }
                    }
                } // if
            } // for
        } // UpdateSprite()
    }
}
