using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace rts
{
    public partial class Rts : GameState
    {
        MouseState mouseState;
        KeyboardState keyboardState;

        const int MAXSELECTIONSIZE = 255;//36;
        static List<RtsObject> SelectingUnits = new List<RtsObject>();
        //static List<Unit> SelectedUnits = new List<Unit>();
        public static Selection SelectedUnits = new Selection();
        static List<RtsObject>[] HotkeyGroups = new List<RtsObject>[10];
        public static bool selectedUnitsChanged;

        bool usingAttackCommand, usingRallyPointCommand, usingTargetedCommand, queueingTargetedCommand;
        //int normalCursorSize = 28, attackCommandCursorSize = 23;


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

        int doubleHotkeySelectDelay = 250, lastHotKeyGroupSelected = -1;
        bool[] allowHotkeyGroupSelect = new bool[10];
        int[] timeSinceLastHotkeyGroupSelect = new int[10];
        Keys[] hotKeyGroupKeys = new Keys[10] { Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9 };
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
    }
}
