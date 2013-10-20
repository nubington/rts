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


namespace rts
{
    public class FpsCounter : Microsoft.Xna.Framework.DrawableGameComponent
    {
        int frameCounter, fps;
        TimeSpan elapsedTime;

        bool enabled;

        public FpsCounter(Game game)
            : base(game)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (!enabled)
                return;

            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                fps = frameCounter;
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameCounter = 0;
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (enabled)
            {
                frameCounter++;

                //Game1.Game.DebugMonitor.AddLine("FPS: " + fps);
                Game1.Game.DebugMonitor.InsertLine(0, "FPS: " + fps);
            }
        }

        new public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;
                if (value)
                    elapsedTime = TimeSpan.Zero;
            }
        }
    }
}
