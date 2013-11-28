using System;
using System.Collections.Generic;
//using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using System.Diagnostics;
using System.Windows.Forms;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using Lidgren.Network;
using Lidgren.Network.Xna;

namespace rts
{
    public partial class Rts : GameState
    {
        public static float GameSpeed = 2.5f;
        public static float MusicVolume = 0f;//.2f;

        public static GameTime gameTime;

        Random rand = new Random();
        //public static Stopwatch GameTimer = new Stopwatch();

        double timeForPathFindingProfiling, pathFindingPercentage;
        int pathFindQueueSize;
        
        static Song rtsMusic;
        static SoundEffect errorSoundEffect;

        // these references to the map and pathfinder are used by everything
        public static Map map;
        public static PathFinder pathFinder;

        int actualMapWidth, actualMapHeight;

        VisionUpdater VisionUpdater;

        public Rts(EventHandler callback, Lidgren.Network.NetPeer netpeer, short myTeam)
            : base(callback)
        {
            netPeer = netpeer;
            iAmServer = netPeer is NetServer;

            Game1.Game.DebugMonitor.Position = Direction.NorthEast;
            //Game1.Game.Graphics.SynchronizeWithVerticalRetrace = false;
            //Game1.Game.IsFixedTimeStep = false;
            //Game1.Game.Graphics.ApplyChanges();
            //GameTimer.Restart();

            //map = new Map(@"Content/map1.muh");
            map = new Map("C:\\rts maps\\map2.muh");
            pathFinder = new PathFinder(map);
            map.InstantiateMapResources();
            //Player.Me.Team = myTeam;
            Player.Me = Player.Players[myTeam];

            actualMapWidth = map.Width * map.TileSize;
            actualMapHeight = map.Height * map.TileSize;

            Unit.UnitCollisionSweeper.Thread.Suspend();
            Unit.UnitCollisionSweeper.Thread.Resume();
            Rts.pathFinder.ResumeThread();

            uiViewport = GraphicsDevice.Viewport;
            worldViewport = GraphicsDevice.Viewport;
            minimapViewport = new Viewport(minimapBorderSize, uiViewport.Height - minimapSize - minimapBorderSize, minimapSize, minimapSize);
            //minimapViewport = new Viewport(0, 0, minimapSize, minimapSize);
            worldViewport.Height -= (minimapSize + minimapBorderSize * 2);
            GraphicsDevice.Viewport = worldViewport;

            camera = new Camera();
            camera.Pos = new Vector2(worldViewport.Width / 2, worldViewport.Height / 2);

            button1 = new BaseObject(new Rectangle(10, 25, 25, 25));
            button2 = new BaseObject(new Rectangle(10, 52, 25, 25));
            button3 = new BaseObject(new Rectangle(10, 79, 25, 25));
            button4 = new BaseObject(new Rectangle(10, 106, 25, 25));
            button5 = new BaseObject(new Rectangle(10, 133, 25, 25));

            if (!contentLoaded)
            {
                pauseFont = Content.Load<SpriteFont>("spritefonts/pausefont");
                fpsFont = Content.Load<SpriteFont>("spritefonts/fpsfont");
                unitInfoUnitNameFont = Content.Load<SpriteFont>("spritefonts/UnitInfoUnitNameFont");
                unitInfoHpFont = Content.Load<SpriteFont>("spritefonts/UnitInfoHpFont");
                unitInfoKillCountFont = Content.Load<SpriteFont>("spritefonts/UnitInfoKillCountFont");
                resourceCountFont = Content.Load<SpriteFont>("spritefonts/ResourceCountFont");
                bigFont = Content.Load<SpriteFont>("spritefonts/BigMessage");
                //brownGuyTexture = Content.Load<Texture2D>("unit textures/browncircleguy");
                //brownGuySelectingTexture = Content.Load<Texture2D>("unit textures/browncircleguyselected2");
                //brownGuySelectedTexture = Content.Load<Texture2D>("unit textures/browncircleguyselecting2");
                greenTeamIndicatorTexture = Content.Load<Texture2D>("unit textures/green team indicator");
                redTeamIndicatorTexture = Content.Load<Texture2D>("unit textures/red team indicator");
                buttonTexture = Content.Load<Texture2D>("titlebutton1");
                moveCommandShrinkerTexture = Content.Load<Texture2D>("greencircle2");
                attackMoveCommandShrinkerTexture = Content.Load<Texture2D>("redcircle2");
                //normalCursorTexture = Content.Load<Texture2D>("greencursor2");
                //attackCommandCursorTexture = Content.Load<Texture2D>("crosshair");
                normalCursor = Util.LoadCustomCursor(@"Content/cursors/SC2-cursor.cur");
                attackCursor = Util.LoadCustomCursor(@"Content/cursors/SC2-target-none.cur");
                boulder1Texture = Content.Load<Texture2D>("boulder1");
                tree1Texture = Content.Load<Texture2D>("tree2");
                rallyFlagTexture = Content.Load<Texture2D>("redflag");
                redCircleTexture = Content.Load<Texture2D>("redcircle");
                transparentTexture = Content.Load<Texture2D>("transparent");
                transparentGrayTexture = Content.Load<Texture2D>("transparentgray");
                transparentBlackTexture = Content.Load<Texture2D>("transparentblack");
                whiteBoxTexture = Content.Load<Texture2D>("whitebox");
                cogWheelTexture = Content.Load<Texture2D>("cogwheel");
                rtsMusic = Content.Load<Song>("music/58 - Weapons Factory");
                errorSoundEffect = Content.Load<SoundEffect>("sounds/Error");
                //Unit.BulletTexture = Content.Load<Texture2D>("bullet");
                Unit.Explosion1Textures = Util.SplitTexture(Content.Load<Texture2D>("explosionsheet1"), 45, 45);
                Structure.Explosion1Textures = Util.SplitTexture(Content.Load<Texture2D>("explosionsheet1"), 45, 45);
                contentLoaded = true;
            }

            winForm = (Form)Form.FromHandle(Game1.Game.Window.Handle);
            //Cursor.Clip = new System.Drawing.Rectangle(winForm.Location, winForm.Size);
            winForm.Cursor = normalCursor;

            initializeMapTexture();
            initializeCommandCardArea();
            initializeSelectionInfoArea();
            line.Alpha = .75f;

            VisionUpdater = new VisionUpdater(map, Rts.pathFinder, Player.Me.Team);

            SelectBox.InitializeSelectBoxLine(GraphicsDevice, Color.Green);
            Initializeline(GraphicsDevice, Color.Yellow);

            minimapScreenIndicatorBoxLine = new PrimitiveLine(GraphicsDevice, 1);
            minimapScreenIndicatorBoxLine.Colour = Color.White;

            for (int i = 0; i < HotkeyGroups.Length; i++)
                HotkeyGroups[i] = new List<RtsObject>();

            MediaPlayer.Play(rtsMusic);
            MediaPlayer.Volume = MusicVolume;
            MediaPlayer.IsRepeating = true;

            /*new TownHall(map.StartingPoints[myTeam], myTeam);
            camera.Pos = new Vector2(map.StartingPoints[myTeam].X * map.TileSize, map.StartingPoints[myTeam].Y * map.TileSize);
            Player.Me.MaxSupply += StructureType.TownHall.Supply;*/
            initializeStartingPoints();

            //new Barracks(new Point(10, 14), 2);
            //new Roks(new Point(3, 3));
            //new Roks(new Point(3, 30));

            Player.Me.Roks = 25;

            if (iAmServer)
            {
                NetOutgoingMessage msg = netPeer.CreateMessage();
                msg.Write(0);
                netPeer.SendMessage(msg, netPeer.Connections[0], NetDeliveryMethod.ReliableUnordered);

                while (true)
                {
                    NetIncomingMessage m;

                    if ((m = netPeer.ReadMessage()) != null)
                    {
                        if (m.ReadInt32() == 0)
                        {
                            NetOutgoingMessage mm = netPeer.CreateMessage();
                            msg.Write(1);
                            msg.Write(gameClock);
                            netPeer.SendMessage(mm, netPeer.Connections[0], NetDeliveryMethod.ReliableUnordered);

                            break;
                        }
                        netPeer.Recycle(m);
                    }
                }
            }
            else
            {
                while (true)
                {
                    NetIncomingMessage msg;
                    if ((msg = netPeer.ReadMessage()) != null)
                    {
                        if (msg.MessageType == NetIncomingMessageType.Data)
                        {
                            int id = msg.ReadInt32();
                            if (id == 0)
                            {
                                NetOutgoingMessage m = netPeer.CreateMessage();
                                m.Write(0);
                                netPeer.SendMessage(m, netPeer.Connections[0], NetDeliveryMethod.ReliableUnordered);
                            }
                            else if (id == 1)
                            {
                                float clock = msg.ReadFloat();

                                //gameClock = netPeer.Connections[0].AverageRoundtripTime / 2f;
                                //countDownTime -= netPeer.Connections[0].AverageRoundtripTime / 2f;

                                gameClock = clock + netPeer.Connections[0].AverageRoundtripTime / 2f;
                                countDownTime -= clock + netPeer.Connections[0].AverageRoundtripTime / 2f;

                                netPeer.Recycle(msg);
                                break;
                            }
                            netPeer.Recycle(msg);
                        }
                    }
                }
            }

            connection = netPeer.Connections[0];

            // clear incoming messages
            NetIncomingMessage muh;
            while ((muh = netPeer.ReadMessage()) != null)
            {
                netPeer.Recycle(muh);
            }
        }

