using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace rts
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        //[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
       // public static extern bool ClipCursor(ref rcClip);
        
        public static Game1 Game { get; private set; }
        public GameState CurrentGameState { get; private set; }

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
            //Graphics.PreferredBackBufferWidth = 1024;
            //Graphics.PreferredBackBufferHeight = 740;
            //Graphics.PreferredBackBufferWidth = 1280;
            //Graphics.PreferredBackBufferHeight = 720;
            Graphics.PreferredBackBufferWidth = 1024;
            Graphics.PreferredBackBufferHeight = 576;
            Graphics.ApplyChanges();

            Window.Title = "";

            IsMouseVisible = true;

            Direction.Init();
            CommandCard.Init();
            UnitType.Init();
            StructureType.Init();

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
            //DebugMonitor.Enabled = true;
            DebugMonitor.DrawBox = true;
            DebugMonitor.Position = Direction.SouthEast;

            ColorTexture.Initialize(GraphicsDevice);

            //CurrentGameState = new Rts(RtsEventHandler);
            CurrentGameState = new StartMenu(StartMenuEventHandler);

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
            CurrentGameState.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            CurrentGameState.Draw(spriteBatch);

            base.Draw(gameTime);
        }

        void StartMenuEventHandler(Object sender, EventArgs e)
        {
            if (e is StartGameArgs)
            {
                StartGameArgs args = (StartGameArgs)e;

                CurrentGameState = new Rts(RtsEventHandler, args.NetPeer, args.Team);
            }
            else if (e is GameStateArgs)
            {
                GameStateArgs args = (GameStateArgs)e;
                if (args.Args.Length > 0)
                {
                    if (args.Args[0] == "exit")
                    {
                        Game.Exit();
                    }
                    else if (args.Args[0] == "create")
                    {
                        CurrentGameState = new HostLobby(HostLobbyEventHandler);
                    }
                }
            }
        }
        void HostLobbyEventHandler(Object sender, EventArgs e)
        {
            if (e is StartGameArgs)
            {
                StartGameArgs args = (StartGameArgs)e;

                CurrentGameState = new Rts(RtsEventHandler, args.NetPeer, args.Team);
            }
            else if (e is GameStateArgs)
            {
                GameStateArgs args = (GameStateArgs)e;
                if (args.Args.Length > 0)
                {
                }
            }
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
