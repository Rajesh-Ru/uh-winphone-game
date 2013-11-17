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

namespace Unfailable_Heart.Run_n_Gun
{
    class UserControlledSprite : Sprite
    {
        public enum Status { FORWARDING, BACKWARDING, CHARGING, JUMPING, FALLING, BEATEN, ATTACKING, TURBO }
        bool[] blockDirections = new bool[4];
        public Status status { get; private set; }
        public int shotCount;
        const float shotBasicSpeed = 20;
        Vector2 shotSpeed;
        public Vector2 speedOfLastUpdate;
        Accelerometer accelerometer;
        float accelerometerDataY = 0; // Accelerometer readings range from -2 to 2
        const float FORWARDING_SPEED_X = 10;
        const float BACKWARDING_SPEED_X = 10;
        const float JUMPING_INITIAL_SPEED_Y = -15;
        const float FALLING_INITIAL_SPEED_Y = 1;
        bool statusChanged = false;
        public const float groundLineY = 275; // The floor for player
        const float turboLineY = 195;
        public float BaseLineY
        {
            get { return baseLineY; }
            set { baseLineY = value; }
        }
        float baseLineY = groundLineY;
        int beatenTimer = 0;
        const int timeToResumeFromBeaten = 132;
        int chargingTimer = 0;
        const int chargingTime = 66;
        int fallOverTimer = 0;
        const int timeToResumeFromFallingOver = 666;
        int attackingTimer = 0;
        const int timeToStopAttack = 132;
        Status preStatus;
        public bool FACINATED = false;
        int fancyTimer = 0;
        const int timeToResumeFromFancy = 5000;
        VibrateController vibrateController = VibrateController.Default;

        public UserControlledSprite(Texture2D[] textureImages, Vector2 position, Point[] frameSizes,
            int collisionOffset, Point currentFrame, Point[] sheetSizes, Vector2 speed,
            SoundEffect collisionSound, int millisecondsPerFrame)
            : base(textureImages, position, frameSizes, collisionOffset, currentFrame,
            sheetSizes, speed, millisecondsPerFrame, collisionSound, 0)
        {
            status = Status.FORWARDING;
            shotCount = 0;
            speedOfLastUpdate = Vector2.Zero;
            for (int i = 0; i < 4; i++)
                blockDirections[i] = false;

            // Accelerometer set up
            accelerometer = new Accelerometer();

            accelerometer.ReadingChanged +=
                new EventHandler<AccelerometerReadingEventArgs>(
                    AccelerometerDataChanged);

            accelerometer.Start();

            // Enable gestures
            TouchPanel.EnabledGestures = GestureType.Tap | GestureType.Flick;
        }

        public override void Update(GameTime gameTime, Rectangle clientBounds,
            UnfailableHeartSpriteManager spriteManager)
        {
            Rectangle playerCollisionRect = GetCollisionRect();

            // Figure out what is the player's base before updating speed
            if (!blockDirections[3] && status != Status.TURBO)
            {
                baseLineY = groundLineY;
            }

            DetectStatus();

            StatusOrientedUpdate(gameTime, spriteManager);

            if (FACINATED)
            {
                fancyTimer += gameTime.ElapsedGameTime.Milliseconds;
                if (fancyTimer > timeToResumeFromFancy)
                {
                    fancyTimer = 0;
                    FACINATED = false;
                }
                // Walk in an invertive pattern
                speedOfLastUpdate = new Vector2(accelerometerDataY * speed.X, speed.Y);
            }
            else
            {
                speedOfLastUpdate = new Vector2(-accelerometerDataY * speed.X, speed.Y);
            }

            if ((blockDirections[0] && speedOfLastUpdate.X + spriteManager.shiftingSpeed.X < 0)
                || (blockDirections[1] && speedOfLastUpdate.X + spriteManager.shiftingSpeed.X > 0 )
                || status == Status.FALLING)
            {
                speedOfLastUpdate.X = -spriteManager.shiftingSpeed.X;
            }

            if (blockDirections[2] && speed.Y < 0)
            {
                speedOfLastUpdate.Y = 0;
            }

            if ((playerCollisionRect.Left - 45 >= clientBounds.Left && playerCollisionRect.Right + 5 <= clientBounds.Right)
                || (playerCollisionRect.Left - 45 < clientBounds.Left && speedOfLastUpdate.X > 0)
                || (playerCollisionRect.Right + 5 > clientBounds.Right && speedOfLastUpdate.X < 0)
                || blockDirections[1] || status == Status.FALLING)
            {
                position.X += speedOfLastUpdate.X;
            }
            position.Y += speedOfLastUpdate.Y;

            base.Update(gameTime, clientBounds, spriteManager);
        }

