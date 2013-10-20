using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace rts
{
    public abstract class GameState
    {
        static protected GraphicsDeviceManager Graphics = Game1.Game.Graphics;
        static protected GraphicsDevice GraphicsDevice = Game1.Game.GraphicsDevice;
        static protected ContentManager Content = Game1.Game.Content;
        static protected SoundEffectManager soundEffectManager = Game1.Game.SoundEffectManager;
        protected EventHandler callback;
        protected bool contentLoaded;

        public GameState(EventHandler callback)
        {
            this.callback = callback;
        }

        public virtual void Update(GameTime gameTime)
        {
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
        }

        static bool allowMute = true;
        public static void checkForMute()
        {
            if (Keyboard.GetState(PlayerIndex.One).IsKeyUp(Keys.D0))
                allowMute = true;
            else if (allowMute && Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.D0))
            {
                MediaPlayer.IsMuted ^= true;
                //LoopingSoundEffect.IsMuted ^= true;
                allowMute = false;
            }
        }

        protected void returnControl()
        {
            callback.Invoke(this, new GameStateArgs(new string[0]));
        }
        protected void returnControl(params string[] args)
        {
            callback.Invoke(this, new GameStateArgs(args));
        }
        /*protected void returnControl(params Character[] chars)
        {
            callback.Invoke(this, new CharacterArgs(chars));
        }*/
    }

    public class GameStateArgs : EventArgs
    {
        private string[] args;
        public GameStateArgs() { }
        public GameStateArgs(params string[] args)
        {
            this.args = args;
        }
        public string[] Args
        {
            get
            {
                return args;
            }
        }
    }

    /*class CharacterArgs : EventArgs
    {
        Character[] chars;
        public CharacterArgs() { }
        public CharacterArgs(params Character[] chars)
        {
            this.chars = chars;
        }
        public Character[] Chars
        {
            get
            {
                return chars;
            }
        }
    }*/

    public class StartGameArgs : EventArgs
    {
        public Lidgren.Network.NetPeer NetPeer { get; private set; }
        public short Team { get; private set; }

        public StartGameArgs(Lidgren.Network.NetPeer netPeer, short team)
        {
            Team = team;
            NetPeer = netPeer;
        }
    }
}