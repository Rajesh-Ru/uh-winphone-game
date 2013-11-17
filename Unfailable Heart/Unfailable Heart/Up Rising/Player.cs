using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Devices;
using Microsoft.Devices.Sensors;
using Microsoft.Xna.Framework.Input.Touch;
using System.Diagnostics;

namespace Unfailable_Heart.Up_Rising
{

    class Player
    {

        public enum State { up, down, ready, hit, plane, rocket, rocketOver,withGirl };
        public State currentState = State.rocket;

        bool isTransparent = false;
        public bool IsTransparent
        {
            get { return isTransparent; }
        }
        const int transparentTime = 2000;
        int countTransparentTime = 0;

        bool isOver = false;

        enum Orientation { left, right };
        Orientation currentOrientation = Orientation.left;
        Texture2D[] textureImages;
        protected Point[] frameSizes;
        Point currentFrame = new Point(0, 0);
        Point[] sheetSizes;
        protected Vector2 speed = new Vector2(0, 0);
        public Vector2 Speed
        {
            get { return speed; }
        }
        protected Vector2 position;
        public Vector2 Position
        {
            get { return position; }
        }
        protected int textureIndex = 0;
        int timeSinceLastFrame = 0;
        const int defaultMillisecondsPerFrame = 33;
        int type = 0;
        int[] millisecondsPerFrame;
        int collisionOffsetX = 30;
        int collisionOffsetY = 10;
        int scale = 1;
        Accelerometer accelerometer;
        float accelerometerDataX = 0,
              accelerometerDataY = 0;

        int animationHitTime = 240;
        int animationTime = 0;

        const int coboundary = 80;

        Level level;

        public Player(Level manage, Vector2 position)
        {
            level = manage;

            initeData(manage);
            this.position = position;

            accelerometer = new Accelerometer();

            accelerometer.ReadingChanged +=
         new EventHandler<AccelerometerReadingEventArgs>(
        AccelerometerDataChanged);

            accelerometer.Start();
        }

        private void initeData(Level manage)
        {
            millisecondsPerFrame = new int[10];
            millisecondsPerFrame[0] = 200;
            millisecondsPerFrame[1] = defaultMillisecondsPerFrame;
            millisecondsPerFrame[2] = defaultMillisecondsPerFrame;
            millisecondsPerFrame[3] = defaultMillisecondsPerFrame;
            millisecondsPerFrame[4] = defaultMillisecondsPerFrame;
            millisecondsPerFrame[5] = defaultMillisecondsPerFrame;
            millisecondsPerFrame[6] = defaultMillisecondsPerFrame;
            millisecondsPerFrame[7] = defaultMillisecondsPerFrame;
            millisecondsPerFrame[8] = defaultMillisecondsPerFrame;
            millisecondsPerFrame[9] = defaultMillisecondsPerFrame;



            textureImages = new Texture2D[10];
            textureImages[0] = manage.Content.Load<Texture2D>(@"UpPlayer\rocket");
            textureImages[1] = manage.Content.Load<Texture2D>(@"UpPlayer\rocketOver");
            textureImages[2] = manage.Content.Load<Texture2D>(@"UpPlayer\ready");
            textureImages[3] = manage.Content.Load<Texture2D>(@"UpPlayer\down");
            textureImages[4] = manage.Content.Load<Texture2D>(@"UpPlayer\hit");
            textureImages[5] = manage.Content.Load<Texture2D>(@"UpPlayer\plane");
            textureImages[6] = manage.Content.Load<Texture2D>(@"UpPlayer\readyTransparent");
            textureImages[7] = manage.Content.Load<Texture2D>(@"UpPlayer\downTransparent");
            textureImages[8] = manage.Content.Load<Texture2D>(@"UpPlayer\hitTransparent");
            textureImages[9] = manage.Content.Load<Texture2D>(@"UpPlayer\withgirl");



            sheetSizes = new Point[10];
            sheetSizes[0] = new Point(2, 1);
            sheetSizes[1] = new Point(1, 1);
            sheetSizes[2] = new Point(1, 1);
            sheetSizes[3] = new Point(1, 1);
            sheetSizes[4] = new Point(1, 1);
            sheetSizes[5] = new Point(2, 1);
            sheetSizes[6] = new Point(2, 1);
            sheetSizes[7] = new Point(2, 1);
            sheetSizes[8] = new Point(2, 1);
            sheetSizes[9] = new Point(2, 1);




            frameSizes = new Point[10];
            for (int i = 0; i < 10; ++i)
            {
                frameSizes[i] = new Point(150, 150);
            }

        }

