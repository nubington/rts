using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace rts
{
    public partial class Rts : GameState
    {
        protected TimeSpan fpsElapsedTime;
        protected int frameCounter;

        Camera camera;
        float cameraScrollSpeed = 1500, cameraZoomSpeed = 1, cameraRotationSpeed = 4.5f, cameraRotationTarget, cameraRotationIncrement = MathHelper.PiOver2;//MathHelper.PiOver4 / 2;

        BaseObject button1, button2, button3, button4, button5;

        Rectangle minimap;
        int minimapSize = 128, minimapBorderSize = 5;
        int minimapPosX, minimapPosY;
        float minimapToMapRatioX, minimapToMapRatioY;
        float minimapToScreenRatioX, minimapToScreenRatioY;

        Rectangle commandCardArea;
        int commandButtonsAreaPosX, commandButtonsAreaPosY;
        int commandButtonsAreaWidth = 150;

        Viewport worldViewport, uiViewport, minimapViewport;

        int roksPerSecondTimer;
        float roksPerSecond;


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
    }
}