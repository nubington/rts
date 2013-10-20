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
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public static Game1 Game { get; private set; }
        GameState currentGameState;

        public GraphicsDeviceManager Graphics { get; private set; }
        SpriteBatch spriteBatch;
        FpsCounter fpsCounter;
        public SoundEffectManager SoundEffectManager { get; private set; }
        public DebugMonitor DebugMonitor { get; private set; }

        public Game1()
        {
            Game = this;
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            Graphics.PreferredBackBufferWidth = 1024;
            Graphics.PreferredBackBufferHeight = 740;
            Graphics.ApplyChanges();

            Direction.Init();
            CommandCard.Init();

            fpsCounter = new FpsCounter(Game);
            Components.Add(fpsCounter);
            fpsCounter.DrawOrder = 0;
            //fpsCounter.Enabled = true;

            SoundEffectManager = new SoundEffectManager(Game);
            Components.Add(SoundEffectManager);
            SoundEffectManager.DrawOrder = 1;
            //soundEffectManager.DrawDebugInfo = true;

            DebugMonitor = new DebugMonitor(Game);
            Components.Add(DebugMonitor);
            DebugMonitor.Enabled = true;
            DebugMonitor.DrawBox = true;
            DebugMonitor.Position = Direction.SouthEast;

            ColorTexture.Initialize(GraphicsDevice);

            currentGameState = new Rts(RtsEventHandler);

            base.Initialize();
        }
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            currentGameState.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            currentGameState.Draw(spriteBatch);

            base.Draw(gameTime);
        }

        void RtsEventHandler(Object sender, EventArgs e)
        {
            if (e is GameStateArgs)
            {
                GameStateArgs args = (GameStateArgs)e;
                if (args.Args.Length > 0)
                {
                    if (args.Args[0] == "exit")
                    {
                        Game.Exit();
                    }
                }
            }
        }
    }
}
