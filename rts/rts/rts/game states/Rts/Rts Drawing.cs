using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Forms;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace rts
{
    public partial class Rts : GameState
    {
        string fpsMessage = "";
        static SpriteFont pauseFont, fpsFont, unitInfoUnitNameFont, unitInfoHpFont, unitInfoKillCountFont, resourceCountFont, bigFont;

        static Texture2D greenTeamIndicatorTexture, redTeamIndicatorTexture;

        Form winForm;
        static Cursor normalCursor, attackCursor;

        static Texture2D buttonTexture;

        //static Texture2D brownGuyTexture, brownGuySelectingTexture, brownGuySelectedTexture;
        static Texture2D moveCommandShrinkerTexture, attackMoveCommandShrinkerTexture, normalCursorTexture, attackCommandCursorTexture;
        static Texture2D redCircleTexture, transparentTexture, whiteBoxTexture,
            transparentGrayTexture, transparentBlackTexture, rallyFlagTexture;
        static Texture2D cogWheelTexture;

        Texture2D fullMapTexture;

        BaseObject minimapScreenIndicatorBox;
        PrimitiveLine minimapScreenIndicatorBoxLine;

        Rectangle selectionInfoArea;
        int selectionInfoAreaPosX, selectionInfoAreaPosY;

        static Texture2D boulder1Texture, tree1Texture;


        static PrimitiveLine line = new PrimitiveLine(GraphicsDevice, 1);
        public override void Draw(SpriteBatch spriteBatch)
        {
            Game1.Game.DebugMonitor.AddLine("camera position: " + camera.Pos);
            Game1.Game.DebugMonitor.AddLine("camera zoom: " + camera.Zoom);
            Game1.Game.DebugMonitor.AddLine("camera rotation: " + camera.Rotation);
            Game1.Game.DebugMonitor.AddLine("pathfinding usage: " + pathFindingPercentage.ToString("F1") + "%");
            Game1.Game.DebugMonitor.AddLine("pathfinding queue: " + pathFindQueueSize);
            //Game1.Game.DebugMonitor.AddLine("time: " + GameTimer.Elapsed.Minutes + ":" + GameTimer.Elapsed.Seconds.ToString("D2"));
            Game1.Game.DebugMonitor.AddLine("roks: " + Player.Players[1].Roks);

            if (SelectedUnits.Count == 1)
            {
                Unit unit = SelectedUnits[0] as Unit;
                if (unit != null)
                {
                    if (unit.Commands.Count == 0)
                        Game1.Game.DebugMonitor.AddLine("idle");
                    else
                        Game1.Game.DebugMonitor.AddLine(unit.Commands[0].ToString());

                    Game1.Game.DebugMonitor.AddLine("ignoring collision: " + unit.IgnoringCollision);
                }
            }

            GraphicsDevice.Clear(Color.Black);

            GraphicsDevice.Viewport = worldViewport;
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, camera.get_transformation(worldViewport));

            drawMap(spriteBatch);

            line.Colour = Color.Black;

            // resources
            drawResources(spriteBatch);

            // structures
            drawStructures(spriteBatch);

            // placing structure
            drawPlacingStructure(spriteBatch);

            // cog wheels
            drawCogWheels(spriteBatch);

            // draw units
            drawUnits(spriteBatch);

            // draw rings around units
            drawSelectionRings(spriteBatch);

            // draw cargo on workers
            drawCargoOnUnits(spriteBatch);

            // draw rally points
            drawRallyPoints(spriteBatch);

            // placed structures
            drawPlacedStructures(spriteBatch);

            // hp and other bars
            drawHpAndOtherBars(spriteBatch);

            // unit animations
            drawUnitAnimations(spriteBatch);

            drawWayPoints(spriteBatch);

            drawDebugStuff(spriteBatch);

            // bullets
            drawBullets(spriteBatch);

            // move command shrinker things
            drawShrinkers(spriteBatch);

            spriteBatch.End();
            spriteBatch.Begin();

            GraphicsDevice.Viewport = uiViewport;

            //drawBars(spriteBatch);

            SelectBox.Draw(spriteBatch, camera);

            drawSelectionInfoArea(spriteBatch);
            drawCommandCardArea(spriteBatch);
            drawCommandCardBorder(spriteBatch);

            drawResourceCounts(spriteBatch);

            //pause and fps count
            Vector2 pauseStringSize = pauseFont.MeasureString("PAUSED");
            if (paused)
                spriteBatch.DrawString(pauseFont, "PAUSED", new Vector2(uiViewport.Width / 2 - pauseStringSize.X / 2, uiViewport.Height / 2 - pauseStringSize.Y / 2), Color.White);
            else
                frameCounter++;

            // fps message
            spriteBatch.DrawString(fpsFont, fpsMessage, new Vector2(8, 5), Color.Black);

            /*spriteBatch.Draw(buttonTexture, button1, Color.White);
            Vector2 button1TextSize = fpsFont.MeasureString("1");
            spriteBatch.DrawString(fpsFont, "1", new Vector2((int)(button1.X + button1.Width / 2 - button1TextSize.X / 2), (int)(button1.Y + button1.Height / 2 - button1TextSize.Y / 2)), Color.White);
            */
            spriteBatch.Draw(buttonTexture, button2.Rectangle, Color.White);
            Vector2 button2TextSize = fpsFont.MeasureString("10");
            spriteBatch.DrawString(fpsFont, "10", new Vector2((int)(button2.X + button2.Width / 2 - button2TextSize.X / 2), (int)(button2.Y + button2.Height / 2 - button2TextSize.Y / 2)), Color.White);
            /*spriteBatch.Draw(buttonTexture, button3, Color.White);
            Vector2 button3TextSize = fpsFont.MeasureString("FS");
            spriteBatch.DrawString(fpsFont, "FS", new Vector2((int)(button3.X + button3.Width / 2 - button3TextSize.X / 2), (int)(button3.Y + button3.Height / 2 - button3TextSize.Y / 2)), Color.White);
            spriteBatch.Draw(buttonTexture, button4, Color.White);
            Vector2 button4TextSize = fpsFont.MeasureString("M");
            spriteBatch.DrawString(fpsFont, "M", new Vector2((int)(button4.X + button4.Width / 2 - button4TextSize.X / 2), (int)(button4.Y + button4.Height / 2 - button4TextSize.Y / 2)), Color.White);
            spriteBatch.Draw(buttonTexture, button5, Color.White);
            Vector2 button5TextSize = fpsFont.MeasureString("X");
            spriteBatch.DrawString(fpsFont, "X", new Vector2((int)(button5.X + button5.Width / 2 - button5TextSize.X / 2), (int)(button5.Y + button5.Height / 2 - button5TextSize.Y / 2)), Color.White);
            */
            // cursor
            /*if (usingAttackCommand)
                spriteBatch.Draw(attackCommandCursorTexture, new Rectangle(mouseState.X - attackCommandCursorSize / 2, mouseState.Y - attackCommandCursorSize / 2, attackCommandCursorSize, attackCommandCursorSize), Color.White);
            else
                spriteBatch.Draw(normalCursorTexture, new Rectangle(mouseState.X, mouseState.Y, attackCommandCursorSize, attackCommandCursorSize), Color.White);*/

            drawSpecialMessages(spriteBatch);

            drawGameClock(spriteBatch);

            spriteBatch.End();

            GraphicsDevice.Viewport = minimapViewport;
            //Matrix transform = camera.get_transformation(minimapViewport);

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, camera.get_minimap_transformation(minimapViewport));

            drawMinimap(spriteBatch);

            spriteBatch.End();
            GraphicsDevice.Viewport = uiViewport;
        }

        void drawUnits(SpriteBatch spriteBatch)
        {
            foreach (Unit unit in Unit.Units)
            {
                //if (unit.CurrentPathNode.Tile.Visible)
                //if (unit.Visible)
                {
                    //if (!SelectingUnits.Contains(unit) && !SelectedUnits.Contains(unit))
                    {
                        spriteBatch.Draw(unit.Texture, new Rectangle((int)unit.CenterPoint.X, (int)unit.CenterPoint.Y, unit.Width, unit.Height), null, Color.White, unit.Rotation, unit.TextureCenterOrigin, SpriteEffects.None, 0f);
                    }

                    /*int teamIndicatorSize = (int)(unit.Diameter / 4);
                    // Rectangle teamIndicator = new Rectangle((int)(unit.CenterPoint.X - teamIndicatorSize / 2), (int)(unit.CenterPoint.Y - teamIndicatorSize / 2), teamIndicatorSize, teamIndicatorSize);
                    Rectangle teamIndicator = new Rectangle((int)unit.CenterPoint.X, (int)unit.CenterPoint.Y, teamIndicatorSize, teamIndicatorSize);

                    if (unit.Team == myTeam)
                        spriteBatch.Draw(ColorTexture.Green, teamIndicator, null, Color.White, unit.Rotation, ColorTexture.CenterVector, SpriteEffects.None, 0f);
                    else
                        spriteBatch.Draw(ColorTexture.Red, teamIndicator, null, Color.White, unit.Rotation, ColorTexture.CenterVector, SpriteEffects.None, 0f);*/

                    if (unit.Team == Player.Me.Team)
                        spriteBatch.Draw(greenTeamIndicatorTexture, new Rectangle((int)unit.CenterPoint.X, (int)unit.CenterPoint.Y, unit.Width, unit.Height), null, Color.White, unit.Rotation, new Vector2(greenTeamIndicatorTexture.Width / 2, greenTeamIndicatorTexture.Height / 2), SpriteEffects.None, 0f);
                    else
                        spriteBatch.Draw(redTeamIndicatorTexture, new Rectangle((int)unit.CenterPoint.X, (int)unit.CenterPoint.Y, unit.Width, unit.Height), null, Color.White, unit.Rotation, new Vector2(redTeamIndicatorTexture.Width / 2, redTeamIndicatorTexture.Height / 2), SpriteEffects.None, 0f);

                    /*line.Colour = Color.Yellow;
                    line.ClearVectors();
                    float incrementRadians = MathHelper.TwoPi / 32f;
                    for (float r = 0f; r <= MathHelper.TwoPi; r += incrementRadians)
                    {
                        line.AddVector(new Vector2(unit.CenterPointX + (unit.AttackRange + unit.Radius) * (float)Math.Cos(r), unit.CenterPointY + (unit.AttackRange + unit.Radius) * (float)Math.Sin(r)));
                    }
                    line.AddVector(new Vector2(unit.CenterPointX + unit.AttackRange + unit.Radius, unit.CenterPointY));
                    line.Render(spriteBatch);*/

                    /*line.Colour = Color.Black;
                    lock (unit.PotentialCollisions)
                    {
                        foreach (Unit u in unit.PotentialCollisions)
                        {
                            line.ClearVectors();
                            line.AddVector(unit.CenterPoint);
                            line.AddVector(u.CenterPoint);
                            line.Render(spriteBatch);
                        }
                    }*/
                }
                //else
                //{
                //spriteBatch.Draw(ColorTexture.Red, new Rectangle((int)(unit.CenterPoint.X - 1), (int)(unit.CenterPoint.Y - 1), 2, 2), Color.White);
                //}
                // red dot showing X and Y location of unit
                //spriteBatch.Draw(ColorTexture.Red, new Rectangle((int)(unit.X - 1), (int)(unit.Y - 1), 2, 2), Color.White);
            }
        }

        void drawStructures(SpriteBatch spriteBatch)
        {
            foreach (Structure structure in Structure.Structures)
            {
                if (structure.Visible || !structure.Visible)
                {
                    // draw team color indicator
                    Rectangle teamIndicator = new Rectangle();
                    teamIndicator.Width = (int)(structure.Rectangle.Width * .4f);
                    teamIndicator.Height = (int)(structure.Rectangle.Height * .4f);
                    teamIndicator.Location = new Point((int)(structure.CenterPointX - teamIndicator.Width / 2), (int)(structure.CenterPointY - teamIndicator.Height / 2));
                    if (structure.Team == Player.Me.Team)
                        spriteBatch.Draw(ColorTexture.Green, teamIndicator, Color.White);
                    else
                        spriteBatch.Draw(ColorTexture.Red, teamIndicator, Color.White);

                    // draw structure
                    //spriteBatch.Draw(structure.Texture, structure.Rectangle, Color.White);
                    spriteBatch.Draw(structure.Texture, new Rectangle((int)structure.CenterPoint.X, (int)structure.CenterPoint.Y, structure.Width, structure.Height), null, Color.White, -camera.Rotation, structure.TextureCenterOrigin, SpriteEffects.None, 0f);
                }
                else if (structure.Revealed)
                {
                    //spriteBatch.Draw(ColorTexture.Red, new Rectangle((int)(structure.CenterPoint.X - 1), (int)(structure.CenterPoint.Y - 1), 2, 2), Color.White);

                    Rectangle teamIndicator = new Rectangle();
                    teamIndicator.Width = (int)(structure.Rectangle.Width * .4f);
                    teamIndicator.Height = (int)(structure.Rectangle.Height * .4f);
                    teamIndicator.Location = new Point((int)(structure.CenterPointX - teamIndicator.Width / 2), (int)(structure.CenterPointY - teamIndicator.Height / 2));
                    //if (structure.Team == myTeam)
                    //spriteBatch.Draw(ColorTexture.Green, teamIndicator, Color.White * .5f);
                    // else
                    //    spriteBatch.Draw(ColorTexture.Red, teamIndicator, Color.White * .5f);

                    // draw structure
                    //spriteBatch.Draw(structure.Texture, structure.Rectangle, Color.White * .9f);
                    spriteBatch.Draw(structure.Texture, new Rectangle((int)structure.CenterPoint.X, (int)structure.CenterPoint.Y, structure.Width, structure.Height), null, Color.White * .9f, -camera.Rotation, structure.TextureCenterOrigin, SpriteEffects.None, 0f);
                }
            }
        }

        void drawResources(SpriteBatch spriteBatch)
        {
            foreach (Resource resource in Resource.Resources)
            {
                //float alpha = 1f;
                //if (resource.Depleted)
                //    alpha = .5f;

                //spriteBatch.Draw(resource.Texture, resource.Rectangle, Color.White);
                spriteBatch.Draw(resource.Texture, new Rectangle((int)resource.CenterPoint.X, (int)resource.CenterPoint.Y, resource.Width, resource.Height), null, Color.White, -camera.Rotation, resource.TextureCenterOrigin, SpriteEffects.None, 0f);

                if (resource.Visible)
                {
                    string s = resource.Amount.ToString();
                    Vector2 stringSize = unitInfoUnitNameFont.MeasureString(s);
                    //spriteBatch.DrawString(unitInfoUnitNameFont, s, new Vector2(resource.Rectangle.X + resource.Rectangle.Width / 2 - stringSize.X / 2, resource.Rectangle.Y + resource.Rectangle.Height / 2 - stringSize.Y / 2), Color.Black, -camera.Rotation, new Vector2(stringSize.X / 4, stringSize.Y / 4), 1f, SpriteEffects.None, 0f);

                    spriteBatch.DrawString(unitInfoUnitNameFont, s, resource.CenterPoint, Color.Black, -camera.Rotation, new Vector2(stringSize.X / 2, stringSize.Y / 2), 1f, SpriteEffects.None, 0f);
                }
                /*Roks roks = resource as Roks;
                if (roks != null)
                {
                    foreach (PathNode pathNode in roks.exitPathNodes)
                    {
                        line.ClearVectors();
                        line.AddVector(new Vector2(pathNode.Tile.Rectangle.X, pathNode.Tile.Rectangle.Y));
                        line.AddVector(new Vector2(pathNode.Tile.Rectangle.X + pathNode.Tile.Width, pathNode.Tile.Rectangle.Y));
                        line.AddVector(new Vector2(pathNode.Tile.Rectangle.X + pathNode.Tile.Width, pathNode.Tile.Rectangle.Y + pathNode.Tile.Rectangle.Height));
                        line.AddVector(new Vector2(pathNode.Tile.Rectangle.X, pathNode.Tile.Rectangle.Y + pathNode.Tile.Rectangle.Height));
                        line.AddVector(new Vector2(pathNode.Tile.Rectangle.X, pathNode.Tile.Rectangle.Y));
                        line.Render(spriteBatch);
                    }
                }*/
            }
        }

        void drawWayPoints(SpriteBatch spriteBatch)
        {
            //foreach (RtsObject o in SelectedUnits)
            foreach (Unit unit in Unit.Units)
            {
                //Unit unit = o as Unit;

                //if (unit != null && unit.IsMoving && unit.Commands.Count > 1 && unit.Commands[1] is MoveCommand)
                if (unit != null && unit.IsMoving)
                {
                    line.ClearVectors();
                    //line.Alpha = .6f;
                    line.AddVector(unit.CenterPoint);
                    //foreach (Vector2 v in unit.WayPoints)
                    //    line.AddVector(v);
                    foreach (UnitCommand command in unit.Commands)
                    {
                        MoveCommand moveCommand = command as MoveCommand;

                        if (moveCommand == null)
                            continue;

                        if (moveCommand is AttackCommand)
                            line.Colour = Color.Red * .85f;
                        else
                            line.Colour = Color.Green * .85f;

                        foreach (Vector2 v in moveCommand.WayPoints)
                            line.AddVector(v);

                        line.RenderWithZoom(spriteBatch, camera.Zoom);
                        line.ClearVectors();
                        line.AddVector(moveCommand.Destination);
                    }

                    line.Alpha = .75f;
                }
            }
        }

        void drawPlacingStructure(SpriteBatch spriteBatch)
        {
            if (placingStructure)
            {
                line.Colour = Color.Beige;
                line.Alpha = .5f;
                foreach (Structure structure in Structure.Structures)
                {
                    if (structure.Team != Player.Me.Team && !structure.Visible)
                        continue;

                    foreach (PathNode pathNode in structure.OccupiedPathNodes)
                    {
                        line.ClearVectors();
                        line.AddVector(new Vector2(pathNode.Tile.Rectangle.X, pathNode.Tile.Rectangle.Y));
                        line.AddVector(new Vector2(pathNode.Tile.Rectangle.X + pathNode.Tile.Width, pathNode.Tile.Rectangle.Y));
                        line.AddVector(new Vector2(pathNode.Tile.Rectangle.X + pathNode.Tile.Width, pathNode.Tile.Rectangle.Y + pathNode.Tile.Rectangle.Height));
                        line.AddVector(new Vector2(pathNode.Tile.Rectangle.X, pathNode.Tile.Rectangle.Y + pathNode.Tile.Rectangle.Height));
                        line.AddVector(new Vector2(pathNode.Tile.Rectangle.X, pathNode.Tile.Rectangle.Y));
                        line.RenderWithZoom(spriteBatch, camera.Zoom);
                    }
                }
                line.Alpha = .75f;


                //spriteBatch.Draw(placingStructureType.PlacingTexture, new Rectangle(placingStructureLocation.X * map.TileSize, placingStructureLocation.Y * map.TileSize, placingStructureType.Size * map.TileSize, placingStructureType.Size * map.TileSize), Color.LightGreen);
                spriteBatch.Draw(placingStructureType.PlacingTexture, new Rectangle((int)placingStructureCenterPoint.X, (int)placingStructureCenterPoint.Y, placingStructureType.Size * map.TileSize, placingStructureType.Size * map.TileSize), null, Color.White, -camera.Rotation, new Vector2(placingStructureType.NormalTexture.Width / 2f, placingStructureType.NormalTexture.Width / 2f), SpriteEffects.None, 0f);
                if (tooCloseToResource)
                {
                    line.Colour = Color.Red;
                    tooCloseToResource = false;
                }
                else
                    line.Colour = Color.Beige;
                foreach (PathNode pathNode in placingStructurePathNodes)
                {
                    line.ClearVectors();
                    line.AddVector(new Vector2(pathNode.Tile.Rectangle.X, pathNode.Tile.Rectangle.Y));
                    line.AddVector(new Vector2(pathNode.Tile.Rectangle.X + pathNode.Tile.Width, pathNode.Tile.Rectangle.Y));
                    line.AddVector(new Vector2(pathNode.Tile.Rectangle.X + pathNode.Tile.Width, pathNode.Tile.Rectangle.Y + pathNode.Tile.Rectangle.Height));
                    line.AddVector(new Vector2(pathNode.Tile.Rectangle.X, pathNode.Tile.Rectangle.Y + pathNode.Tile.Rectangle.Height));
                    line.AddVector(new Vector2(pathNode.Tile.Rectangle.X, pathNode.Tile.Rectangle.Y));
                    line.RenderWithZoom(spriteBatch, camera.Zoom);
                }
                line.Colour = Color.Red;
                foreach (PathNode pathNode in placingStructureBlockedPathNodes)
                {
                    line.ClearVectors();
                    line.AddVector(new Vector2(pathNode.Tile.Rectangle.X, pathNode.Tile.Rectangle.Y));
                    line.AddVector(new Vector2(pathNode.Tile.Rectangle.X + pathNode.Tile.Width, pathNode.Tile.Rectangle.Y));
                    line.AddVector(new Vector2(pathNode.Tile.Rectangle.X + pathNode.Tile.Width, pathNode.Tile.Rectangle.Y + pathNode.Tile.Rectangle.Height));
                    line.AddVector(new Vector2(pathNode.Tile.Rectangle.X, pathNode.Tile.Rectangle.Y + pathNode.Tile.Rectangle.Height));
                    line.AddVector(new Vector2(pathNode.Tile.Rectangle.X, pathNode.Tile.Rectangle.Y));
                    line.RenderWithZoom(spriteBatch, camera.Zoom);
                }
            }
        }

        void drawCogWheels(SpriteBatch spriteBatch)
        {
            foreach (StructureCogWheel cog in CogWheels)
            {
                if (cog.Structure.Visible)
                    spriteBatch.Draw(cogWheelTexture, cog.Rectangle, null, Color.White, cog.Rotation, new Vector2(cogWheelTexture.Width / 2f, cogWheelTexture.Height / 2f), SpriteEffects.None, 0f);
            }
        }

        void drawSelectionRings(SpriteBatch spriteBatch)
        {
            foreach (RtsObject o in SelectingUnits)
                //drawSelectionRing(unit, spriteBatch, Color.Green);
                drawSelectingRing(o, spriteBatch);
            foreach (RtsObject o in SelectedUnits)
                //drawSelectionRing(unit, spriteBatch, Color.Khaki);
                drawSelectedRing(o, spriteBatch);
        }

        void drawPlacedStructures(SpriteBatch spriteBatch)
        {
            foreach (PlacedStructure link in placedStructures)
            {
                //spriteBatch.Draw(link.Object1.PlacingTexture, link.Object2, Color.LightGreen * .5f);
                if (link.Team == Player.Me.Team)
                    spriteBatch.Draw(link.Object1.PlacingTexture, new Rectangle(link.Object2.X + link.Object2.Width / 2, link.Object2.Y + link.Object2.Height / 2, link.Object2.Width, link.Object2.Height), null, Color.LightGreen * .5f, -camera.Rotation, new Vector2(link.Object1.PlacingTexture.Width / 2, link.Object1.PlacingTexture.Height / 2), SpriteEffects.None, 0f);
            }
        }

        void drawUnitAnimations(SpriteBatch spriteBatch)
        {
            foreach (UnitAnimation a in UnitAnimation.UnitAnimations)
            {
                // only draw if visible
                if (a.Unit.Visible)
                    spriteBatch.Draw(a, new Rectangle(a.Rectangle.Center.X, a.Rectangle.Center.Y, a.Rectangle.Width, a.Rectangle.Height), null, Color.White, a.Rotation, new Vector2(((Texture2D)a).Width / 2, ((Texture2D)a).Height / 2), SpriteEffects.None, 0f);
            }
        }

        void drawDebugStuff(SpriteBatch spriteBatch)
        {
            if (SelectedUnits.Count == 1)
            {
                Unit unit = SelectedUnits[0] as Unit;
                if (unit != null)
                {
                    WorkerNublet worker = unit as WorkerNublet;

                    if (worker == null || !worker.Building)
                    {
                        //PrimitiveLine line = new PrimitiveLine(GraphicsDevice, 1);
                        line.Colour = Color.Black;
                        //lock (SelectedUnits[0].PotentialCollisions)
                        //{
                        foreach (Unit u in unit.PotentialCollisions)
                        {
                            line.ClearVectors();
                            line.AddVector(SelectedUnits[0].CenterPoint);
                            line.AddVector(u.CenterPoint);
                            line.Render(spriteBatch);
                        }

                        foreach (BoundingBox box in unit.OccupiedBoundingBoxes)
                        {
                            line.Colour = Color.Red * .3f;

                            line.ClearVectors();
                            line.CreateBox(box.Rectangle);
                            line.Render(spriteBatch);

                            line.Colour = Color.White * .3f;
                            foreach (MapTile tile in box.Tiles)
                            {
                                line.ClearVectors();
                                line.CreateBox(tile.Rectangle);
                                line.Render(spriteBatch);
                            }
                        }
                        //}

                        /*line.Colour = Color.Red;
                        line.ClearVectors();
                        line.AddVector(new Vector2(unit.CurrentPathNode.Tile.Rectangle.X, unit.CurrentPathNode.Tile.Rectangle.Y));
                        line.AddVector(new Vector2(unit.CurrentPathNode.Tile.Rectangle.X + unit.CurrentPathNode.Tile.Width, unit.CurrentPathNode.Tile.Rectangle.Y));
                        line.AddVector(new Vector2(unit.CurrentPathNode.Tile.Rectangle.X + unit.CurrentPathNode.Tile.Width, unit.CurrentPathNode.Tile.Rectangle.Y + unit.CurrentPathNode.Tile.Rectangle.Height));
                        line.AddVector(new Vector2(unit.CurrentPathNode.Tile.Rectangle.X, unit.CurrentPathNode.Tile.Rectangle.Y + unit.CurrentPathNode.Tile.Rectangle.Height));
                        line.AddVector(new Vector2(unit.CurrentPathNode.Tile.Rectangle.X, unit.CurrentPathNode.Tile.Rectangle.Y));
                        line.Render(spriteBatch);*/

                        /*line.Colour = Color.Beige;
                        foreach (PathNode pathNode in unit.PathNodeBufferSquare)
                        {
                            line.ClearVectors();
                            line.AddVector(new Vector2(pathNode.Tile.Rectangle.X, pathNode.Tile.Rectangle.Y));
                            line.AddVector(new Vector2(pathNode.Tile.Rectangle.X + pathNode.Tile.Width, pathNode.Tile.Rectangle.Y));
                            line.AddVector(new Vector2(pathNode.Tile.Rectangle.X + pathNode.Tile.Width, pathNode.Tile.Rectangle.Y + pathNode.Tile.Rectangle.Height));
                            line.AddVector(new Vector2(pathNode.Tile.Rectangle.X, pathNode.Tile.Rectangle.Y + pathNode.Tile.Rectangle.Height));
                            line.AddVector(new Vector2(pathNode.Tile.Rectangle.X, pathNode.Tile.Rectangle.Y));
                            line.Render(spriteBatch);
                        }

                        line.Colour = Color.Red;
                        foreach (PathNode pathNode in unit.OccupiedPathNodes)
                        {
                            line.ClearVectors();
                            line.AddVector(new Vector2(pathNode.Tile.Rectangle.X, pathNode.Tile.Rectangle.Y));
                            line.AddVector(new Vector2(pathNode.Tile.Rectangle.X + pathNode.Tile.Width, pathNode.Tile.Rectangle.Y));
                            line.AddVector(new Vector2(pathNode.Tile.Rectangle.X + pathNode.Tile.Width, pathNode.Tile.Rectangle.Y + pathNode.Tile.Rectangle.Height));
                            line.AddVector(new Vector2(pathNode.Tile.Rectangle.X, pathNode.Tile.Rectangle.Y + pathNode.Tile.Rectangle.Height));
                            line.AddVector(new Vector2(pathNode.Tile.Rectangle.X, pathNode.Tile.Rectangle.Y));
                            line.Render(spriteBatch);
                        }*/

                        /*line.Colour = Color.Black;
                        line.ClearVectors();
                        line.AddVector(new Vector2(unit.X, unit.Y));
                        line.AddVector(new Vector2(unit.X + unit.Width, unit.Y));
                        line.AddVector(new Vector2(unit.X + unit.Width, unit.Y + unit.Height));
                        line.AddVector(new Vector2(unit.X, unit.Y + unit.Height));
                        line.AddVector(new Vector2(unit.X, unit.Y));
                        line.Render(spriteBatch);*/

                        line.Colour = Color.Red;
                        line.ClearVectors();
                        line.CreateCircle(unit.CenterPoint, unit.AttackRange + unit.Radius);
                        line.RenderWithAlpha(spriteBatch, .75f);

                        line.Colour = Color.Yellow;
                        line.ClearVectors();
                        line.CreateCircle(unit.CenterPoint, unit.SightRange * map.TileSize);
                        line.RenderWithAlpha(spriteBatch, .5f);

                        /*line.Colour = Color.Black;
                        foreach (MapTile tile in unit.CurrentPathNode.Tile.Neighbors)
                        {
                            line.ClearVectors();
                            line.AddVector(new Vector2(tile.Rectangle.X, tile.Rectangle.Y));
                            line.AddVector(new Vector2(tile.Rectangle.X + tile.Width, tile.Rectangle.Y));
                            line.AddVector(new Vector2(tile.Rectangle.X + tile.Width, tile.Rectangle.Y + tile.Height));
                            line.AddVector(new Vector2(tile.Rectangle.X, tile.Rectangle.Y + tile.Height));
                            line.AddVector(new Vector2(tile.Rectangle.X, tile.Rectangle.Y));
                            line.Render(spriteBatch);
                        }*/
                    }
                }

                Structure structure = SelectedUnits[0] as Structure;
                if (structure != null)
                {
                    line.Colour = Color.Yellow;
                    line.ClearVectors();
                    line.CreateCircle(structure.CenterPoint, structure.SightRange * map.TileSize);
                    //line.Render(spriteBatch);
                    line.RenderWithAlpha(spriteBatch, .5f);
                }
            }

            /*if (SelectedUnits.Count == 1 && SelectedUnits[0] is Structure)
            {
                Structure structure = SelectedUnits[0] as Structure;

                line.Colour = Color.Red;
                foreach (PathNode pathNode in structure.OccupiedPathNodes)
                {
                    line.ClearVectors();
                    line.AddVector(new Vector2(pathNode.Tile.Rectangle.X, pathNode.Tile.Rectangle.Y));
                    line.AddVector(new Vector2(pathNode.Tile.Rectangle.X + pathNode.Tile.Width, pathNode.Tile.Rectangle.Y));
                    line.AddVector(new Vector2(pathNode.Tile.Rectangle.X + pathNode.Tile.Width, pathNode.Tile.Rectangle.Y + pathNode.Tile.Rectangle.Height));
                    line.AddVector(new Vector2(pathNode.Tile.Rectangle.X, pathNode.Tile.Rectangle.Y + pathNode.Tile.Rectangle.Height));
                    line.AddVector(new Vector2(pathNode.Tile.Rectangle.X, pathNode.Tile.Rectangle.Y));
                    line.Render(spriteBatch);
                }
            }*/

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

            // all bounding boxes
            /*line.Colour = Color.Black;
            foreach (BoundingBox box in map.BigBoundingBoxes)
            {
                line.ClearVectors();
                line.CreateBox(box.Rectangle);
                line.Render(spriteBatch);
            }*/
        }

        void drawBullets(SpriteBatch spriteBatch)
        {
            foreach (RtsBullet b in RtsBullet.RtsBullets)
            {
                // find tile the bullet is in
                int y = (int)MathHelper.Clamp(b.CenterPoint.Y / map.TileSize, 0, map.Height - 1);
                int x = (int)MathHelper.Clamp(b.CenterPoint.X / map.TileSize, 0, map.Width - 1);

                // only draw if tile is visible
                if (map.Tiles[y, x].Visible)
                {
                    spriteBatch.Draw(b.Texture, new Rectangle((int)b.CenterPoint.X, (int)b.CenterPoint.Y, b.Width, b.Height), null, Color.White, b.Rotation, b.TextureCenterOrigin, SpriteEffects.None, 0f);

                    //spriteBatch.Draw(b.Texture, new Rectangle((int)b.CenterPoint.X, (int)b.CenterPoint.Y, b.Width, b.Height), null, Color.Black * .4f, b.Rotation, b.TextureCenterOrigin, SpriteEffects.None, 0f);
                }
            }
        }

        void drawShrinkers(SpriteBatch spriteBatch)
        {
            foreach (Shrinker shrinker in Shrinker.Shrinkers)
                spriteBatch.Draw(shrinker.Texture, shrinker.Rectangle, Color.White);
            //spriteBatch.Draw(shrinker.Texture, new Rectangle((int)shrinker.CenterPoint.X, (int)shrinker.CenterPoint.Y, shrinker.Width, shrinker.Height), null, Color.White, shrinker.Rotation, shrinker.TextureCenterOrigin, SpriteEffects.None, 0f);
        }

        void drawResourceCounts(SpriteBatch spriteBatch)
        {
            int iconSize = 25;
            int spacingX = 10;
            int spacingY = 10;

            string maxSupplyString = "/" + Player.Me.MaxSupply;
            Vector2 maxSupplyStringSize = resourceCountFont.MeasureString(maxSupplyString);
            int maxSupplyStringX = (int)(uiViewport.Width - maxSupplyStringSize.X - spacingX);
            spriteBatch.DrawString(resourceCountFont, maxSupplyString, new Vector2(maxSupplyStringX, spacingY), Color.White);

            string currentSupplyString = Player.Me.CurrentSupply.ToString();
            Vector2 currentSupplyStringSize = resourceCountFont.MeasureString(currentSupplyString);
            int currentSupplyStringX = (int)(maxSupplyStringX - currentSupplyStringSize.X);
            Color currentSupplyColor = (Player.Me.CurrentSupply > Player.Me.MaxSupply) ? (Color.Red) : (Color.White);
            spriteBatch.DrawString(resourceCountFont, currentSupplyString, new Vector2(currentSupplyStringX, spacingY), currentSupplyColor);

            Rectangle supplyIconRectangle = new Rectangle(currentSupplyStringX - iconSize - 5, spacingY, iconSize, iconSize);
            spriteBatch.Draw(StructureType.Farm.NormalTexture, supplyIconRectangle, Color.White);

            string roksString = Player.Me.Roks.ToString();
            Vector2 roksStringSize = resourceCountFont.MeasureString(roksString);
            int roksStringX = (int)(supplyIconRectangle.X - roksStringSize.X - spacingX);
            spriteBatch.DrawString(resourceCountFont, roksString, new Vector2(roksStringX, spacingY), Color.White);

            Rectangle roksIconRectangle = new Rectangle(roksStringX - iconSize - 5, spacingY, iconSize, iconSize);
            spriteBatch.Draw(ResourceType.Roks.CargoTexture, roksIconRectangle, Color.White);

            string roksPerSecondString = "+ " + roksPerSecond;
            Vector2 roksPerSecondStringSize = resourceCountFont.MeasureString(roksPerSecondString);
            spriteBatch.DrawString(resourceCountFont, roksPerSecondString, new Vector2((int)(roksStringX + roksStringSize.X / 2 - roksPerSecondStringSize.Y / 2), (int)(spacingY + roksPerSecondStringSize.Y)), Color.White);
        }

        void drawRallyPoints(SpriteBatch spriteBatch)
        {
            line.Colour = Color.Green;
            line.Size = (int)MathHelper.Max(1, 2 / camera.Zoom);

            foreach (Structure structure in Structure.Structures)
            {
                if (structure.Visible)
                {
                    if (SelectedUnits.Contains(structure) && structure.RallyPoints.Count > 0)
                    {
                        line.ClearVectors();
                        line.AddVector(structure.CenterPoint);
                        foreach (RallyPoint rallyPoint in structure.RallyPoints)
                            line.AddVector(rallyPoint.Point);
                        line.Render(spriteBatch);

                        Rectangle flag = new Rectangle(0, 0, 26, 26);
                        //flag.X = (int)(structure.RallyPoints[structure.RallyPoints.Count - 1].Point.X) - 10;
                        //flag.Y = (int)(structure.RallyPoints[structure.RallyPoints.Count - 1].Point.Y) - 18;
                        flag.X = (int)structure.RallyPoints[structure.RallyPoints.Count - 1].Point.X - (int)(5 * (float)Math.Cos(-camera.Rotation + MathHelper.PiOver2));
                        flag.Y = (int)structure.RallyPoints[structure.RallyPoints.Count - 1].Point.Y - (int)(5 * (float)Math.Sin(-camera.Rotation + MathHelper.PiOver2));

                        //spriteBatch.Draw(rallyFlagTexture, flag, Color.White);
                        spriteBatch.Draw(rallyFlagTexture, flag, null, Color.White, -camera.Rotation, new Vector2(rallyFlagTexture.Width * .35f, rallyFlagTexture.Height / 2f), SpriteEffects.None, 0f);

                    }
                }
            }

            line.Size = 1;
        }

        void drawCargoOnUnits(SpriteBatch spriteBatch)
        {
            foreach (Unit unit in Unit.Units)
            {
                //if (unit.CurrentPathNode.Tile.Visible)
                if (unit.Visible)
                {
                    WorkerNublet worker = unit as WorkerNublet;
                    if (worker != null)
                    {
                        if (worker.CargoAmount > 0 && worker.CargoType != null)
                        {
                            spriteBatch.Draw(worker.CargoType.CargoTexture, new Rectangle((int)unit.CenterPoint.X, (int)unit.CenterPoint.Y, (int)(unit.Width * .75f), (int)(unit.Height * .75f)), null, Color.White, unit.Rotation, unit.TextureCenterOrigin, SpriteEffects.None, 0f);
                        }
                    }
                }
            }
        }

        void drawHpAndOtherBars(SpriteBatch spriteBatch)
        {
            int hpBarSpacingX = 1;
            int hpBarSpacingY = 1;
            int hpBarWidth = (int)(25 * camera.Zoom);
            int hpBarHeight = (int)(5 / camera.Zoom);
            //Rectangle hpBarRed = new Rectangle(0, 0, 0, hpBarHeight);
            //Rectangle hpBarGreen = new Rectangle(0, 0, 0, hpBarHeight);

            //foreach (Unit unit in Unit.Units)
            foreach (RtsObject unit in RtsObject.RtsObjects)
            {
                //if (!unit.CurrentPathNode.Tile.Visible)
                if (!unit.Visible)
                    continue;

                //hpBarSpacingX = (int)(unit.Width * .05f);
                hpBarSpacingX = 2;
                hpBarWidth = unit.Width - hpBarSpacingX * 2;
                //int hpBarPosY = unit.Rectangle.Y - hpBarHeight - hpBarSpacingY;
                //int hpBarPosX = unit.Rectangle.X + hpBarSpacingX;

                //hpBarRed.X = hpBarPosX;
                //hpBarRed.Y = hpBarPosY;
                //hpBarRed.Width = hpBarWidth - hpBarSpacingX * 2;
                //int hpBarRedWidth = hpBarWidth - hpBarSpacingX * 2;
                int hpBarRedWidth = hpBarWidth;

                //hpBarGreen.X = hpBarPosX;
                //hpBarGreen.Y = hpBarPosY;
                //int hpBarGreenWidth = (int)(hpBarWidth * unit.PercentHp);
                int hpBarGreenWidth = (int)(hpBarWidth * unit.PercentHp);
                //hpBarGreen.Width = hpBarGreenWidth - hpBarSpacingX * 2;

                Vector2 position = new Vector2(unit.CenterPoint.X - (unit.Radius + hpBarHeight / 2 + hpBarSpacingX) * (float)Math.Cos(-camera.Rotation + MathHelper.PiOver2), unit.CenterPoint.Y - (unit.Radius + hpBarHeight / 2 + hpBarSpacingX) * (float)Math.Sin(-camera.Rotation + MathHelper.PiOver2));
                position = new Vector2(position.X - (hpBarRedWidth / 2f) * (float)Math.Sin(-camera.Rotation + MathHelper.PiOver2), position.Y + (hpBarRedWidth / 2f) * (float)Math.Cos(-camera.Rotation + MathHelper.PiOver2));

                spriteBatch.Draw(ColorTexture.Red, new Rectangle((int)position.X, (int)position.Y, hpBarRedWidth, hpBarHeight), null, Color.White, -camera.Rotation, new Vector2(0, ColorTexture.Red.Height / 2f), SpriteEffects.None, 0f);

                //spriteBatch.Draw(ColorTexture.Red, hpBarRed, Color.White);

                int widthDifference = hpBarRedWidth - hpBarGreenWidth;

                //Vector2 greenPosition = new Vector2(position.X - (float)Math.Ceiling(widthDifference / 2f) * (float)Math.Sin(-camera.Rotation + MathHelper.PiOver2), position.Y + widthDifference / 2f * (float)Math.Cos(-camera.Rotation + MathHelper.PiOver2));
                //Vector2 greenPosition = new Vector2(position.X - (hpBarRedWidth / 2f) * (float)Math.Sin(-camera.Rotation + MathHelper.PiOver2), position.Y + (hpBarRedWidth / 2f) * (float)Math.Cos(-camera.Rotation + MathHelper.PiOver2));
                Vector2 greenPosition = position;

                //spriteBatch.Draw(ColorTexture.Green, hpBarGreen, Color.White);
                spriteBatch.Draw(ColorTexture.Green, new Rectangle((int)greenPosition.X, (int)greenPosition.Y, hpBarGreenWidth, hpBarHeight), null, Color.White, -camera.Rotation, new Vector2(0, ColorTexture.Green.Height / 2f), SpriteEffects.None, 0f);
                //spriteBatch.Draw(ColorTexture.Green, new Rectangle((int)(greenPosition.X - (hpBarRedWidth / 2) * (float)Math.Sin(-camera.Rotation + MathHelper.PiOver2)), (int)(greenPosition.Y + (hpBarRedWidth / 2) * (float)Math.Cos(-camera.Rotation + MathHelper.PiOver2)), hpBarGreenWidth, hpBarHeight), null, Color.White, -camera.Rotation, new Vector2(0, ColorTexture.Green.Height / 2f), SpriteEffects.None, 0f);

                //spriteBatch.Draw(ColorTexture.Black, new Rectangle((int)position.X, (int)position.Y, 2, 2), null, Color.White, -camera.Rotation, new Vector2(1, 1), SpriteEffects.None, 0f);

                Structure structure = unit as Structure;
                if (structure != null && structure.Team == Player.Me.Team)
                {
                    if (structure.BuildQueue.Count > 0)
                    {
                        BuildQueueItem item = structure.BuildQueue[0];

                        int buildBarWidth = (int)(hpBarWidth * item.PercentDone / 100f);
                        int buildBarHeight = hpBarHeight - 1;

                        widthDifference = hpBarWidth - buildBarWidth;
                        //int buildBarPosX = hpBarPosX;
                        //int buildBarPosY = hpBarPosY + hpBarHeight + 1;
                        //Rectangle buildBarWhite = new Rectangle(buildBarPosX, buildBarPosY, (int)(buildBarWidth * (item.PercentDone / 100d)), buildBarHeight);
                        //Rectangle buildBarWhite = new Rectangle(buildBarPosX, buildBarPosY, buildBarWidth * item.PercentDone / 100, buildBarHeight);

                        //Vector2 grayPosition = new Vector2(position.X - (widthDifference / 2) * (float)Math.Sin(-camera.Rotation + MathHelper.PiOver2) + (buildBarHeight + 1) * (float)Math.Cos(-camera.Rotation + MathHelper.PiOver2), position.Y + widthDifference / 2f * (float)Math.Cos(-camera.Rotation + MathHelper.PiOver2) + (buildBarHeight + 1) * (float)Math.Sin(-camera.Rotation + MathHelper.PiOver2));
                        Vector2 grayPosition = new Vector2(position.X + (buildBarHeight + 1) * (float)Math.Cos(-camera.Rotation + MathHelper.PiOver2), position.Y + (buildBarHeight + 1) * (float)Math.Sin(-camera.Rotation + MathHelper.PiOver2));

                        //spriteBatch.Draw(ColorTexture.LightGray, buildBarWhite, Color.White);
                        spriteBatch.Draw(ColorTexture.LightGray, new Rectangle((int)grayPosition.X, (int)grayPosition.Y, buildBarWidth, buildBarHeight), null, Color.White, -camera.Rotation, new Vector2(0, ColorTexture.Gray.Height / 2f), SpriteEffects.None, 0f);

                    }
                }

                if (structure != null && structure.Team == Player.Me.Team && structure.UnderConstruction)
                {
                    //int buildBarWidth = hpBarWidth - hpBarSpacingX * 2;
                    //int buildBarHeight = hpBarHeight - 1;
                    //int buildBarPosX = hpBarPosX;
                    //int buildBarPosY = hpBarPosY + hpBarHeight + 1;

                    int buildBarWidth = (int)(hpBarWidth * structure.PercentDone);
                    int buildBarHeight = hpBarHeight - 1;

                    widthDifference = hpBarWidth - buildBarWidth;

                    //Vector2 grayPosition = new Vector2(position.X - (widthDifference / 2) * (float)Math.Sin(-camera.Rotation + MathHelper.PiOver2) + (buildBarHeight + 1) * (float)Math.Cos(-camera.Rotation + MathHelper.PiOver2), position.Y + widthDifference / 2f * (float)Math.Cos(-camera.Rotation + MathHelper.PiOver2) + (buildBarHeight + 1) * (float)Math.Sin(-camera.Rotation + MathHelper.PiOver2));
                    Vector2 grayPosition = new Vector2(position.X + (buildBarHeight + 1) * (float)Math.Cos(-camera.Rotation + MathHelper.PiOver2), position.Y + (buildBarHeight + 1) * (float)Math.Sin(-camera.Rotation + MathHelper.PiOver2));

                    spriteBatch.Draw(ColorTexture.DarkGray, new Rectangle((int)grayPosition.X, (int)grayPosition.Y, buildBarWidth, buildBarHeight), null, Color.White, -camera.Rotation, new Vector2(0, ColorTexture.Gray.Height / 2f), SpriteEffects.None, 0f);

                    //Rectangle buildBarWhite = new Rectangle(buildBarPosX, buildBarPosY, (int)(buildBarWidth * structure.PercentDone), buildBarHeight);

                    //spriteBatch.Draw(ColorTexture.DarkGray, buildBarWhite, Color.White);
                }
            }
        }

        void drawBars(SpriteBatch spriteBatch)
        {
            int hpBarSpacingX = 1;
            int hpBarSpacingY = 1;
            int hpBarWidth = (int)(25 * camera.Zoom);
            int hpBarHeight = (int)(5 / camera.Zoom);
            Rectangle hpBarRed = new Rectangle(0, 0, 0, hpBarHeight);
            Rectangle hpBarGreen = new Rectangle(0, 0, 0, hpBarHeight);

            //foreach (Unit unit in Unit.Units)
            foreach (RtsObject unit in RtsObject.RtsObjects)
            {
                //if (!unit.CurrentPathNode.Tile.Visible)
                if (!unit.Visible)
                    continue;

                //hpBarSpacingX = (int)(unit.Width * .05f);
                hpBarSpacingX = 1;
                //hpBarWidth = (int)(unit.Width * .9f);
                hpBarWidth = unit.Width - hpBarSpacingX * 2;
                //int hpBarHeight = unit.Height / 6;
                Vector2 point = Vector2.Transform(new Vector2(unit.Rectangle.X + hpBarSpacingX, unit.Rectangle.Y - hpBarHeight - hpBarSpacingY), camera.get_transformation(worldViewport));
                //int hpBarPosY = unit.Rectangle.Y - hpBarHeight - hpBarSpacingY;
                //int hpBarPosX = unit.Rectangle.X + hpBarSpacingX;
                int hpBarPosY = (int)point.Y;
                int hpBarPosX = (int)point.X;

                hpBarRed.X = hpBarPosX;
                hpBarRed.Y = hpBarPosY;
                hpBarRed.Width = hpBarWidth - hpBarSpacingX * 2;
                //hpBarRed.Height = hpBarHeight;

                hpBarGreen.X = hpBarPosX;
                hpBarGreen.Y = hpBarPosY;
                int hpBarGreenWidth = (int)(hpBarWidth * unit.PercentHp);
                hpBarGreen.Width = hpBarGreenWidth - hpBarSpacingX * 2;
                //hpBarGreen.Height = hpBarHeight;

                //spriteBatch.Draw(ColorTexture.Red, new Rectangle(point.X, point.Y, hpBarRed.Width, hpBarHeight), null, Color.White * .9f, -camera.Rotation, new Vector2(0, 0), SpriteEffects.None, 0f);
                //spriteBatch.Draw(ColorTexture.Red, hpBarRed, null, Color.White * .9f, -camera.Rotation, new Vector2(0, unit.Height / 2), SpriteEffects.None, 0f);

                spriteBatch.Draw(ColorTexture.Red, hpBarRed, Color.White);
                spriteBatch.Draw(ColorTexture.Green, hpBarGreen, Color.White);

                Structure structure = unit as Structure;
                if (structure != null && structure.Team == Player.Me.Team)
                {
                    if (structure.BuildQueue.Count > 0)
                    {
                        BuildQueueItem item = structure.BuildQueue[0];

                        int buildBarWidth = hpBarWidth - hpBarSpacingX * 2;
                        int buildBarHeight = hpBarHeight - 1;
                        int buildBarPosX = hpBarPosX;
                        int buildBarPosY = hpBarPosY + hpBarHeight + 1;
                        //Rectangle buildBarWhite = new Rectangle(buildBarPosX, buildBarPosY, (int)(buildBarWidth * (item.PercentDone / 100d)), buildBarHeight);
                        Rectangle buildBarWhite = new Rectangle(buildBarPosX, buildBarPosY, buildBarWidth * item.PercentDone / 100, buildBarHeight);

                        spriteBatch.Draw(ColorTexture.LightGray, buildBarWhite, Color.White);
                    }
                }

                if (structure != null && structure.Team == Player.Me.Team && structure.UnderConstruction)
                {
                    /*int buildBarWidth = structure.CogWheel.Rectangle.Width;
                    int buildBarHeight = hpBarHeight - 1;
                    int buildBarPosX = structure.CogWheel.Rectangle.X - structure.CogWheel.Rectangle.Width / 2;
                    int buildBarPosY = structure.CogWheel.Rectangle.Y - structure.CogWheel.Rectangle.Height / 2 - buildBarHeight;*/
                    int buildBarWidth = hpBarWidth - hpBarSpacingX * 2;
                    int buildBarHeight = hpBarHeight - 1;
                    int buildBarPosX = hpBarPosX;
                    int buildBarPosY = hpBarPosY + hpBarHeight + 1;

                    Rectangle buildBarWhite = new Rectangle(buildBarPosX, buildBarPosY, (int)(buildBarWidth * structure.PercentDone), buildBarHeight);

                    spriteBatch.Draw(ColorTexture.DarkGray, buildBarWhite, Color.White);
                }
            }
        }

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

            // draw full map texture
            spriteBatch.Draw(fullMapTexture, new Rectangle(0, 0, map.Width * map.TileSize, map.Height * map.TileSize), Color.White);

            // draw fog
            /*foreach (MapTile tile in map.Tiles)
            {
                if (!tile.Visible)
                {
                    if (tile.Revealed)
                        spriteBatch.Draw(ColorTexture.Black, tile.Rectangle, Color.White * .25f);
                    else
                    //spriteBatch.Draw(transparentBlackTexture, tile.Rectangle, Color.White);
                        spriteBatch.Draw(ColorTexture.Black, tile.Rectangle, Color.White * .6f);
                }
            }*/
            foreach (BoundingBox box in map.BigBoundingBoxes)
            {
                if (box.FullyRevealedAndNotVisible)
                {
                    spriteBatch.Draw(ColorTexture.Black, box.Rectangle, Color.White * .25f);
                }
                else if (box.Revealed)
                {
                    foreach (MapTile tile in box.Tiles)
                    {
                        if (!tile.Visible)
                        {
                            if (tile.Revealed)
                                spriteBatch.Draw(ColorTexture.Black, tile.Rectangle, Color.White * .25f);
                            else
                                //spriteBatch.Draw(transparentBlackTexture, tile.Rectangle, Color.White);
                                spriteBatch.Draw(ColorTexture.Black, tile.Rectangle, Color.White * .6f);
                        }
                    }
                }
                else
                {
                    spriteBatch.Draw(ColorTexture.Black, box.Rectangle, Color.White * .6f);
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

            spriteBatch.Draw(fullMapTexture, new Rectangle(0, 0, minimap.Width, minimap.Height), Color.White);
            //spriteBatch.Draw(fullMapTexture, new Rectangle(minimap.Center.X, minimap.Center.Y, minimap.Width, minimap.Height), null, Color.White, -camera.Rotation, new Vector2(fullMapTexture.Width / 2, fullMapTexture.Height / 2), SpriteEffects.None, 0f);
            int oldPosY = minimapPosY, oldPosX = minimapPosX;
            minimapPosX = minimapPosY = 0;

            // rectangle for pixels on minimap
            //Rectangle fogRectangle = new Rectangle(0, 0, (int)(map.TileSize * minimapToMapRatioX), (int)(map.TileSize * minimapToMapRatioY));

            /*foreach (MapTile tile in map.Tiles)
            {
                if (!tile.Visible)
                {
                    fogRectangle.X = (int)Math.Round(tile.CenterPointX * minimapToMapRatioX + minimapPosX);
                    fogRectangle.Y = (int)Math.Round(tile.CenterPointY * minimapToMapRatioY + minimapPosY);
                    spriteBatch.Draw(transparentBlackTexture, fogRectangle, Color.White);
                }
            }*/

            // draw fog on minimap
            Rectangle fogRectangle = new Rectangle(0, 0, 2, 2);
            int tileX;
            for (int x = 0; x < minimapSize; x += 2)
            {
                fogRectangle.X = minimapPosX + x;
                tileX = (int)(x / minimapToMapRatioX / map.TileSize);
                for (int y = 0; y < minimapSize; y += 2)
                {
                    MapTile tile = map.Tiles[(int)(y / minimapToMapRatioY / map.TileSize), tileX];

                    //if (!tile.BoundingBox.Revealed || tile.BoundingBox.FullyRevealedAndNotVisible)
                    //    continue;

                    if (!tile.Visible)
                    {
                        fogRectangle.Y = minimapPosY + y;
                        if (tile.Revealed)
                            spriteBatch.Draw(ColorTexture.Black, fogRectangle, Color.White * .25f);
                        else
                            spriteBatch.Draw(ColorTexture.Black, fogRectangle, Color.White * .5f);
                    }
                }
            }

            /*foreach (BoundingBox box in map.BigBoundingBoxes)
            {
                if (box.FullyRevealedAndNotVisible)
                {
                    spriteBatch.Draw(ColorTexture.Black, new Rectangle((int)Math.Round(minimapPosX + box.Rectangle.X * minimapToMapRatioX), (int)Math.Round(minimapPosY + box.Rectangle.Y * minimapToMapRatioY), (int)(box.Rectangle.Width * minimapToMapRatioX), (int)(box.Rectangle.Height * minimapToMapRatioY)), Color.White * .25f);
                }
                else if (!box.Revealed)
                {
                    spriteBatch.Draw(ColorTexture.Black, new Rectangle((int)Math.Round(minimapPosX + box.Rectangle.X * minimapToMapRatioX), (int)Math.Round(minimapPosY + box.Rectangle.Y * minimapToMapRatioY), (int)(box.Rectangle.Width * minimapToMapRatioX), (int)(box.Rectangle.Height * minimapToMapRatioY)), Color.White * .6f);
                }
            }*/

            /*foreach (BoundingBox box in map.BigBoundingBoxes)
            {
                if (box.FullyRevealedAndNotVisible)
                {
                    spriteBatch.Draw(ColorTexture.Black, new Rectangle((int)(minimapPosX + box.Rectangle.X * minimapToMapRatioX), (int)(minimapPosY + box.Rectangle.Y * minimapToMapRatioY), (int)(box.Rectangle.Width * minimapToMapRatioX), (int)(box.Rectangle.Height * minimapToMapRatioY)), Color.White * .25f);
                }
                else if (box.Revealed)
                {
                    Rectangle fogRectangle = new Rectangle(0, 0, 2, 2);

                    int X = box.Rectangle.X / map.TileSize;
                    int Y = box.Rectangle.Y / map.TileSize;

                    for (int x = X; x < X + Map.BOUNDING_BOX_SIZE; x += 2)
                    {
                        fogRectangle.X = (int)(minimapPosX + box.Rectangle.X * minimapToMapRatioX + x);
                        //tileX = (int)(x / minimapToMapRatioX / map.TileSize);
                        for (int y = Y; y < Y + Map.BOUNDING_BOX_SIZE; y += 2)
                        {
                            MapTile tile = map.Tiles[y, x];
                            if (!tile.Visible)
                            {
                                fogRectangle.Y = (int)(minimapPosY + box.Rectangle.Y * minimapToMapRatioY + y);
                                if (tile.Revealed)
                                    spriteBatch.Draw(ColorTexture.Black, fogRectangle, Color.White * .25f);
                                else
                                    spriteBatch.Draw(ColorTexture.Black, fogRectangle, Color.White * .5f);
                            }
                        }
                    }

                    /*foreach (MapTile tile in box.Tiles)
                    {
                        fogRectangle.X = (int)(minimapPosX + tile.Rectangle.X * minimapToMapRatioX);
                        fogRectangle.Y = (int)(minimapPosY + tile.Rectangle.Y * minimapToMapRatioY);
                        
                        if (!tile.Visible)
                        {
                            if (tile.Revealed)
                                spriteBatch.Draw(ColorTexture.Black, fogRectangle, Color.White * .25f);
                            else
                                //spriteBatch.Draw(transparentBlackTexture, tile.Rectangle, Color.White);
                                spriteBatch.Draw(ColorTexture.Black, fogRectangle, Color.White * .6f);
                        }
                    }*/
            //}
            // else
            //{
            //    spriteBatch.Draw(ColorTexture.Black, new Rectangle((int)(minimapPosX + box.Rectangle.X * minimapToMapRatioX), (int)(minimapPosY + box.Rectangle.Y * minimapToMapRatioY), (int)(box.Rectangle.Width * minimapToMapRatioX), (int)(box.Rectangle.Height * minimapToMapRatioY)), Color.White * .6f);
            //}
            //}

            // draw units on minimap
            Rectangle unitRectangle = new Rectangle(0, 0, 2, 2);
            foreach (Unit unit in Unit.Units)
            {
                unitRectangle.X = (int)(unit.CenterPointX * minimapToMapRatioX + minimapPosX);
                unitRectangle.Y = (int)(unit.CenterPointY * minimapToMapRatioY + minimapPosY);

                if (unit.Team == Player.Me.Team)
                    spriteBatch.Draw(ColorTexture.Green, unitRectangle, Color.White);
                else if (unit.Visible)
                    spriteBatch.Draw(ColorTexture.Red, unitRectangle, Color.White);
            }

            foreach (Structure structure in Structure.Structures)
            {
                unitRectangle.Width = unitRectangle.Height = ((StructureType)structure.Type).Size;
                unitRectangle.X = (int)Math.Ceiling(structure.Rectangle.X * minimapToMapRatioX + minimapPosX);
                unitRectangle.Y = (int)Math.Ceiling(structure.Rectangle.Y * minimapToMapRatioY + minimapPosY);

                if (structure.Team == Player.Me.Team)
                    spriteBatch.Draw(ColorTexture.Green, unitRectangle, Color.White);
                else if (structure.Visible || structure.Revealed)
                    spriteBatch.Draw(ColorTexture.Red, unitRectangle, Color.White);
            }

            foreach (Resource resource in Resource.Resources)
            {
                unitRectangle.Width = unitRectangle.Height = (int)(resource.Type.Size * 2.5f);
                unitRectangle.X = (int)Math.Ceiling(resource.Rectangle.X * minimapToMapRatioX + minimapPosX);
                unitRectangle.Y = (int)Math.Ceiling(resource.Rectangle.Y * minimapToMapRatioY + minimapPosY);

                spriteBatch.Draw(resource.Texture, unitRectangle, Color.White);
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

            minimapPosX = oldPosX;
            minimapPosY = oldPosY;
        }

        void drawGameClock(SpriteBatch spriteBatch)
        {
            //string s = gameClock.ToString("0.00");
            TimeSpan t = TimeSpan.FromSeconds(GameClock);
            string s;

            if (GameClock >= 3600f)
                s = t.ToString(@"hh\:mm\:ss");
            else
                s = t.ToString(@"mm\:ss");

            Vector2 sSize = bigFont.MeasureString(s);
            spriteBatch.DrawString(bigFont, s, new Vector2(minimapSize / 2 - sSize.X / 2, worldViewport.Height - sSize.Y), Color.White);
        }

        void drawSpecialMessages(SpriteBatch spriteBatch)
        {
            if (countingDown)
            {
                string str = countDownTime.ToString("0") + "...";
                Vector2 strSize = bigFont.MeasureString(str);
                spriteBatch.DrawString(bigFont, str, new Vector2(uiViewport.Width / 2 - strSize.X / 2, uiViewport.Height / 2 - strSize.Y / 2), Color.White);
            }

            if (waitingForMessage)
            {
                string str = "Waiting for other player...";
                Vector2 strSize = bigFont.MeasureString(str);
                spriteBatch.DrawString(bigFont, str, new Vector2(uiViewport.Width / 2 - strSize.X / 2, uiViewport.Height / 2 - strSize.Y / 2), Color.White);
            }
        }

        // draw selection info area
        Rectangle unitPictureRectangle;
        List<UnitButton> unitButtons = new List<UnitButton>();
        Rectangle[] QueuedItems;
        UnitButton builderButton;
        void drawSelectionInfoArea(SpriteBatch spriteBatch)
        {
            QueuedItems = null;

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
                //if (unitButtons.Count > 0)
                if (unitButtons.Count > 0 && selectedUnitsChanged)
                {
                    SimpleButton.RemoveButtons(unitButtons);
                    unitButtons.Clear();
                }

                RtsObject o = SelectedUnits[0];

                // unit picture
                unitPictureRectangle = new Rectangle(selectionInfoArea.X + selectionInfoArea.Width / 2 - 25, selectionInfoArea.Y + selectionInfoArea.Height / 2 - 25, 50, 50);
                spriteBatch.Draw(o.Texture, unitPictureRectangle, Color.White);

                // hp bar
                int hpBarHeight = 8;
                int hpBarSpacing = 6;
                int hpBarPosY = unitPictureRectangle.Y - hpBarHeight - hpBarSpacing;

                Rectangle hpBarRed = new Rectangle(unitPictureRectangle.X, hpBarPosY, unitPictureRectangle.Width, hpBarHeight);

                int hpBarGreenWidth = (int)(unitPictureRectangle.Width * o.PercentHp);
                Rectangle hpBarGreen = new Rectangle(unitPictureRectangle.X, hpBarPosY, hpBarGreenWidth, hpBarHeight);

                spriteBatch.Draw(ColorTexture.Red, hpBarRed, Color.White);
                spriteBatch.Draw(ColorTexture.Green, hpBarGreen, Color.White);

                // unit name
                Vector2 unitNameSize = unitInfoUnitNameFont.MeasureString(o.Name);
                int unitNameSpacing = 2;
                spriteBatch.DrawString(unitInfoUnitNameFont, o.Name, new Vector2((int)(unitPictureRectangle.X + unitPictureRectangle.Width / 2 - unitNameSize.X / 2), (int)(hpBarPosY - unitNameSize.Y - unitNameSpacing)), Color.White);

                // hp
                int hpSpacing = 6;
                int hpPosY = unitPictureRectangle.Y + unitPictureRectangle.Height + hpSpacing;
                string hpString = o.Hp + "/" + o.MaxHp;
                Vector2 hpSize = unitInfoHpFont.MeasureString(hpString);
                spriteBatch.DrawString(unitInfoHpFont, hpString, new Vector2((int)(unitPictureRectangle.X + unitPictureRectangle.Width / 2 - hpSize.X / 2), hpPosY), Color.White);

                Unit unit = o as Unit;
                if (unit != null)
                {
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
                    int attackDamagePosY = hpBarPosY;// unitPictureRectangle.Y;
                    Vector2 attackDamageSize = unitInfoHpFont.MeasureString(unit.AttackDamage.ToString());
                    spriteBatch.DrawString(unitInfoHpFont, unit.AttackDamage.ToString(), new Vector2(statsPosX, attackDamagePosY), Color.Red);
                    spriteBatch.DrawString(unitInfoHpFont, " attack damage.", new Vector2((int)(statsPosX + attackDamageSize.X), attackDamagePosY), Color.White);

                    // attack range
                    float displayedAttackRange = (float)unit.AttackRange / map.TileSize;
                    int attackRangePosY = (int)(attackDamagePosY + attackDamageSize.Y + statsSpacing);
                    Vector2 attackRangeSize = unitInfoHpFont.MeasureString(displayedAttackRange.ToString());
                    spriteBatch.DrawString(unitInfoHpFont, displayedAttackRange.ToString(), new Vector2(statsPosX, attackRangePosY), Color.Orange);
                    //Vector2 attackRangeSize = unitInfoHpFont.MeasureString(unit.AttackRange.ToString());
                    //spriteBatch.DrawString(unitInfoHpFont, unit.AttackRange.ToString(), new Vector2(statsPosX, attackRangePosY), Color.Yellow);
                    spriteBatch.DrawString(unitInfoHpFont, " attack range.", new Vector2((int)(statsPosX + attackRangeSize.X), attackRangePosY), Color.White);

                    // attack speed
                    float attackSpeedValue = 60000f / unit.AttackDelay / 60;
                    int attackSpeedPosY = (int)(attackRangePosY + attackRangeSize.Y + statsSpacing);
                    Vector2 attackSpeedSize = unitInfoHpFont.MeasureString(attackSpeedValue.ToString("0.0"));
                    spriteBatch.DrawString(unitInfoHpFont, attackSpeedValue.ToString("0.0"), new Vector2(statsPosX, attackSpeedPosY), Color.Yellow);
                    spriteBatch.DrawString(unitInfoHpFont, " attack speed.", new Vector2((int)(statsPosX + attackSpeedSize.X), attackSpeedPosY), Color.White);

                    // armor
                    int armorPosY = (int)(attackSpeedPosY + attackSpeedSize.Y + statsSpacing);
                    Vector2 armorSize = unitInfoHpFont.MeasureString(unit.Armor.ToString());
                    spriteBatch.DrawString(unitInfoHpFont, unit.Armor.ToString(), new Vector2(statsPosX, armorPosY), Color.Green);
                    spriteBatch.DrawString(unitInfoHpFont, " armor.", new Vector2((int)(statsPosX + armorSize.X), armorPosY), Color.White);

                    // move speed
                    int moveSpeedPosY = (int)(armorPosY + armorSize.Y + statsSpacing);
                    Vector2 moveSpeedSize = unitInfoHpFont.MeasureString(unit.Speed.ToString());
                    spriteBatch.DrawString(unitInfoHpFont, unit.Speed.ToString(), new Vector2(statsPosX, moveSpeedPosY), Color.LightBlue);
                    spriteBatch.DrawString(unitInfoHpFont, " move speed.", new Vector2((int)(statsPosX + moveSpeedSize.X), moveSpeedPosY), Color.White);
                }

                Structure structure = o as Structure;
                if (structure != null)
                {
                    int statsPosX = (int)(selectionInfoArea.X + selectionInfoArea.Width * .66f);
                    int statsSpacing = -1;

                    // armor
                    Vector2 armorSize = unitInfoHpFont.MeasureString(structure.Armor.ToString());
                    int armorPosY = unitPictureRectangle.Y + (int)(unitPictureRectangle.Height * 1.25f);
                    spriteBatch.DrawString(unitInfoHpFont, structure.Armor.ToString(), new Vector2(statsPosX, armorPosY), Color.Green);
                    spriteBatch.DrawString(unitInfoHpFont, " armor.", new Vector2((int)(statsPosX + armorSize.X), armorPosY), Color.White);

                    // if under construction
                    if (structure.UnderConstruction)
                    {
                        //int barX = selectionInfoAreaPosX + selectionInfoArea.Width / 7;
                        int barX = statsPosX;
                        int barY = selectionInfoAreaPosY + (int)(selectionInfoArea.Height / 3f);
                        int barHeight = selectionInfoArea.Height / 15;// builderBoxSize / 6;
                        Rectangle progressBar = new Rectangle(barX, barY, 0, barHeight);
                        int barWidth = selectionInfoArea.Width / 5;// builderBoxSize * 5;
                        progressBar.Width = barWidth;
                        spriteBatch.Draw(ColorTexture.Gray, progressBar, Color.White);
                        progressBar.Width = (int)(barWidth * structure.PercentDone);
                        spriteBatch.Draw(ColorTexture.LightGray, progressBar, Color.White);

                        string constructingString = "Constructing... " + ((int)(structure.PercentDone * 100f)).ToString() + "%";
                        Vector2 constructingSize = unitInfoHpFont.MeasureString(constructingString);
                        int constructingPosX = progressBar.X + barWidth / 2 - (int)constructingSize.X / 2;
                        int constructingPosY = progressBar.Y - (int)constructingSize.Y;
                        spriteBatch.DrawString(unitInfoHpFont, constructingString, new Vector2(constructingPosX, constructingPosY), Color.White);

                        /*string percentString = ((int)structure.PercentDone * 100).ToString();
                        Vector2 percentSize = unitInfoHpFont.MeasureString(percentString);
                        int percentPosX = constructingPosX + (int)constructingSize.X;
                        int percentPosY = constructingPosY;
                        spriteBatch.DrawString(unitInfoHpFont, percentString, new Vector2(percentPosX, percentPosY), Color.White);*/

                        int builderBoxSize = 30;
                        //Rectangle builderBox = new Rectangle(selectionInfoAreaPosX + selectionInfoArea.Width / 7, selectionInfoAreaPosY + (int)(selectionInfoArea.Height / 4.75f), builderBoxSize, builderBoxSize);
                        Rectangle builderBox = new Rectangle(progressBar.X + progressBar.Width - builderBoxSize / 2, progressBar.Y + progressBar.Height + 1, builderBoxSize, builderBoxSize);
                        if (structure.Builder != null)
                        {
                            if (unitButtons.Count == 0)
                            {
                                builderButton = new UnitButton(builderBox, structure.Builder);
                                unitButtons.Add(builderButton);
                                SimpleButton.AddButtons(unitButtons);
                            }
                            else
                            {
                                builderButton.Position = new Vector2(builderBox.X, builderBox.Y);
                            }
                            spriteBatch.Draw(structure.Builder.Texture, builderBox, Color.White);
                            spriteBatch.Draw(whiteBoxTexture, builderBox, Color.White);
                        }
                    }

                    // training queue
                    if (structure.BuildQueue.Count > 0)
                    {
                        BuildQueueItem item = structure.BuildQueue[0];

                        /*int unitImageY = unitPictureRectangle.Y;
                        spriteBatch.Draw(item.Type.Texture, new Rectangle(statsPosX, unitImageY, 25, 25), Color.White);

                        Vector2 percentSize = unitInfoHpFont.MeasureString(item.PercentDone.ToString());
                        int percentPosY = unitImageY + 25;
                        spriteBatch.DrawString(unitInfoHpFont, item.PercentDone.ToString(), new Vector2(statsPosX, percentPosY), Color.White);
                        spriteBatch.DrawString(unitInfoHpFont, " percent complete. " + (structure.BuildQueue.Count - 1) + " in queue.", new Vector2((int)(statsPosX + percentSize.X), percentPosY), Color.White);
                        */
                        int queueBoxSize = 36;

                        QueuedItems = new Rectangle[Structure.MAX_QUEUE_SIZE];
                        //QueuedItems = new SimpleButton[Structure.MAX_QUEUE_SIZE];

                        //QueuedItems[0] = new Rectangle(selectionInfoAreaPosX + selectionInfoArea.Width / 7, selectionInfoAreaPosY + (int)(selectionInfoArea.Height / 4.75f), queueBoxSize, queueBoxSize);
                        QueuedItems[0] = new Rectangle(statsPosX, selectionInfoAreaPosY + (int)(selectionInfoArea.Height / 6.5f), queueBoxSize, queueBoxSize);

                        QueuedItems[1] = new Rectangle(QueuedItems[0].X, QueuedItems[0].Y + QueuedItems[0].Height + 1, queueBoxSize, queueBoxSize);

                        for (int i = 2; i < QueuedItems.Length; i++)
                            QueuedItems[i] = new Rectangle(QueuedItems[i - 1].X + queueBoxSize + 1, QueuedItems[i - 1].Y, queueBoxSize, queueBoxSize);

                        for (int i = 0; i < QueuedItems.Length; i++)
                        {
                            if (structure.BuildQueue.Count > i)
                                spriteBatch.Draw(structure.BuildQueue[i].Type.Texture, QueuedItems[i], Color.White);
                            spriteBatch.Draw(whiteBoxTexture, QueuedItems[i], Color.White);
                        }

                        int barHeight = queueBoxSize / 6;
                        Rectangle progressBar = new Rectangle(QueuedItems[0].X + queueBoxSize + 1, QueuedItems[0].Y + queueBoxSize - barHeight - 1, 0, barHeight);
                        int barWidth = (QueuedItems[4].X + queueBoxSize) - progressBar.X;
                        progressBar.Width = barWidth;
                        spriteBatch.Draw(ColorTexture.Gray, progressBar, Color.White);
                        progressBar.Width = (int)(barWidth * item.PercentDone / 100f);
                        spriteBatch.Draw(ColorTexture.LightGray, progressBar, Color.White);

                        unitNameSize = unitInfoHpFont.MeasureString(item.Type.Name);
                        spriteBatch.DrawString(unitInfoHpFont, item.Type.Name, new Vector2((int)(progressBar.X + barWidth / 2 - unitNameSize.X / 2), (int)(progressBar.Y - unitNameSize.Y)), Color.White);
                    }
                }
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
                    //if (SelectedUnits[0].GetType() == SelectedUnits.ActiveType)
                    box = new Rectangle(x, y, boxWidth, boxHeight);
                    //else
                    //box = new Rectangle(x + 3, y + 3, boxWidth - 6, boxHeight - 6);
                    unitButtons.Add(new UnitButton(box, SelectedUnits[0]));

                    for (int i = 1; i < SelectedUnits.Count; i++)
                    {
                        RtsObject unit = SelectedUnits[i];

                        x += boxWidth + 1;
                        if (i % 12 == 0)
                        {
                            x = boxPosX;
                            y += boxHeight + 1;
                        }

                        //if (unit.GetType() == SelectedUnits.ActiveType)
                        box = new Rectangle(x, y, boxWidth, boxHeight);
                        //else
                        //   box = new Rectangle(x + 2, y + 2, boxWidth - 4, boxHeight - 4);
                        unitButtons.Add(new UnitButton(box, unit));
                    }
                    SimpleButton.AddButtons(unitButtons);
                }

                //selectedUnitsChanged = false;

                int hpBarSpacing = 1;
                Rectangle hpBarRed = new Rectangle(0, 0, boxWidth - hpBarSpacing * 2, 0);
                Rectangle hpBarGreen = new Rectangle(0, 0, 0, 0);

                foreach (UnitButton box in unitButtons)
                {

                    Rectangle shrunkenBox = box.Rectangle;
                    shrunkenBox.Width = (int)(box.Width * .75f);
                    shrunkenBox.Height = (int)(box.Height * .75f);
                    shrunkenBox.Location = new Point((int)(box.CenterPoint.X - shrunkenBox.Width / 2), (int)(box.CenterPoint.Y - shrunkenBox.Height / 2));

                    spriteBatch.Draw(box.Unit.Texture, shrunkenBox, Color.White);
                    if (box.Unit.Type == SelectedUnits.ActiveType)
                        spriteBatch.Draw(box.Texture, box.Rectangle, Color.White);

                    //hp bar
                    int hpBarHeight = box.Height / 6;
                    int hpBarPosY = box.Y + box.Height - hpBarHeight - hpBarSpacing;

                    hpBarRed.X = box.X + hpBarSpacing;
                    hpBarRed.Y = hpBarPosY;
                    hpBarRed.Height = hpBarHeight;


                    int hpBarGreenWidth = (int)(box.Width * box.Unit.PercentHp);
                    hpBarGreen.X = box.X + hpBarSpacing;
                    hpBarGreen.Y = hpBarPosY;
                    hpBarGreen.Width = hpBarGreenWidth - hpBarSpacing * 2;
                    hpBarGreen.Height = hpBarHeight;

                    spriteBatch.Draw(ColorTexture.Red, hpBarRed, Color.White);
                    spriteBatch.Draw(ColorTexture.Green, hpBarGreen, Color.White);

                    // build queues
                    Structure s = box.Unit as Structure;
                    if (s != null)
                    {
                        int qSize = boxWidth / 6;
                        Rectangle q = new Rectangle(box.X + 2, box.Y + 2, qSize, qSize);
                        //int x = box.X;

                        if (s.BuildQueue.Count > 0)
                        {
                            spriteBatch.Draw(ColorTexture.Beige, q, Color.White);
                        }

                        for (int i = 1; i < s.BuildQueue.Count; i++)
                        {
                            q.X += qSize + 1;
                            spriteBatch.Draw(ColorTexture.Beige, q, Color.White);
                        }
                    }
                }

                int endOfUnitBoxesX = boxPosX + boxWidth * 12 + 22;
                int areaWidth = (selectionInfoArea.X + selectionInfoArea.Width) - endOfUnitBoxesX;

                drawSelectionBorder(spriteBatch, boxPosX - SELECTION_BORDER_WIDTH - 1, selectionInfoArea.Y + 5 - SELECTION_BORDER_WIDTH - 1, endOfUnitBoxesX - selectionInfoArea.X - 9, selectionInfoArea.Height + 4);

                string unitCount = SelectedUnits.Count.ToString();
                // string unitCountMsg = "selected.";
                Vector2 unitCountSize = unitInfoHpFont.MeasureString(unitCount);
                //Vector2 unitCountMsgSize = unitInfoKillCountFont.MeasureString(unitCountMsg);

                //spriteBatch.DrawString(unitInfoHpFont, unitCount, new Vector2((int)(endOfUnitBoxesX + areaWidth / 2 - unitCountSize.X / 2), (int)(selectionInfoArea.Y + selectionInfoArea.Height / 2 - unitCountSize.Y / 2)), Color.White);
                //spriteBatch.DrawString(unitInfoKillCountFont, unitCountMsg, new Vector2((int)(endOfUnitBoxesX + areaWidth / 2 - unitCountMsgSize.X / 2), (int)(selectionInfoArea.Y + selectionInfoArea.Height / 2)), Color.White);

                // info for active type
                if (SelectedUnits.ActiveType != null)
                {
                    int unitPictureSize = 40;
                    unitPictureRectangle = new Rectangle((int)(selectionInfoArea.X + selectionInfoArea.Width * .675f), selectionInfoArea.Y + selectionInfoArea.Height / 2 - unitPictureSize / 2, unitPictureSize, unitPictureSize);
                    spriteBatch.Draw(SelectedUnits.ActiveType.NormalTexture, unitPictureRectangle, Color.White);

                    /*Vector2 selectedTypeSize = unitInfoHpFont.MeasureString("Selected Type");
                    int selectedTypeNameSpacing = 2;
                    spriteBatch.DrawString(unitInfoHpFont, "Selected Type", new Vector2((int)(unitPictureRectangle.X + unitPictureRectangle.Width / 2 - selectedTypeSize.X / 2), (int)(unitPictureRectangle.Y - selectedTypeSize.Y - selectedTypeNameSpacing)), Color.White);
                    */
                    Vector2 unitNameSize = unitInfoHpFont.MeasureString(SelectedUnits.ActiveType.Name);
                    int unitNameSpacing = 7;
                    spriteBatch.DrawString(unitInfoHpFont, SelectedUnits.ActiveType.Name, new Vector2((int)(unitPictureRectangle.X + unitPictureRectangle.Width / 2 - unitNameSize.X / 2), (int)(unitPictureRectangle.Y + unitPictureRectangle.Height + unitNameSpacing)), Color.White);

                    int statsPosX = (int)(selectionInfoArea.X + selectionInfoArea.Width * .8f);
                    int statsSpacing = -1;

                    UnitType unitType = SelectedUnits.ActiveType as UnitType;
                    if (unitType != null)
                    {
                        // attack damage
                        int attackDamagePosY = selectionInfoArea.Y + selectionInfoArea.Height / 7;// unitPictureRectangle.Y;
                        Vector2 attackDamageSize = unitInfoHpFont.MeasureString(unitType.AttackDamage.ToString());
                        spriteBatch.DrawString(unitInfoHpFont, unitType.AttackDamage.ToString(), new Vector2(statsPosX, attackDamagePosY), Color.Red);
                        spriteBatch.DrawString(unitInfoHpFont, " attack damage.", new Vector2((int)(statsPosX + attackDamageSize.X), attackDamagePosY), Color.White);

                        // attack range
                        float displayedAttackRange = (float)unitType.AttackRange / map.TileSize;
                        int attackRangePosY = (int)(attackDamagePosY + attackDamageSize.Y + statsSpacing);
                        Vector2 attackRangeSize = unitInfoHpFont.MeasureString(displayedAttackRange.ToString());
                        spriteBatch.DrawString(unitInfoHpFont, displayedAttackRange.ToString(), new Vector2(statsPosX, attackRangePosY), Color.Orange);
                        //Vector2 attackRangeSize = unitInfoHpFont.MeasureString(unit.AttackRange.ToString());
                        //spriteBatch.DrawString(unitInfoHpFont, unit.AttackRange.ToString(), new Vector2(statsPosX, attackRangePosY), Color.Yellow);
                        spriteBatch.DrawString(unitInfoHpFont, " attack range.", new Vector2((int)(statsPosX + attackRangeSize.X), attackRangePosY), Color.White);

                        // attack speed
                        float attackSpeedValue = 60000f / unitType.AttackDelay / 60;
                        int attackSpeedPosY = (int)(attackRangePosY + attackRangeSize.Y + statsSpacing);
                        Vector2 attackSpeedSize = unitInfoHpFont.MeasureString(attackSpeedValue.ToString("0.0"));
                        spriteBatch.DrawString(unitInfoHpFont, attackSpeedValue.ToString("0.0"), new Vector2(statsPosX, attackSpeedPosY), Color.Yellow);
                        spriteBatch.DrawString(unitInfoHpFont, " attack speed.", new Vector2((int)(statsPosX + attackSpeedSize.X), attackSpeedPosY), Color.White);

                        // armor
                        int armorPosY = (int)(attackSpeedPosY + attackSpeedSize.Y + statsSpacing);
                        Vector2 armorSize = unitInfoHpFont.MeasureString(unitType.Armor.ToString());
                        spriteBatch.DrawString(unitInfoHpFont, unitType.Armor.ToString(), new Vector2(statsPosX, armorPosY), Color.Green);
                        spriteBatch.DrawString(unitInfoHpFont, " armor.", new Vector2((int)(statsPosX + armorSize.X), armorPosY), Color.White);

                        // move speed
                        int moveSpeedPosY = (int)(armorPosY + armorSize.Y + statsSpacing);
                        Vector2 moveSpeedSize = unitInfoHpFont.MeasureString(unitType.MoveSpeed.ToString());
                        spriteBatch.DrawString(unitInfoHpFont, unitType.MoveSpeed.ToString(), new Vector2(statsPosX, moveSpeedPosY), Color.LightBlue);
                        spriteBatch.DrawString(unitInfoHpFont, " move speed.", new Vector2((int)(statsPosX + moveSpeedSize.X), moveSpeedPosY), Color.White);
                    }
                    StructureType structureType = SelectedUnits.ActiveType as StructureType;
                    if (structureType != null)
                    {
                        // armor
                        Vector2 armorSize = unitInfoHpFont.MeasureString(structureType.Armor.ToString());
                        int armorPosY = unitPictureRectangle.Y + (int)(unitPictureRectangle.Height * 1.25f);
                        spriteBatch.DrawString(unitInfoHpFont, structureType.Armor.ToString(), new Vector2(statsPosX, armorPosY), Color.Green);
                        spriteBatch.DrawString(unitInfoHpFont, " armor.", new Vector2((int)(statsPosX + armorSize.X), armorPosY), Color.White);
                    }
                }
            }
        }

        const int SELECTION_BORDER_WIDTH = 5;
        void drawSelectionBorder(SpriteBatch spriteBatch, int x, int y, int width, int height)
        {
            Rectangle border = new Rectangle(x, y, width, SELECTION_BORDER_WIDTH);
            spriteBatch.Draw(ColorTexture.Gray, border, Color.White * .25f);

            border = new Rectangle(x + width - SELECTION_BORDER_WIDTH, y, SELECTION_BORDER_WIDTH, height);
            spriteBatch.Draw(ColorTexture.Gray, border, Color.White * .25f);

            border = new Rectangle(x, y + height - SELECTION_BORDER_WIDTH, width, SELECTION_BORDER_WIDTH);
            spriteBatch.Draw(ColorTexture.Gray, border, Color.White * .25f);

            border = new Rectangle(x, y, SELECTION_BORDER_WIDTH, height);
            spriteBatch.Draw(ColorTexture.Gray, border, Color.White * .25f);
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

            // if current selection has changed, change buttons
            if (selectedUnitsChanged)
            {
                resetCommandCard();
                selectedUnitsChanged = false;
            }

            // check if all selected units are holding position
            bool selectedUnitsHoldingPosition = true;
            foreach (RtsObject o in SelectedUnits)
            {
                Unit unit = o as Unit;
                if (unit != null && !unit.IsHoldingPosition)
                    selectedUnitsHoldingPosition = false;
            }

            // draw buttons
            foreach (CommandButton button in CommandCardButtons)
            {
                if (button.Pressing || button.PressingHotkey || (selectedUnitsHoldingPosition && button.Type == CommandButtonType.HoldPosition))
                    spriteBatch.Draw(transparentGrayTexture, button.Rectangle, Color.White);
                spriteBatch.Draw(button.Texture, button.Rectangle, Color.White);
                spriteBatch.Draw(whiteBoxTexture, button.Rectangle, Color.White);
                if (button.Hotkey != Keys.Kana)
                {
                    string hotkeyString = button.Hotkey.ToString();
                    if (hotkeyString == "Escape")
                        hotkeyString = "Esc";
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

            spriteBatch.Draw(ColorTexture.White, border, Color.White * .9f);
        }

        public static void Initializeline(GraphicsDevice graphicsDevice, Color color)
        {
            line = new PrimitiveLine(graphicsDevice, 1);
            line.Colour = color;
        }

        /*void drawSelectionRing(Unit unit, SpriteBatch spriteBatch, Color color)
        {
            line.Colour = color;

            line.Position = unit.CenterPoint;
            line.CreateCircle(unit.Radius, (int)Math.Round(unit.Radius * 2));
            line.Render(spriteBatch);
        }*/
        void drawSelectingRing(RtsObject o, SpriteBatch spriteBatch)
        {
            Unit unit = o as Unit;
            if (unit != null)
            {
                //spriteBatch.Draw(unit.SelectingTexture, new Rectangle((int)unit.CenterPoint.X, (int)unit.CenterPoint.Y, unit.Width, unit.Height), null, Color.White, unit.Rotation, unit.TextureCenterOrigin, SpriteEffects.None, 0f);

                line.ClearVectors();
                line.Colour = Color.Green;
                line.CreateCircle(unit.CenterPoint, unit.Radius);
                line.RenderWithZoom(spriteBatch, camera.Zoom);
            }

            Structure structure = o as Structure;
            if (structure != null)
            {
                //PrimitiveLine line = new PrimitiveLine(GraphicsDevice, 1);
                line.ClearVectors();
                line.Colour = Color.Green;
                line.CreateCircle(structure.CenterPoint, structure.Radius);
                line.RenderWithZoom(spriteBatch, camera.Zoom);
            }
        }
        void drawSelectedRing(RtsObject o, SpriteBatch spriteBatch)
        {
            Unit unit = o as Unit;
            if (unit != null)
            {
                //spriteBatch.Draw(unit.SelectedTexture, new Rectangle((int)unit.CenterPoint.X, (int)unit.CenterPoint.Y, unit.Width, unit.Height), null, Color.White, unit.Rotation, unit.TextureCenterOrigin, SpriteEffects.None, 0f);

                line.ClearVectors();
                line.Colour = Color.Beige;
                line.CreateCircle(unit.CenterPoint, unit.Radius);
                line.RenderWithZoom(spriteBatch, camera.Zoom);
            }
            Structure structure = o as Structure;
            if (structure != null)
            {
                //PrimitiveLine line = new PrimitiveLine(GraphicsDevice, 1);
                line.ClearVectors();
                line.Colour = Color.Beige;
                line.CreateCircle(structure.CenterPoint, structure.Radius);
                line.RenderWithZoom(spriteBatch, camera.Zoom);
            }
        }
    }
}