        public void Update(GameTime gameTime)
        {
            act();
            dealType();
            dealTransparent(gameTime);
            changeImage(gameTime);
            dealAnimationtime(gameTime);
            correctXbound();
        }

        void act()
        {
            if (currentState == State.rocket)
            {
                actAsRocket();
            }
            else if (currentState == State.ready || currentState == State.up || currentState == State.hit || currentState == State.down)
            {
                move();
            }
            else if (currentState == State.plane)
            {
                actAsPlane();
            }
            else if (currentState == State.rocketOver)
            {
                actRocketOver();
            }
            else if (currentState == State.withGirl) 
            {
                actWithGirl();
            }
        }

        void actAsRocket()
        {
            speed.Y = -10;
            position += speed;
            if (position.Y < 150)
            {
                speed.Y += 2;
                currentState = State.rocketOver;
                changeIndex();
            }
        }

        void actRocketOver()
        {
            if (speed.Y < -4)
            {
                speed.Y += 1;
            }
            position += speed;
            if (position.Y < coboundary)
            {
                speed.Y = 0;
                position.Y = coboundary;
                currentState = State.ready;
                changeIndex();
            }
        }

        void actAsPlane()
        {
            dealAccelerometerData();
        }

        void move()
        {
            Vector2 certer = new Vector2((position.X + frameSizes[textureIndex].X / 2), (position.Y + frameSizes[textureIndex].Y / 2));
            if (currentState == State.ready || currentState == State.up)
            {
                Vector2 touchPosition;
                TouchCollection touchCollection = TouchPanel.GetState();
                foreach (TouchLocation touchLocation in touchCollection)
                {
                    if (touchLocation.State == TouchLocationState.Pressed)
                    {
                        touchPosition = new Vector2((int)touchLocation.Position.X, (int)touchLocation.Position.Y);
                        if (touchPosition.Y > certer.Y)
                        {
                            speed.X = (touchPosition.X - certer.X) / 50;
                            speed.Y = 2;
                            currentState = State.down;
                            changeIndex();
                            if (speed.X < 0) currentOrientation = Orientation.left;
                            else currentOrientation = Orientation.right;
                        }
                    }
                }
            }

            if (currentState == State.down)
            {
                speed.Y += 1;
            }

            if (currentState == State.up)
            {
                if (speed.Y < -8)
                {
                    speed.Y += 1;
                }
            }

            if (speed.X > 5)
                speed.X = 5;
            if (speed.X < -5)
                speed.X = -5;

            dealAccelerometerData();

            position += speed;

            if (position.Y < coboundary)
            {
                speed.Y = 0;
                position.Y = coboundary;
                currentState = State.ready;
                changeIndex();
            }
        }

        void actWithGirl() 
        {
            speed.Y = -10;
            position += speed;
        }

        void dealType()
        {
            int currentType = getCollideType();
            if (currentType == 0) return;
            else if (currentType == 1 || currentType == 3)
            {
                currentState = State.hit;
                changeIndex();
                if(speed.Y>0)
                speed.Y = -speed.Y;
                speed.X = 0;
            }
            else if (currentType == 2)
            {
                currentState = State.plane;
                speed.Y = -8;
                changeIndex();
            }
            else if (currentType == -1)
            {
                currentState = State.rocket;
                changeIndex();
                position.Y = 950;
            }
            else if (currentType == 4)
            {
                isTransparent = true;
            }
            else if (currentType == 5)
            {
                currentState = State.hit;
                changeIndex();
                if (speed.Y > 0)
                    speed.Y = -speed.Y;
                speed.X = 0;
                isTransparent = true;
            }
            else if (currentType == 6) 
            {
                currentState = State.ready;
                changeIndex();
                isOver = true;
            }
            else if (currentType == 7) 
            {
                currentState = State.withGirl;
                speed.X = 0;
                changeIndex();
            }
        }

        void dealAnimationtime(GameTime gameTime)
        {

            if (currentState == State.hit)
            {
                animationTime += gameTime.ElapsedGameTime.Milliseconds;
                if (animationTime >= animationHitTime)
                {
                    currentState = State.up;
                    changeIndex();
                    animationTime = 0;
                }
            }
        }