        void initializeStartingPoints()
        {
            foreach (Player player in Player.Players)
            {
                new TownHall(map.StartingPoints[player.Team], player.Team);
                player.MaxSupply += StructureType.TownHall.Supply;
            }

            camera.Pos = new Vector2(map.StartingPoints[Player.Me.Team].X * map.TileSize, map.StartingPoints[Player.Me.Team].Y * map.TileSize);
        }

        public override void Update(GameTime gameTime)
        {
            // check for exit
            /*if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Game1.Game.Exit();
            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Escape))
            {
                //Graphics.ToggleFullScreen();
                cleanup();
                returnControl("exit");
                return;
            }*/

            //Cursor.Clip = new System.Drawing.Rectangle(winForm.Location, winForm.Size);
            Rts.gameTime = gameTime;
            
            gameClock += (float)gameTime.ElapsedGameTime.TotalSeconds * GameSpeed;

            // count down
            if (countingDown)
            {
                countDownTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (countDownTime <= 0f)
                {
                    countingDown = false;
                    gameClock = 0f;
                }
                else
                {
                    if (iAmServer)
                    {
                        //NetOutgoingMessage msg = netPeer.CreateMessage();
                        //msg.Write(countDownTime);
                        //netPeer.SendMessage(msg, connection, NetDeliveryMethod.UnreliableSequenced, 0);
                        if (gameClock <= 3f)
                            checkToSync(gameTime);
                    }
                    else
                    {
                        NetIncomingMessage msg;
                        if ((msg = netPeer.ReadMessage()) != null)
                        {
                            if (msg.MessageType == NetIncomingMessageType.Data)
                            {
                                if (msg.ReadByte() == MessageID.SYNC && countingDown)
                                {
                                    gameClock = msg.ReadFloat() + connection.AverageRoundtripTime / 2f;
                                    countDownTime = 4f - gameClock;

                                    if (countDownTime <= 0f)
                                    {
                                        countingDown = false;
                                        gameClock = 0f;
                                    }
                                }
                                //countDownTime = msg.ReadFloat() - connection.AverageRoundtripTime / 2f;
                            }
                        }
                    }
                    return;
                }
            }

            if (waitingForMessage)
            {
                checkToCheckup(gameTime);
                receiveData(gameTime);
                gameClock -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                return;
            }

            // send time sync message if server
            checkToSync(gameTime);
            checkToCheckup(gameTime);

            receiveData(gameTime);

            // mute check
            checkForMute();

            // pause check
            /*if (Keyboard.GetState(PlayerIndex.One).IsKeyUp(Keys.P))
                allowPause = true;
            if (allowPause && Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.P))
            {
                paused ^= true;
                allowPause = false;
                if (paused)
                {
                    MediaPlayer.Volume /= 4;
                    //GameTimer.Stop();
                }
                else
                {
                    MediaPlayer.Volume *= 4;
                    //GameTimer.Start();
                }
            }*/

            // update mouse and keyboard state
            mouseState = Mouse.GetState();
            keyboardState = Keyboard.GetState();

            // pathfinding performance info
            timeForPathFindingProfiling += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (timeForPathFindingProfiling >= 500)
            {
                double pathFindingTime;
                lock (Rts.pathFinder.TimeSpentPathFindingLock)
                {
                    pathFindingTime = Rts.pathFinder.TimeSpentPathFinding.TotalMilliseconds;
                    Rts.pathFinder.TimeSpentPathFinding = TimeSpan.Zero;
                }
                pathFindingPercentage = pathFindingTime / timeForPathFindingProfiling * 100;
                timeForPathFindingProfiling = 0;

                lock (PathFindRequest.HighPriorityPathFindRequests)
                {
                    pathFindQueueSize = PathFindRequest.HighPriorityPathFindRequests.Count;
                }
            }

            // do nothing else if paused
            if (paused)
                return;

            doScheduledActions();
            doDelayedPathUpdates();

            checkForUnitStatusUpdates(gameTime);
            checkForStructureStatusUpdates(gameTime);

            //update stats
            updateStats(gameTime);

            //update fps
            fpsElapsedTime += gameTime.ElapsedGameTime;
            if (fpsElapsedTime > TimeSpan.FromSeconds(1))
            {
                //Game1.Game.Window.Title = "FPS: " + (frameCounter > 2 ? frameCounter.ToString() : "COOL");
                fpsMessage = "FPS: " + (frameCounter > 2 ? frameCounter.ToString() : "COOL");
                fpsMessage += " - Unit count: " + Unit.Units.Count;
                fpsElapsedTime -= TimeSpan.FromSeconds(1);
                frameCounter = 0;
            }

            // update buttons
            SimpleButton.UpdateAll(mouseState, keyboardState);

            /*if (button1.Rectangle.Contains(mouseState.X, mouseState.Y) && mouseState.LeftButton == ButtonState.Pressed)
            {
                Unit brownGuy;
                brownGuy = new RangedNublet(new Vector2(worldViewport.Width * .25f, worldViewport.Height / 2), 1);
                brownGuy.InitializeCurrentPathNode();
                //brownGuy.Texture = brownGuyTexture;
                //brownGuy.AddWayPoint(new Vector2(Graphics.GraphicsDevice.Viewport.Width * .75f, Graphics.GraphicsDevice.Viewport.Height / 2));
                brownGuy.GiveCommand(new MoveCommand(new Vector2(worldViewport.Width * .75f, worldViewport.Height / 2), 1));
            }*/
            /*if (button2.Rectangle.Contains(mouseState.X, mouseState.Y) && mouseState.LeftButton == ButtonState.Pressed)
            {
                for (int i = 0; i < 10; i++)
                {
                    Unit brownGuy;
                    brownGuy = new RangedNublet(new Vector2(worldViewport.Width * .25f, worldViewport.Height / 2), 0);
                    brownGuy.InitializeCurrentPathNode();
                    //brownGuy.Texture = brownGuyTexture;
                    //brownGuy.AddWayPoint(new Vector2(Graphics.GraphicsDevice.Viewport.Width * .75f, Graphics.GraphicsDevice.Viewport.Height / 2));
                    brownGuy.GiveCommand(new MoveCommand(brownGuy, new Vector2(worldViewport.Width * .75f, worldViewport.Height / 2), 1));
                }
            }/*
            else if (button3.Rectangle.Contains(mouseState.X, mouseState.Y) && mouseState.LeftButton == ButtonState.Pressed)
            {
                Graphics.ToggleFullScreen();
                Graphics.ApplyChanges();
                uiViewport = GraphicsDevice.Viewport;
                worldViewport = GraphicsDevice.Viewport;
                worldViewport.Height -= (minimapSize + minimapBorderSize * 2);
                GraphicsDevice.Viewport = worldViewport;
                initializeMapTexture();
                initializeCommandCardArea();
                initializeSelectionInfoArea();
            }
            else if (button4.Rectangle.Contains(mouseState.X, mouseState.Y) && mouseState.LeftButton == ButtonState.Pressed)
            {
                Unit meleeNublet;
                meleeNublet = new MeleeNublet(new Vector2(worldViewport.Width * .25f, worldViewport.Height / 2), 0);
                meleeNublet.InitializeCurrentPathNode();
                //brownGuy.Texture = brownGuyTexture;
                //brownGuy.AddWayPoint(new Vector2(Graphics.GraphicsDevice.Viewport.Width * .75f, Graphics.GraphicsDevice.Viewport.Height / 2));
                meleeNublet.GiveCommand(new MoveCommand(new Vector2(worldViewport.Width * .75f, worldViewport.Height / 2), 1));
            }
            else if (button5.Rectangle.Contains(mouseState.X, mouseState.Y) && mouseState.LeftButton == ButtonState.Pressed)
            {
                foreach (Unit unit in Unit.Units)
                {
                    unit.Die();
                }
            }*/

            /*if (SelectedUnits.Count == 1)
            {
                Game1.Game.Window.Title = "idle: " + SelectedUnits[0].IsIdle + " hp: " + SelectedUnits[0].Hp + "/" + SelectedUnits[0].MaxHp + ". position " + SelectedUnits[0].CenterPoint;
            }*/

            checkForShift(gameTime);

            checkForCommands();

            map.UpdateBoundingBoxes();

            updatePlacingStructure();
            updatePlacedStructures();

            updateCogWheels(gameTime);

            Shrinker.UpdateShrinkers(gameTime);

            checkHotKeyGroups(gameTime);

            if (!placingStructure)
                SelectBox.Update(worldViewport, camera);

            checkForLeftClick(gameTime);

            checkForRightClick();

            checkForTab();

            RtsBullet.UpdateAll(gameTime);

            Resource.UpdateResources(gameTime);
            Structure.UpdateStructures(gameTime);
            Unit.UpdateUnits(gameTime, netPeer, connection);
            UnitAnimation.UpdateAll(gameTime);

            removeDeadUnitsFromSelections();

            applyVisibilityToMap();

            checkForMouseCameraScroll(gameTime);
            checkForCameraZoom(gameTime);
            checkForCameraRotate(gameTime);
            if (keyboardState.IsKeyDown(Keys.Space))
                centerCameraOnSelectedUnits();
            clampCameraToMap();
        }

