using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace rts
{
    public class Animation
    {
        protected Texture2D[] textures;
        //private int index = 0;
        private int currentTexture;
        private float duration;
        private float frameDelay;
        //private float timeSinceLastFrame;
        private Stopwatch timer;

        public Animation(float duration, float framesPerSecond, params Texture2D[] textures)
        {
            this.textures = textures;
            this.duration = duration;
            frameDelay = 1 / framesPerSecond;
            currentTexture = 0;
            timer = new Stopwatch();
        }
        public Animation(float duration, params Texture2D[] textures)
        {
            this.textures = textures;
            this.duration = duration;
            frameDelay = duration / textures.Length;
            currentTexture = 0;
            timer = new Stopwatch();
        }

        public void Start()
        {
            //index = 0;
            //timeSinceLastFrame = 0;
            timer.Restart();
        }
        public void Stop()
        {
            currentTexture = 0;
            timer.Stop();
        }

        public void Pause()
        {
            timer.Stop();
        }
        public void Resume()
        {
            timer.Start();
        }

        float time = 0f;
        public virtual void Update(GameTime gameTime)
        {
            if (!IsRunning)
                return;

            if (duration > 0 && timer.Elapsed.TotalSeconds >= duration)
            {
                Stop();
                return;
            }

            time += (float)gameTime.ElapsedGameTime.TotalSeconds * Rts.GameSpeed;
            if (time >= frameDelay)
            {
                time -= frameDelay;

                currentTexture = ++currentTexture % textures.Length;
            }

            currentTexture = (int)(timer.Elapsed.TotalSeconds / frameDelay) % textures.Length;
            /*timeSinceLastFrame += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timeSinceLastFrame >= frameDelay)
            {
                currentTexture = ++index % textures.Length;
                timeSinceLastFrame -= frameDelay;
            }*/
        }

        public int CurrentTexture
        {
            get
            {
                return currentTexture;
            }
        }
        public bool IsRunning
        {
            get
            {
                return timer.IsRunning;
            }
        }
        public float ElapsedSeconds
        {
            get
            {
                return (float)timer.Elapsed.TotalSeconds;
            }
        }
        public float Duration
        {
            get
            {
                return duration;
            }
        }

        static public implicit operator Texture2D(Animation o)
        {
            return o.textures[o.currentTexture];
        }
    }

    /*class SpriteSheetAnimation
    {
        Texture2D spriteSheet;

        Point frameSize;
        Point sheetSize;
        Point currentFrame;

        float frameDelay, timeSinceLastFrame;
        float duration;

        public Stopwatch timer;

        public SpriteSheetAnimation(Texture2D spriteSheet, Point frameSize, Point sheetSize, float fps, float duration)
        {
            this.spriteSheet = spriteSheet;

            this.frameSize = frameSize;
            this.sheetSize = sheetSize;

            frameDelay = 1 / fps;
            this.duration = duration;

            timer = new Stopwatch();
        }

        public SpriteSheetAnimation(Texture2D spriteSheet, Point sheetSize, float duration)
        {
            this.spriteSheet = spriteSheet;

            //this.frameSize = frameSize;
            frameSize = new Point(spriteSheet.Width / sheetSize.X, spriteSheet.Height / sheetSize.Y);
            this.sheetSize = sheetSize;

            frameDelay = duration / (sheetSize.X * sheetSize.Y);
            this.duration = duration;

            timer = new Stopwatch();
        }

        public void Start()
        {
            timer.Restart();
        }
        public void Stop()
        {
            currentFrame = new Point(0, 0);
            timer.Stop();
        }

        public void Pause()
        {
            timer.Stop();
        }
        public void Resume()
        {
            timer.Start();
        }

        public void Update(GameTime gameTime)
        {
            if (!IsRunning)
                return;
            if (duration > 0 && timer.Elapsed.TotalSeconds >= duration)
            {
                Stop();
                return;
            }

            timeSinceLastFrame += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timeSinceLastFrame >= frameDelay)
            {
                timeSinceLastFrame -= frameDelay;

                currentFrame.X++;
                if (currentFrame.X >= sheetSize.X)
                {
                    currentFrame.X = 0;
                    currentFrame.Y++;
                    if (currentFrame.Y >= sheetSize.Y)
                        currentFrame.Y = 0;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(spriteSheet, new Rectangle(),
                new Rectangle(currentFrame.X * frameSize.X, currentFrame.Y * frameSize.Y, frameSize.X, frameSize.Y), 
                Color.White);
        }

        public Point CurrentFrame
        {
            get
            {
                return currentFrame;
            }
        }
        public bool IsRunning
        {
            get
            {
                return timer.IsRunning;
            }
        }
        public float ElapsedSeconds
        {
            get
            {
                return (float)timer.Elapsed.TotalSeconds;
            }
        }
        public float Duration
        {
            get
            {
                return duration;
            }
        }
    }*/

    public class SpriteSheetAnimation : Animation
    {
        public SpriteSheetAnimation(Texture2D spriteSheet, int frameWidth, int frameHeight, float duration, float fps)
            : base(duration, fps, Util.SplitTexture(spriteSheet, frameWidth, frameHeight))
        {
        }

        public SpriteSheetAnimation(Texture2D spriteSheet, int frameWidth, int frameHeight, float duration)
            : base(duration, Util.SplitTexture(spriteSheet, frameWidth, frameHeight))
        {
        }
    }

    public class UnitAnimation : Animation
    {
        public static List<UnitAnimation> UnitAnimations = new List<UnitAnimation>();

        public RtsObject Unit;
        public Rectangle Rectangle;
        public float Rotation;
        public bool StayAfterDeath;

        public UnitAnimation(RtsObject unit, int size, float duration, bool stayAfterDeath, params Texture2D[] textures)
            : base(duration, textures)
        {
            Unit = unit;
            Rectangle = new Rectangle(0, 0, size, size);
            Rectangle.Location = new Point((int)Unit.CenterPoint.X - Rectangle.Width / 2, (int)Unit.CenterPoint.Y - Rectangle.Height / 2);
            Rotation = Unit.Rotation;
            StayAfterDeath = stayAfterDeath;
            UnitAnimations.Add(this);
        }
        public UnitAnimation(RtsObject unit, int size, float duration, float fps, bool stayAfterDeath, params Texture2D[] textures)
            : base(duration, fps, textures)
        {
            Unit = unit;
            Rectangle = new Rectangle(0, 0, size, size);
            Rectangle.Location = new Point((int)Unit.CenterPoint.X - Rectangle.Width / 2, (int)Unit.CenterPoint.Y - Rectangle.Height / 2);
            Rotation = Unit.Rotation;
            StayAfterDeath = stayAfterDeath;
            UnitAnimations.Add(this);
        }
        public UnitAnimation(RtsObject unit, int size, float duration, bool stayAfterDeath, Texture2D spriteSheet, int frameWidth, int frameHeight)
            : base(duration, Util.SplitTexture(spriteSheet, frameWidth, frameHeight))
        {
            Unit = unit;
            Rectangle = new Rectangle(0, 0, size, size);
            Rectangle.Location = new Point((int)Unit.CenterPoint.X - Rectangle.Width / 2, (int)Unit.CenterPoint.Y - Rectangle.Height / 2);
            Rotation = Unit.Rotation;
            StayAfterDeath = stayAfterDeath;
            UnitAnimations.Add(this);
        }
        public UnitAnimation(RtsObject unit, int size, float duration, float fps, bool stayAfterDeath, Texture2D spriteSheet, int frameWidth, int frameHeight)
            : base(duration, fps, Util.SplitTexture(spriteSheet, frameWidth, frameHeight))
        {
            Unit = unit;
            Rectangle = new Rectangle(0, 0, size, size);
            Rectangle.Location = new Point((int)Unit.CenterPoint.X - Rectangle.Width / 2, (int)Unit.CenterPoint.Y - Rectangle.Height / 2);
            Rotation = Unit.Rotation;
            StayAfterDeath = stayAfterDeath;
            UnitAnimations.Add(this);
        }

        public override void Update(GameTime gameTime)
        {
            Rectangle.Location = new Point((int)Unit.CenterPoint.X - Rectangle.Width / 2, (int)Unit.CenterPoint.Y - Rectangle.Height / 2);
            Rotation = Unit.Rotation;

            base.Update(gameTime);
        }

        public static void UpdateAll(GameTime gameTime)
        {
            for (int i = 0; i < UnitAnimations.Count; i++)
            {
                UnitAnimation a = UnitAnimations[i];

                a.Update(gameTime);

                if (!a.IsRunning || (!a.StayAfterDeath && a.Unit.IsDead))
                {
                    UnitAnimations.Remove(a);
                    i--;
                }
            }
        }
    }
}