        void changeIndex()
        {
            switch (currentState)
            {
                case State.rocket:
                    textureIndex = 0;
                    break;
                case State.rocketOver:
                    textureIndex = 1;
                    break;
                case State.ready:
                    textureIndex = 2;
                    break;
                case State.down:
                case State.up:
                    textureIndex = 3;
                    if (isOver) textureIndex = 2;
                    break;
                case State.hit:
                    textureIndex = 4;
                    break;
                case State.plane:
                    textureIndex = 5;
                    break;
                case State.withGirl:
                    textureIndex = 9;
                    break;
            }
            timeSinceLastFrame = 0;
        }



        void changeImage(GameTime gameTime)
        {
            timeSinceLastFrame += gameTime.ElapsedGameTime.Milliseconds;
            if (timeSinceLastFrame > millisecondsPerFrame[textureIndex])
            {
                timeSinceLastFrame -= millisecondsPerFrame[textureIndex];
                currentFrame.X++;
                if (currentFrame.X >= sheetSizes[textureIndex].X)
                {
                    currentFrame.X = 0;
                    currentFrame.Y++;
                    if (currentFrame.Y >= sheetSizes[textureIndex].Y)
                        currentFrame.Y = 0;
                }
            }
        }

        void dealTransparent(GameTime gameTime)
        {
            if (isTransparent == false) return;
            countTransparentTime += gameTime.ElapsedGameTime.Milliseconds;
            if (countTransparentTime >= transparentTime)
            {
                isTransparent = false;
                changeIndex();
                countTransparentTime = 0;
                return;
            }
            switch (currentState)
            {
                case State.ready:
                    textureIndex = 6;
                    break;
                case State.down:
                case State.up:
                    textureIndex = 7;
                    break;
                case State.hit:
                    textureIndex = 8;
                    break;
            }
        }

        // type 1:与怪物相撞
        // type 2:与向上冲的飞机相撞
        // type 3:与加命的相撞
        // type 4:受伤闪动
        // 若没有与任何东西相撞不要设置任务值
        // type 5:攻击成功且受伤
        // type 6:进入结束状态
        // type 7:与女生坐火箭飞出
        public void collide(int type)
        {
            this.type = type;
        }
        public bool canCollide()
        {
            if (isTransparent)
                return false;
            if (currentState == State.ready || currentState == State.hit || currentState == State.down || currentState == State.up)
                return true;

            return false;
        }
        private int getCollideType()
        {
            int temp = type;
            type = 0;
            return temp;
        }

        private void correctXbound()
        {
            if (position.X + frameSizes[textureIndex].X - 50 > 480)
            {
                position.X = 480 - frameSizes[textureIndex].X + 50;
                speed.X = 0;
            }
            if (position.X + 50 < 0)
            {
                position.X = -50;
                speed.X = 0;
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            SpriteEffects effect = SpriteEffects.None;
            if (currentOrientation == Orientation.right)
                effect = SpriteEffects.FlipHorizontally;

            spriteBatch.Draw(textureImages[textureIndex],
                position + new Vector2(level.BackgroundHorizontalOffset, level.BackgroundVirticalOffset),
                 new Rectangle(currentFrame.X * frameSizes[textureIndex].X,
                 currentFrame.Y * frameSizes[textureIndex].Y,
                 frameSizes[textureIndex].X, frameSizes[textureIndex].Y),
                  Color.White, 0, Vector2.Zero,
                  1, effect, 0);
        }


        //从屏幕下方飞出
        public void reStart()
        {
            if (currentState == State.rocket || currentState == State.rocketOver) return;
            type = -1;
            speed.X = 0;
        }

        // 停止飞机状态
        public void endPlane()
        {
            currentState = State.ready;
            changeIndex();
        }
        public void drop()
        {
            currentState = State.ready;
            speed.Y = 4;
            changeIndex();
        }

        private void dealAccelerometerData()
        {
            position += new Vector2(accelerometerDataX * 8, accelerometerDataY);
            if (accelerometerDataX < -0.05)
            {
                currentOrientation = Orientation.left;
            }
            if (accelerometerDataX > 0.05)
            {
                currentOrientation = Orientation.right;
            }

        }

        public Rectangle GetCollisionRect()
        {
            return new Rectangle((int)(position.X + collisionOffsetX * scale),
                (int)(position.Y + collisionOffsetY * scale),
                (int)((frameSizes[textureIndex].X - 2 * collisionOffsetX) * scale),
                (int)((frameSizes[textureIndex].Y - 2 * collisionOffsetY) * scale));
        }

        public void AccelerometerDataChanged(object sender,
          AccelerometerReadingEventArgs e)
        {
            accelerometerDataX = (float)e.X;
        }
    }
}