        void cleanup()
        {
            Unit.UnitCollisionSweeper.Thread.Abort();
            Rts.pathFinder.AbortThread();
            VisionUpdater.Thread.Abort();
            lock (Unit.Units)
            {
                Unit.Units.Clear();
            }
            Unit.UnitsSorted.Clear();
            /*lock (PathFindRequest.PathFindRequests)
            {
                for (int i = 0; i < PathFindRequest.PathFindRequests.Count; )
                    PathFindRequest.PathFindRequests.DeleteMax();
            }*/
            lock (PathFindRequest.HighPriorityPathFindRequests)
            {
                //PathFindRequest.HighPriorityPathFindRequests.Clear();
                while (PathFindRequest.HighPriorityPathFindRequests.Count > 0)
                {
                    PathFindRequest.HighPriorityPathFindRequests.DeleteMax();
                }
            }
            lock (PathFindRequest.LowPriorityPathFindRequests)
            {
                //PathFindRequest.LowPriorityPathFindRequests.Clear();
                while (PathFindRequest.LowPriorityPathFindRequests.Count > 0)
                {
                    PathFindRequest.LowPriorityPathFindRequests.DeleteMax();
                }
            }
            lock (PathFindRequest.DonePathFindRequestsLock)
            {
                PathFindRequest.DonePathFindRequests.Clear();
            }
            //Rts.pathFinder.SuspendThread();
            //Game1.Game.DebugMonitor.Position = Direction.SouthEast;
            //Game1.Game.IsMouseVisible = true;
            //GraphicsDevice.Viewport = uiViewport;
        }

