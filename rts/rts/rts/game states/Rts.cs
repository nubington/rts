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
        public static float GameSpeed = 2.00f;

        protected TimeSpan fpsElapsedTime;
        protected int frameCounter;
        protected static bool paused, allowPause;
        Random rand = new Random();
        //public static Stopwatch GameTimer = new Stopwatch();
        string fpsMessage = "";
        static SpriteFont pauseFont, fpsFont, unitInfoUnitNameFont, unitInfoHpFont, unitInfoKillCountFont, resourceCountFont, bigFont;

        static Texture2D greenTeamIndicatorTexture, redTeamIndicatorTexture;

        Form winForm;
        static Cursor normalCursor, attackCursor;

        double timeForPathFindingProfiling, pathFindingPercentage;
        int pathFindQueueSize;

        MouseState mouseState;
        KeyboardState keyboardState;

        Camera camera;
        float cameraScrollSpeed = 1500, cameraZoomSpeed = 1, cameraRotationSpeed = 4.5f, cameraRotationTarget, cameraRotationIncrement = MathHelper.PiOver2;//MathHelper.PiOver4 / 2;

        static Song rtsMusic;
        static SoundEffect errorSoundEffect;

        BaseObject button1, button2, button3, button4, button5;
        static Texture2D buttonTexture;

        //static Texture2D brownGuyTexture, brownGuySelectingTexture, brownGuySelectedTexture;
        static Texture2D moveCommandShrinkerTexture, attackMoveCommandShrinkerTexture, normalCursorTexture, attackCommandCursorTexture;
        static Texture2D redCircleTexture, transparentTexture, whiteBoxTexture, 
            transparentGrayTexture, transparentBlackTexture, rallyFlagTexture;
        static Texture2D cogWheelTexture;

        const int MAXSELECTIONSIZE = 255;//36;
        static List<RtsObject> SelectingUnits = new List<RtsObject>();
        //static List<Unit> SelectedUnits = new List<Unit>();
        public static Selection SelectedUnits = new Selection();
        static List<RtsObject>[] HotkeyGroups = new List<RtsObject>[10];
        public static bool selectedUnitsChanged;

        bool usingAttackCommand, usingRallyPointCommand, usingTargetedCommand, queueingTargetedCommand;
        //int normalCursorSize = 28, attackCommandCursorSize = 23;

        Map map;
        static Texture2D boulder1Texture, tree1Texture;
        int actualMapWidth, actualMapHeight;

        VisionUpdater VisionUpdater;

        Texture2D fullMapTexture;
        Rectangle minimap;
        int minimapSize = 128, minimapBorderSize = 5;
        int minimapPosX, minimapPosY;
        float minimapToMapRatioX, minimapToMapRatioY;
        float minimapToScreenRatioX, minimapToScreenRatioY;
        BaseObject minimapScreenIndicatorBox;
        PrimitiveLine minimapScreenIndicatorBoxLine;

        Rectangle commandCardArea;
        int commandButtonsAreaPosX, commandButtonsAreaPosY;
        int commandButtonsAreaWidth = 150;

        Rectangle selectionInfoArea;
        int selectionInfoAreaPosX, selectionInfoAreaPosY;

        Viewport worldViewport, uiViewport, minimapViewport;

        int roksPerSecondTimer;
        float roksPerSecond;

        public static Lidgren.Network.NetPeer netPeer;
        public static Lidgren.Network.NetConnection connection;
        bool iAmServer;

        float gameClock = 0;
        float currentScheduleTime;
        float countDownTime = 4f;
        bool countingDown = true;

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
            Unit.Map = map;
            Structure.Map = map;
            Resource.Map = map;
            RtsObject.PathFinder = new PathFinder(map);
            Resource.PathFinder = RtsObject.PathFinder;
            map.InstantiateMapResources();
            //Player.Me.Team = myTeam;
            Player.Me = Player.Players[myTeam];

            actualMapWidth = map.Width * map.TileSize;
            actualMapHeight = map.Height * map.TileSize;

            Unit.UnitCollisionSweeper.Thread.Suspend();
            Unit.UnitCollisionSweeper.Thread.Resume();
            RtsObject.PathFinder.ResumeThread();

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

            VisionUpdater = new VisionUpdater(map, Unit.PathFinder, Player.Me.Team);

            SelectBox.InitializeSelectBoxLine(GraphicsDevice, Color.Green);
            Initializeline(GraphicsDevice, Color.Yellow);

            minimapScreenIndicatorBoxLine = new PrimitiveLine(GraphicsDevice, 1);
            minimapScreenIndicatorBoxLine.Colour = Color.White;

            for (int i = 0; i < HotkeyGroups.Length; i++)
                HotkeyGroups[i] = new List<RtsObject>();

            MediaPlayer.Play(rtsMusic);
            MediaPlayer.Volume = 0f;// .25f;
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
                lock (Unit.PathFinder.TimeSpentPathFindingLock)
                {
                    pathFindingTime = Unit.PathFinder.TimeSpentPathFinding.TotalMilliseconds;
                    Unit.PathFinder.TimeSpentPathFinding = TimeSpan.Zero;
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
            Unit.PathFinder.AbortThread();
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
            //Unit.PathFinder.SuspendThread();
            //Game1.Game.DebugMonitor.Position = Direction.SouthEast;
            //Game1.Game.IsMouseVisible = true;
            //GraphicsDevice.Viewport = uiViewport;
        }

        void cancel()
        {
            if (activeCommandCard == CommandCard.BarracksCommandCard || activeCommandCard == CommandCard.TownHallCommandCard)
            {
                Structure structureWithLargestQueue = null;
                int largest = 0;

                //foreach (RtsObject o in SelectedUnits)
                for (int i = SelectedUnits.Count - 1; i >= 0; i--)
                {
                    Structure s = SelectedUnits[i] as Structure;

                    if (s.Team != Player.Me.Team)
                        return;

                    if (s != null && s.Type == SelectedUnits.ActiveType && !s.UnderConstruction)
                    {
                        if (s.BuildQueue.Count > largest)
                        {
                            structureWithLargestQueue = s;
                            largest = s.BuildQueue.Count;
                        }
                    }
                }
                if (structureWithLargestQueue != null)
                    //structureWithLargestQueue.BuildQueue.RemoveAt(structureWithLargestQueue.BuildQueue.Count - 1);
                    structureWithLargestQueue.RemoveFromBuildQueue(structureWithLargestQueue.BuildQueue.Count - 1);
            }
            else if (activeCommandCard == CommandCard.UnderConstructionCommandCard)
            {
                for (int i = SelectedUnits.Count - 1; i >= 0; i--)
                {
                    Structure structure = SelectedUnits[i] as Structure;
                    if (structure != null)
                    {
                        if (structure.Team == Player.Me.Team)
                            structure.Cancel();
                        break;
                    }
                }
            }
        }

        bool selecting, unitsSelected, unitInSelection, newUnitInSelection, myTeamInSelection;
        const int doubleClickDelay = 225, simpleClickSize = 5;
        int timeSinceLastSimpleClick = doubleClickDelay;
        RtsObject lastUnitClicked = null;
        void SelectUnits(GameTime gameTime)
        {
            //SelectBox.Box.CalculateCorners();

            bool structureInPreviousSelection = false;
            foreach (RtsObject o in SelectedUnits)
            {
                if (o is Structure)
                    structureInPreviousSelection = true;
            }

            int selectingUnitsCount = SelectingUnits.Count;
            SelectingUnits.Clear();

            timeSinceLastSimpleClick += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            bool simpleClick = (SelectBox.Box.GreaterOfWidthAndHeight <= simpleClickSize);


            if (SelectBox.IsSelecting)
            {
                selecting = true;
                unitsSelected = false;
                /*foreach (Unit unit in Unit.Units)
                {
                    if ((simpleClick && unit.Contains(Vector2.Transform(new Vector2(mouseState.X, mouseState.Y), Matrix.Invert(camera.get_transformation(worldViewport))))) ||
                            (!simpleClick && SelectBox.Box.Rectangle.Intersects(unit.Rectangle)))
                    {
                        if (SelectingUnits.Count < MAXSELECTIONSIZE)
                            SelectingUnits.Add(unit);
                    }
                }
                foreach (Structure structure in Structure.Structures)
                {
                    if ((simpleClick && structure.Contains(Vector2.Transform(new Vector2(mouseState.X, mouseState.Y), Matrix.Invert(camera.get_transformation(worldViewport))))) ||
                            (!simpleClick && SelectBox.Box.Rectangle.Intersects(structure.Rectangle)))
                    {
                        if (SelectingUnits.Count < MAXSELECTIONSIZE)
                            SelectingUnits.Add(structure);
                    }
                }*/
                foreach (RtsObject o in RtsObject.RtsObjects)
                {
                    if ((simpleClick && o.Contains(Vector2.Transform(new Vector2(mouseState.X, mouseState.Y), Matrix.Invert(camera.get_transformation(worldViewport))))) ||
                            (!simpleClick && SelectBox.Box.Rectangle.Intersects(o.Rectangle)))
                    {
                        if (!simpleClick && o.Team != Player.Me.Team)
                            continue;

                        if (SelectingUnits.Count < MAXSELECTIONSIZE && o.Visible)
                            SelectingUnits.Add(o);
                    }
                }

                /*for (int i = 0; i < SelectingUnits.Count; )
                {
                    RtsObject o = SelectingUnits[i];

                    if (!o.Visible)
                        SelectingUnits.Remove(o);
                    else
                        i++;
                }*/
            }
            else if (unitsSelected == false)
            {
                selecting = false;
                unitsSelected = true;
                selectedUnitsChanged = true;
                unitInSelection = false;
                newUnitInSelection = false;
                myTeamInSelection = false;

                bool objectClicked = false;

                // holding shift
                if (usingShift)
                {
                    // dont do if enemy unit selected
                    if (SelectedUnits.Count == 0 || SelectedUnits[0].Team == Player.Me.Team)
                    {
                        foreach (RtsObject o in RtsObject.RtsObjects)
                        {
                            if (!o.Visible)// || (SelectedUnits.Count > 0 && o.Team != SelectedUnits[0].Team))
                                continue;

                            if ((simpleClick && o.Contains(Vector2.Transform(new Vector2(mouseState.X, mouseState.Y), Matrix.Invert(camera.get_transformation(worldViewport))))) ||
                                (!simpleClick && SelectBox.Box.Rectangle.Intersects(o.Rectangle)))
                            {
                                // holding ctrl or double click
                                if ((simpleClick && lastUnitClicked == o && timeSinceLastSimpleClick <= doubleClickDelay) ||
                                    (simpleClick && keyboardState.IsKeyDown(Keys.LeftControl)))
                                {
                                    timeSinceLastSimpleClick = 0;

                                    Unit unit = o as Unit;
                                    if (unit != null)
                                    {
                                        foreach (Unit u in Unit.Units)
                                        {
                                            if (u.Type == unit.Type && u.Team == unit.Team && !u.IsOffScreen(worldViewport, camera))
                                            {
                                                if (SelectedUnits.Count >= MAXSELECTIONSIZE)
                                                    break;

                                                if (!SelectedUnits.Contains(u))
                                                {
                                                    SelectedUnits.Add(u);
                                                    newUnitInSelection = true;
                                                }
                                            }
                                        }
                                    }

                                    Structure structure = o as Structure;
                                    if (structure != null)
                                    {
                                        foreach (Structure s in Structure.Structures)
                                        {
                                            if (s.Type == structure.Type && s.Team == structure.Team && !s.IsOffScreen(worldViewport, camera))
                                            {
                                                if (SelectedUnits.Count >= MAXSELECTIONSIZE)
                                                    break;

                                                if (!SelectedUnits.Contains(s))
                                                    SelectedUnits.Add(s);
                                            }
                                        }
                                    }
                                }
                                // not holding ctrl or double click
                                else
                                {
                                    if (!SelectedUnits.Contains(o))
                                    {
                                        if (SelectedUnits.Count < MAXSELECTIONSIZE)
                                        {
                                            SelectedUnits.Add(o);
                                            if (o is Unit)
                                                newUnitInSelection = true;
                                        }
                                        //selectedUnitsChanged = true;
                                    }
                                    else if (simpleClick)
                                    {
                                        SelectedUnits.Remove(o);
                                        //selectedUnitsChanged = true;
                                    }
                                }
                                lastUnitClicked = o;
                                objectClicked = true;
                            }
                        }
                    }
                }
                // not holding shift
                else
                {
                    SelectedUnits.Clear();
                    //selectedUnitsChanged = true;

                    foreach (RtsObject o in RtsObject.RtsObjects)
                    {
                        if (!o.Visible)
                            continue;

                        if ((simpleClick && o.Contains(Vector2.Transform(new Vector2(mouseState.X, mouseState.Y), Matrix.Invert(camera.get_transformation(worldViewport))))) ||
                            (!simpleClick && SelectBox.Box.Rectangle.Intersects(o.Rectangle)))
                        {
                            // holding ctrl or double click
                            if ((simpleClick && lastUnitClicked == o && timeSinceLastSimpleClick <= doubleClickDelay) ||
                                (simpleClick && keyboardState.IsKeyDown(Keys.LeftControl)))
                            {
                                timeSinceLastSimpleClick = 0;

                                //if (o.Team != Player.Me.Team)
                                //    continue;

                                Unit unit = o as Unit;
                                if (unit != null)
                                {
                                    foreach (Unit u in Unit.Units)
                                    {
                                        if (u.Type == unit.Type && u.Team == unit.Team && !u.IsOffScreen(worldViewport, camera))
                                        {
                                            if (SelectedUnits.Count >= MAXSELECTIONSIZE)
                                                break;

                                            if (!SelectedUnits.Contains(u))
                                            {
                                                SelectedUnits.Add(u);
                                                newUnitInSelection = true;
                                            }
                                        }
                                    }
                                }

                                Structure structure = o as Structure;
                                if (structure != null)
                                {
                                    foreach (Structure s in Structure.Structures)
                                    {
                                        if (s.Type == structure.Type && s.Team == structure.Team && !s.IsOffScreen(worldViewport, camera))
                                        {
                                            if (SelectedUnits.Count >= MAXSELECTIONSIZE)
                                                break;

                                            if (!SelectedUnits.Contains(s))
                                                SelectedUnits.Add(s);
                                        }
                                    }
                                }
                            }
                            // not holding ctrl or double click
                            else
                            {
                                if (SelectedUnits.Count < MAXSELECTIONSIZE && !SelectedUnits.Contains(o))
                                {
                                    //if (SelectedUnits.Count == 0 || SelectedUnits[0].Team == Player.Me.Team)
                                        SelectedUnits.Add(o);
                                    if (o is Unit)
                                        newUnitInSelection = true;
                                }
                            }

                            lastUnitClicked = o;
                            objectClicked = true;
                        }
                    }

                    SelectedUnits.SetActiveTypeToMostPopulousType();
                }
                if (simpleClick)
                    timeSinceLastSimpleClick = 0;

                foreach (RtsObject o in SelectedUnits)
                {
                    if (o.Team == Player.Me.Team)
                    {
                        myTeamInSelection = true;
                        break;
                    }
                }

                if (myTeamInSelection)
                {
                    for (int i = 0; i < SelectedUnits.Count; )
                    {
                        RtsObject o = SelectedUnits[i];
                        if (o.Team != Player.Me.Team)
                            SelectedUnits.Remove(o);
                        else
                            i++;
                    }
                }

                foreach (RtsObject o in SelectedUnits)
                {
                    if (o is Unit)
                    {
                        unitInSelection = true;
                        break;
                    }
                }

                if (unitInSelection)
                {
                    if (!usingShift || (!structureInPreviousSelection && newUnitInSelection))
                    {
                        for (int i = 0; i < SelectedUnits.Count; )
                        {
                            RtsObject o = SelectedUnits[i];
                            if (o is Structure)
                                SelectedUnits.Remove(o);
                            else
                                i++;
                        }
                    }
                }

                if (!objectClicked)
                    lastUnitClicked = null;
            }
        }

        bool allowRightClick = true;
        void checkForRightClick()
        {
            if (mouseState.RightButton == ButtonState.Released)
                allowRightClick = true;
            else if (allowRightClick && mouseState.RightButton == ButtonState.Pressed)
            {
                allowRightClick = false;

                if (usingTargetedCommand)
                {
                    stopTargetedCommands();
                }
                else if (placingStructure)
                {
                    placingStructure = false;
                }
                else
                    rightClick();
            }
        }

        const int magicBoxMaxSize = 300, magicBoxMaxDistance = 800;
        const int moveCommandShrinkerSize = 12;//18;
        const int moveCommandShrinkDelay = 20;
        void rightClick()
        {
            if (SelectedUnits.Count == 0)
                return;

            //magicBoxMaxSize = SelectedUnits.Count * 5;

            Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y);
            if (minimap.Contains(mouseState.X, mouseState.Y))
            {
                //mousePosition = new Vector2((mousePosition.X - minimapPosX) / minimapToMapRatioX, (mousePosition.Y - minimapPosY) / minimapToMapRatioY);

                Vector2 minimapCenterPoint = new Vector2(minimap.X + minimap.Width / 2f, minimap.Y + minimap.Height / 2f);

                float distance = Vector2.Distance(mousePosition, minimapCenterPoint);
                float angle = (float)Math.Atan2(mousePosition.Y - minimapCenterPoint.Y, mousePosition.X - minimapCenterPoint.X);

                mousePosition = new Vector2(minimapCenterPoint.X + distance * (float)Math.Cos(angle - camera.Rotation), minimapCenterPoint.Y + distance * (float)Math.Sin(angle - camera.Rotation));

                mousePosition = new Vector2((mousePosition.X - minimapPosX) / minimapToMapRatioX, (mousePosition.Y - minimapPosY) / minimapToMapRatioY);
            }
            else if (mouseState.Y > worldViewport.Height)
            {
                //return;
                mousePosition = Vector2.Transform(new Vector2(mousePosition.X, worldViewport.Height), Matrix.Invert(camera.get_transformation(worldViewport)));
            }
            else
                mousePosition = Vector2.Transform(mousePosition, Matrix.Invert(camera.get_transformation(worldViewport)));

            // follow a unit
            /*foreach (Unit unit in Unit.Units)
            {
                if (unit.Contains(mousePosition))
                {
                    foreach (Unit u in SelectedUnits)
                    {
                        if (u != unit)
                            u.FollowTarget = unit;
                        else if (SelectedUnits.Count == 1)
                            u.MoveTarget = mousePosition;
                    }
                    return;
                }
            }*/

            // set rally point if active type is rallyable
            setRallyPoint(mousePosition);

            if (giveHarvestCommand(mousePosition))
                return;

            bool attacking = false;
            // attack enemy
            foreach (RtsObject o in RtsObject.RtsObjects)
            {
                if (!o.Visible)
                    continue;

                if (o.Contains(mousePosition) &&  o.Team != Player.Me.Team)
                {
                    List<ScheduledUnitCommand> scheduledUnitCommands = new List<ScheduledUnitCommand>();

                    foreach (RtsObject ob in SelectedUnits)
                    {
                        if (ob.Team != Player.Me.Team)
                            break;

                        Unit u = ob as Unit;
                        if (u == null)
                            continue;

                        scheduledUnitCommands.Add(new ScheduledUnitCommand(currentScheduleTime, new AttackCommand(u, o, false, false), usingShift));
                        attacking = true;
                        //if (u.Team != o.Team)
                        //{
                            /*if (!usingShift)
                            {
                                AttackCommand command = new AttackCommand(u, o, false, false);
                                u.GiveCommand(command);
                                attacking = true;
                                //Unit.PathFinder.AddHighPriorityPathFindRequest(u, command, u.CurrentPathNode, (int)Vector2.DistanceSquared(u.CenterPoint, command.Destination), false);
                            }
                            else
                                u.QueueCommand(new AttackCommand(u, o, false, false));*/
                        //}
                    }

                    if (attacking)
                    {
                        if (scheduledUnitCommands.Count > 0)
                        {
                            scheduleAttackCommands(scheduledUnitCommands, o);

                            UnitAnimation redCircleAnimation = new UnitAnimation(o, o.Width, .75f, 8, false, redCircleTexture, transparentTexture);
                            redCircleAnimation.Start();
                        }
                    }
                    else
                    {
                        // give move command to units
                        giveMoveCommand(mousePosition);
                    }

                    return;
                }
            }

            // give move command to units
            giveMoveCommand(mousePosition);
        }

        void checkForCommands()
        {
            foreach (CommandButton button in CommandCardButtons)
            {
                if (button.Triggered)
                {
                    if (button.Type == CommandButtonType.Attack)
                    {
                        usingAttackCommand = usingTargetedCommand = true;
                        winForm.Cursor = attackCursor;
                    }
                    else if (button.Type == CommandButtonType.HoldPosition)
                    {
                        holdPosition();
                    }
                    else if (button.Type == CommandButtonType.Stop)
                    {
                        stop();
                    }
                    else if (button.Type == CommandButtonType.RallyPoint)
                    {
                        usingRallyPointCommand = usingTargetedCommand = true;
                        winForm.Cursor = attackCursor;
                    }
                    else if (button.Type == CommandButtonType.Build)
                    {
                        if (placingStructure)
                            break;
                        switchToBuildMenuCommandCard();
                        SimpleButton.Reset();
                        //placingStructure = false;
                        break;
                    }
                    else if (button.Type == CommandButtonType.Cancel)
                    {
                        cancel();
                        break;
                    }
                    else if (button.Type == CommandButtonType.ReturnCargo)
                    {
                        giveReturnCargoCommand();
                        break;
                    }
                    else if (button.Type is BuildUnitButtonType)
                    {
                        giveStructureCommand(button.Type as BuildUnitButtonType);
                        break;
                    }
                    else if (button.Type is BuildStructureButtonType)
                    {
                        BuildStructureButtonType buttonType = button.Type as BuildStructureButtonType;
                        if (buttonType.StructureType.RoksCost > Player.Me.Roks)
                            playErrorSound();
                        else
                        {
                            placingStructure = true;
                            placingStructureType = ((BuildStructureButtonType)button.Type).StructureType;
                            resetCommandCard();
                            SimpleButton.Reset();
                        }
                        break;
                    }
                }
            }

            if (!usingShift)
            {
                if (queueingTargetedCommand)
                    stopTargetedCommands();
                if (queueingPlacingStructure)
                {
                    placingStructure = queueingPlacingStructure = false;
                }
            }

            //checkForAttackCommand();
            //checkForHoldPosition();
            //checkForStop();
        }

        void giveMoveCommand(Vector2 mousePosition)
        {
            List<Unit> units = new List<Unit>();
            foreach (RtsObject o in SelectedUnits)
            {
                Unit unit = o as Unit;
                if (unit != null && unit.Team == Player.Me.Team)
                    units.Add(unit);
            }

            if (units.Count == 0)
                return;

            List<ScheduledUnitCommand> scheduledUnitCommands = new List<ScheduledUnitCommand>();
            //float scheduledTime = gameClock + connection.AverageRoundtripTime;
            //float scheduledTime = gameClock + 1f;

            // create magic box
            Rectangle magicBox = units[0];
            foreach (Unit unit in units)
            {
                magicBox = Rectangle.Union(magicBox, unit.Rectangle);
            }

            // box is too big or clicked inside magic box or too far away
            if (magicBox.Width > magicBoxMaxSize || magicBox.Height > magicBoxMaxSize ||
                magicBox.Contains((int)mousePosition.X, (int)mousePosition.Y) ||
                Vector2.Distance(new Vector2(magicBox.Center.X, magicBox.Center.Y), mousePosition) > magicBoxMaxDistance)
            {
                //bool isPointWalkable = Unit.PathFinder.IsPointWalkable(mousePosition);
                // assign move targets to mouse position
                foreach (Unit unit in units)
                {
                    // if mouse position is not in a walkable tile, find nearest walkable tile
                    Vector2 destinationPoint;
                    if (Unit.PathFinder.IsPointWalkable(mousePosition, unit))
                        destinationPoint = mousePosition;
                    else
                        //destinationPoint = map.FindNearestWalkableTile(mousePosition);
                        destinationPoint = (Unit.PathFinder.FindNearestPathNode((int)(mousePosition.Y / map.TileSize), (int)(mousePosition.X / map.TileSize), unit)).Tile.CenterPoint;

                    createMoveCommandShrinker(destinationPoint, false);

                    // not holding shift
                    /*if (!usingShift)
                    {
                        //MoveCommand command = new MoveCommand(unit, destinationPoint, 1);
                        //unit.GiveCommand(command);
                        scheduledUnitCommands.Add(new ScheduledUnitCommand(currentScheduleTime, new MoveCommand(unit, destinationPoint, 1), false));
                    }
                    // holding shift
                    else
                    {
                        //MoveCommand command = new MoveCommand(unit, destinationPoint, 1);
                        //unit.QueueCommand(command);
                        scheduledUnitCommands.Add(new ScheduledUnitCommand(currentScheduleTime, new MoveCommand(unit, destinationPoint, 1), true));
                    }*/

                    scheduledUnitCommands.Add(new ScheduledUnitCommand(currentScheduleTime, new MoveCommand(unit, destinationPoint, 1), usingShift));
                }
            }
            // clicked outside magic box
            else
            {
                // make destination box and keep in screen
                Rectangle destBox = magicBox;
                destBox.X = (int)mousePosition.X - destBox.Width / 2;
                destBox.Y = (int)mousePosition.Y - destBox.Height / 2;

                // calculate angle from magic box to destination box
                float angle = (float)Math.Atan2(destBox.Center.Y - magicBox.Center.Y, destBox.Center.X - magicBox.Center.X);
                float angleX = (float)Math.Cos(angle);
                float angleY = (float)Math.Sin(angle);
                float distance = Vector2.Distance(new Vector2(magicBox.Center.X, magicBox.Center.Y), new Vector2(destBox.Center.X, destBox.Center.Y));

                // assign move targets based on angle
                foreach (Unit unit in units)
                {
                    // if mouse position is not in a walkable tile, find nearest walkable tile
                    Vector2 destinationPoint = unit.CenterPoint + new Vector2(distance * angleX, distance * angleY);
                    if (!Unit.PathFinder.IsPointWalkable(destinationPoint, unit))
                        //destinationPoint = map.FindNearestWalkableTile(destinationPoint);
                        destinationPoint = (Unit.PathFinder.FindNearestPathNode((int)(destinationPoint.Y / map.TileSize), (int)(destinationPoint.X / map.TileSize), unit)).Tile.CenterPoint;

                    createMoveCommandShrinker(destinationPoint, false);

                    // not holding shift
                    /*if (!usingShift)
                    {
                        //MoveCommand command = new MoveCommand(unit, destinationPoint, 1);
                        //unit.GiveCommand(command);
                        scheduledUnitCommands.Add(new ScheduledUnitCommand(currentScheduleTime, new MoveCommand(unit, destinationPoint, 1), false));
                    }
                    // holding shift
                    else
                    {
                        //unit.QueueCommand(new MoveCommand(unit, destinationPoint, 1));
                        scheduledUnitCommands.Add(new ScheduledUnitCommand(currentScheduleTime, new MoveCommand(unit, destinationPoint, 1), true));
                    }*/

                    scheduledUnitCommands.Add(new ScheduledUnitCommand(currentScheduleTime, new MoveCommand(unit, destinationPoint, 1), usingShift));
                }
            }

            // add scheduled actions and send them in batch over network
            NetOutgoingMessage msg = netPeer.CreateMessage();

            msg.Write(MessageID.UNIT_MOVE_COMMAND_BATCH);
            msg.Write(currentScheduleTime);
            msg.Write(Player.Me.Team);
            msg.Write(usingShift);
            msg.Write((short)scheduledUnitCommands.Count);

            foreach (ScheduledUnitCommand s in scheduledUnitCommands)
            {
                MoveCommand moveCommand = s.UnitCommand as MoveCommand;

                Player.Players[s.Team].ScheduledActions.Add(new ScheduledUnitCommand(currentScheduleTime, moveCommand, s.Queued));
                //Unit.PathFinder.AddHighPriorityPathFindRequest(moveCommand, (int)Vector2.DistanceSquared(moveCommand.Unit.CenterPoint, moveCommand.Destination), false);

                //if (s.Queued)
                    //moveCommand.Unit.QueueCommand(moveCommand);
                //else
                    //moveCommand.Unit.GiveCommand(moveCommand);

                msg.Write(moveCommand.Unit.ID);
                msg.Write(moveCommand.Destination.X);
                msg.Write(moveCommand.Destination.Y);
            }

            netPeer.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);
        }

        void giveAttackCommand(Vector2 mousePosition)
        {
            foreach (RtsObject o in RtsObject.RtsObjects)
            {
                if (!o.Visible)
                    continue;

                if (o.Contains(mousePosition))
                {
                    List<ScheduledUnitCommand> scheduledUnitCommands = new List<ScheduledUnitCommand>();

                    foreach (RtsObject u in SelectedUnits)
                    {
                        if (u.Team != Player.Me.Team)
                            break;

                        Unit unit = u as Unit;
                        if (unit != null && unit != o)
                        {
                            /*if (!usingShift)
                            {
                                AttackCommand command = new AttackCommand(unit, o, false, false);
                                unit.GiveCommand(command);
                            }
                            else
                                unit.QueueCommand(new AttackCommand(unit, o, false, false));*/

                            scheduledUnitCommands.Add(new ScheduledUnitCommand(currentScheduleTime, new AttackCommand(unit, o, false, false), usingShift));
                        }
                    }

                    if (scheduledUnitCommands.Count > 0)
                    {
                        scheduleAttackCommands(scheduledUnitCommands, o);

                        UnitAnimation redCircleAnimation = new UnitAnimation(o, o.Width, .75f, 8, false, redCircleTexture, transparentTexture);
                        redCircleAnimation.Start();
                    }

                    return;
                }
            }

            //giveMoveCommand(mousePosition);
            giveAttackMoveCommand(mousePosition);
        }

        // used by rightClick() and giveAttackCommand() to schedule the attack commands
        void scheduleAttackCommands(List<ScheduledUnitCommand> scheduledUnitCommands, RtsObject target)
        {
            // add scheduled actions and send them in batch over network
            NetOutgoingMessage msg = netPeer.CreateMessage();

            msg.Write(MessageID.UNIT_ATTACK_COMMAND_BATCH);
            msg.Write(currentScheduleTime);
            msg.Write(Player.Me.Team);
            msg.Write(usingShift);
            msg.Write((short)scheduledUnitCommands.Count);

            Unit targetUnit = target as Unit;
            if (targetUnit != null)
            {
                msg.Write((short)0);
                msg.Write(targetUnit.ID);
            }

            Structure targetStructure = target as Structure;
            if (targetStructure != null)
            {
                msg.Write((short)1);
                msg.Write(targetStructure.ID);
            }
            msg.Write(target.Team);

            foreach (ScheduledUnitCommand s in scheduledUnitCommands)
            {
                AttackCommand attackCommand = s.UnitCommand as AttackCommand;

                Player.Players[s.Team].ScheduledActions.Add(new ScheduledUnitCommand(currentScheduleTime, attackCommand, s.Queued));
                //Unit.PathFinder.AddHighPriorityPathFindRequest(attackCommand, (int)Vector2.DistanceSquared(attackCommand.Unit.CenterPoint, attackCommand.Destination), false);

                //if (s.Queued)
                //moveCommand.Unit.QueueCommand(moveCommand);
                //else
                //moveCommand.Unit.GiveCommand(moveCommand);

                msg.Write(attackCommand.Unit.ID);
            }

            netPeer.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);
        }

        void setRallyPoint(Vector2 mousePosition)
        {
            List<ScheduledStructureTargetedCommand> scheduledActionBatch = new List<ScheduledStructureTargetedCommand>();

            //float scheduledTime = gameClock + connection.AverageRoundtripTime;

            // rally to a resource
            foreach (Resource resource in Resource.Resources)
            {
                if (resource.Touches(mousePosition))
                {
                    foreach (RtsObject o in SelectedUnits)
                    {
                        if (o.Team != Player.Me.Team)
                            return;

                        Structure structure = o as Structure;
                        if (structure != null && structure.Rallyable)
                        {
                            //if (!usingShift)
                            //    structure.RallyPoints.Clear();

                            //structure.RallyPoints.Add(new RallyPoint(resource.CenterPoint, resource));

                            ScheduledStructureTargetedCommand action = new ScheduledStructureTargetedCommand(currentScheduleTime, structure, CommandButtonType.RallyPoint, resource, resource.CenterPoint, usingShift);
                            Player.Me.ScheduledActions.Add(action);
                            scheduledActionBatch.Add(action);
                        }
                    }

                    NetOutgoingMessage msg = netPeer.CreateMessage();

                    msg.Write(MessageID.RALLY_POINT_COMMAND);
                    msg.Write(currentScheduleTime);
                    msg.Write(Player.Me.Team);
                    msg.Write(usingShift);
                    msg.Write((short)scheduledActionBatch.Count);

                    foreach (ScheduledStructureTargetedCommand action in scheduledActionBatch)
                    {
                        msg.Write(action.Structure.ID);
                        msg.Write(((Resource)action.Target).ID);
                        msg.Write(action.Point.X);
                        msg.Write(action.Point.Y);
                    }

                    netPeer.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);

                    return;
                }
            }

            // rally to a point
            foreach (RtsObject o in SelectedUnits)
            {
                if (o.Team != Player.Me.Team)
                    continue;

                Structure s = o as Structure;
                if (s != null && s.Rallyable)
                {
                    if (s.Contains(mousePosition))
                    {
                        s.RallyPoints.Clear();
                        continue;
                    }

                    //if (!usingShift)
                    //    s.RallyPoints.Clear();



                    Vector2 rallyPoint;
                    if (Unit.PathFinder.IsPointWalkable(mousePosition))
                        rallyPoint = mousePosition;
                    else
                        rallyPoint = Unit.PathFinder.FindNearestPathNode((int)(mousePosition.Y / map.TileSize), (int)(mousePosition.X / map.TileSize), s).Tile.CenterPoint;

                    //s.RallyPoints.Add(new RallyPoint(rallyPoint, null));
                    ScheduledStructureTargetedCommand action = new ScheduledStructureTargetedCommand(currentScheduleTime, s, CommandButtonType.RallyPoint, null, rallyPoint, usingShift);
                    Player.Me.ScheduledActions.Add(action);
                    scheduledActionBatch.Add(action);
                }
            }

            NetOutgoingMessage m = netPeer.CreateMessage();

            m.Write(MessageID.RALLY_POINT_COMMAND);
            m.Write(currentScheduleTime);
            m.Write(Player.Me.Team);
            m.Write(usingShift);
            m.Write((short)scheduledActionBatch.Count);

            foreach (ScheduledStructureTargetedCommand action in scheduledActionBatch)
            {
                m.Write(action.Structure.ID);
                m.Write((short)-1);
                m.Write(action.Point.X);
                m.Write(action.Point.Y);
            }

            netPeer.SendMessage(m, connection, NetDeliveryMethod.ReliableOrdered);
        }

        void giveAttackMoveCommand(Vector2 mousePosition)
        {
            List<Unit> units = new List<Unit>();
            foreach (RtsObject o in SelectedUnits)
            {
                Unit unit = o as Unit;
                if (unit != null && unit.Team == Player.Me.Team)
                    units.Add(unit);
            }

            if (units.Count == 0)
                return;

            List<ScheduledUnitCommand> scheduledUnitCommands = new List<ScheduledUnitCommand>();

            // create magic box
            Rectangle magicBox = units[0];
            foreach (Unit unit in units)
                magicBox = Rectangle.Union(magicBox, unit.Rectangle);

            // box is too big or clicked inside magic box
            if (magicBox.Width > magicBoxMaxSize || magicBox.Height > magicBoxMaxSize ||
                magicBox.Contains((int)mousePosition.X, (int)mousePosition.Y))
            {
                //bool isPointWalkable = Unit.PathFinder.IsPointWalkable(mousePosition);
                // assign move targets to mouse position
                foreach (Unit unit in units)
                {
                    // if mouse position is not in a walkable tile, find nearest walkable tile
                    Vector2 destinationPoint;
                    if (Unit.PathFinder.IsPointWalkable(mousePosition, unit))
                        destinationPoint = mousePosition;
                    else
                        //destinationPoint = map.FindNearestWalkableTile(mousePosition);\
                        destinationPoint = (Unit.PathFinder.FindNearestPathNode((int)(mousePosition.Y / map.TileSize), (int)(mousePosition.X / map.TileSize), unit)).Tile.CenterPoint;

                    createMoveCommandShrinker(destinationPoint, true);

                    // not holding shift
                    /*if (!usingShift)
                    {
                        AttackMoveCommand command = new AttackMoveCommand(unit, destinationPoint, 1);
                        unit.GiveCommand(command);
                    }
                    // holding shift
                    else
                    {
                        AttackMoveCommand command = new AttackMoveCommand(unit, destinationPoint, 1);
                        unit.QueueCommand(command);
                    }*/
                    scheduledUnitCommands.Add(new ScheduledUnitCommand(currentScheduleTime, new AttackMoveCommand(unit, destinationPoint, 1), usingShift));
                }
            }
            // clicked outside magic box
            else
            {
                // make destination box and keep in screen
                Rectangle destBox = magicBox;
                destBox.X = (int)mousePosition.X - destBox.Width / 2;
                destBox.Y = (int)mousePosition.Y - destBox.Height / 2;

                // calculate angle from magic box to destination box
                float angle = (float)Math.Atan2(destBox.Center.Y - magicBox.Center.Y, destBox.Center.X - magicBox.Center.X);
                float angleX = (float)Math.Cos(angle);
                float angleY = (float)Math.Sin(angle);
                float distance = Vector2.Distance(new Vector2(magicBox.Center.X, magicBox.Center.Y), new Vector2(destBox.Center.X, destBox.Center.Y));

                // assign move targets based on angle
                foreach (Unit unit in units)
                {
                    // if mouse position is not in a walkable tile, find nearest walkable tile
                    Vector2 destinationPoint = unit.CenterPoint + new Vector2(distance * angleX, distance * angleY);
                    if (!Unit.PathFinder.IsPointWalkable(destinationPoint, unit))
                        //destinationPoint = map.FindNearestWalkableTile(destinationPoint);
                        destinationPoint = (Unit.PathFinder.FindNearestPathNode((int)(destinationPoint.Y / map.TileSize), (int)(destinationPoint.X / map.TileSize), unit)).Tile.CenterPoint;

                    createMoveCommandShrinker(destinationPoint, true);

                    // not holding shift
                    /*if (!usingShift)
                    {
                        AttackMoveCommand command = new AttackMoveCommand(unit, destinationPoint, 1);
                        unit.GiveCommand(command);
                    }
                    // holding shift
                    else
                    {
                        unit.QueueCommand(new AttackMoveCommand(unit, destinationPoint, 1));
                    }*/
                    scheduledUnitCommands.Add(new ScheduledUnitCommand(currentScheduleTime, new AttackMoveCommand(unit, destinationPoint, 1), usingShift));
                }
            }

            // add scheduled actions and send them in batch over network
            NetOutgoingMessage msg = netPeer.CreateMessage();

            msg.Write(MessageID.UNIT_ATTACK_MOVE_COMMAND_BATCH);
            msg.Write(currentScheduleTime);
            msg.Write(Player.Me.Team);
            msg.Write(usingShift);
            msg.Write((short)scheduledUnitCommands.Count);

            foreach (ScheduledUnitCommand s in scheduledUnitCommands)
            {
                AttackMoveCommand attackMoveCommand = s.UnitCommand as AttackMoveCommand;

                Player.Players[s.Team].ScheduledActions.Add(new ScheduledUnitCommand(currentScheduleTime, attackMoveCommand, s.Queued));
                //Unit.PathFinder.AddHighPriorityPathFindRequest(attackMoveCommand, (int)Vector2.DistanceSquared(attackMoveCommand.Unit.CenterPoint, attackMoveCommand.Destination), false);

                //if (s.Queued)
                //moveCommand.Unit.QueueCommand(moveCommand);
                //else
                //moveCommand.Unit.GiveCommand(moveCommand);

                msg.Write(attackMoveCommand.Unit.ID);
                msg.Write(attackMoveCommand.Destination.X);
                msg.Write(attackMoveCommand.Destination.Y);
            }

            netPeer.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);
        }

        void giveBuildCommand()
        {
            if (Player.Me.Roks < placingStructureType.RoksCost)
            {
                playErrorSound();
                return;
            }

            List<WorkerNublet> workers = new List<WorkerNublet>();

            foreach (RtsObject o in SelectedUnits)
            {
                WorkerNublet worker = o as WorkerNublet;
                if (worker != null && worker.Team == Player.Me.Team)
                    workers.Add(worker);
            }

            if (workers.Count == 0)
                return;

            // sort workers by distance to build location
            for (int i = 1; i < workers.Count; i++)
            {
                for (int j = i; j >= 1 && Vector2.DistanceSquared(workers[j].CenterPoint, placingStructureCenterPoint) < Vector2.DistanceSquared(workers[j - 1].CenterPoint, placingStructureCenterPoint); j--)
                {
                    WorkerNublet tempItem = workers[j];
                    workers.RemoveAt(j);
                    workers.Insert(j - 1, tempItem);
                }
            }

            /*foreach (WorkerNublet worker in workers)
            {
                if (worker.Commands.Count == 0 || !(worker.Commands[0] is BuildStructureCommand))
                {
                    if (!usingShift)
                        worker.GiveCommand(new BuildStructureCommand(placingStructureType, placingStructureLocation, placingStructureCenterPoint, 1));
                    else
                        worker.QueueCommand(new BuildStructureCommand(placingStructureType, placingStructureLocation, placingStructureCenterPoint, 1));
                    return;
                }
            }*/

            WorkerNublet workerWithSmallestQueue = null;
            int smallest = int.MaxValue;

            foreach (WorkerNublet worker in workers)
            {
                //int count = worker.Commands.Count;
                int count = 0;
                foreach (UnitCommand command in worker.Commands)
                {
                    if (command is BuildStructureCommand)
                        count++;
                }
                if (count < smallest)
                {
                    workerWithSmallestQueue = worker;
                    smallest = count;
                }
            }

            //placingStructureCenterPoint.X = placingStructureLocation.X * map.TileSize + (placingStructureType.Size * map.TileSize) / 2;
            //placingStructureCenterPoint.Y = placingStructureLocation.Y * map.TileSize + (placingStructureType.Size * map.TileSize) / 2;

            /*if (!usingShift)
            //workers[0].GiveCommand(new BuildStructureCommand(placingStructureType, placingStructureLocation, placingStructureCenterPoint, 1));
            {
                if (!workerWithSmallestQueue.Busy)
                {
                    workerWithSmallestQueue.GiveCommand(new BuildStructureCommand(workerWithSmallestQueue, placingStructureType, placingStructureLocation, placingStructureCenterPoint, 1));
                    Player.Me.Roks -= placingStructureType.RoksCost;
                }
            }
            else
            {
                //workers[0].QueueCommand(new BuildStructureCommand(placingStructureType, placingStructureLocation, placingStructureCenterPoint, 1));
                workerWithSmallestQueue.QueueCommand(new BuildStructureCommand(workerWithSmallestQueue, placingStructureType, placingStructureLocation, placingStructureCenterPoint, 1));
                Player.Me.Roks -= placingStructureType.RoksCost;
            }*/
            BuildStructureCommand buildStructureCommand = new BuildStructureCommand(workerWithSmallestQueue, placingStructureType, placingStructureLocation, placingStructureCenterPoint, 1);
            ScheduledUnitBuildCommand scheduledUnitBuildCommand = new ScheduledUnitBuildCommand(currentScheduleTime, buildStructureCommand, usingShift);
            Player.Players[workerWithSmallestQueue.Team].ScheduledActions.Add(scheduledUnitBuildCommand);
            Player.Players[workerWithSmallestQueue.Team].Roks -= placingStructureType.RoksCost;
            RtsObject.PathFinder.AddHighPriorityPathFindRequest(buildStructureCommand, (int)Vector2.DistanceSquared(buildStructureCommand.Unit.CenterPoint, buildStructureCommand.Destination), false);

            NetOutgoingMessage msg = netPeer.CreateMessage();

            msg.Write(MessageID.UNIT_BUILD_COMMAND);
            msg.Write(currentScheduleTime);
            msg.Write(workerWithSmallestQueue.Team);

            msg.Write(workerWithSmallestQueue.ID);
            msg.Write(placingStructureType.ID);
            msg.Write((short)placingStructureLocation.X);
            msg.Write((short)placingStructureLocation.Y);
            msg.Write(usingShift);

            netPeer.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);

            /*foreach (RtsObject o in SelectedUnits)
            {
                WorkerNublet worker = o as WorkerNublet;
                if (worker != null)
                {
                    worker.GiveCommand(new BuildStructureCommand(placingStructureType, placingStructureLocation, placingStructureCenterPoint, 1));
                    break;
                }
            }*/
        }

        void giveStructureCommand(BuildUnitButtonType buttonType)
        {
            if (buttonType.UnitType.RoksCost > Player.Me.Roks)
            {
                playErrorSound();
                return;
            }

            Structure structureWithSmallestQueue = null;
            int smallest = int.MaxValue;

            foreach (RtsObject o in SelectedUnits)
            {
                Structure s = o as Structure;

                if (s.Team != Player.Me.Team)
                    return;

                if (s != null && s.Type == SelectedUnits.ActiveType && !s.UnderConstruction)
                {
                    if (s.BuildQueue.Count < smallest)
                    {
                        structureWithSmallestQueue = s;
                        smallest = s.BuildQueue.Count;
                    }
                }
            }

            float highestPercentDone = 0;
            foreach (RtsObject o in SelectedUnits)
            {
                Structure s = o as Structure;

                if (s != null)
                {
                    if (s != null && s.Type == SelectedUnits.ActiveType && !s.UnderConstruction)
                    {
                        if (smallest > 0 && s.BuildQueue.Count == smallest)
                        {
                            if (s.BuildQueue[0].PercentDone > highestPercentDone)
                            {
                                structureWithSmallestQueue = s;
                                highestPercentDone = s.BuildQueue[0].PercentDone;
                            }
                        }
                    }
                }
            }

            if (structureWithSmallestQueue != null)
            {
                //if (structureWithSmallestQueue.AddToBuildQueue(buttonType))
                if (structureWithSmallestQueue.CanAddToBuildQueue(buttonType))
                {
                    //float scheduledTime = gameClock + connection.AverageRoundtripTime;
                    short unitID = Player.Me.UnitIDCounter++;

                    Player.Me.ScheduledActions.Add(new ScheduledStructureCommand(currentScheduleTime, structureWithSmallestQueue, buttonType, unitID));
                    Player.Me.Roks -= buttonType.UnitType.RoksCost;

                    NetOutgoingMessage msg = netPeer.CreateMessage();

                    msg.Write(MessageID.STRUCTURE_COMMAND);
                    msg.Write(currentScheduleTime);
                    msg.Write(structureWithSmallestQueue.Team);
                    msg.Write(structureWithSmallestQueue.ID);
                    msg.Write(buttonType.ID);
                    msg.Write(unitID);

                    netPeer.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);

                    //Player.Me.Roks -= buttonType.UnitType.RoksCost;
                }
            }
        }

        bool giveHarvestCommand(Vector2 mousePosition)
        {
            foreach (Resource resource in Resource.Resources)
            {
                if (resource.Touches(mousePosition))
                {
                    List<ScheduledUnitTargetedCommand> scheduledHarvestCommands = new List<ScheduledUnitTargetedCommand>();
                    List<ScheduledUnitCommand> scheduledMoveCommands = new List<ScheduledUnitCommand>();

                    foreach (RtsObject o in SelectedUnits)
                    {
                        if (o.Team != Player.Me.Team)
                            return false;

                        Unit unit = o as Unit;
                        if (unit != null)
                        {
                            WorkerNublet worker = unit as WorkerNublet;
                            if (worker != null)
                            {
                                /*if (!usingShift)
                                    worker.GiveCommand(new HarvestCommand(worker, resource, 1));
                                else
                                    worker.QueueCommand(new HarvestCommand(worker, resource, 1));*/
                                scheduledHarvestCommands.Add(new ScheduledUnitTargetedCommand(currentScheduleTime, new HarvestCommand(worker, resource, 1), resource, usingShift));
                            }
                            else
                            {
                                Vector2 destinationPoint;
                                if (Unit.PathFinder.IsPointWalkable(mousePosition, unit))
                                    destinationPoint = mousePosition;
                                else
                                    destinationPoint = (Unit.PathFinder.FindNearestPathNode((int)(mousePosition.Y / map.TileSize),
                                        (int)(mousePosition.X / map.TileSize), unit)).Tile.CenterPoint;

                                /*if (!usingShift)
                                    unit.GiveCommand(new MoveCommand(unit, destinationPoint, 1));
                                else
                                    unit.QueueCommand(new MoveCommand(unit, destinationPoint, 1));*/

                                scheduledMoveCommands.Add(new ScheduledUnitCommand(currentScheduleTime, new MoveCommand(unit, destinationPoint, 1), usingShift));
                            }
                        }

                        /*Structure structure = o as Structure;
                        if (structure != null && structure.Rallyable)
                        {
                            if (!usingShift)
                                structure.RallyPoints.Clear();

                            structure.RallyPoints.Add(new RallyPoint(resource.CenterPoint, resource));
                        }*/
                    }

                    if (scheduledHarvestCommands.Count > 0)
                    {
                        NetOutgoingMessage msg = netPeer.CreateMessage();

                        msg.Write(MessageID.UNIT_HARVEST_COMMAND_BATCH);
                        msg.Write(currentScheduleTime);
                        msg.Write(Player.Me.Team);
                        msg.Write(usingShift);
                        msg.Write((short)scheduledHarvestCommands.Count);

                        foreach (ScheduledUnitTargetedCommand s in scheduledHarvestCommands)
                        {
                            HarvestCommand harvestCommand = s.UnitCommand as HarvestCommand;
                            Player.Players[harvestCommand.Unit.Team].ScheduledActions.Add(new ScheduledUnitTargetedCommand(currentScheduleTime, harvestCommand, harvestCommand.TargetResource, usingShift));
                            //Unit.PathFinder.AddHighPriorityPathFindRequest(harvestCommand, (int)Vector2.DistanceSquared(harvestCommand.Unit.CenterPoint, harvestCommand.Destination), false);

                            msg.Write(harvestCommand.Unit.ID);
                            msg.Write(harvestCommand.TargetResource.ID);
                        }

                        netPeer.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);
                    }

                    if (scheduledMoveCommands.Count > 0)
                    {
                        NetOutgoingMessage msg = netPeer.CreateMessage();

                        msg.Write(MessageID.UNIT_MOVE_COMMAND_BATCH);
                        msg.Write(currentScheduleTime);
                        msg.Write(Player.Me.Team);
                        msg.Write(usingShift);
                        msg.Write((short)scheduledMoveCommands.Count);

                        foreach (ScheduledUnitCommand s in scheduledMoveCommands)
                        {
                            MoveCommand moveCommand = s.UnitCommand as MoveCommand;

                            Player.Players[s.Team].ScheduledActions.Add(new ScheduledUnitCommand(currentScheduleTime, moveCommand, s.Queued));
                            //Unit.PathFinder.AddHighPriorityPathFindRequest(moveCommand, (int)Vector2.DistanceSquared(moveCommand.Unit.CenterPoint, moveCommand.Destination), false);

                            //if (s.Queued)
                            //moveCommand.Unit.QueueCommand(moveCommand);
                            //else
                            //moveCommand.Unit.GiveCommand(moveCommand);

                            msg.Write(moveCommand.Unit.ID);
                            msg.Write(moveCommand.Destination.X);
                            msg.Write(moveCommand.Destination.Y);
                        }

                        netPeer.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);
                    }

                    return true;
                }
            }

            return false;
        }

        void giveReturnCargoCommand()
        {
            List<ScheduledReturnCargoCommand> scheduledUnitCommands = new List<ScheduledReturnCargoCommand>();

            foreach (RtsObject o in SelectedUnits)
            {
                if (o.Team != Player.Me.Team)
                    return;

                WorkerNublet worker = o as WorkerNublet;
                if (worker != null)
                {
                    Resource source = null;
                    if (worker.Commands.Count > 0)
                    {
                        if (worker.Commands[0] is ReturnCargoCommand)
                            return;

                        if (worker.Commands[0] is HarvestCommand)
                            source = ((HarvestCommand)worker.Commands[0]).TargetResource;
                    }

                    //worker.ReturnCargoToNearestTownHall(source);

                    TownHall townHall = worker.FindNearestTownHall();

                    if (townHall != null)
                        scheduledUnitCommands.Add(new ScheduledReturnCargoCommand(currentScheduleTime, new ReturnCargoCommand(worker, townHall, source, 1)));
                }
            }

            NetOutgoingMessage msg = netPeer.CreateMessage();

            msg.Write(MessageID.UNIT_RETURN_CARGO_COMMAND_BATCH);
            msg.Write(currentScheduleTime);
            msg.Write(Player.Me.Team);
            msg.Write((short)scheduledUnitCommands.Count);

            foreach (ScheduledReturnCargoCommand scheduledCommand in scheduledUnitCommands)
            {
                msg.Write(scheduledCommand.ReturnCargoCommand.Unit.ID);
                msg.Write(scheduledCommand.ReturnCargoCommand.TargetStructure.ID);
                if (scheduledCommand.ReturnCargoCommand.Source != null)
                    msg.Write(scheduledCommand.ReturnCargoCommand.Source.ID);
                else
                    msg.Write((short)-1);

                Player.Me.ScheduledActions.Add(new ScheduledReturnCargoCommand(currentScheduleTime, scheduledCommand.ReturnCargoCommand));
            }

            netPeer.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);
        }

        void stop()
        {
            List<ScheduledUnitCommand> scheduledUnitCommands = new List<ScheduledUnitCommand>();

            /*if (usingShift)
            {
                foreach (RtsObject o in SelectedUnits)
                {
                    Unit unit = o as Unit;
                    if (unit != null)
                        unit.QueueCommand(new StopCommand(unit));
                }
            }
            else
            {
                foreach (RtsObject o in SelectedUnits)
                {
                    Unit unit = o as Unit;
                    if (unit != null)
                        unit.GiveCommand(new StopCommand(unit));
                }
            }*/

            foreach (RtsObject o in SelectedUnits)
            {
                Unit unit = o as Unit;
                if (unit != null && unit.Team == Player.Me.Team)
                    scheduledUnitCommands.Add(new ScheduledUnitCommand(currentScheduleTime, new StopCommand(unit), usingShift));
            }

            if (scheduledUnitCommands.Count == 0)
                return;

            NetOutgoingMessage msg = netPeer.CreateMessage();

            msg.Write(MessageID.UNIT_STOP_COMMAND_BATCH);
            msg.Write(currentScheduleTime);
            msg.Write(Player.Me.Team);
            msg.Write(usingShift);
            msg.Write((short)scheduledUnitCommands.Count);

            foreach (ScheduledUnitCommand command in scheduledUnitCommands)
            {
                msg.Write(command.UnitCommand.Unit.ID);

                Player.Me.ScheduledActions.Add(command);
            }

            netPeer.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);

            stopTargetedCommands();
        }

        void holdPosition()
        {
            List<ScheduledUnitCommand> scheduledUnitCommands = new List<ScheduledUnitCommand>();

            /*if (usingShift)
            {
                foreach (RtsObject o in SelectedUnits)
                {
                    Unit unit = o as Unit;
                    if (unit != null)
                        unit.QueueCommand(new HoldPositionCommand(unit));
                }
            }
            else
            {
                foreach (RtsObject o in SelectedUnits)
                {
                    Unit unit = o as Unit;
                    if (unit != null)
                        unit.GiveCommand(new HoldPositionCommand(unit));
                }
            }*/

            foreach (RtsObject o in SelectedUnits)
            {
                Unit unit = o as Unit;
                if (unit != null && unit.Team == Player.Me.Team)
                    scheduledUnitCommands.Add(new ScheduledUnitCommand(currentScheduleTime, new HoldPositionCommand(unit), usingShift));
            }

            if (scheduledUnitCommands.Count == 0)
                return;

            NetOutgoingMessage msg = netPeer.CreateMessage();

            msg.Write(MessageID.UNIT_HOLD_POSITION_COMMAND_BATCH);
            msg.Write(currentScheduleTime);
            msg.Write(Player.Me.Team);
            msg.Write(usingShift);
            msg.Write((short)scheduledUnitCommands.Count);

            foreach (ScheduledUnitCommand command in scheduledUnitCommands)
            {
                msg.Write(command.UnitCommand.Unit.ID);

                Player.Me.ScheduledActions.Add(command);
            }

            netPeer.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);

            stopTargetedCommands();
        }

        bool placingStructure, allowPlacingStructure, queueingPlacingStructure, tooCloseToResource;
        StructureType placingStructureType;
        List<PathNode> placingStructurePathNodes = new List<PathNode>(), placingStructureBlockedPathNodes = new List<PathNode>();
        Point placingStructureLocation = new Point();
        Vector2 placingStructureCenterPoint;
        void updatePlacingStructure()
        {
            if (!placingStructure)
                return;

            Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y);
            mousePosition = Vector2.Transform(mousePosition, Matrix.Invert(camera.get_transformation(worldViewport)));
            
            placingStructureLocation.X = (int)Math.Round(MathHelper.Clamp(mousePosition.X / map.TileSize - placingStructureType.Size / 2f, 0, map.Width - 1));
            placingStructureLocation.Y = (int)Math.Round(MathHelper.Clamp(mousePosition.Y / map.TileSize - placingStructureType.Size / 2f, 0, map.Height - 1));

            placingStructureCenterPoint.X = placingStructureLocation.X * map.TileSize + (placingStructureType.Size * map.TileSize) / 2;
            placingStructureCenterPoint.Y = placingStructureLocation.Y * map.TileSize + (placingStructureType.Size * map.TileSize) / 2;

            placingStructurePathNodes.Clear();
            placingStructureBlockedPathNodes.Clear();
            allowPlacingStructure = true;

            Circle collisionCircle = new Circle(new Vector2(placingStructureLocation.X * map.TileSize + (placingStructureType.Size / 2) * map.TileSize, placingStructureLocation.Y * map.TileSize + (placingStructureType.Size / 2) * map.TileSize), placingStructureType.Size * map.TileSize);
            //placingStructureCenterPoint = collisionCircle.CenterPoint;

            if (placingStructureType == StructureType.TownHall)
            {
                foreach (Resource resource in Resource.Resources)
                {
                    if (Vector2.Distance(collisionCircle.CenterPoint, resource.CenterPoint) < map.TileSize * 9)
                    {
                        allowPlacingStructure = false;
                        tooCloseToResource = true;
                    }
                }
            }
            else
                tooCloseToResource = false;

            for (int x = placingStructureLocation.X; x < placingStructureLocation.X + placingStructureType.Size; x++)
            {
                for (int y = placingStructureLocation.Y; y < placingStructureLocation.Y + placingStructureType.Size; y++)
                {
                    PathNode node = Structure.PathFinder.PathNodes[(int)MathHelper.Clamp(y, 0, map.Height - 1), (int)MathHelper.Clamp(x, 0, map.Width - 1)];
                    if (collisionCircle.Intersects(node.Tile))
                    {
                        placingStructurePathNodes.Add(node);
                    }
                }
            }

            // remove corners
            if (placingStructureType.CutCorners)
            {
                placingStructurePathNodes.Remove(Structure.PathFinder.PathNodes[placingStructureLocation.Y, placingStructureLocation.X]);
                if (placingStructureLocation.X + placingStructureType.Size <= map.Width - 1)
                    placingStructurePathNodes.Remove(Structure.PathFinder.PathNodes[placingStructureLocation.Y, placingStructureLocation.X + placingStructureType.Size - 1]);
                else
                    allowPlacingStructure = false;
                if (placingStructureLocation.Y + placingStructureType.Size <= map.Height - 1)
                    placingStructurePathNodes.Remove(Structure.PathFinder.PathNodes[placingStructureLocation.Y + placingStructureType.Size - 1, placingStructureLocation.X]);
                else
                    allowPlacingStructure = false;
                if (allowPlacingStructure)
                    placingStructurePathNodes.Remove(Structure.PathFinder.PathNodes[placingStructureLocation.Y + placingStructureType.Size - 1, placingStructureLocation.X + placingStructureType.Size - 1]);
            }

            for (int i = 0; i < placingStructurePathNodes.Count; )
            {
                PathNode node = placingStructurePathNodes[i];

                if (!node.Tile.Walkable || (node.Blocked && node.Blocker is Structure && ((Structure)node.Blocker).Visible) || (node.Blocked && node.Blocker is Resource))
                {
                    allowPlacingStructure = false;
                    placingStructurePathNodes.Remove(node);
                    placingStructureBlockedPathNodes.Add(node);
                }
                else
                    i++;
            }
        }

        List<PlacedStructure> placedStructures = new List<PlacedStructure>();
        void updatePlacedStructures()
        {
            placedStructures.Clear();

            foreach (Unit unit in Unit.Units)
            {
                WorkerNublet worker = unit as WorkerNublet;
                if (worker != null)
                {
                    if (worker.Commands.Count > 0)
                    {
                        foreach (UnitCommand command in worker.Commands)
                        {
                            BuildStructureCommand buildCommand = command as BuildStructureCommand;
                            if (buildCommand != null)
                            {
                                //placedStructures.Add(new ObjectLink<StructureType, Rectangle>(buildCommand.StructureType, new Rectangle((int)(buildCommand.Destination.X - buildCommand.StructureType.Size * map.TileSize / 2), (int)(buildCommand.Destination.Y - buildCommand.StructureType.Size * map.TileSize / 2), buildCommand.StructureType.Size * map.TileSize, buildCommand.StructureType.Size * map.TileSize)));
                                placedStructures.Add(new PlacedStructure(buildCommand.StructureType, new Rectangle(buildCommand.StructureLocation.X * map.TileSize, buildCommand.StructureLocation.Y * map.TileSize, buildCommand.StructureType.Size * map.TileSize, buildCommand.StructureType.Size * map.TileSize), worker.Team));
                            }
                        }
                    }
                }
            }

            foreach (Structure structure in Structure.Structures)
            {
                if (structure.Builder != null)
                {
                    WorkerNublet worker = structure.Builder as WorkerNublet;
                    if (worker != null)
                    {
                        if (worker.Commands.Count > 0)
                        {
                            foreach (UnitCommand command in worker.Commands)
                            {
                                BuildStructureCommand buildCommand = command as BuildStructureCommand;
                                if (buildCommand != null)
                                {
                                    //placedStructures.Add(new ObjectLink<StructureType, Rectangle>(buildCommand.StructureType, new Rectangle((int)(buildCommand.Destination.X - buildCommand.StructureType.Size * map.TileSize / 2), (int)(buildCommand.Destination.Y - buildCommand.StructureType.Size * map.TileSize / 2), buildCommand.StructureType.Size * map.TileSize, buildCommand.StructureType.Size * map.TileSize)));
                                    placedStructures.Add(new PlacedStructure(buildCommand.StructureType, new Rectangle(buildCommand.StructureLocation.X * map.TileSize, buildCommand.StructureLocation.Y * map.TileSize, buildCommand.StructureType.Size * map.TileSize, buildCommand.StructureType.Size * map.TileSize), worker.Team));
                                }
                            }
                        }
                    }
                }
            }
        }

        void createMoveCommandShrinker(Vector2 position, bool attackMove)
        {
            Shrinker moveCommandThing;
            if (Unit.PathFinder.IsPointWalkable(position))
                moveCommandThing = new Shrinker(position - new Vector2(moveCommandShrinkerSize / 2f, moveCommandShrinkerSize / 2f), moveCommandShrinkerSize, moveCommandShrinkDelay);
            else
                moveCommandThing = new Shrinker(map.FindNearestWalkableTile(position) - new Vector2(moveCommandShrinkerSize / 2f, moveCommandShrinkerSize / 2f), moveCommandShrinkerSize, moveCommandShrinkDelay);

            if (attackMove)
                moveCommandThing.Texture = attackMoveCommandShrinkerTexture;
            else
                moveCommandThing.Texture = moveCommandShrinkerTexture;
        }

        bool allowTab;
        void checkForTab()
        {
            if (keyboardState.IsKeyUp(Keys.Tab))
                allowTab = true;
            else if (allowTab && keyboardState.IsKeyDown(Keys.Tab))
            {
                SelectedUnits.TabActiveType();
                selectedUnitsChanged = true;
                allowTab = false;
            }
        }

        void switchToBuildMenuCommandCard()
        {
            SimpleButton.RemoveButtons(CommandCardButtons);
            CommandCardButtons.Clear();

            int buttonSize = commandCardArea.Width / 4;
            int spacing = 2;
            Rectangle box = new Rectangle(commandCardArea.X, commandCardArea.Y, buttonSize, buttonSize);

            for (int x = 0; x < CommandCard.BuildMenuCommandCard.Buttons.GetLength(0); x++)
            {
                for (int y = 0; y < CommandCard.BuildMenuCommandCard.Buttons.GetLength(1); y++)
                {
                    if ((Object)(CommandCard.BuildMenuCommandCard.Buttons[x, y]) != null)
                    {
                        //spriteBatch.Draw(commandCard.Buttons[x, y].Texture, button, Color.White);

                        //CommandCardButtons.Add(new SimpleButton(box, SelectedUnits.ActiveCommandCard.Buttons[x, y].Texture, null, null));

                        CommandCard.BuildMenuCommandCard.Buttons[x, y].Rectangle = box;
                        CommandCardButtons.Add(CommandCard.BuildMenuCommandCard.Buttons[x, y]);
                    }
                    box.X += buttonSize + spacing;
                }
                box.X = commandCardArea.X;
                box.Y += buttonSize + spacing;
            }

            SimpleButton.AddButtons(CommandCardButtons);
        }

        bool allowSelect = true, allowTargetedCommand = true, allowClickToPlaceStructure = true, allowMiniMapClick = true, allowRemoveFromQueue = true;
        void checkForLeftClick(GameTime gameTime)
        {
            // check if clicked on unit portrait
            if (SelectedUnits.Count == 1)
            {
                if (mouseState.LeftButton == ButtonState.Pressed &&
                    unitPictureRectangle.Contains(mouseState.X, mouseState.Y))
                {
                    centerCameraOnSelectedUnits();
                    return;
                }
            }
            // check if clicked on a unit box
            foreach (UnitButton box in unitButtons)
            {
                if (box.Triggered)
                {
                    // holding shift
                    if (usingShift)
                    {
                        SelectedUnits.Remove(box.Unit);
                    }
                    // not holding shift
                    else
                    {
                        SelectedUnits.Clear();
                        SelectedUnits.Add(box.Unit);
                    }
                    selectedUnitsChanged = true;
                    return;
                }
            }
            //check if clicked on structure queue item
            if (QueuedItems != null)
            {
                if (mouseState.LeftButton == ButtonState.Released)
                    allowRemoveFromQueue = true;
                else if (allowRemoveFromQueue)
                {
                    allowRemoveFromQueue = false;
                    //foreach (SimpleButton item in QueuedItems)
                    for (int i = 0; i < QueuedItems.Length; i++)
                    {
                        Structure s = SelectedUnits[0] as Structure;
                        if (mouseState.LeftButton == ButtonState.Pressed && QueuedItems[i].Contains(mouseState.X, mouseState.Y) && s.BuildQueue.Count > i)
                        //if (QueuedItems[i].Triggered)
                        {
                            //s.BuildQueue.RemoveAt(i);
                            s.RemoveFromBuildQueue(i);
                        }
                    }
                }
            }

            if ((usingTargetedCommand && mouseState.LeftButton == ButtonState.Pressed) || selecting)
                allowMiniMapClick = false;
            else if (mouseState.LeftButton == ButtonState.Released)
                allowMiniMapClick = true;

            // clicked on bottom ui
            if (mouseState.Y > worldViewport.Height)
            {
                if (!selecting)
                {
                    SelectBox.Enabled = false;
                    //SelectBox.Clear();
                    //SelectingUnits.Clear();
                }
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    if (minimap.Contains(mouseState.X, mouseState.Y))
                    {
                        if (allowMiniMapClick)
                        {
                            //Vector2 mousePosition = Vector2.Transform(new Vector2((mouseState.X - minimapPosX) / minimapToMapRatioX, (mouseState.Y - minimapPosY) / minimapToMapRatioY), camera.get_transformation(worldViewport));

                            Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y);
                            Vector2 minimapCenterPoint = new Vector2(minimap.X + minimap.Width / 2f, minimap.Y + minimap.Height / 2f);

                            float distance = Vector2.Distance(mousePosition, minimapCenterPoint);
                            float angle = (float)Math.Atan2(mousePosition.Y - minimapCenterPoint.Y, mousePosition.X - minimapCenterPoint.X);

                            mousePosition = new Vector2(minimapCenterPoint.X + distance * (float)Math.Cos(angle - camera.Rotation), minimapCenterPoint.Y + distance * (float)Math.Sin(angle - camera.Rotation));

                            camera.Pos = new Vector2((mousePosition.X - minimapPosX) / minimapToMapRatioX, (mousePosition.Y - minimapPosY) / minimapToMapRatioY);

                            //Matrix transform = camera.get_minimap_transformation(minimapViewport);
                            //Vector2 mousePosition = Vector2.Transform(new Vector2(mouseState.X - minimapPosX, mouseState.Y - minimapPosY), transform);
                            
                            //camera.Pos = new Vector2((mouseState.X - minimapPosX) / minimapToMapRatioX, (mouseState.Y - minimapPosY) / minimapToMapRatioY);

                            //float asdf = minimapToMapRatioX - minimapToMapRatioY;
                            //float smallest = minimapToMapRatioY;

                            //camera.Pos = new Vector2(mousePosition.X / minimapToMapRatioX, mousePosition.Y / minimapToMapRatioY);
                            //camera.Pos = mousePosition;
                            stopTargetedCommands();
                        }
                    }
                    else
                    {
                        stopTargetedCommands();
                        //SimpleButton.PressingHotkey = false;
                    }
                }
            }
            // clicked somewhere above bottom ui
            else
            {
                if (mouseState.LeftButton == ButtonState.Released)
                    SelectBox.Enabled = true;
            }

            if (usingTargetedCommand)
            {
                if (mouseState.LeftButton == ButtonState.Released)
                    allowTargetedCommand = true;
                else if (allowTargetedCommand && mouseState.LeftButton == ButtonState.Pressed)
                {
                    allowTargetedCommand = false;

                    allowSelect = false;
                    SelectBox.Enabled = false;

                    Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y);
                    if (minimap.Contains(mouseState.X, mouseState.Y))
                        mousePosition = new Vector2((mousePosition.X - minimapPosX) / minimapToMapRatioX, (mousePosition.Y - minimapPosY) / minimapToMapRatioY);
                    else
                        mousePosition = Vector2.Transform(mousePosition, Matrix.Invert(camera.get_transformation(worldViewport)));

                    if (usingAttackCommand)
                    {
                        giveAttackCommand(mousePosition);
                    }
                    else if (usingRallyPointCommand)
                    {
                        setRallyPoint(mousePosition);
                    }

                    if (usingShift)
                        queueingTargetedCommand = true;
                    else
                        stopTargetedCommands();
                }
            }
            else if (placingStructure)
            {
                if (mouseState.LeftButton == ButtonState.Released)
                    allowClickToPlaceStructure = true;
                else if (allowClickToPlaceStructure && mouseState.LeftButton == ButtonState.Pressed)
                {
                    allowClickToPlaceStructure = false;

                    allowSelect = false;
                    SelectBox.Enabled = false;

                    if (allowPlacingStructure)
                    {
                        //new Barracks(placingStructureLocation, myTeam);
                        giveBuildCommand();

                        if (usingShift)
                        {
                            queueingPlacingStructure = true;
                        }
                        else
                        {
                            placingStructure = false;
                        }
                    }
                    else
                    {
                        playErrorSound();
                    }
                }
            }
            else
            {
                if (mouseState.LeftButton == ButtonState.Released)
                {
                    allowSelect = true;
                    SelectBox.Enabled = true;
                }

                if (allowSelect)
                    SelectUnits(gameTime);
            }
        }

        /*bool allowStop;
        void checkForStop()
        {
            if (keyboardState.IsKeyUp(Keys.S))
                allowStop = true;
            else if (allowStop && keyboardState.IsKeyDown(Keys.S))
            {
                //allowStop = false;

                stop();
            }
        }*/

        void stopTargetedCommands()
        {
            usingTargetedCommand = false;
            usingAttackCommand = false;
            usingRallyPointCommand = false;

            queueingTargetedCommand = false;

            winForm.Cursor = normalCursor;
        }

        /*bool allowHoldPosition;
        void checkForHoldPosition()
        {
            if (keyboardState.IsKeyUp(Keys.H))
                allowHoldPosition = true;
            else if (allowHoldPosition && keyboardState.IsKeyDown(Keys.H))
            {
                allowHoldPosition = false;

                holdPosition();
            }
        }*/

        const int SHIFTDELAY = 100;
        int timeSinceShift;
        bool usingShift;
        void checkForShift(GameTime gameTime)
        {
            if (keyboardState.IsKeyDown(Keys.LeftShift))
            {
                if (!usingShift)
                {
                    usingShift = true;
                    timeSinceShift = 0;
                }
            }
            else
            {
                if (usingShift)
                {
                    timeSinceShift += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (timeSinceShift >= SHIFTDELAY)
                    {
                        usingShift = false;
                    }
                }
            }
        }

        int doubleHotkeySelectDelay = 250, lastHotKeyGroupSelected = -1;
        bool[] allowHotkeyGroupSelect = new bool[10];
        int[] timeSinceLastHotkeyGroupSelect = new int[10];
        Keys[] hotKeyGroupKeys = new Keys[10] 
        { Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9 };
        void checkHotKeyGroups(GameTime gameTime)
        {
            int elapsedMilliseconds = (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            for (int i = 0; i < 10; i++)
            {
                timeSinceLastHotkeyGroupSelect[i] += elapsedMilliseconds;

                if (keyboardState.IsKeyUp(hotKeyGroupKeys[i]))
                    allowHotkeyGroupSelect[i] = true;
                else if (allowHotkeyGroupSelect[i] && keyboardState.IsKeyDown(hotKeyGroupKeys[i]))
                {
                    allowHotkeyGroupSelect[i] = false;
                    if (usingTargetedCommand)
                    {
                        stopTargetedCommands();
                    }
                    if (placingStructure)
                        placingStructure = false;

                    // assign hotkey group
                    if (keyboardState.IsKeyDown(Keys.LeftControl))
                    {
                        selectedUnitsChanged = true;

                        //HotkeyGroups[i] = new List<RtsObject>(SelectedUnits.ToArray<RtsObject>());
                        HotkeyGroups[i] = new List<RtsObject>(SelectedUnits.ToArray());
                    }
                    // select hotkey group
                    else
                    {
                        selectedUnitsChanged = true;
                        if (HotkeyGroups[i].Count > 0)
                        {
                            //SelectedUnits = new List<Unit>(HotkeyGroups[i].ToArray<Unit>());
                            SelectedUnits = new Selection(HotkeyGroups[i]);
                            if (lastHotKeyGroupSelected == i && 
                                timeSinceLastHotkeyGroupSelect[i] <= doubleHotkeySelectDelay)
                                centerCameraOnSelectedUnits();
                            timeSinceLastHotkeyGroupSelect[i] = 0;
                            lastHotKeyGroupSelected = i;
                        }
                    }
                }
            }

            /*timeSinceLastHotkeyGroupSelect += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (keyboardState.IsKeyUp(Keys.D0) && keyboardState.IsKeyUp(Keys.D1) &&
                keyboardState.IsKeyUp(Keys.D2) && keyboardState.IsKeyUp(Keys.D3) &&
                keyboardState.IsKeyUp(Keys.D4) && keyboardState.IsKeyUp(Keys.D5) &&
                keyboardState.IsKeyUp(Keys.D6) && keyboardState.IsKeyUp(Keys.D7) &&
                keyboardState.IsKeyUp(Keys.D8) && keyboardState.IsKeyUp(Keys.D9))
                allowHotkeyGroupSelect = true;
            else if (allowHotkeyGroupSelect &&
                (keyboardState.IsKeyDown(Keys.D0) || keyboardState.IsKeyDown(Keys.D1) ||
                keyboardState.IsKeyDown(Keys.D2) || keyboardState.IsKeyDown(Keys.D3) ||
                keyboardState.IsKeyDown(Keys.D4) || keyboardState.IsKeyDown(Keys.D5) ||
                keyboardState.IsKeyDown(Keys.D6) || keyboardState.IsKeyDown(Keys.D7) ||
                keyboardState.IsKeyDown(Keys.D8) || keyboardState.IsKeyDown(Keys.D9)))
            {
                allowHotkeyGroupSelect = false;
                if (usingAttackCommand)
                {
                    usingAttackCommand = false;
                    winForm.Cursor = normalCursor;
                }

                if (keyboardState.IsKeyDown(Keys.LeftControl))
                {
                    selectedUnitsChanged = true;
                    if (keyboardState.IsKeyDown(Keys.D0))
                        HotkeyGroups[0] = new List<Unit>(SelectedUnits.ToArray<Unit>());
                    else if (keyboardState.IsKeyDown(Keys.D1))
                        HotkeyGroups[1] = new List<Unit>(SelectedUnits.ToArray<Unit>());
                    else if (keyboardState.IsKeyDown(Keys.D2))
                        HotkeyGroups[2] = new List<Unit>(SelectedUnits.ToArray<Unit>());
                    else if (keyboardState.IsKeyDown(Keys.D3))
                        HotkeyGroups[3] = new List<Unit>(SelectedUnits.ToArray<Unit>());
                    else if (keyboardState.IsKeyDown(Keys.D4))
                        HotkeyGroups[4] = new List<Unit>(SelectedUnits.ToArray<Unit>());
                    else if (keyboardState.IsKeyDown(Keys.D5))
                        HotkeyGroups[5] = new List<Unit>(SelectedUnits.ToArray<Unit>());
                    else if (keyboardState.IsKeyDown(Keys.D6))
                        HotkeyGroups[6] = new List<Unit>(SelectedUnits.ToArray<Unit>());
                    else if (keyboardState.IsKeyDown(Keys.D7))
                        HotkeyGroups[7] = new List<Unit>(SelectedUnits.ToArray<Unit>());
                    else if (keyboardState.IsKeyDown(Keys.D8))
                        HotkeyGroups[8] = new List<Unit>(SelectedUnits.ToArray<Unit>());
                    else if (keyboardState.IsKeyDown(Keys.D9))
                        HotkeyGroups[9] = new List<Unit>(SelectedUnits.ToArray<Unit>());
                    else
                        selectedUnitsChanged = false;
                }
                else
                {
                    selectedUnitsChanged = true;
                    if (keyboardState.IsKeyDown(Keys.D0) && HotkeyGroups[0].Count > 0)
                    {
                        SelectedUnits = new List<Unit>(HotkeyGroups[0].ToArray<Unit>());
                        if (lastHotkeyGroupSelected == 0 && timeSinceLastHotkeyGroupSelect <= doubleHotkeySelectDelay)
                            centerCameraOnSelectedUnits();
                        timeSinceLastHotkeyGroupSelect = 0;
                        lastHotkeyGroupSelected = 0;
                    }
                    else if (keyboardState.IsKeyDown(Keys.D1) && HotkeyGroups[1].Count > 0)
                    {
                        SelectedUnits = new List<Unit>(HotkeyGroups[1].ToArray<Unit>());
                        if (lastHotkeyGroupSelected == 1 && timeSinceLastHotkeyGroupSelect <= doubleHotkeySelectDelay)
                            centerCameraOnSelectedUnits();
                        timeSinceLastHotkeyGroupSelect = 0;
                        lastHotkeyGroupSelected = 1;
                    }
                    else if (keyboardState.IsKeyDown(Keys.D2) && HotkeyGroups[2].Count > 0)
                    {
                        SelectedUnits = new List<Unit>(HotkeyGroups[2].ToArray<Unit>());
                        if (lastHotkeyGroupSelected == 2 && timeSinceLastHotkeyGroupSelect <= doubleHotkeySelectDelay)
                            centerCameraOnSelectedUnits();
                        timeSinceLastHotkeyGroupSelect = 0;
                        lastHotkeyGroupSelected = 2;
                    }
                    else if (keyboardState.IsKeyDown(Keys.D3) && HotkeyGroups[3].Count > 0)
                    {
                        SelectedUnits = new List<Unit>(HotkeyGroups[3].ToArray<Unit>());
                        if (lastHotkeyGroupSelected == 3 && timeSinceLastHotkeyGroupSelect <= doubleHotkeySelectDelay)
                            centerCameraOnSelectedUnits();
                        timeSinceLastHotkeyGroupSelect = 0;
                        lastHotkeyGroupSelected = 3;
                    }
                    else if (keyboardState.IsKeyDown(Keys.D4) && HotkeyGroups[4].Count > 0)
                    {
                        SelectedUnits = new List<Unit>(HotkeyGroups[4].ToArray<Unit>());
                        if (lastHotkeyGroupSelected == 4 && timeSinceLastHotkeyGroupSelect <= doubleHotkeySelectDelay)
                            centerCameraOnSelectedUnits();
                        timeSinceLastHotkeyGroupSelect = 0;
                        lastHotkeyGroupSelected = 4;
                    }
                    else if (keyboardState.IsKeyDown(Keys.D5) && HotkeyGroups[5].Count > 0)
                    {
                        SelectedUnits = new List<Unit>(HotkeyGroups[5].ToArray<Unit>());
                        if (lastHotkeyGroupSelected == 5 && timeSinceLastHotkeyGroupSelect <= doubleHotkeySelectDelay)
                            centerCameraOnSelectedUnits();
                        timeSinceLastHotkeyGroupSelect = 0;
                        lastHotkeyGroupSelected = 5;
                    }
                    else if (keyboardState.IsKeyDown(Keys.D6) && HotkeyGroups[6].Count > 0)
                    {
                        SelectedUnits = new List<Unit>(HotkeyGroups[6].ToArray<Unit>());
                        if (lastHotkeyGroupSelected == 6 && timeSinceLastHotkeyGroupSelect <= doubleHotkeySelectDelay)
                            centerCameraOnSelectedUnits();
                        timeSinceLastHotkeyGroupSelect = 0;
                        lastHotkeyGroupSelected = 6;
                    }
                    else if (keyboardState.IsKeyDown(Keys.D7) && HotkeyGroups[7].Count > 0)
                    {
                        SelectedUnits = new List<Unit>(HotkeyGroups[7].ToArray<Unit>());
                        if (lastHotkeyGroupSelected == 7 && timeSinceLastHotkeyGroupSelect <= doubleHotkeySelectDelay)
                            centerCameraOnSelectedUnits();
                        timeSinceLastHotkeyGroupSelect = 0;
                        lastHotkeyGroupSelected = 7;
                    }
                    else if (keyboardState.IsKeyDown(Keys.D8) && HotkeyGroups[8].Count > 0)
                    {
                        SelectedUnits = new List<Unit>(HotkeyGroups[8].ToArray<Unit>());
                        if (lastHotkeyGroupSelected == 8 && timeSinceLastHotkeyGroupSelect <= doubleHotkeySelectDelay)
                            centerCameraOnSelectedUnits();
                        timeSinceLastHotkeyGroupSelect = 0;
                        lastHotkeyGroupSelected = 8;
                    }
                    else if (keyboardState.IsKeyDown(Keys.D9) && HotkeyGroups[9].Count > 0)
                    {
                        SelectedUnits = new List<Unit>(HotkeyGroups[9].ToArray<Unit>());
                        if (lastHotkeyGroupSelected == 9 && timeSinceLastHotkeyGroupSelect <= doubleHotkeySelectDelay)
                            centerCameraOnSelectedUnits();
                        timeSinceLastHotkeyGroupSelect = 0;
                        lastHotkeyGroupSelected = 9;
                    }
                    else
                        selectedUnitsChanged = false;
                }
            }*/
        }

        void checkForMouseCameraScroll(GameTime gameTime)
        {
            Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y);

            Vector2 movement = Vector2.Zero;

            /*if (mousePosition.X <= 0)
                movement += new Vector2(-cameraScrollSpeed / camera.Zoom, 0);
            else if (mousePosition.X >= GraphicsDevice.Viewport.Width - 1)
                movement += new Vector2(cameraScrollSpeed / camera.Zoom, 0);

            if (mousePosition.Y <= 0)
                movement += new Vector2(0, -cameraScrollSpeed / camera.Zoom);
            else if (mousePosition.Y >= GraphicsDevice.Viewport.Height - 1)
                movement += new Vector2(0, cameraScrollSpeed / camera.Zoom);*/

            float adjustedScrollSpeed = cameraScrollSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds / camera.Zoom;

            if (mousePosition.X <= 0 || keyboardState.IsKeyDown(Keys.Left))
            {
                float angle = MathHelper.WrapAngle(-camera.Rotation + (float)Math.PI);
                movement += new Vector2(adjustedScrollSpeed * (float)Math.Cos(angle), adjustedScrollSpeed * (float)Math.Sin(angle));
            }
            else if (mousePosition.X >= uiViewport.Width - 1 || keyboardState.IsKeyDown(Keys.Right))
            {
                float angle = MathHelper.WrapAngle(-camera.Rotation);
                movement += new Vector2(adjustedScrollSpeed * (float)Math.Cos(angle), adjustedScrollSpeed * (float)Math.Sin(angle));
            }

            if (mousePosition.Y <= 0 || keyboardState.IsKeyDown(Keys.Up))
            {
                float angle = MathHelper.WrapAngle(-camera.Rotation - (float)Math.PI / 2);
                movement += new Vector2(adjustedScrollSpeed * (float)Math.Cos(angle), adjustedScrollSpeed * (float)Math.Sin(angle));
            }
            else if (mousePosition.Y >= uiViewport.Height - 1 || keyboardState.IsKeyDown(Keys.Down))
            {
                float angle = MathHelper.WrapAngle(-camera.Rotation + (float)Math.PI / 2);
                movement += new Vector2(adjustedScrollSpeed * (float)Math.Cos(angle), adjustedScrollSpeed * (float)Math.Sin(angle));
            }

            if (movement != Vector2.Zero)
                camera.Move(movement);
        }

        void checkForCameraZoom(GameTime gameTime)
        {
            if (keyboardState.IsKeyDown(Keys.OemMinus))
                //camera.Zoom -= cameraZoomSpeed;
                camera.Zoom = MathHelper.Max(camera.Zoom - camera.Zoom * Util.ScaleWithGameTime(cameraZoomSpeed, gameTime), .5f);

            if (keyboardState.IsKeyDown(Keys.OemPlus))
                //camera.Zoom += cameraZoomSpeed;
                camera.Zoom = MathHelper.Min(camera.Zoom + camera.Zoom * Util.ScaleWithGameTime(cameraZoomSpeed, gameTime), 2f);
        }

        bool allowCameraRotate;
        void checkForCameraRotate(GameTime gameTime)
        {
            // check for changes to rotation target
            if (keyboardState.IsKeyUp(Keys.PageDown) && keyboardState.IsKeyUp(Keys.PageUp))
                allowCameraRotate = true;
            else if (allowCameraRotate)
            {
                if (keyboardState.IsKeyDown(Keys.PageDown))
                {
                    cameraRotationTarget += cameraRotationIncrement;
                    allowCameraRotate = false;
                }

                if (keyboardState.IsKeyDown(Keys.PageUp))
                {
                    cameraRotationTarget -= cameraRotationIncrement;
                    allowCameraRotate = false;
                }
            }

            // rotate camera to target rotation
            float actualRotationSpeed = Util.ScaleWithGameTime(cameraRotationSpeed, gameTime);
            if (Util.AngleDifference(camera.Rotation, cameraRotationTarget) < actualRotationSpeed)
                camera.Rotation = cameraRotationTarget;
            else if (camera.Rotation < cameraRotationTarget)
                camera.Rotation += actualRotationSpeed;
            else
                camera.Rotation -= actualRotationSpeed;
        }

        void centerCameraOnSelectedUnits()
        {
            if (SelectedUnits.Count == 0)
                return;

            Rectangle rectangle = SelectedUnits[0];
            foreach (RtsObject o in SelectedUnits)
                rectangle = Rectangle.Union(rectangle, o.Rectangle);

            camera.Pos = new Vector2(rectangle.Center.X, rectangle.Center.Y);
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

        CommandCard activeCommandCard;
        void resetCommandCard()
        {
            SimpleButton.RemoveButtons(CommandCardButtons);
            CommandCardButtons.Clear();

            activeCommandCard = SelectedUnits.ActiveCommandCard;

            bool allSelectedUnitsAreUnderConstruction = true;
            //int unitsUnderConstruction = 0, unitsNotUnderConstruction = 0;
            foreach (RtsObject o in SelectedUnits)
            {
                Structure s = o as Structure;
                if (s != null && s.UnderConstruction)
                    //unitsUnderConstruction++;
                    continue;
                allSelectedUnitsAreUnderConstruction = false;
                break;
                //else
                //    unitsNotUnderConstruction++;
            }
            if (allSelectedUnitsAreUnderConstruction)
            //if (unitsUnderConstruction > unitsNotUnderConstruction)
                activeCommandCard = CommandCard.UnderConstructionCommandCard;
            /*if (SelectedUnits.Count == 1)
            {
                Structure s = SelectedUnits[0] as Structure;
                if (s != null && s.UnderConstruction)
                {
                    commandCard = CommandCard.UnderConstructionCommandCard;
                }
            }*/

            if (activeCommandCard == null)
                return;

            int buttonSize = commandCardArea.Width / 4;
            int spacing = 2;
            Rectangle box = new Rectangle(commandCardArea.X, commandCardArea.Y, buttonSize, buttonSize);

            for (int x = 0; x < activeCommandCard.Buttons.GetLength(0); x++)
            {
                for (int y = 0; y < activeCommandCard.Buttons.GetLength(1); y++)
                {
                    if ((Object)(activeCommandCard.Buttons[x, y]) != null)
                    {
                        activeCommandCard.Buttons[x, y].Rectangle = box;
                        CommandCardButtons.Add(activeCommandCard.Buttons[x, y]);
                    }
                    box.X += buttonSize + spacing;
                }
                box.X = commandCardArea.X;
                box.Y += buttonSize + spacing;
            }

            SimpleButton.AddButtons(CommandCardButtons);
        }

        public void CheckForResetCommandCardWhenStructureCompletes(Structure s)
        {
            if (SelectedUnits.Count == 1 && SelectedUnits.Contains(s))
                resetCommandCard();

            foreach (StructureCogWheel cog in CogWheels)
            {
                if (cog.Structure == s)
                {
                    CogWheels.Remove(cog);
                    break;
                }
            }
        }

        public List<StructureCogWheel> CogWheels = new List<StructureCogWheel>();
        float cogWheelRotationSpeed = 2.5f;
        void updateCogWheels(GameTime gameTime)
        {
            foreach (StructureCogWheel cog in CogWheels)
                cog.Rotation += Util.ScaleWithGameTime(cogWheelRotationSpeed, gameTime);
        }

        BaseObject cameraView = new BaseObject(new Rectangle());
        void clampCameraToMap()
        {
            cameraView.Width = (int)(worldViewport.Width / camera.Zoom);
            cameraView.Height = (int)(worldViewport.Height / camera.Zoom);
            cameraView.CenterPoint = camera.Pos;
            cameraView.Rotation = -camera.Rotation;
            cameraView.CalculateCorners();

            // upper left corner
            if (cameraView.UpperLeftCorner.X < 0)
            {
                cameraView.X += (int)Math.Round(-cameraView.UpperLeftCorner.X * (float)Math.Cos(0));
                cameraView.CalculateCorners();
            }
            if (cameraView.UpperLeftCorner.Y < 0)
            {
                cameraView.Y += (int)Math.Round(-cameraView.UpperLeftCorner.Y * (float)Math.Sin(MathHelper.PiOver2));
                cameraView.CalculateCorners();
            }
            if (cameraView.UpperLeftCorner.X > actualMapWidth)
            {
                cameraView.X += (int)Math.Round((cameraView.UpperLeftCorner.X - actualMapWidth) * (float)Math.Cos(Math.PI));
                cameraView.CalculateCorners();
            }
            if (cameraView.UpperLeftCorner.Y > actualMapHeight)
            {
                cameraView.Y += (int)Math.Round((cameraView.UpperLeftCorner.Y - actualMapHeight) * (float)Math.Sin(-MathHelper.PiOver2));
                cameraView.CalculateCorners();
            }

            // lower left corner
            if (cameraView.LowerLeftCorner.X < 0)
            {
                cameraView.X += (int)Math.Round(-cameraView.LowerLeftCorner.X * (float)Math.Cos(0));
                cameraView.CalculateCorners();
            }
            if (cameraView.LowerLeftCorner.Y < 0)
            {
                cameraView.Y += (int)Math.Round(-cameraView.LowerLeftCorner.Y * (float)Math.Sin(MathHelper.PiOver2));
                cameraView.CalculateCorners();
            }
            if (cameraView.LowerLeftCorner.X > actualMapWidth)
            {
                cameraView.X += (int)Math.Round((cameraView.LowerLeftCorner.X - actualMapWidth) * (float)Math.Cos(Math.PI));
                cameraView.CalculateCorners();
            }
            if (cameraView.LowerLeftCorner.Y > actualMapHeight)
            {
                cameraView.Y += (int)Math.Round((cameraView.LowerLeftCorner.Y - actualMapHeight) * (float)Math.Sin(-MathHelper.PiOver2));
                cameraView.CalculateCorners();
            }

            // upper right corner
            if (cameraView.UpperRightCorner.X < 0)
            {
                cameraView.X += (int)Math.Round(-cameraView.UpperRightCorner.X * (float)Math.Cos(0));
                cameraView.CalculateCorners();
            }
            if (cameraView.UpperRightCorner.Y < 0)
            {
                cameraView.Y += (int)Math.Round(-cameraView.UpperRightCorner.Y * (float)Math.Sin(MathHelper.PiOver2));
                cameraView.CalculateCorners();
            }
            if (cameraView.UpperRightCorner.X > actualMapWidth)
            {
                cameraView.X += (int)Math.Round((cameraView.UpperRightCorner.X - actualMapWidth) * (float)Math.Cos(Math.PI));
                cameraView.CalculateCorners();
            }
            if (cameraView.UpperRightCorner.Y > actualMapHeight)
            {
                cameraView.Y += (int)Math.Round((cameraView.UpperRightCorner.Y - actualMapHeight) * (float)Math.Sin(-MathHelper.PiOver2));
                cameraView.CalculateCorners();
            }

            // lower right corner
            if (cameraView.LowerRightCorner.X < 0)
            {
                cameraView.X += (int)Math.Round(-cameraView.LowerRightCorner.X * (float)Math.Cos(0));
                cameraView.CalculateCorners();
            }
            if (cameraView.LowerRightCorner.Y < 0)
            {
                cameraView.Y += (int)Math.Round(-cameraView.LowerRightCorner.Y * (float)Math.Sin(MathHelper.PiOver2));
                cameraView.CalculateCorners();
            }
            if (cameraView.LowerRightCorner.X > actualMapWidth)
            {
                cameraView.X += (int)Math.Round((cameraView.LowerRightCorner.X - actualMapWidth) * (float)Math.Cos(Math.PI));
                cameraView.CalculateCorners();
            }
            if (cameraView.LowerRightCorner.Y > actualMapHeight)
            {
                cameraView.Y += (int)Math.Round((cameraView.LowerRightCorner.Y - actualMapHeight) * (float)Math.Sin(-MathHelper.PiOver2));
                cameraView.CalculateCorners();
            }

            camera.Pos = cameraView.CenterPoint;

            if (cameraView.Width > map.Width * map.TileSize)
                camera.Pos = new Vector2(map.Width / 2 * map.TileSize, camera.Pos.Y);
            if (cameraView.Height > map.Height * map.TileSize)
                camera.Pos = new Vector2(map.Height / 2 * map.TileSize, camera.Pos.X);

            /*float cameraLeftBound = camera.Pos.X + (GraphicsDevice.Viewport.Width / 2 * (float)Math.Cos((float)Math.PI + camera.Rotation));
            float cameraRightBound = camera.Pos.X + (GraphicsDevice.Viewport.Width / 2 * (float)Math.Cos(camera.Rotation));
            float cameraTopBound = camera.Pos.Y + (GraphicsDevice.Viewport.Height / 2 * (float)Math.Sin(-MathHelper.PiOver2 + camera.Rotation));
            float cameraBottomBound = camera.Pos.Y + (GraphicsDevice.Viewport.Height / 2 * (float)Math.Sin(MathHelper.PiOver2 + camera.Rotation));*/

            /*if (camera.Pos.X < GraphicsDevice.Viewport.Width / camera.Zoom / 2)
                camera.Pos = new Vector2(GraphicsDevice.Viewport.Width / camera.Zoom / 2, camera.Pos.Y);
            if (camera.Pos.X > map.Width * Map.TILESIZE - GraphicsDevice.Viewport.Width / camera.Zoom / 2)
                camera.Pos = new Vector2(map.Width * Map.TILESIZE - GraphicsDevice.Viewport.Width / camera.Zoom / 2, camera.Pos.Y);
            if (camera.Pos.Y < GraphicsDevice.Viewport.Height / camera.Zoom / 2)
                camera.Pos = new Vector2(camera.Pos.X, GraphicsDevice.Viewport.Height / camera.Zoom / 2);
            if (camera.Pos.Y > map.Height * Map.TILESIZE - GraphicsDevice.Viewport.Height / camera.Zoom / 2)
                camera.Pos = new Vector2(camera.Pos.X, map.Height * Map.TILESIZE - GraphicsDevice.Viewport.Height / camera.Zoom / 2);*/
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