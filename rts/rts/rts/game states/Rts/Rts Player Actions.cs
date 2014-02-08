using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Lidgren.Network;
using Lidgren.Network.Xna;

namespace rts
{
    public partial class Rts : GameState
    {
        protected static bool paused, allowPause;


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
            Rectangle magicBox = units[0].Rectangle;
            foreach (Unit unit in units)
            {
                magicBox = Rectangle.Union(magicBox, unit.Rectangle);
            }

            // box is too big or clicked inside magic box or too far away
            if (magicBox.Width > magicBoxMaxSize || magicBox.Height > magicBoxMaxSize ||
                magicBox.Contains((int)mousePosition.X, (int)mousePosition.Y) ||
                Vector2.Distance(new Vector2(magicBox.Center.X, magicBox.Center.Y), mousePosition) > magicBoxMaxDistance)
            {
                //bool isPointWalkable = Rts.pathFinder.IsPointWalkable(mousePosition);
                // assign move targets to mouse position
                foreach (Unit unit in units)
                {
                    // if mouse position is not in a walkable tile, find nearest walkable tile
                    Vector2 destinationPoint;
                    if (Rts.pathFinder.Tools.IsPointWalkable(mousePosition, unit))
                        destinationPoint = mousePosition;
                    else
                        //destinationPoint = map.FindNearestWalkableTile(mousePosition);
                        destinationPoint = (Rts.pathFinder.Tools.FindNearestPathNode((int)(mousePosition.Y / map.TileSize), (int)(mousePosition.X / map.TileSize), unit)).Tile.CenterPoint;

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

                    scheduledUnitCommands.Add(new ScheduledUnitCommand(currentScheduleTime, new MoveCommand(unit, destinationPoint), usingShift));
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
                    if (!Rts.pathFinder.Tools.IsPointWalkable(destinationPoint, unit))
                        //destinationPoint = map.FindNearestWalkableTile(destinationPoint);
                        destinationPoint = (Rts.pathFinder.Tools.FindNearestPathNode((int)(destinationPoint.Y / map.TileSize), (int)(destinationPoint.X / map.TileSize), unit)).Tile.CenterPoint;

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

                    scheduledUnitCommands.Add(new ScheduledUnitCommand(currentScheduleTime, new MoveCommand(unit, destinationPoint), usingShift));
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
                //Rts.pathFinder.AddHighPriorityPathFindRequest(moveCommand, (int)Vector2.DistanceSquared(moveCommand.Unit.CenterPoint, moveCommand.Destination), false);

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
                //Rts.pathFinder.AddHighPriorityPathFindRequest(attackCommand, (int)Vector2.DistanceSquared(attackCommand.Unit.CenterPoint, attackCommand.Destination), false);

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
                    if (Rts.pathFinder.Tools.IsPointWalkable(mousePosition))
                        rallyPoint = mousePosition;
                    else
                        rallyPoint = Rts.pathFinder.Tools.FindNearestPathNode((int)(mousePosition.Y / map.TileSize), (int)(mousePosition.X / map.TileSize), s).Tile.CenterPoint;

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
            Rectangle magicBox = units[0].Rectangle;
            foreach (Unit unit in units)
                magicBox = Rectangle.Union(magicBox, unit.Rectangle);

            // box is too big or clicked inside magic box
            if (magicBox.Width > magicBoxMaxSize || magicBox.Height > magicBoxMaxSize ||
                magicBox.Contains((int)mousePosition.X, (int)mousePosition.Y))
            {
                //bool isPointWalkable = Rts.pathFinder.IsPointWalkable(mousePosition);
                // assign move targets to mouse position
                foreach (Unit unit in units)
                {
                    // if mouse position is not in a walkable tile, find nearest walkable tile
                    Vector2 destinationPoint;
                    if (Rts.pathFinder.Tools.IsPointWalkable(mousePosition, unit))
                        destinationPoint = mousePosition;
                    else
                        //destinationPoint = map.FindNearestWalkableTile(mousePosition);\
                        destinationPoint = (Rts.pathFinder.Tools.FindNearestPathNode((int)(mousePosition.Y / map.TileSize), (int)(mousePosition.X / map.TileSize), unit)).Tile.CenterPoint;

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
                    scheduledUnitCommands.Add(new ScheduledUnitCommand(currentScheduleTime, new AttackMoveCommand(unit, destinationPoint), usingShift));
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
                    if (!Rts.pathFinder.Tools.IsPointWalkable(destinationPoint, unit))
                        //destinationPoint = map.FindNearestWalkableTile(destinationPoint);
                        destinationPoint = (Rts.pathFinder.Tools.FindNearestPathNode((int)(destinationPoint.Y / map.TileSize), (int)(destinationPoint.X / map.TileSize), unit)).Tile.CenterPoint;

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
                    scheduledUnitCommands.Add(new ScheduledUnitCommand(currentScheduleTime, new AttackMoveCommand(unit, destinationPoint), usingShift));
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
                //Rts.pathFinder.AddHighPriorityPathFindRequest(attackMoveCommand, (int)Vector2.DistanceSquared(attackMoveCommand.Unit.CenterPoint, attackMoveCommand.Destination), false);

                //if (s.Queued)
                //moveCommand.Unit.QueueCommand(moveCommand);
                //else
                //moveCommand.Unit.GiveCommand(moveCommand);

                if (attackMoveCommand.Unit.ID < 0)
                {
                    int wut = 0;
                }

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
            BuildStructureCommand buildStructureCommand = new BuildStructureCommand(workerWithSmallestQueue, placingStructureType, placingStructureLocation, placingStructureCenterPoint);
            ScheduledUnitBuildCommand scheduledUnitBuildCommand = new ScheduledUnitBuildCommand(currentScheduleTime, buildStructureCommand, usingShift);
            Player.Players[workerWithSmallestQueue.Team].ScheduledActions.Add(scheduledUnitBuildCommand);
            Player.Players[workerWithSmallestQueue.Team].Roks -= placingStructureType.RoksCost;

            Rts.pathFinder.AddPathFindRequest(buildStructureCommand, usingShift, false, false);

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
            // error if not enough roks
            if (buttonType.UnitType.RoksCost > Player.Me.Roks)
            {
                playErrorSound();
                return;
            }

            // find selected structure with smallest queue
            // also counts scheduled commands
            Structure structureWithSmallestQueue = null;
            int smallest = int.MaxValue;
            foreach (RtsObject o in SelectedUnits)
            {
                Structure s = o as Structure;

                if (s.Team != Player.Me.Team)
                    return;

                if (s != null && s.Type == SelectedUnits.ActiveType && !s.UnderConstruction)
                {
                    int queueCount = s.BuildQueue.Count + Player.Me.CountScheduledStructureCommands(s);
                    if (queueCount < smallest)
                    {
                        structureWithSmallestQueue = s;
                        smallest = queueCount;
                    }
                }
            }

            // find structure with highest percent done (and smallest queue)
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

            // give command to the structure
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
                                scheduledHarvestCommands.Add(new ScheduledUnitTargetedCommand(currentScheduleTime, new HarvestCommand(worker, resource), resource, usingShift));
                            }
                            else
                            {
                                Vector2 destinationPoint;
                                if (Rts.pathFinder.Tools.IsPointWalkable(mousePosition, unit))
                                    destinationPoint = mousePosition;
                                else
                                    destinationPoint = (Rts.pathFinder.Tools.FindNearestPathNode((int)(mousePosition.Y / map.TileSize),
                                        (int)(mousePosition.X / map.TileSize), unit)).Tile.CenterPoint;

                                /*if (!usingShift)
                                    unit.GiveCommand(new MoveCommand(unit, destinationPoint, 1));
                                else
                                    unit.QueueCommand(new MoveCommand(unit, destinationPoint, 1));*/

                                scheduledMoveCommands.Add(new ScheduledUnitCommand(currentScheduleTime, new MoveCommand(unit, destinationPoint), usingShift));
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
                            //Rts.pathFinder.AddHighPriorityPathFindRequest(harvestCommand, (int)Vector2.DistanceSquared(harvestCommand.Unit.CenterPoint, harvestCommand.Destination), false);

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
                            //Rts.pathFinder.AddHighPriorityPathFindRequest(moveCommand, (int)Vector2.DistanceSquared(moveCommand.Unit.CenterPoint, moveCommand.Destination), false);

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
                        scheduledUnitCommands.Add(new ScheduledReturnCargoCommand(currentScheduleTime, new ReturnCargoCommand(worker, townHall, source)));
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

                if (o.Contains(mousePosition) && o.Team != Player.Me.Team)
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
                            //Rts.pathFinder.AddHighPriorityPathFindRequest(u, command, u.CurrentPathNode, (int)Vector2.DistanceSquared(u.CenterPoint, command.Destination), false);
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

        void stopTargetedCommands()
        {
            usingTargetedCommand = false;
            usingAttackCommand = false;
            usingRallyPointCommand = false;

            queueingTargetedCommand = false;

            winForm.Cursor = normalCursor;
        }

        void centerCameraOnSelectedUnits()
        {
            if (SelectedUnits.Count == 0)
                return;

            Rectangle rectangle = SelectedUnits[0].Rectangle;
            foreach (RtsObject o in SelectedUnits)
                rectangle = Rectangle.Union(rectangle, o.Rectangle);

            camera.Pos = new Vector2(rectangle.Center.X, rectangle.Center.Y);
        }
    }
}