        private void DetectStatus()
        {
            switch (status)
            {
                case Status.FORWARDING:
                    while (TouchPanel.IsGestureAvailable)
                    {
                        GestureSample gestureSample = TouchPanel.ReadGesture();
                        
                        switch (gestureSample.GestureType)
                        {
                            case GestureType.Tap:
                                if (speed.Y == 0)
                                {
                                    status = Status.CHARGING;
                                    statusChanged = true;
                                }
                                break;
                            case GestureType.Flick:
                                if (gestureSample.Delta.X > 0 && shotCount > 0)
                                {
                                    preStatus = status;
                                    status = Status.ATTACKING;
                                    shotSpeed = new Vector2(shotBasicSpeed, 0);
                                    statusChanged = true;
                                }
                                break;
                        }
                    }

                    if (status != Status.CHARGING && status != Status.ATTACKING
                        && accelerometerDataY > 0)
                    {
                        status = Status.BACKWARDING;
                        statusChanged = true;
                    }
                    break;
                case Status.BACKWARDING:
                    while (TouchPanel.IsGestureAvailable)
                    {
                        GestureSample gestureSample = TouchPanel.ReadGesture();

                        switch(gestureSample.GestureType)
                        {
                            case GestureType.Tap:
                                if (speed.Y == 0)
                                {
                                    status = Status.CHARGING;
                                    statusChanged = true;
                                }
                                break;
                            case GestureType.Flick:
                                if (gestureSample.Delta.X > 0 && shotCount > 0)
                                {
                                    preStatus = status;
                                    status = Status.ATTACKING;
                                    shotSpeed = new Vector2(shotBasicSpeed, 0);
                                    statusChanged = true;
                                }
                                break;
                        }
                    }

                    if (status != Status.CHARGING && accelerometerDataY < 0)
                    {
                        status = Status.FORWARDING;
                        statusChanged = true;
                    }
                    break;
                case Status.JUMPING:
                    while (TouchPanel.IsGestureAvailable)
                    {
                        GestureSample gestureSample = TouchPanel.ReadGesture();

                        switch (gestureSample.GestureType)
                        {
                            case GestureType.Flick:
                                if (gestureSample.Delta.X > 0 && shotCount > 0)
                                {
                                    preStatus = status;
                                    status = Status.ATTACKING;
                                    shotSpeed = new Vector2(shotBasicSpeed, 0);
                                    statusChanged = true;
                                }
                                break;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void StatusOrientedUpdate(GameTime gameTime,
            UnfailableHeartSpriteManager spriteManager)
        {
            switch (status)
            {
                case Status.FORWARDING:
                    if (statusChanged)
                    {
                        ChangeTextureAnimated(0);
                        speed = new Vector2(FORWARDING_SPEED_X, speed.Y);
                        statusChanged = false;
                    }

                    speed.Y++;
                    if (position.Y + speed.Y > baseLineY)
                    {
                        position.Y = baseLineY;
                        speed.Y = 0;
                    }
                    break;
                case Status.BACKWARDING:
                    if (statusChanged)
                    {
                        ChangeTextureAnimated(1);
                        speed = new Vector2(BACKWARDING_SPEED_X, speed.Y);
                        statusChanged = false;
                    }

                    speed.Y++;
                    if (position.Y + speed.Y > baseLineY)
                    {
                        position.Y = baseLineY;
                        speed.Y = 0;
                    }
                    break;
                case Status.CHARGING:
                    if (statusChanged)
                    {
                        ChangeTextureAnimated(2);
                        chargingTimer = 0;
                        statusChanged = false;
                    }
                    else
                    {
                        chargingTimer += gameTime.ElapsedGameTime.Milliseconds;
                        if (chargingTimer > chargingTime)
                        {
                            status = Status.JUMPING;
                            statusChanged = true;
                        }
                    }
                    break;
                case Status.JUMPING:
                    if (statusChanged)
                    {
                        collisionSound.Play();
                        ChangeTextureAnimated(3);
                        speed = new Vector2(speed.X, JUMPING_INITIAL_SPEED_Y);
                        statusChanged = false;
                    }
                    else
                    {
                        speed.Y++;
                        if (position.Y + speed.Y > baseLineY)
                        {
                            position.Y = baseLineY;
                            speed.Y = 0;
                            status = Status.FORWARDING;
                            statusChanged = true;
                        }
                    }
                    break;
                case Status.ATTACKING:
                    if (statusChanged)
                    {
                        // spawn a shot
                        spriteManager.SpawnShot(position, shotSpeed);

                        ChangeTextureAnimated(4);
                        statusChanged = false;
                        attackingTimer = 0;
                    }
                    else
                    {
                        attackingTimer += gameTime.ElapsedGameTime.Milliseconds;
                        if (attackingTimer > timeToStopAttack)
                        {
                            if (preStatus == Status.JUMPING)
                            {
                                ChangeTextureAnimated(3);
                                status = Status.JUMPING;
                            }
                            else
                            {
                                status = Status.FORWARDING;
                                statusChanged = true;
                            }
                        }
                    }

                    speed.Y++;
                    if (position.Y + speed.Y > baseLineY)
                    {
                        position.Y = baseLineY;
                        speed.Y = 0;
                    }
                    break;
                case Status.BEATEN:
                    if (statusChanged)
                    {
                        ChangeTextureAnimated(5);
                        statusChanged = false;
                        beatenTimer = 0;
                        vibrateController.Start(TimeSpan.FromMilliseconds(100));
                    }
                    else
                    {
                        beatenTimer += gameTime.ElapsedGameTime.Milliseconds;
                        if (beatenTimer > timeToResumeFromBeaten)
                        {
                            if (preStatus == Status.JUMPING)
                            {
                                ChangeTextureAnimated(3);
                                status = Status.JUMPING;
                            }
                            else
                            {
                                status = Status.FORWARDING;
                                statusChanged = true;
                            }
                        }
                    }

                    speed.Y++;
                    if (position.Y + speed.Y > baseLineY)
                    {
                        position.Y = baseLineY;
                        speed.Y = 0;
                    }
                    break;
                case Status.FALLING:
                    if (statusChanged)
                    {
                        ChangeTextureAnimated(6);
                        fallOverTimer = 0;
                        statusChanged = false;
                        vibrateController.Start(TimeSpan.FromMilliseconds(200));
                    }
                    else
                    {
                        fallOverTimer += gameTime.ElapsedGameTime.Milliseconds;
                        if (fallOverTimer > timeToResumeFromFallingOver)
                        {
                            if (preStatus == Status.JUMPING)
                            {
                                ChangeTextureAnimated(3);
                                status = Status.JUMPING;
                            }
                            else
                            {
                                status = Status.FORWARDING;
                                statusChanged = true;
                            }
                        }
                    }

                    speed.Y++;
                    if (position.Y + speed.Y > baseLineY)
                    {
                        position.Y = baseLineY;
                        speed.Y = 0;
                    }
                    break;
                case Status.TURBO:
                    if (statusChanged)
                    {
                        baseLineY = turboLineY;
                        ChangeTextureAnimated(7);
                        statusChanged = false;
                    }
                    speed.Y++;
                    if (position.Y + speed.Y > baseLineY)
                    {
                        position.Y = baseLineY;
                        speed.Y = 0;
                    }
                    break;
            }
        }

        public void ChangeStatus(UserControlledSprite.Status newStatus)
        {
            if (status != newStatus)
            {
                preStatus = status;
                status = newStatus;
                statusChanged = true;
            }
        }

        public override Rectangle GetCollisionRect()
        {
            return new Rectangle((int)(position.X + 3.3f * collisionOffset * scale),
                (int)(position.Y + collisionOffset * scale),
                (int)((frameSizes[textureIndex].X - 5f * collisionOffset) * scale),
                (int)((frameSizes[textureIndex].Y - 2 * collisionOffset) * scale)); ;
        }

        public void BlockFromRight()
        {
            blockDirections[1] = true;
        }

        public void BlockFromLeft()
        {
            blockDirections[0] = true;
        }

        public void BlockFromTop()
        {
            blockDirections[2] = true;
        }

        public void BlockFromBottom(float newBaseLineY)
        {
            blockDirections[3] = true;
            position.Y = baseLineY = newBaseLineY;
        }

        public void ClearBlock()
        {
            for (int i = 0; i < 4; i++)
                blockDirections[i] = false;
        }

        public void AccelerometerDataChanged(object sender,
            AccelerometerReadingEventArgs e)
        {
            accelerometerDataY = (float)e.Y;
        }
    }
}
