using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace rts
{
    public class SoundEffectManager : Microsoft.Xna.Framework.DrawableGameComponent
    {
        List<SoundEffectInstance> soundEffectInstances = new List<SoundEffectInstance>();

        bool drawDebugInfo = false;

        int cleanupCounter = 0;

        public SoundEffectManager(Game game)
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
            if (++cleanupCounter >= 60)
            {
                Cleanup();
                cleanupCounter = 0;
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (drawDebugInfo)
            {
                Game1.Game.DebugMonitor.AddLine(soundEffectInstances.Count + " sound effects.");
            }
        }

        public void Add(SoundEffectInstance s)
        {
            soundEffectInstances.Add(s);
        }

        public void Play(SoundEffect s, float volume)
        {
            if (MediaPlayer.IsMuted)
                return;
            SoundEffectInstance si = s.CreateInstance();
            si.Volume = volume;
            si.Play();
            Add(si);
        }

        public void Cleanup()
        {
            for (int i = 0; i < soundEffectInstances.Count; )
            {
                SoundEffectInstance s = soundEffectInstances[i];
                if (s.State == SoundState.Stopped)
                    soundEffectInstances.Remove(s);
                else
                    i++;
            }
        }

        public bool DrawDebugInfo
        {
            get
            {
                return drawDebugInfo;
            }
            set
            {
                drawDebugInfo = value;
            }
        }
    }
}
