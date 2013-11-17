using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Unfailable_Heart.Up_Rising
{
    class Level : IDisposable
    {
        // Game preferred back buffer size
        public static int PreferredBackBufferWidth
        {
            get { return preferredBackBufferWidth; }
        }
        static readonly int preferredBackBufferWidth = 480;
        
        public static int PreferredBackBufferHeight
        {
            get { return preferredBackBufferHeight; }
        }
        static readonly int preferredBackBufferHeight = 800;

        // Background related stuff
        const int backgroundTextureNum = 4;
        int currentBackgroundIndex = 0;
        Texture2D[] backgroundTextures;
        int leadingEdgeY;
        int trailingEdgeY;
        const int backgroundSpeedDecrementTimeMilliseconds = 10000;
        int backgroundSpeedDecrementTimer = 0;
        const int backgroundTurboSpeedY = 40;
        int speedBeforeTurboY;
        const int turboTimeMilliseconds = 5000;
        int turboTimer = 0;
        bool onceThrough = false;
        // Used to make the background jitter
        const int backgroundHorizontalOffsetMax = 5;
        const int backgroundVirticalOffsetMax = 5;

        bool isTurbo = false;
        public bool IsTurbo
        {
            get { return isTurbo; }
        }

        int backgroundHorizontalOffset = 0;
        public int BackgroundHorizontalOffset
        {
            get { return backgroundHorizontalOffset; }
        }
        
        int backgroundVirticalOffset = 0;
        public int BackgroundVirticalOffset
        {
            get { return backgroundVirticalOffset; }
        }

        const int backgroundThresholdSpeedY = 5;
        public int BackgroundThresholdSpeedY
        {
            get { return backgroundThresholdSpeedY; }
        }

        public int BackgroundSpeedY
        {
            get { return backgroundSpeedY; }
        }
        int backgroundSpeedY = 10;

        public ContentManager Content
        {
            get { return content; }
        }
        ContentManager content;

        Random random;
        public Random RandomNum
        {
            get { return random; }
        }

        // Player related stuff
        Player player;
        readonly Vector2 playerInitialPos = new Vector2(170, 950);
        bool isPlayerDead = false;
        int numOfLives = 3;
        bool isFallOutOfBound = false;
        GirlFriend girlFriend;
        bool isCollideWithGirlFriend = false;
        const int thresholdScore = 100;

        // Various Lists
        List<Monster> monstersList;
        public List<Monster> MonsterList
        {
            get { return monstersList; }
        }

        List<Vestigial> vestigialList;
        public List<Vestigial> VestigialList
        {
            get { return vestigialList; }
        }

        List<Ammo> ammoList;
        public List<Ammo> AmmoList
        {
            get { return ammoList; }
        }

        List<ArtFont> fontList;
        public List<ArtFont> FontList
        {
            get { return fontList; }
        }

        List<PowerUp> powerUpsList;
        public List<PowerUp> PowerUpsList
        {
            get { return powerUpsList; }
        }

        // Monster Spawning related
        const int monsterSpawnMillisecondsMin = 4000;
        const int monsterSpawnMillisecondsMax = 6000;
        int monsterSpawnTime = 5000;
        int monsterSpawnTimer = 0;

        // Control flow related
        bool theEnd = false;
        public bool TheEnd
        {
            get { return theEnd; }
        }

        // Game score
        int score = 0;
        SpriteFont scoreFont;

        public Level(IServiceProvider serviceProvider, Random random,
            int levelIndex)
        {
            content = new ContentManager(serviceProvider, "Content");
            this.random = random;

            LoadContent(levelIndex);
        }

        private void LoadContent(int levelIndex)
        {
            // Load map
            backgroundTextures = new Texture2D[backgroundTextureNum];

            switch (levelIndex)
            {
                case 0:
                    for (int i = 0; i < backgroundTextureNum; i++)
                    {
                        backgroundTextures[i] = Content.Load<Texture2D>(
                            @"Background Images/Background Vertical " + (i + 1).ToString());
                    }
                    break;
            }

            leadingEdgeY = backgroundTextures[currentBackgroundIndex].Height - preferredBackBufferHeight;
            trailingEdgeY = backgroundTextures[currentBackgroundIndex].Height - 1;

            // Create lists
            monstersList = new List<Monster>();
            vestigialList = new List<Vestigial>();
            ammoList = new List<Ammo>();
            fontList = new List<ArtFont>();
            powerUpsList = new List<PowerUp>();

            // Load player
            player = new Player(this, playerInitialPos);
            girlFriend = new GirlFriend(this, new Vector2(300, 950), new Vector2(-5, -30));

            // Load boss
            monstersList.Add(new Devourer(this, new Vector2(-80, Level.PreferredBackBufferHeight),
                new Vector2(0, -5)));

            // Load font
            scoreFont = Content.Load<SpriteFont>(@"Fonts\score");
        }

        public void Update(GameTime gameTime)
        {
            UpdateBackground(gameTime);

            // No enemy will be spawned during turbo mode
            if (!isTurbo && score < thresholdScore)
            {
                monsterSpawnTimer += gameTime.ElapsedGameTime.Milliseconds;
                if (monsterSpawnTimer > monsterSpawnTime)
                {
                    SpawnWave();
                    ResetSpawnTime();
                }
            }

            UpdateSprite(gameTime);
        }

        private void UpdateBackground(GameTime gameTime)
        {
            if (!isTurbo)
            {
                // Reduce background scrolling speed constantly
                backgroundSpeedDecrementTimer += gameTime.ElapsedGameTime.Milliseconds;
                if (backgroundSpeedDecrementTimer > backgroundSpeedDecrementTimeMilliseconds)
                {
                    backgroundSpeedDecrementTimer = 0;
                    --backgroundSpeedY;
                }
                // Clamp background speed to ensure that it's non-negative
                if (backgroundSpeedY < 0)
                    backgroundSpeedY = 0;
            }
            else // Turbo mode logic
            {
                // When just enter turbo mode
                if (!onceThrough)
                {
                    speedBeforeTurboY = backgroundSpeedY;
                    backgroundSpeedY = backgroundTurboSpeedY;
                    onceThrough = true;
                    player.collide(2);
                }

                turboTimer += gameTime.ElapsedGameTime.Milliseconds;
                if (turboTimer > turboTimeMilliseconds)
                {
                    if (backgroundSpeedY <= speedBeforeTurboY)
                    {
                        // Exit turbo mode
                        turboTimer = 0;
                        isTurbo = false;
                        backgroundSpeedY = speedBeforeTurboY;
                        onceThrough = false;
                        player.endPlane();
                    }
                    else
                        --backgroundSpeedY;
                }

                // Extra score due to trubo
                ++score;
            }

            // Scroll the background
            leadingEdgeY -= backgroundSpeedY;
            if (leadingEdgeY < 0)
            {
                currentBackgroundIndex = (currentBackgroundIndex + 1) % backgroundTextureNum;
                leadingEdgeY = backgroundTextures[currentBackgroundIndex].Height + leadingEdgeY;
            }
            trailingEdgeY -= backgroundSpeedY;
            if (trailingEdgeY < 0)
            {
                trailingEdgeY = backgroundTextures[currentBackgroundIndex].Height + trailingEdgeY;
            }

            // Refresh background horizontal offset
            if (backgroundSpeedY < backgroundThresholdSpeedY && !isPlayerDead)
            {
                backgroundHorizontalOffset = random.Next(-backgroundHorizontalOffsetMax,
                    backgroundHorizontalOffsetMax);
                backgroundVirticalOffset = random.Next(-backgroundVirticalOffsetMax,
                    backgroundVirticalOffsetMax);
            }
            else
            {
                backgroundHorizontalOffset = 0;
                backgroundVirticalOffset = 0;
            }
        }

        private void SpawnWave()
        {
            int r = random.Next(100);

            if (r < 35)
            {
                monstersList.Add(new Octopuscs(this, new Vector2(random.Next(100, 300), 950),
                    new Vector2(random.Next(-10, 11), random.Next(-35, -25))));                
            }
            else if (r < 55)
            {
                monstersList.Add(new Skelecton(this, new Vector2(random.Next(100, 300), 950),
                    new Vector2(random.Next(-5, 6), random.Next(-32, -23))));
            }
            else if (r < 75)
            {
                monstersList.Add(new BloodSquirter(this, new Vector2(random.Next(100, 300), 700),
                    new Vector2(random.Next(1, 6), random.Next(20, 27)),
                    new Vector2(random.Next(-1, 2), -1), 80,
                    new Vector2(random.Next(2, 5), 0), new Vector2(0, random.Next(1, 5))));
            }
            else if (r < 100)
            {
                monstersList.Add(new BigMouth(this, new Vector2(random.Next(100, 300), 950),
                    random.Next(550, 750)));
            }
        }

        private void ResetSpawnTime()
        {
            monsterSpawnTimer = 0;

            monsterSpawnTime = random.Next(monsterSpawnMillisecondsMin,
                monsterSpawnMillisecondsMax);
        }

        private void UpdateSprite(GameTime gameTime)
        {
            if (!isPlayerDead)
            {
                player.Update(gameTime);

                // Rescue the player when he falls out of the window
                if (!isFallOutOfBound && player.Position.Y > Level.PreferredBackBufferHeight + 50
                    && player.currentState != Player.State.rocket)
                {
                    if (numOfLives > 0)
                        --numOfLives;
                    player.reStart();
                    isFallOutOfBound = true;
                }
                else if (isFallOutOfBound && player.Position.Y < Level.PreferredBackBufferHeight)
                    isFallOutOfBound = false;

                Rectangle playerCollisionRect = player.GetCollisionRect();

                if (score >= thresholdScore)
                {
                    girlFriend.Update(gameTime);

                    if (!isCollideWithGirlFriend
                        && girlFriend.CollisionRect.Intersects(playerCollisionRect))
                    {
                        isCollideWithGirlFriend = true;
                        player.collide(7);
                    }
                }

                for (int i = 0; i < monstersList.Count; ++i)
                {
                    Monster m = monstersList[i];

                    m.Update(gameTime);

                    // Collision detection and response
                    Rectangle monsterCollisionRect = m.CollisionRect;

                    if (m is Octopuscs || m is Skelecton)
                    {
                        if (monsterCollisionRect.Intersects(playerCollisionRect)
                            && player.Speed.Y > 0
                            && playerCollisionRect.Center.Y < monsterCollisionRect.Top)
                        {
                            // Octopus killed by the player
                            player.collide(1);
                            m.CurrentStatus = Monster.Status.DYING;
                        }
                    }
                    else if (m is BigMouth)
                    {
                        if (monsterCollisionRect.Intersects(playerCollisionRect)
                            && player.Speed.Y > 0
                            && playerCollisionRect.Center.Y < monsterCollisionRect.Top)
                        {
                            if (m.CurrentStatus == Monster.Status.ATTACKING_MELEE &&
                                player.canCollide())
                            {
                                // Player hurts
                                player.collide(5);
                                score -= m.Score;
                                if (numOfLives > 0)
                                    --numOfLives;
                            }
                            else
                            {
                                // Monster's killed
                                player.collide(1);
                                m.CurrentStatus = Monster.Status.DYING;
                            }
                        }
                    }
                    else if (m is BloodSquirter && m.CurrentStatus != Monster.Status.DYING)
                    {
                        if (monsterCollisionRect.Intersects(playerCollisionRect)
                            && player.Speed.Y > 0
                            && playerCollisionRect.Center.Y < monsterCollisionRect.Top)
                        {
                            if (player.canCollide())
                            {
                                score -= m.Score;
                                if (numOfLives > 0)
                                    --numOfLives;
                            }
                            // Player get hurt whatsoever
                            player.collide(5);
                            m.CurrentStatus = Monster.Status.DYING;
                        }
                        else if (monsterCollisionRect.Intersects(playerCollisionRect)
                            && player.currentState != Player.State.rocket)
                        {
                            if (player.canCollide())
                            {
                                score -= m.Score;
                                if (numOfLives > 0)
                                    --numOfLives;
                            }
                            player.collide(4);
                            m.CurrentStatus = Monster.Status.DYING;
                        }
                    }
                    else if (m is Devourer)
                    {
                        if (monsterCollisionRect.Intersects(playerCollisionRect)
                            && player.currentState != Player.State.rocket
                            && backgroundSpeedY < backgroundThresholdSpeedY)
                        {
                            // player get devoured by the boss
                            ((Devourer)m).IsHitPlayer = true;
                            isPlayerDead = true;
                            numOfLives = 0;

                            // Display "You Fail" on the screen
                            fontList.Add(new ArtFont(this, @"Art Font\fail", Vector2.Zero, Vector2.Zero,
                                true, false)); ;

                            // Game Over
                            theEnd = true;
                        }
                    }

                    // Erase monsters that are out of bound or dead
                    if (m.CurrentStatus == Monster.Status.DEAD)
                    {
                        monstersList.RemoveAt(i--);

                        // Increase background speed when a monster is killed
                        ++backgroundSpeedY;

                        score += m.Score;
                    }
                    else if (m.Position.Y > Level.PreferredBackBufferHeight + 50
                        && (m.CurrentStatus != Monster.Status.SURGING || isTurbo))
                    {
                        monstersList.RemoveAt(i--);
                    }
                    else if (m is Devourer && m.Position.Y + 700 < 0)
                    {
                        monstersList.RemoveAt(i--);
                    }
                } // for

                for (int i = 0; i < ammoList.Count; ++i)
                {
                    Ammo a = ammoList[i];

                    a.Update(gameTime);

                    // Erase ammo that is out of bound
                    if (a.Position.Y + 50 < 0 || a.Position.Y - 50 > Level.PreferredBackBufferHeight)
                        ammoList.RemoveAt(i--);

                    if (a.CollisionRect.Intersects(playerCollisionRect) && player.canCollide())
                    {
                        score -= a.Score;
                        if (numOfLives > 0)
                            --numOfLives;
                        player.collide(4);
                        ammoList.RemoveAt(i--);

                        // Lose speed when player hurts
                        --backgroundSpeedY;
                    }
                } // Monsters

                for (int i = 0; i < vestigialList.Count; ++i)
                {
                    Vestigial v = vestigialList[i];

                    v.Update(gameTime);

                    // Erase vestigial that is out of bound
                    if (v.Position.Y > Level.PreferredBackBufferHeight)
                    {
                        vestigialList.RemoveAt(i--);
                    }
                } // Vestigial

                for (int i = 0; i < fontList.Count; ++i)
                {
                    ArtFont af = fontList[i];

                    af.Update(gameTime);

                    if (af.CurrentStatus == ArtFont.Status.DONE)
                        fontList.RemoveAt(i--);
                } // Art Font

                for (int i = 0; i < powerUpsList.Count; ++i)
                {
                    PowerUp p = powerUpsList[i];

                    p.Update(gameTime);

                    // Player gets power up
                    if (p.CollisionRect.Intersects(playerCollisionRect))
                    {
                        switch (p.Type)
                        {
                            case PowerUp.PowerUpType.Heart:
                                ++numOfLives;
                                player.collide(1);
                                p.CurrentState = PowerUp.State.consuming;
                                break;
                            case PowerUp.PowerUpType.DogFood:
                                // Turbo mode
                                fontList.Add(new ArtFont(this, @"Art Font\Turbo",
                                    new Vector2(p.Position.X + 75, p.Position.Y + 75),
                                    Vector2.Zero, true, true));
                                p.CurrentState = PowerUp.State.consuming;
                                powerUpsList.RemoveAt(i--);
                                isTurbo = true;
                                break;
                        }
                    }

                    if (p.IsConsumed == true || p.Position.Y > Level.PreferredBackBufferHeight + 50)
                    {
                        powerUpsList.RemoveAt(i--);
                    }
                } // Power ups
            }
            else // When player is dead
            {
                foreach (Monster m in monstersList)
                    m.Update(gameTime);

                foreach (Ammo a in ammoList)
                    a.Update(gameTime);

                foreach (Vestigial v in vestigialList)
                    v.Update(gameTime);

                foreach (ArtFont af in fontList)
                    af.Update(gameTime);

                foreach (PowerUp p in powerUpsList)
                    p.Update(gameTime);
            }
        } // UpdateSprite()

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawBackground(spriteBatch);

            if (!isPlayerDead)
                player.Draw(gameTime, spriteBatch);

            foreach (PowerUp p in powerUpsList)
                p.Draw(gameTime, spriteBatch);

            foreach (Monster m in monstersList)
                m.Draw(gameTime, spriteBatch);

            foreach (Ammo a in ammoList)
                a.Draw(gameTime, spriteBatch);

            foreach (Vestigial v in vestigialList)
                v.Draw(gameTime, spriteBatch);

            foreach (ArtFont af in fontList)
                af.Draw(gameTime, spriteBatch);

            if (!isCollideWithGirlFriend)
                girlFriend.Draw(gameTime, spriteBatch);

            DrawScore(spriteBatch);

            DrawLives(spriteBatch);
        }

        private void DrawBackground(SpriteBatch spriteBatch)
        {
            int height1;
            int height2;

            if (leadingEdgeY + preferredBackBufferHeight <=
                backgroundTextures[currentBackgroundIndex].Height)
            {
                height1 = preferredBackBufferHeight;
                height2 = 0;
            }
            else
            {
                height1 = backgroundTextures[currentBackgroundIndex].Height - leadingEdgeY;
                height2 = trailingEdgeY + 1;
            }
            
            spriteBatch.Draw(backgroundTextures[currentBackgroundIndex],
                new Vector2((float)backgroundHorizontalOffset, (float)backgroundVirticalOffset),
                new Rectangle(0, leadingEdgeY, preferredBackBufferWidth, height1),
                Color.White);
            spriteBatch.Draw(backgroundTextures[(currentBackgroundIndex + 3) % backgroundTextureNum],
                new Vector2((float)backgroundHorizontalOffset, (float)(backgroundVirticalOffset + height1)),
                new Rectangle(0, 0, preferredBackBufferWidth, height2),
                Color.White);
        }

        private void DrawScore(SpriteBatch spriteBatch)
        {
            string scoreText = "SCORE: " + score.ToString();
            Vector2 stringSize = scoreFont.MeasureString(scoreText);
            spriteBatch.DrawString(scoreFont, scoreText,
                new Vector2(.5f * preferredBackBufferWidth, .5f * stringSize.Y + 10),
                Color.Gold, 0, new Vector2(.5f * stringSize.X, .5f * stringSize.Y), 1.5f, SpriteEffects.None, 0);
        }

        private void DrawLives(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Content.Load<Texture2D>(@"PowerUps\Heart"), new Vector2(10, 10),
                null, Color.White, 0, Vector2.Zero, .8f, SpriteEffects.None, 0);

            string lifeString = numOfLives.ToString();
            Vector2 stringSize = scoreFont.MeasureString(lifeString);
            spriteBatch.DrawString(scoreFont, lifeString, new Vector2(40, 40),
                Color.Gold, 0, new Vector2(.5f * stringSize.X, .5f * stringSize.Y),
                1.5f, SpriteEffects.None, 0);
        }

        /// <summary>
        /// Unload level content
        /// </summary>
        public void Dispose()
        {
            Content.Unload();
        }
    }
}
