using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;
using System.Windows.Forms;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace rts
{
    class Rts : GameState
    {
        static bool contentLoaded = false;
        protected TimeSpan fpsElapsedTime;
        protected int frameCounter;
        protected static bool paused, allowPause;
        Random rand = new Random();
        public static Stopwatch GameTimer = new Stopwatch();
        string fpsMessage = "";
        static SpriteFont pauseFont, fpsFont, unitInfoUnitNameFont, unitInfoHpFont, unitInfoKillCountFont;

        int myTeam = 1;
        static Texture2D greenTeamIndicatorTexture, redTeamIndicatorTexture;

        Form winForm;
        static Cursor normalCursor, attackCursor;

        double timeForPathFindingProfiling, pathFindingPercentage;
        int pathFindQueueSize;

        MouseState mouseState;
        KeyboardState keyboardState;

        Camera camera;
        float cameraScrollSpeed = 1000, cameraZoomSpeed = 1, cameraRotationSpeed = 4.5f, cameraRotationTarget, cameraRotationIncrement = MathHelper.PiOver2;//MathHelper.PiOver4 / 2;

        static Song rtsMusic;

        BaseObject button1, button2, button3, button4, button5;
        static Texture2D buttonTexture;

        //static Texture2D brownGuyTexture, brownGuySelectingTexture, brownGuySelectedTexture;
        static Texture2D moveCommandTexture, normalCursorTexture, attackCommandCursorTexture;
        static Texture2D redCircleTexture, transparentTexture, whiteBoxTexture, 
            transparentGrayTexture, transparentBlackTexture;

        const int MAXSELECTIONSIZE = int.MaxValue;//36;
        static List<Unit> SelectingUnits = new List<Unit>();
        //static List<Unit> SelectedUnits = new List<Unit>();
        static Selection SelectedUnits = new Selection();
        static List<RtsObject>[] HotkeyGroups = new List<RtsObject>[10];
        bool selectedUnitsChanged;

        bool usingAttackCommand, queueingAttackCommand;
        int normalCursorSize = 28, attackCommandCursorSize = 23;

        Map map;
        static Texture2D boulder1Texture, tree1Texture;
        int actualMapWidth, actualMapHeight;

        VisionUpdater VisionUpdater;

        Texture2D fullMapTexture;
        Rectangle minimap;
        int minimapSize = 125, minimapBorderSize = 5;
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

        Viewport worldViewport, uiViewport;

        public Rts(EventHandler callback)
            : base(callback)
        {
            Game1.Game.DebugMonitor.Position = Direction.NorthEast;
            Game1.Game.IsMouseVisible = true;
            //Game1.Game.Graphics.SynchronizeWithVerticalRetrace = false;
            //Game1.Game.IsFixedTimeStep = false;
            //Game1.Game.Graphics.ApplyChanges();
            GameTimer.Restart();

            map = new Map(@"Content/map1.txt");
            Unit.Map = map;
            actualMapWidth = map.Width * map.TileSize;
            actualMapHeight = map.Height * map.TileSize;

            Unit.UnitCollisionSweeper.Thread.Suspend();
            Unit.UnitCollisionSweeper.Thread.Resume();
            Unit.PathFinder.ResumeThread();

            uiViewport = GraphicsDevice.Viewport;
            worldViewport = GraphicsDevice.Viewport;
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
                //brownGuyTexture = Content.Load<Texture2D>("unit textures/browncircleguy");
                //brownGuySelectingTexture = Content.Load<Texture2D>("unit textures/browncircleguyselected2");
                //brownGuySelectedTexture = Content.Load<Texture2D>("unit textures/browncircleguyselecting2");
                greenTeamIndicatorTexture = Content.Load<Texture2D>("unit textures/green team indicator");
                redTeamIndicatorTexture = Content.Load<Texture2D>("unit textures/red team indicator");
                buttonTexture = Content.Load<Texture2D>("titlebutton1");
                moveCommandTexture = Content.Load<Texture2D>("greencircle2");
                //normalCursorTexture = Content.Load<Texture2D>("greencursor2");
                //attackCommandCursorTexture = Content.Load<Texture2D>("crosshair");
                normalCursor = Util.LoadCustomCursor(@"Content/cursors/SC2-cursor.cur");
                attackCursor = Util.LoadCustomCursor(@"Content/cursors/SC2-target-none.cur");
                boulder1Texture = Content.Load<Texture2D>("boulder1");
                tree1Texture = Content.Load<Texture2D>("tree2");
                redCircleTexture = Content.Load<Texture2D>("redcircle");
                transparentTexture = Content.Load<Texture2D>("transparent");
                transparentGrayTexture = Content.Load<Texture2D>("transparentgray");
                transparentBlackTexture = Content.Load<Texture2D>("transparentblack");
                whiteBoxTexture = Content.Load<Texture2D>("whitebox");
                rtsMusic = Content.Load<Song>("sounds/crossingthosehills");
                //Unit.BulletTexture = Content.Load<Texture2D>("bullet");
                Unit.Explosion1Textures = Util.SplitTexture(Content.Load<Texture2D>("explosionsheet1"), 45, 45);
                contentLoaded = true;
            }

            winForm = (Form)Form.FromHandle(Game1.Game.Window.Handle);
            winForm.Cursor = normalCursor;

            VisionUpdater = new VisionUpdater(map, Unit.PathFinder, myTeam);

            initializeMapTexture();
            initializeCommandCardArea();
            initializeSelectionInfoArea();

            SelectBox.InitializeSelectBoxLine(GraphicsDevice, Color.Green);
            InitializeSelectionRingLine(GraphicsDevice, Color.Yellow);

            minimapScreenIndicatorBoxLine = new PrimitiveLine(GraphicsDevice, 1);
            minimapScreenIndicatorBoxLine.Colour = Color.White;

            for (int i = 0; i < HotkeyGroups.Length; i++)
                HotkeyGroups[i] = new List<RtsObject>();

            MediaPlayer.Play(rtsMusic);
            MediaPlayer.Volume = .25f;
            MediaPlayer.IsRepeating = true;
        }

        public override void Update(GameTime gameTime)
        {
            // check for exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Game1.Game.Exit();
            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Escape))
            {
                //Graphics.ToggleFullScreen();
                cleanup();
                returnControl("exit");
                return;
            }

            // mute check
            checkForMute();

            // pause check
            if (Keyboard.GetState(PlayerIndex.One).IsKeyUp(Keys.P))
                allowPause = true;
            if (allowPause && Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.P))
            {
                paused ^= true;
                allowPause = false;
                if (paused)
                {
                    MediaPlayer.Volume /= 4;
                    GameTimer.Stop();
                }
                else
                {
                    MediaPlayer.Volume *= 4;
                    GameTimer.Start();
                }
            }

            // tell pathfinder to start calculating
            Unit.PathFinder.Go = true;

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

            if (button1.Rectangle.Contains(mouseState.X, mouseState.Y) && mouseState.LeftButton == ButtonState.Pressed)
            {
                Unit brownGuy;
                brownGuy = new RangedNublet(new Vector2(worldViewport.Width * .25f, worldViewport.Height / 2), 1);
                //brownGuy.Texture = brownGuyTexture;
                //brownGuy.AddWayPoint(new Vector2(Graphics.GraphicsDevice.Viewport.Width * .75f, Graphics.GraphicsDevice.Viewport.Height / 2));
                brownGuy.GiveCommand(new MoveCommand(new Vector2(worldViewport.Width * .75f, worldViewport.Height / 2), 1));
            }
            else if (button2.Rectangle.Contains(mouseState.X, mouseState.Y) && mouseState.LeftButton == ButtonState.Pressed)
            {
                for (int i = 0; i < 10; i++)
                {
                    Unit brownGuy;
                    brownGuy = new RangedNublet(new Vector2(worldViewport.Width * .25f, worldViewport.Height / 2), 1);
                    //brownGuy.Texture = brownGuyTexture;
                    //brownGuy.AddWayPoint(new Vector2(Graphics.GraphicsDevice.Viewport.Width * .75f, Graphics.GraphicsDevice.Viewport.Height / 2));
                    brownGuy.GiveCommand(new MoveCommand(new Vector2(worldViewport.Width * .75f, worldViewport.Height / 2), 1));
                }
            }
            else if (button3.Rectangle.Contains(mouseState.X, mouseState.Y) && mouseState.LeftButton == ButtonState.Pressed)
            {
                Graphics.ToggleFullScreen();
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
                meleeNublet = new MeleeNublet(new Vector2(worldViewport.Width * .25f, worldViewport.Height / 2), 1);
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
            }

            /*if (SelectedUnits.Count == 1)
            {
                Game1.Game.Window.Title = "idle: " + SelectedUnits[0].IsIdle + " hp: " + SelectedUnits[0].Hp + "/" + SelectedUnits[0].MaxHp + ". position " + SelectedUnits[0].CenterPoint;
            }*/

            checkForShift(gameTime);

            checkForCommands();

            Shrinker.UpdateShrinkers(gameTime);

            checkHotKeyGroups(gameTime);

            SelectBox.Update(worldViewport, camera);

            checkForLeftClick(gameTime);

            checkForRightClick();

            checkForStop();
            checkForTab();

            RtsBullet.UpdateAll(gameTime);

            Unit.UpdateUnits(gameTime);
            UnitAnimation.UpdateAll();

            removeDeadUnitsFromSelections();

            checkForMouseCameraScroll(gameTime);
            checkForCameraZoom(gameTime);
            checkForCameraRotate(gameTime);
            if (keyboardState.IsKeyDown(Keys.Space))
                centerCameraOnSelectedUnits();
            clampCameraToMap();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Game1.Game.DebugMonitor.AddLine("camera position: " + camera.Pos);
            Game1.Game.DebugMonitor.AddLine("camera zoom: " + camera.Zoom);
            Game1.Game.DebugMonitor.AddLine("camera rotation: " + camera.Rotation);
            Game1.Game.DebugMonitor.AddLine("pathfinding usage: " + pathFindingPercentage.ToString("F1") + "%");
            Game1.Game.DebugMonitor.AddLine("pathfinding queue: " + pathFindQueueSize);
            Game1.Game.DebugMonitor.AddLine("time: " + GameTimer.Elapsed.Minutes + ":" + GameTimer.Elapsed.Seconds.ToString("D2"));

            GraphicsDevice.Clear(Color.Black);

            GraphicsDevice.Viewport = worldViewport;
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, camera.get_transformation(worldViewport));

            drawMap(spriteBatch);

            // units
            foreach (Unit unit in SelectingUnits)
                //drawSelectionRing(unit, spriteBatch, Color.Green);
                drawSelectingRing(unit, spriteBatch);
            foreach (Unit unit in SelectedUnits)
                //drawSelectionRing(unit, spriteBatch, Color.Khaki);
                drawSelectedRing(unit, spriteBatch);

            foreach (Unit unit in Unit.Units)
            {
                if (!SelectingUnits.Contains(unit) && !SelectedUnits.Contains(unit))
                    spriteBatch.Draw(unit.Texture, new Rectangle((int)unit.CenterPoint.X, (int)unit.CenterPoint.Y, unit.Width, unit.Height), null, Color.White, unit.Rotation, unit.TextureCenterOrigin, SpriteEffects.None, 0f);

                /*int teamIndicatorSize = (int)(unit.Diameter / 4);
                // Rectangle teamIndicator = new Rectangle((int)(unit.CenterPoint.X - teamIndicatorSize / 2), (int)(unit.CenterPoint.Y - teamIndicatorSize / 2), teamIndicatorSize, teamIndicatorSize);
                Rectangle teamIndicator = new Rectangle((int)unit.CenterPoint.X, (int)unit.CenterPoint.Y, teamIndicatorSize, teamIndicatorSize);

                if (unit.Team == myTeam)
                    spriteBatch.Draw(ColorTexture.Green, teamIndicator, null, Color.White, unit.Rotation, ColorTexture.CenterVector, SpriteEffects.None, 0f);
                else
                    spriteBatch.Draw(ColorTexture.Red, teamIndicator, null, Color.White, unit.Rotation, ColorTexture.CenterVector, SpriteEffects.None, 0f);*/

                if (unit.Team == myTeam)
                    spriteBatch.Draw(greenTeamIndicatorTexture, new Rectangle((int)unit.CenterPoint.X, (int)unit.CenterPoint.Y, unit.Width, unit.Height), null, Color.White, unit.Rotation, new Vector2(greenTeamIndicatorTexture.Width / 2, greenTeamIndicatorTexture.Height / 2), SpriteEffects.None, 0f);
                else
                    spriteBatch.Draw(redTeamIndicatorTexture, new Rectangle((int)unit.CenterPoint.X, (int)unit.CenterPoint.Y, unit.Width, unit.Height), null, Color.White, unit.Rotation, new Vector2(redTeamIndicatorTexture.Width / 2, redTeamIndicatorTexture.Height / 2), SpriteEffects.None, 0f);
            }

            // unit animations
            foreach (UnitAnimation a in UnitAnimation.UnitAnimations)
                spriteBatch.Draw(a, new Rectangle(a.Rectangle.Center.X, a.Rectangle.Center.Y, a.Rectangle.Width, a.Rectangle.Height), null, Color.White, a.Rotation, new Vector2(((Texture2D)a).Width / 2, ((Texture2D)a).Height / 2), SpriteEffects.None, 0f);

            foreach (Unit unit in SelectedUnits)
            {
                if (unit.IsMoving)
                {
                    selectionRingLine.ClearVectors();
                    selectionRingLine.AddVector(unit.CenterPoint);
                    //foreach (Vector2 v in unit.WayPoints)
                    //    selectionRingLine.AddVector(v);
                    foreach (MoveCommand command in unit.Commands)
                    {
                        if (command is AttackCommand)
                            selectionRingLine.Colour = Color.Red;
                        else
                            selectionRingLine.Colour = Color.Green;

                        foreach (Vector2 v in command.WayPoints)
                            selectionRingLine.AddVector(v);

                        selectionRingLine.Render(spriteBatch);
                        selectionRingLine.ClearVectors();
                        selectionRingLine.AddVector(command.Destination);
                    }
                }
            }

            PrimitiveLine line = new PrimitiveLine(GraphicsDevice, 1);
            line.Colour = Color.Black;

            if (SelectedUnits.Count == 1 && SelectedUnits[0] is Unit)
            {
                Unit unit = SelectedUnits[0] as Unit;

                //PrimitiveLine line = new PrimitiveLine(GraphicsDevice, 1);
                //line.Colour = Color.Black;
                lock (SelectedUnits[0].PotentialCollisions)
                {
                    foreach (Unit u in SelectedUnits[0].PotentialCollisions)
                    {
                        line.ClearVectors();
                        line.AddVector(SelectedUnits[0].CenterPoint);
                        line.AddVector(u.CenterPoint);
                        line.Render(spriteBatch);
                    }
                }
                line.Colour = Color.Red;
                line.ClearVectors();
                line.AddVector(new Vector2(unit.CurrentPathNode.Tile.X, unit.CurrentPathNode.Tile.Y));
                line.AddVector(new Vector2(unit.CurrentPathNode.Tile.X + unit.CurrentPathNode.Tile.Width, unit.CurrentPathNode.Tile.Y));
                line.AddVector(new Vector2(unit.CurrentPathNode.Tile.X + unit.CurrentPathNode.Tile.Width, unit.CurrentPathNode.Tile.Y + unit.CurrentPathNode.Tile.Height));
                line.AddVector(new Vector2(unit.CurrentPathNode.Tile.X, unit.CurrentPathNode.Tile.Y + unit.CurrentPathNode.Tile.Height));
                line.AddVector(new Vector2(unit.CurrentPathNode.Tile.X, unit.CurrentPathNode.Tile.Y));
                line.Render(spriteBatch);
                line.Colour = Color.Black;
                foreach (MapTile tile in unit.CurrentPathNode.Tile.Neighbors)
                {
                    line.ClearVectors();
                    line.AddVector(new Vector2(tile.X, tile.Y));
                    line.AddVector(new Vector2(tile.X + tile.Width, tile.Y));
                    line.AddVector(new Vector2(tile.X + tile.Width, tile.Y + tile.Height));
                    line.AddVector(new Vector2(tile.X, tile.Y + tile.Height));
                    line.AddVector(new Vector2(tile.X, tile.Y));
                    line.Render(spriteBatch);
                }
            }

            /*foreach (Unit unit in SelectedUnits)
            {
                lock (unit.PotentialCollisions)
                {
                    foreach (Unit u in unit.PotentialCollisions)
                    {
                        line.ClearVectors();
                        line.AddVector(unit.CenterPoint);
                        line.AddVector(u.CenterPoint);
                        line.Render(spriteBatch);
                    }
                }
            }*/

            // bullets
            foreach (RtsBullet b in RtsBullet.RtsBullets)
                //spriteBatch.Draw(b.Texture, b, Color.White);
                spriteBatch.Draw(b.Texture, new Rectangle((int)b.CenterPoint.X, (int)b.CenterPoint.Y, b.Width, b.Height), null, Color.White, b.Rotation, b.TextureCenterOrigin, SpriteEffects.None, 0f);

            // move command shrinker things
            foreach (Shrinker shrinker in Shrinker.Shrinkers)
                spriteBatch.Draw(shrinker.Texture, shrinker, Color.White);
            //spriteBatch.Draw(shrinker.Texture, new Rectangle((int)shrinker.CenterPoint.X, (int)shrinker.CenterPoint.Y, shrinker.Width, shrinker.Height), null, Color.White, shrinker.Rotation, shrinker.TextureCenterOrigin, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin();

            GraphicsDevice.Viewport = uiViewport;
            
            SelectBox.Draw(spriteBatch, camera);

            drawMinimap(spriteBatch);
            drawSelectionInfoArea(spriteBatch);
            drawCommandCardArea(spriteBatch);
            drawCommandCardBorder(spriteBatch);

            //pause and fps count
            Vector2 pauseStringSize = pauseFont.MeasureString("PAUSED");
            if (paused)
                spriteBatch.DrawString(pauseFont, "PAUSED", new Vector2(uiViewport.Width / 2 - pauseStringSize.X / 2, uiViewport.Height / 2 - pauseStringSize.Y / 2), Color.White);
            else
                frameCounter++;

            // fps message
            spriteBatch.DrawString(fpsFont, fpsMessage, new Vector2(8, 5), Color.Black);

            spriteBatch.Draw(buttonTexture, button1, Color.White);
            Vector2 button1TextSize = fpsFont.MeasureString("1");
            spriteBatch.DrawString(fpsFont, "1", new Vector2((int)(button1.X + button1.Width / 2 - button1TextSize.X / 2), (int)(button1.Y + button1.Height / 2 - button1TextSize.Y / 2)), Color.White);
            spriteBatch.Draw(buttonTexture, button2, Color.White);
            Vector2 button2TextSize = fpsFont.MeasureString("10");
            spriteBatch.DrawString(fpsFont, "10", new Vector2((int)(button2.X + button2.Width / 2 - button2TextSize.X / 2), (int)(button2.Y + button2.Height / 2 - button2TextSize.Y / 2)), Color.White);
            spriteBatch.Draw(buttonTexture, button3, Color.White);
            Vector2 button3TextSize = fpsFont.MeasureString("FS");
            spriteBatch.DrawString(fpsFont, "FS", new Vector2((int)(button3.X + button3.Width / 2 - button3TextSize.X / 2), (int)(button3.Y + button3.Height / 2 - button3TextSize.Y / 2)), Color.White);
            spriteBatch.Draw(buttonTexture, button4, Color.White);
            Vector2 button4TextSize = fpsFont.MeasureString("M");
            spriteBatch.DrawString(fpsFont, "M", new Vector2((int)(button4.X + button4.Width / 2 - button4TextSize.X / 2), (int)(button4.Y + button4.Height / 2 - button4TextSize.Y / 2)), Color.White);
            spriteBatch.Draw(buttonTexture, button5, Color.White);
            Vector2 button5TextSize = fpsFont.MeasureString("X");
            spriteBatch.DrawString(fpsFont, "X", new Vector2((int)(button5.X + button5.Width / 2 - button5TextSize.X / 2), (int)(button5.Y + button5.Height / 2 - button5TextSize.Y / 2)), Color.White);

            // cursor
            /*if (usingAttackCommand)
                spriteBatch.Draw(attackCommandCursorTexture, new Rectangle(mouseState.X - attackCommandCursorSize / 2, mouseState.Y - attackCommandCursorSize / 2, attackCommandCursorSize, attackCommandCursorSize), Color.White);
            else
                spriteBatch.Draw(normalCursorTexture, new Rectangle(mouseState.X, mouseState.Y, attackCommandCursorSize, attackCommandCursorSize), Color.White);*/

            spriteBatch.End();

            // tell pathfinder to stop calculating
            Unit.PathFinder.Go = false;
        }

        void cleanup()
        {
            Unit.UnitCollisionSweeper.Thread.Abort();
            Unit.PathFinder.AbortThread();
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

        static PrimitiveLine selectionRingLine;
        public static void InitializeSelectionRingLine(GraphicsDevice graphicsDevice, Color color)
        {
            selectionRingLine = new PrimitiveLine(graphicsDevice, 1);
            selectionRingLine.Colour = color;
        }

        void drawSelectionRing(Unit unit, SpriteBatch spriteBatch, Color color)
        {
            selectionRingLine.Colour = color;

            selectionRingLine.Position = unit.CenterPoint;
            selectionRingLine.CreateCircle(unit.Radius, (int)Math.Round(unit.Radius * 2));
            selectionRingLine.Render(spriteBatch);
        }
        void drawSelectingRing(Unit unit, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(unit.SelectingTexture, new Rectangle((int)unit.CenterPoint.X, (int)unit.CenterPoint.Y, unit.Width, unit.Height), null, Color.White, unit.Rotation, unit.TextureCenterOrigin, SpriteEffects.None, 0f);
        }
        void drawSelectedRing(Unit unit, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(unit.SelectedTexture, new Rectangle((int)unit.CenterPoint.X, (int)unit.CenterPoint.Y, unit.Width, unit.Height), null, Color.White, unit.Rotation, unit.TextureCenterOrigin, SpriteEffects.None, 0f);
        }

        void checkForCommands()
        {
            foreach (CommandButton button in CommandCardButtons)
            {
                if (button.Triggered)
                {
                    if (button.Type == CommandButtonType.Attack)
                    {
                        usingAttackCommand = true;
                        winForm.Cursor = attackCursor;
                    }
                }
            }

            checkForAttackCommand();
        }

        bool allowAKey;
        void checkForAttackCommand()
        {
            if (keyboardState.IsKeyUp(Keys.A))
                allowAKey = true;
            else if (allowAKey && keyboardState.IsKeyDown(Keys.A) &&
                SelectedUnits.Count > 0)
            {
                foreach (CommandButton commandButton in SelectedUnits.ActiveCommandCard.Buttons)
                {
                    if (commandButton.Type == CommandButtonType.Attack)
                    {
                        usingAttackCommand = true;
                        winForm.Cursor = attackCursor;
                        allowAKey = false;

                        //commandButton.Pressing = true;

                        break;
                    }
                }
            }

            if (queueingAttackCommand && keyboardState.IsKeyUp(Keys.LeftShift))
            {
                usingAttackCommand = queueingAttackCommand = false;
                winForm.Cursor = normalCursor;
            }
        }

        bool allowSelect = true, allowAttackCommand = true, allowMiniMapClick = true;
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

            if ((usingAttackCommand && mouseState.LeftButton == ButtonState.Pressed) || selecting)
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
                            camera.Pos = new Vector2((mouseState.X - minimapPosX) / minimapToMapRatioX, (mouseState.Y - minimapPosY) / minimapToMapRatioY);
                            usingAttackCommand = false;
                            winForm.Cursor = normalCursor;
                        }
                    }
                    else
                    {
                        usingAttackCommand = false;
                        //SimpleButton.PressingHotkey = false;
                        winForm.Cursor = normalCursor;
                    }
                }
            }
            // clicked somewhere above bottom ui
            else
            {
                if (mouseState.LeftButton == ButtonState.Released)
                    SelectBox.Enabled = true;
            }

            if (usingAttackCommand)
            {
                if (mouseState.LeftButton == ButtonState.Released)
                    allowAttackCommand = true;
                else if (allowAttackCommand && mouseState.LeftButton == ButtonState.Pressed)
                {
                    allowAttackCommand = false;

                    if (usingShift)
                        queueingAttackCommand = true;
                    else
                    {
                        usingAttackCommand = false;
                        winForm.Cursor = normalCursor;
                    }
                    allowSelect = false;
                    SelectBox.Enabled = false;

                    Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y);
                    if (minimap.Contains(mouseState.X, mouseState.Y))
                        mousePosition = new Vector2((mousePosition.X - minimapPosX) / minimapToMapRatioX, (mousePosition.Y - minimapPosY) / minimapToMapRatioY);
                    else
                        mousePosition = Vector2.Transform(mousePosition, Matrix.Invert(camera.get_transformation(worldViewport)));

                    giveAttackCommand(mousePosition);
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

        void giveAttackCommand(Vector2 mousePosition)
        {
            foreach (Unit unit in Unit.Units)
            {
                if (unit.Contains(mousePosition))
                {
                    UnitAnimation redCircleAnimation = new UnitAnimation(unit, unit.Width, .75f, 8, false, redCircleTexture, transparentTexture);
                    redCircleAnimation.Start();

                    foreach (Unit u in SelectedUnits)
                    {
                        if (u != unit)
                        {
                            if (keyboardState.IsKeyUp(Keys.LeftShift))
                            {
                                AttackCommand command = new AttackCommand(unit);
                                u.GiveCommand(command);
                                Unit.PathFinder.AddHighPriorityPathFindRequest(u, command, u.CurrentPathNode, (int)Vector2.DistanceSquared(u.CenterPoint, command.Destination), false);
                                //u.GiveCommand(new AttackCommand(unit));
                            }
                            else
                                u.QueueCommand(new AttackCommand(unit));
                        }
                    }
                    return;
                }
            }

            giveMoveCommand(mousePosition);
        }

        bool selecting, unitsSelected;
        const int doubleClickDelay = 225, simpleClickSize = 4;
        int timeSinceLastSimpleClick = doubleClickDelay;
        Unit lastUnitClicked = null;
        void SelectUnits(GameTime gameTime)
        {
            //SelectBox.Box.CalculateCorners();

            int selectingUnitsCount = SelectingUnits.Count;
            SelectingUnits.Clear();

            timeSinceLastSimpleClick += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            bool simpleClick = (SelectBox.Box.GreaterOfWidthAndHeight <= simpleClickSize);


            if (SelectBox.IsSelecting)
            {
                selecting = true;
                unitsSelected = false;
                foreach (Unit unit in Unit.Units)
                {
                    if ((simpleClick && unit.Contains(Vector2.Transform(new Vector2(mouseState.X, mouseState.Y), Matrix.Invert(camera.get_transformation(worldViewport))))) ||
                            (!simpleClick && SelectBox.Box.Rectangle.Intersects(unit.Rectangle)))
                    {
                        if (SelectingUnits.Count < MAXSELECTIONSIZE)
                            SelectingUnits.Add(unit);
                    }
                }
            }
            else if (unitsSelected == false)
            {
                selecting = false;
                unitsSelected = true;
                selectedUnitsChanged = true;

                // holding shift
                if (usingShift)
                {
                    foreach (Unit unit in Unit.Units)
                    {
                        if ((simpleClick && unit.Contains(Vector2.Transform(new Vector2(mouseState.X, mouseState.Y), Matrix.Invert(camera.get_transformation(worldViewport))))) ||
                            (!simpleClick && SelectBox.Box.Rectangle.Intersects(unit.Rectangle)))
                        {
                            // holding ctrl or double click
                            if ((simpleClick && lastUnitClicked == unit && timeSinceLastSimpleClick <= doubleClickDelay) ||
                                (simpleClick && keyboardState.IsKeyDown(Keys.LeftControl)))
                            {
                                timeSinceLastSimpleClick = 0;

                                foreach (Unit u in Unit.Units)
                                    if (u.Type == unit.Type && !u.IsOffScreen(worldViewport, camera))
                                    {
                                        if (SelectedUnits.Count >= MAXSELECTIONSIZE)
                                            break;

                                        SelectedUnits.Add(u);
                                        //selectedUnitsChanged = true;
                                    }
                            }
                            // not holding ctrl or double click
                            else
                            {
                                if (!SelectedUnits.Contains(unit))
                                {
                                    if (SelectedUnits.Count < MAXSELECTIONSIZE)
                                        SelectedUnits.Add(unit);
                                    //selectedUnitsChanged = true;
                                }
                                else if (simpleClick)
                                {
                                    SelectedUnits.Remove(unit);
                                    //selectedUnitsChanged = true;
                                }
                            }
                            lastUnitClicked = unit;
                        }
                    }
                }
                // not holding shift
                else
                {
                    SelectedUnits.Clear();
                    //selectedUnitsChanged = true;

                    foreach (Unit unit in Unit.Units)
                    {
                        if ((simpleClick && unit.Contains(Vector2.Transform(new Vector2(mouseState.X, mouseState.Y), Matrix.Invert(camera.get_transformation(worldViewport))))) ||
                            (!simpleClick && SelectBox.Box.Rectangle.Intersects(unit.Rectangle)))
                        {
                            // holding ctrl or double click
                            if ((simpleClick && lastUnitClicked == unit && timeSinceLastSimpleClick <= doubleClickDelay) ||
                                (simpleClick && keyboardState.IsKeyDown(Keys.LeftControl)))
                            {
                                timeSinceLastSimpleClick = 0;

                                foreach (Unit u in Unit.Units)
                                    if (u.Type == unit.Type && !u.IsOffScreen(worldViewport, camera))
                                    {
                                        if (SelectedUnits.Count >= MAXSELECTIONSIZE)
                                            break;

                                        SelectedUnits.Add(u);
                                    }
                            }
                            // not holding ctrl or double click
                            else
                            {
                                if (SelectedUnits.Count < MAXSELECTIONSIZE)
                                    SelectedUnits.Add(unit);
                            }

                            lastUnitClicked = unit;
                        }
                    }

                    SelectedUnits.SetActiveTypeToMostPopulousType();
                }
                if (simpleClick)
                    timeSinceLastSimpleClick = 0;
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

                if (usingAttackCommand)
                {
                    usingAttackCommand = false;
                    winForm.Cursor = normalCursor;
                }
                else
                    rightClick();
            }
        }

        const int magicBoxMaxSize = 500;
        const int moveCommandShrinkerSize = 18;
        void rightClick()
        {
            if (SelectedUnits.Count == 0)
                return;

            //magicBoxMaxSize = SelectedUnits.Count * 5;

            Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y);
            if (minimap.Contains(mouseState.X, mouseState.Y))
                mousePosition = new Vector2((mousePosition.X - minimapPosX) / minimapToMapRatioX, (mousePosition.Y - minimapPosY) / minimapToMapRatioY);
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

            // move to a point
            giveMoveCommand(mousePosition);
        }

        void giveMoveCommand(Vector2 mousePosition)
        {
            // create move command shrinker thing
            Shrinker moveCommandThing;
            if (Unit.PathFinder.IsPointWalkable(mousePosition))
                moveCommandThing = new Shrinker(mousePosition - new Vector2(moveCommandShrinkerSize / 2f, moveCommandShrinkerSize / 2f), moveCommandShrinkerSize, 10);
            else
                moveCommandThing = new Shrinker(map.FindNearestWalkableTile(mousePosition) - new Vector2(moveCommandShrinkerSize / 2f, moveCommandShrinkerSize / 2f), moveCommandShrinkerSize, 10);
            moveCommandThing.Texture = moveCommandTexture;

            // create magic box
            Rectangle magicBox = SelectedUnits[0];
            foreach (Unit unit in SelectedUnits)
                magicBox = Rectangle.Union(magicBox, unit.Rectangle);

            // box is too big or clicked inside magic box
            if (magicBox.Width > magicBoxMaxSize || magicBox.Height > magicBoxMaxSize ||
                magicBox.Contains((int)mousePosition.X, (int)mousePosition.Y))
            {
                //bool isPointWalkable = Unit.PathFinder.IsPointWalkable(mousePosition);
                // assign move targets to mouse position
                foreach (Unit unit in SelectedUnits)
                {
                    // if mouse position is not in a walkable tile, find nearest walkable tile
                    Vector2 destinationPoint;
                    //if (isPointWalkable)
                    if (Unit.PathFinder.IsPointWalkable(mousePosition, unit))
                        destinationPoint = mousePosition;
                    else
                        destinationPoint = map.FindNearestWalkableTile(mousePosition);

                    // not holding shift
                    if (keyboardState.IsKeyUp(Keys.LeftShift))
                    {
                        //float distanceToDestination = Vector2.Distance(unit.CurrentPathNode.Tile.CenterPoint, destinationPoint);
                        //if (distanceToDestination <= unit.Diameter)
                        //unit.GiveCommand(new MoveCommand(destinationPoint, 1));
                        //else
                        MoveCommand command = new MoveCommand(destinationPoint, 1);
                        unit.GiveCommand(command);
                    }
                    // holding shift
                    else
                    {
                        //float distanceBetweenCurrentAndNewMoveTarget = Vector2.Distance(unit.FinalMoveDestination, destinationPoint);

                        //if (distanceBetweenCurrentAndNewMoveTarget <= unit.Diameter)
                        //    unit.QueueCommand(new MoveCommand(destinationPoint, 1));
                        //else
                        MoveCommand command = new MoveCommand(destinationPoint, 1);
                        unit.QueueCommand(command);
                        //Unit.PathFinder.AddPathFindRequest(unit, command, unit.CurrentPathNode);
                    }
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
                foreach (Unit unit in SelectedUnits)
                {
                    // if mouse position is not in a walkable tile, find nearest walkable tile
                    Vector2 destinationPoint = unit.CenterPoint + new Vector2(distance * angleX, distance * angleY);
                    if (!Unit.PathFinder.IsPointWalkable(destinationPoint, unit))
                        destinationPoint = map.FindNearestWalkableTile(destinationPoint);

                    // not holding shift
                    if (keyboardState.IsKeyUp(Keys.LeftShift))
                    {
                        //float distanceToDestination = Vector2.Distance(unit.CurrentPathNode.Tile.CenterPoint, destinationPoint);
                        //if (distanceToDestination <= unit.Diameter)
                        //    unit.GiveCommand(new MoveCommand(destinationPoint, 1));
                        //else

                        MoveCommand command = new MoveCommand(destinationPoint, 1);
                        unit.GiveCommand(command);
                        //Unit.PathFinder.AddPathFindRequest(unit, command, unit.CurrentPathNode, 0, false);
                    }
                    // holding shift
                    else
                    {
                        //float distanceBetweenCurrentAndNewMoveTarget = Vector2.Distance(unit.FinalMoveDestination, destinationPoint);

                        //if (distanceBetweenCurrentAndNewMoveTarget <= unit.Diameter)
                        //    unit.QueueCommand(new MoveCommand(destinationPoint, 1));
                        //else
                        unit.QueueCommand(new MoveCommand(destinationPoint, 1));
                    }
                }
            }
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

        void checkForStop()
        {
            if (keyboardState.IsKeyDown(Keys.S))
            {
                foreach (Unit unit in SelectedUnits)
                    unit.Stop();

                usingAttackCommand = false;
                winForm.Cursor = normalCursor;
            }
        }

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
                    if (usingAttackCommand)
                    {
                        usingAttackCommand = false;
                        winForm.Cursor = normalCursor;
                    }

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
            foreach (Unit unit in SelectedUnits)
                rectangle = Rectangle.Union(rectangle, unit.Rectangle);

            camera.Pos = new Vector2(rectangle.Center.X, rectangle.Center.Y);
        }

        void initializeMapTexture()
        {
            // set minimap fields and create rectangle object
            minimapPosX = minimapBorderSize;
            minimapPosY = uiViewport.Height - minimapSize - minimapBorderSize;
            minimapToMapRatioX = (float)minimapSize / (map.Width * map.TileSize);
            minimapToMapRatioY = (float)minimapSize / (map.Height * map.TileSize);
            minimapToScreenRatioX = (float)minimapSize / worldViewport.Width;
            minimapToScreenRatioY = (float)minimapSize / worldViewport.Height;
            minimap = new Rectangle(minimapPosX, minimapPosY, minimapSize, minimapSize);
            minimapScreenIndicatorBox = new BaseObject(new Rectangle(0, 0, (int)(worldViewport.Width * minimapToMapRatioX), (int)(worldViewport.Height * minimapToMapRatioY)));
            //fullMapTexture = new Texture2D(GraphicsDevice, map.Width * Map.TILESIZE, map.Height * Map.TILESIZE);
            //fullMapTexture = new RenderTarget2D(GraphicsDevice, map.Width * Map.TILESIZE, map.Height * Map.TILESIZE);

            // create minimap texture from map tiles
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

        // only draws tiles that are on the screen
        void drawMap(SpriteBatch spriteBatch)
        {
            // finds indices to start and stop drawing at based on the camera transform, viewport size, and tile size
            /*Vector2 minIndices = Vector2.Transform(Vector2.Zero, Matrix.Invert(camera.get_transformation(GraphicsDevice))) / Map.TILESIZE;
            Vector2 maxIndices = Vector2.Transform(new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Matrix.Invert(camera.get_transformation(GraphicsDevice))) / Map.TILESIZE;

            // keeps min indices >= 0
            minIndices.Y = MathHelper.Max(minIndices.Y, 0);
            minIndices.X = MathHelper.Max(minIndices.X, 0);

            // keeps max indices within map size
            maxIndices.Y = (float)Math.Ceiling(MathHelper.Min(maxIndices.Y, map.Height));
            maxIndices.X = (float)Math.Ceiling(MathHelper.Min(maxIndices.X, map.Width));

            for (int y = (int)minIndices.Y; y < (int)maxIndices.Y; y++)
            {
                for (int x = (int)minIndices.X; x < (int)maxIndices.X; x++)
                {
                    MapTile tile = map.Tiles[y, x];

                    if (tile.Type == 0)
                        spriteBatch.Draw(ColorTexture.Gray, tile.Rectangle, Color.White);
                    else if (tile.Type == 1)
                        spriteBatch.Draw(boulder1Texture, tile.Rectangle, Color.White);
                    else if (tile.Type == 2)
                        spriteBatch.Draw(tree1Texture, tile.Rectangle, Color.White);
                }
            }*/

            spriteBatch.Draw(fullMapTexture, new Rectangle(0, 0, map.Width * map.TileSize, map.Height * map.TileSize), Color.White);

            foreach (MapTile tile in map.Tiles)
            {
                if (!tile.Visible)
                {
                    spriteBatch.Draw(transparentBlackTexture, tile.Rectangle, Color.White);
                }
            }
        }

        // draw minimap with units
        void drawMinimap(SpriteBatch spriteBatch)
        {
            //float aspectRatio = (float)map.Width / map.Height;

            // draw minimap border then minimap
            //spriteBatch.Draw(ColorTexture.Black, new Rectangle(minimapPosX - minimapBorderSize, minimapPosY - minimapBorderSize, minimapSize + minimapBorderSize * 2, minimapSize + minimapBorderSize * 2), Color.White);
            spriteBatch.Draw(ColorTexture.Black, new Rectangle(minimapPosX - minimapBorderSize, minimapPosY - minimapBorderSize, uiViewport.Width, minimapSize + minimapBorderSize * 2), Color.White);
            spriteBatch.Draw(fullMapTexture, minimap, Color.White);

            // draw units on minimap
            Rectangle rectangle = new Rectangle(0, 0, 2, 2);
            foreach (Unit unit in Unit.Units)
            {
                rectangle.X = (int)(unit.X * minimapToMapRatioX + minimapPosX);
                rectangle.Y = (int)(unit.Y * minimapToMapRatioY + minimapPosY);
                spriteBatch.Draw(ColorTexture.Green, rectangle, Color.White);
            }

            // update size of screen indicator box
            minimapScreenIndicatorBox.Width = (int)(worldViewport.Width * minimapToMapRatioX / camera.Zoom);
            minimapScreenIndicatorBox.Height = (int)(worldViewport.Height * minimapToMapRatioY / camera.Zoom);

            // calculate position of screen indicator box
            minimapScreenIndicatorBox.CenterPoint = new Vector2(camera.Pos.X * minimapToMapRatioX + minimapPosX, camera.Pos.Y * minimapToMapRatioY + minimapPosY);
            minimapScreenIndicatorBox.Rotation = -camera.Rotation;
            minimapScreenIndicatorBox.CalculateCorners();

            // draw screen indicator box on minimap
            minimapScreenIndicatorBoxLine.ClearVectors();
            minimapScreenIndicatorBoxLine.AddVector(minimapScreenIndicatorBox.UpperLeftCorner);
            minimapScreenIndicatorBoxLine.AddVector(minimapScreenIndicatorBox.UpperRightCorner);
            minimapScreenIndicatorBoxLine.AddVector(minimapScreenIndicatorBox.LowerRightCorner);
            minimapScreenIndicatorBoxLine.AddVector(minimapScreenIndicatorBox.LowerLeftCorner);
            minimapScreenIndicatorBoxLine.AddVector(minimapScreenIndicatorBox.UpperLeftCorner);
            minimapScreenIndicatorBoxLine.Render(spriteBatch);
        }

        // draw selection info area
        Rectangle unitPictureRectangle;
        List<UnitButton> unitButtons = new List<UnitButton>();
        void drawSelectionInfoArea(SpriteBatch spriteBatch)
        {
            //spriteBatch.Draw(ColorTexture.White, selectionInfoArea, Color.White);
            if (SelectedUnits.Count == 0)
            {
                if (unitButtons.Count > 0)
                {
                    SimpleButton.RemoveButtons(unitButtons);
                    unitButtons.Clear();
                }
            }
            else if (SelectedUnits.Count == 1)
            {
                if (unitButtons.Count > 0)
                {
                    SimpleButton.RemoveButtons(unitButtons);
                    unitButtons.Clear();
                }

                RtsObject unit = SelectedUnits[0];

                // unit picture
                unitPictureRectangle = new Rectangle(selectionInfoArea.X + selectionInfoArea.Width / 2 - 25, selectionInfoArea.Y + selectionInfoArea.Height / 2 - 25, 50, 50);
                spriteBatch.Draw(unit.Texture, unitPictureRectangle, Color.White);

                // hp bar
                int hpBarHeight = 8;
                int hpBarSpacing = 6;
                int hpBarPosY = unitPictureRectangle.Y - hpBarHeight - hpBarSpacing;

                Rectangle hpBarRed = new Rectangle(unitPictureRectangle.X, hpBarPosY, unitPictureRectangle.Width, hpBarHeight);

                int hpBarGreenWidth = (int)(unitPictureRectangle.Width * (unit.Hp / (float)unit.MaxHp));
                Rectangle hpBarGreen = new Rectangle(unitPictureRectangle.X, hpBarPosY, hpBarGreenWidth, hpBarHeight);

                spriteBatch.Draw(ColorTexture.Red, hpBarRed, Color.White);
                spriteBatch.Draw(ColorTexture.Green, hpBarGreen, Color.White);

                // unit name
                Vector2 unitNameSize = unitInfoUnitNameFont.MeasureString(unit.UnitName);
                int unitNameSpacing = 2;
                spriteBatch.DrawString(unitInfoUnitNameFont, unit.UnitName, new Vector2((int)(unitPictureRectangle.X + unitPictureRectangle.Width / 2 - unitNameSize.X / 2), (int)(hpBarPosY - unitNameSize.Y - unitNameSpacing)), Color.White);

                // hp
                int hpSpacing = 6;
                int hpPosY = unitPictureRectangle.Y + unitPictureRectangle.Height + hpSpacing;
                string hpString = unit.Hp + "/" + unit.MaxHp;
                Vector2 hpSize = unitInfoHpFont.MeasureString(hpString);
                spriteBatch.DrawString(unitInfoHpFont, hpString, new Vector2((int)(unitPictureRectangle.X + unitPictureRectangle.Width / 2 - hpSize.X / 2), hpPosY), Color.White);

                // kill count
                if (unit is Unit)
                {
                    int killCountPosY = (int)(hpPosY + hpSize.Y);
                    string killCountString = ((Unit)unit).KillCount + " kill" + (((Unit)unit).KillCount != 1 ? "s" : "") + ".";
                    Vector2 killCountSize = unitInfoKillCountFont.MeasureString(killCountString);
                    spriteBatch.DrawString(unitInfoKillCountFont, killCountString, new Vector2((int)(unitPictureRectangle.X + unitPictureRectangle.Width / 2 - killCountSize.X / 2), killCountPosY), Color.White);
                }

                // attack damage, speed, and move speed
                int statsPosX = (int)(selectionInfoArea.X + selectionInfoArea.Width * .66f);
                int statsSpacing = -1;
                //string attackDamageString = unit.AttackDamage + " attack damage.";
                //Vector2 attackDamageSize = unitInfoHpFont.MeasureString(attackDamageString);

                // attack damage
                int attackDamagePosY = unitPictureRectangle.Y;
                Vector2 attackDamageSize = unitInfoHpFont.MeasureString(unit.AttackDamage.ToString());
                spriteBatch.DrawString(unitInfoHpFont, unit.AttackDamage.ToString(), new Vector2(statsPosX, attackDamagePosY), Color.Red);
                spriteBatch.DrawString(unitInfoHpFont, " attack damage.", new Vector2((int)(statsPosX + attackDamageSize.X), attackDamagePosY), Color.White);

                // attack range
                decimal displayedAttackRange = (decimal)unit.AttackRange / map.TileSize;
                int attackRangePosY = (int)(attackDamagePosY + attackDamageSize.Y + statsSpacing);
                Vector2 attackRangeSize = unitInfoHpFont.MeasureString(displayedAttackRange.ToString());
                spriteBatch.DrawString(unitInfoHpFont, displayedAttackRange.ToString(), new Vector2(statsPosX, attackRangePosY), Color.Yellow);
                //Vector2 attackRangeSize = unitInfoHpFont.MeasureString(unit.AttackRange.ToString());
                //spriteBatch.DrawString(unitInfoHpFont, unit.AttackRange.ToString(), new Vector2(statsPosX, attackRangePosY), Color.Yellow);
                spriteBatch.DrawString(unitInfoHpFont, " attack range.", new Vector2((int)(statsPosX + attackRangeSize.X), attackRangePosY), Color.White);

                // armor
                int armorPosY = (int)(attackRangePosY + attackRangeSize.Y + statsSpacing);
                Vector2 armorSize = unitInfoHpFont.MeasureString(unit.Armor.ToString());
                spriteBatch.DrawString(unitInfoHpFont, unit.Armor.ToString(), new Vector2(statsPosX, armorPosY), Color.Blue);
                spriteBatch.DrawString(unitInfoHpFont, " armor.", new Vector2((int)(statsPosX + armorSize.X), armorPosY), Color.White);

                // move speed
                int moveSpeedPosY = (int)(armorPosY + armorSize.Y + statsSpacing);
                Vector2 moveSpeedSize = unitInfoHpFont.MeasureString(unit.Speed.ToString());
                spriteBatch.DrawString(unitInfoHpFont, unit.Speed.ToString(), new Vector2(statsPosX, moveSpeedPosY), Color.Green);
                spriteBatch.DrawString(unitInfoHpFont, " move speed.", new Vector2((int)(statsPosX + moveSpeedSize.X), moveSpeedPosY), Color.White);
            }

            else
            {
                int boxWidth = 35;// (selectionInfoArea.Width - 20 - SelectedUnits.Count) / 12;
                int boxPosX = selectionInfoArea.X + 10;

                if (selectedUnitsChanged)
                {
                    SimpleButton.RemoveButtons(unitButtons);
                    unitButtons.Clear();

                    int boxSize = (selectionInfoArea.Width - 20 - SelectedUnits.Count) / SelectedUnits.Count;
                    
                    int rows = (int)Math.Ceiling(SelectedUnits.Count / 12f);
                    int boxHeight = (rows > 3 ? boxWidth * 3 / rows : boxWidth);
                    int boxPosY = selectionInfoArea.Y + 5;

                    int x = boxPosX;
                    int y = boxPosY;

                    Rectangle box;
                    if (SelectedUnits[0].GetType() == SelectedUnits.ActiveType)
                        box = new Rectangle(x, y, boxWidth, boxHeight);
                    else
                        box = new Rectangle(x + 3, y + 3, boxWidth - 6, boxHeight - 6);
                    unitButtons.Add(new UnitButton(box, SelectedUnits[0]));

                    for (int i = 1; i < SelectedUnits.Count; i++)
                    {
                        x += boxWidth + 1;
                        if (i % 12 == 0)
                        {
                            x = boxPosX;
                            y += boxHeight + 1;
                        }

                        if (SelectedUnits[i].GetType() == SelectedUnits.ActiveType)
                            box = new Rectangle(x, y, boxWidth, boxHeight);
                        else
                            box = new Rectangle(x + 2, y + 2, boxWidth - 4, boxHeight - 4);
                        unitButtons.Add(new UnitButton(box, SelectedUnits[i]));
                    }
                    SimpleButton.AddButtons(unitButtons);
                }
                
                //selectedUnitsChanged = false;

                foreach (UnitButton box in unitButtons)
                {
                    spriteBatch.Draw(box.Unit.Texture, box, Color.White);
                    if (box.Unit.GetType() == SelectedUnits.ActiveType)
                        spriteBatch.Draw(box.Texture, box, Color.White);
                }

                int endOfUnitBoxesX = boxPosX + boxWidth * 12 + 22;
                int areaWidth = (selectionInfoArea.X + selectionInfoArea.Width) - endOfUnitBoxesX;
                string unitCount = SelectedUnits.Count.ToString();
               // string unitCountMsg = "selected.";
                Vector2 unitCountSize = unitInfoHpFont.MeasureString(unitCount);
                //Vector2 unitCountMsgSize = unitInfoKillCountFont.MeasureString(unitCountMsg);

                spriteBatch.DrawString(unitInfoHpFont, unitCount, new Vector2((int)(endOfUnitBoxesX + areaWidth / 2 - unitCountSize.X / 2), (int)(selectionInfoArea.Y + selectionInfoArea.Height / 2 - unitCountSize.Y / 2)), Color.White);
                //spriteBatch.DrawString(unitInfoKillCountFont, unitCountMsg, new Vector2((int)(endOfUnitBoxesX + areaWidth / 2 - unitCountMsgSize.X / 2), (int)(selectionInfoArea.Y + selectionInfoArea.Height / 2)), Color.White);
            }
        }

        // draw command card area
        List<CommandButton> CommandCardButtons = new List<CommandButton>();
        void drawCommandCardArea(SpriteBatch spriteBatch)
        {
            //spriteBatch.Draw(ColorTexture.White, commandCardArea, Color.White);

            // if the current selection is empty, make sure there are no command buttons
            if (SelectedUnits.ActiveType == null)
            {
                if (CommandCardButtons.Count > 0)
                {
                    SimpleButton.RemoveButtons(CommandCardButtons);
                    CommandCardButtons.Clear();
                }
                selectedUnitsChanged = false;
                return;
            }

            // updates active command card of current selection
            SelectedUnits.ActiveCommandCard = CommandCard.DefaultCommandCard;

            if (SelectedUnits.ActiveType.Name == "RangedNublet")
            {
                SelectedUnits.ActiveCommandCard = RangedNublet.CommandCard;
            }
            else if (SelectedUnits.ActiveType.Name == "MeleeNublet")
            {
                SelectedUnits.ActiveCommandCard = MeleeNublet.CommandCard;
            }

            // if current selection has changed, change buttons
            if (selectedUnitsChanged)
            {
                SimpleButton.RemoveButtons(CommandCardButtons);
                CommandCardButtons.Clear();

                int buttonSize = commandCardArea.Width / 4;
                int spacing = 2;
                Rectangle box = new Rectangle(commandCardArea.X, commandCardArea.Y, buttonSize, buttonSize);

                for (int x = 0; x < SelectedUnits.ActiveCommandCard.Buttons.GetLength(0); x++)
                {
                    for (int y = 0; y < SelectedUnits.ActiveCommandCard.Buttons.GetLength(1); y++)
                    {
                        if ((Object)(SelectedUnits.ActiveCommandCard.Buttons[x, y]) != null)
                        {
                            //spriteBatch.Draw(commandCard.Buttons[x, y].Texture, button, Color.White);

                            //CommandCardButtons.Add(new SimpleButton(box, SelectedUnits.ActiveCommandCard.Buttons[x, y].Texture, null, null));

                            SelectedUnits.ActiveCommandCard.Buttons[x, y].Rectangle = box;
                            CommandCardButtons.Add(SelectedUnits.ActiveCommandCard.Buttons[x, y]);
                        }
                        box.X += buttonSize + spacing;
                    }
                    box.X = commandCardArea.X;
                    box.Y += buttonSize + spacing;
                }

                SimpleButton.AddButtons(CommandCardButtons);

                selectedUnitsChanged = false;
            }

            // draw buttons
            foreach (CommandButton button in CommandCardButtons)
            {
                if (button.Pressing || keyboardState.IsKeyDown(button.Type.Hotkey))
                    spriteBatch.Draw(transparentGrayTexture, button, Color.White);
                spriteBatch.Draw(button.Texture, button, Color.White);
                spriteBatch.Draw(whiteBoxTexture, button, Color.White);
                if (button.Hotkey != Keys.Kana)
                {
                    string hotkeyString = button.Hotkey.ToString();
                    Vector2 HotKeyStringSize = unitInfoHpFont.MeasureString(hotkeyString);
                    spriteBatch.DrawString(unitInfoHpFont, hotkeyString, new Vector2(button.X + 5, button.Y + button.Height - HotKeyStringSize.Y), Color.White);
                    //spriteBatch.DrawString(unitInfoHpFont, hotkeyString, new Vector2(button.X + button.Width - HotKeyStringSize.X, button.Y + button.Height - HotKeyStringSize.Y), Color.White);
                }
            }
        }

        const int COMMANDCARD_BORDER_WIDTH = 5;
        void drawCommandCardBorder(SpriteBatch spriteBatch)
        {
            Rectangle border = new Rectangle(commandCardArea.X - 12, commandCardArea.Y, COMMANDCARD_BORDER_WIDTH, commandCardArea.Height);

            spriteBatch.Draw(ColorTexture.White, border, Color.White);
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
        }
    }
}