        void initializeMapTexture()
        {
            // set minimap fields and create rectangle object
            minimapPosX = minimapBorderSize;
            minimapPosY = uiViewport.Height - minimapSize - minimapBorderSize;
            //minimapPosX = 0;
            //minimapPosY = 0;
            minimapToMapRatioX = (float)minimapSize / (map.Width * map.TileSize);
            minimapToMapRatioY = (float)minimapSize / (map.Height * map.TileSize);
            minimapToScreenRatioX = (float)minimapSize / worldViewport.Width;
            minimapToScreenRatioY = (float)minimapSize / worldViewport.Height;
            minimap = new Rectangle(minimapPosX, minimapPosY, minimapSize, minimapSize);
            //minimap = new Rectangle(0, 0, minimapSize, minimapSize);
            minimapScreenIndicatorBox = new BaseObject(new Rectangle(0, 0, (int)(worldViewport.Width * minimapToMapRatioX), (int)(worldViewport.Height * minimapToMapRatioY)));
            //fullMapTexture = new Texture2D(GraphicsDevice, map.Width * Map.TILESIZE, map.Height * Map.TILESIZE);
            //fullMapTexture = new RenderTarget2D(GraphicsDevice, map.Width * Map.TILESIZE, map.Height * Map.TILESIZE);

            // create full map texture from map tiles
            RenderTarget2D renderTarget = new RenderTarget2D(GraphicsDevice, map.Width * map.TileSize, map.Height * map.TileSize);
            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(Color.Gray);
            SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatch.Begin();
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    MapTile tile = map.Tiles[y, x];

                    if (tile.Type == 0)
                        spriteBatch.Draw(ColorTexture.Gray, tile.Rectangle, Color.White);
                    else if (tile.Type == 1)
                        spriteBatch.Draw(boulder1Texture, tile.Rectangle, Color.White);
                    else if (tile.Type == 2)
                        spriteBatch.Draw(tree1Texture, tile.Rectangle, Color.White);
                }
            }
            spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);
            //fullMapTexture = (Texture2D)renderTarget;
            fullMapTexture = new Texture2D(GraphicsDevice, map.Width * map.TileSize, map.Height * map.TileSize);
            Color[] textureData = new Color[(map.Width * map.TileSize) * (map.Height * map.TileSize)];
            renderTarget.GetData<Color>(textureData);
            fullMapTexture.SetData<Color>(textureData);

            //renderTarget = null;
            //GC.Collect();
        }

        void initializeCommandCardArea()
        {
            commandButtonsAreaPosX = uiViewport.Width - commandButtonsAreaWidth - 5;
            commandButtonsAreaPosY = minimapPosY;
            commandCardArea = new Rectangle(commandButtonsAreaPosX, commandButtonsAreaPosY, commandButtonsAreaWidth, minimapSize);
        }

        void initializeSelectionInfoArea()
        {
            selectionInfoAreaPosX = minimapPosX + minimapSize + 10;
            selectionInfoAreaPosY = minimapPosY + 5;
            selectionInfoArea = new Rectangle(selectionInfoAreaPosX, selectionInfoAreaPosY, commandButtonsAreaPosX - selectionInfoAreaPosX - 10, minimapSize - 10);
        }

        int revealBoundingBoxCounter;
        int visibilityCounter;
        void applyVisibilityToMap()
        {
            if (visibilityCounter++ % 2 != 0)
            {
                visibilityCounter = 0;
                return;
            }


            foreach (MapTile tile in map.Tiles)
                tile.Visible = false;

            foreach (RtsObject o in RtsObject.RtsObjects)
            {
                if (o.Team != Player.Me.Team)
                    continue;

                foreach (MapTile tile in o.VisibleTiles)
                {
                    tile.Visible = true;
                    tile.Revealed = true;

                    if (!tile.BoundingBox.FullyRevealed && revealBoundingBoxCounter++ % 1 == 0)
                    {
                        revealBoundingBoxCounter = 0;
                        tile.BoundingBox.Revealed = true;
                    }
                }
            }
        }

        void removeDeadUnitsFromSelections()
        {
            // units
            for (int i = 0; i < Unit.DeadUnits.Count; )
            {
                Unit unit = Unit.DeadUnits[i];

                SelectingUnits.Remove(unit);
                SelectedUnits.Remove(unit);
                selectedUnitsChanged = true;

                foreach (List<RtsObject> group in HotkeyGroups)
                    group.Remove(unit);

                Unit.DeadUnits.Remove(unit);
            }

            // structures
            for (int i = 0; i < Structure.DeadStructures.Count; )
            {
                Structure structure = Structure.DeadStructures[i];

                SelectingUnits.Remove(structure);
                SelectedUnits.Remove(structure);
                selectedUnitsChanged = true;

                foreach (List<RtsObject> group in HotkeyGroups)
                    group.Remove(structure);

                Structure.DeadStructures.Remove(structure);
            }
        }

        void updateStats(GameTime gameTime)
        {
            roksPerSecondTimer += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (roksPerSecondTimer >= 4000)
            {
                roksPerSecondTimer = 0;
                roksPerSecond = Player.Players[Player.Me.Team].Stats.RoksCounter / 4f;
                Player.Players[Player.Me.Team].Stats.RoksCounter = 0;
            }
        }

        void playErrorSound()
        {
            soundEffectManager.Play(errorSoundEffect, .15f);
        }

        void doScheduledActions()
        {
            foreach (Player player in Player.Players)
            {
                for (int i = 0; i < player.ScheduledActions.Count; i++)
                {
                    ScheduledAction action = player.ScheduledActions[i];

                    if (action.ScheduledTime <= gameClock)
                    {
                        ScheduledReturnCargoCommand scheduledReturnCargoCommand = action as ScheduledReturnCargoCommand;
                        if (scheduledReturnCargoCommand != null)
                        {
                            ((WorkerNublet)scheduledReturnCargoCommand.ReturnCargoCommand.Unit).ReturnCargoToNearestTownHall(scheduledReturnCargoCommand.ReturnCargoCommand.Source);
                            player.ScheduledActions.Remove(action);
                            continue;
                        }

                        ScheduledUnitCommand scheduledUnitCommand = action as ScheduledUnitCommand;
                        if (scheduledUnitCommand != null)
                        {
                            MoveCommand moveCommand = scheduledUnitCommand.UnitCommand as MoveCommand;
                            if (moveCommand != null)
                            {
                                if (scheduledUnitCommand.Queued)
                                    scheduledUnitCommand.UnitCommand.Unit.QueueCommand(moveCommand);
                                else
                                    scheduledUnitCommand.UnitCommand.Unit.GiveCommand(moveCommand);
                            }
                            else
                            {
                                if (scheduledUnitCommand.Queued)
                                    scheduledUnitCommand.UnitCommand.Unit.QueueCommand(scheduledUnitCommand.UnitCommand);
                                else
                                    scheduledUnitCommand.UnitCommand.Unit.GiveCommand(scheduledUnitCommand.UnitCommand);
                            }
                        }

                        ScheduledUnitBuildCommand scheduledUnitBuildCommand = action as ScheduledUnitBuildCommand;
                        if (scheduledUnitBuildCommand != null)
                        {

                        }

                        ScheduledStructureCommand scheduledStructureCommand = action as ScheduledStructureCommand;
                        if (scheduledStructureCommand != null)
                        {
                            // shouldnt happen but wtf
                            if (scheduledStructureCommand.Structure == null)
                                continue;

                            if (scheduledStructureCommand.CommandType == CommandButtonType.RallyPoint)
                            {
                                ScheduledStructureTargetedCommand rallyPointCommand = action as ScheduledStructureTargetedCommand;

                                if (!rallyPointCommand.Queued)
                                    rallyPointCommand.Structure.RallyPoints.Clear();

                                rallyPointCommand.Structure.RallyPoints.Add(new RallyPoint(rallyPointCommand.Point, (Resource)rallyPointCommand.Target));
                            }
                            else
                                scheduledStructureCommand.Structure.AddToBuildQueue((ProductionButtonType)scheduledStructureCommand.CommandType, scheduledStructureCommand.ID);
                        }

                        player.ScheduledActions.Remove(action);
                    }
                    else
                        i++;
                }
            }
        }

        void doDelayedPathUpdates()
        {
            for (int i = 0; i < DelayedPathUpdate.DelayedPathUpdates.Count; )
            {
                DelayedPathUpdate update = DelayedPathUpdate.DelayedPathUpdates[i];

                if (update.CommandID >= Player.Players[update.Team].UnitCommands.Count)
                {
                    i++;
                    continue;
                }

                MoveCommand moveCommand = Player.Players[update.Team].UnitCommands[update.CommandID] as MoveCommand;

                moveCommand.WayPoints = update.WayPoints;

                DelayedPathUpdate.DelayedPathUpdates.Remove(update);
            }
        }
    }
}