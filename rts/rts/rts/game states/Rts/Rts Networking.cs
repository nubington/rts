using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Lidgren.Network;
using Lidgren.Network.Xna;

namespace rts
{
    public class MessageID
    {
        public const byte
            SYNC = 100,
            CHECKUP = 101,
            PATH_UPDATE = 102,
            UNIT_MOVE_COMMAND_BATCH = 103,
            STRUCTURE_COMMAND = 104,
            RALLY_POINT_COMMAND = 105,
            UNIT_STATUS_UPDATE = 106,
            UNIT_ATTACK_COMMAND_BATCH = 107,
            UNIT_ATTACK_MOVE_COMMAND_BATCH = 108,
            UNIT_BUILD_COMMAND = 109,
            UNIT_HARVEST_COMMAND_BATCH = 110,
            UNIT_RETURN_CARGO_COMMAND_BATCH = 111,
            UNIT_STOP_COMMAND_BATCH = 112,
            UNIT_HOLD_POSITION_COMMAND_BATCH = 113,
            UNIT_DEATH = 114,
            STRUCTURE_DEATH = 115,
            UNIT_HP_UPDATE = 116,
            STRUCTURE_STATUS_UPDATE = 117,
            RESOURCE_STATUS_UPDATE = 118,
            UNIT_COMMAND_FINISHED = 119
            ;
    }

    public partial class Rts : GameState
    {
        public static Lidgren.Network.NetPeer netPeer;
        public static Lidgren.Network.NetConnection connection;
        bool iAmServer;

        public static float GameClock = 0;
        float currentScheduleTime;
        float countDownTime = COUNTDOWN_TIME;
        bool countingDown = true;


        float timeSinceLastCheckup, checkupDelay = .5f;
        void checkToCheckup(GameTime gameTime)
        {
            if (iAmServer)
                return;

            timeSinceLastCheckup += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (timeSinceLastCheckup >= checkupDelay)
            {
                timeSinceLastCheckup = 0f;

                NetOutgoingMessage msg = netPeer.CreateMessage();
                msg.Write(MessageID.CHECKUP);
                netPeer.SendMessage(msg, connection, NetDeliveryMethod.ReliableUnordered);
            }
        }

        float timeSinceLastSync, syncDelay = .5f;
        void checkToSync(GameTime gameTime)
        {
            if (iAmServer)
            {
                timeSinceLastSync += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (timeSinceLastSync >= syncDelay)
                {
                    timeSinceLastSync = 0f;
                    NetOutgoingMessage msg = netPeer.CreateMessage();
                    msg.Write(MessageID.SYNC);
                    msg.Write(GameClock);
                    netPeer.SendMessage(msg, connection, NetDeliveryMethod.ReliableSequenced, 0);
                }
            }
        }

        float countDownSyncDelay = .05f;
        void countDownSync(GameTime gameTime)
        {
            timeSinceLastSync += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (timeSinceLastSync >= countDownSyncDelay)
            {
                timeSinceLastSync = 0f;
                NetOutgoingMessage msg = netPeer.CreateMessage();
                msg.Write(MessageID.SYNC);
                msg.Write(GameClock);
                netPeer.SendMessage(msg, connection, NetDeliveryMethod.ReliableSequenced, 0);
            }
        }

        void checkForUnitStatusUpdates(GameTime gameTime)
        {
            foreach (Unit unit in Unit.Units)
            {
                //if (unit.Team != Player.Me.Team)
                //    continue;

                //if (unit.IsIdle)
                    unit.CheckForStatusUpdate(gameTime, netPeer, connection);
            }
        }
        void checkForStructureStatusUpdates(GameTime gameTime)
        {
            foreach (Structure structure in Structure.Structures)
            {
                // update if not under construction or
                // under construction and on my team
                if (!structure.UnderConstruction || structure.Team == Player.Me.Team)
                    structure.CheckForStatusUpdate(gameTime, netPeer, connection);
            }
        }
        // resource status updates are handled by the resources themselves

        float timeSinceMessageReceived, timeOutDelay = .53f;
        float CurrentPing;
        bool waitingForMessage;
        void receiveData(GameTime gameTime)
        {
            timeSinceMessageReceived += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (timeSinceMessageReceived >= timeOutDelay)
                waitingForMessage = true;

            NetIncomingMessage msg;
            while ((msg = netPeer.ReadMessage()) != null)
            {
                timeSinceMessageReceived = 0f;
                waitingForMessage = false;

                if (msg.MessageType == NetIncomingMessageType.Data)
                {
                    byte id = msg.ReadByte();

                    if (id == MessageID.SYNC)
                    {
                        GameClock = msg.ReadFloat() + connection.AverageRoundtripTime / 2f;
                    }
                    else if (id == MessageID.UNIT_MOVE_COMMAND_BATCH)
                    {
                        processUnitMoveCommandBatch(msg);
                    }
                    else if (id == MessageID.STRUCTURE_COMMAND)
                    {
                        processStructureCommand(msg);
                    }
                    else if (id == MessageID.RALLY_POINT_COMMAND)
                    {
                        processRallyPointCommand(msg);
                    }
                    else if (id == MessageID.UNIT_STATUS_UPDATE)
                    {
                        processUnitStatusUpdate(msg);
                    }
                    else if (id == MessageID.UNIT_HP_UPDATE)
                    {
                        processUnitHpUpdate(msg);
                    }
                    else if (id == MessageID.STRUCTURE_STATUS_UPDATE)
                    {
                        processStructureStatusUpdate(msg);
                    }
                    else if (id == MessageID.RESOURCE_STATUS_UPDATE)
                    {
                        processResourceStatusUpdate(msg);
                    }
                    else if (id == MessageID.UNIT_ATTACK_COMMAND_BATCH)
                    {
                        processUnitAttackCommandBatch(msg);
                    }
                    else if (id == MessageID.UNIT_ATTACK_MOVE_COMMAND_BATCH)
                    {
                        processUnitAttackMoveCommandBatch(msg);
                    }
                    else if (id == MessageID.UNIT_BUILD_COMMAND)
                    {
                        processUnitBuildCommand(msg);
                    }
                    else if (id == MessageID.UNIT_HARVEST_COMMAND_BATCH)
                    {
                        processUnitHarvestCommandBatch(msg);
                    }
                    else if (id == MessageID.UNIT_RETURN_CARGO_COMMAND_BATCH)
                    {
                        processUnitReturnCargoCommandBatch(msg);
                    }
                    else if (id == MessageID.UNIT_STOP_COMMAND_BATCH)
                    {
                        processUnitStopCommandBatch(msg);
                    }
                    else if (id == MessageID.UNIT_HOLD_POSITION_COMMAND_BATCH)
                    {
                        processUnitHoldPositionCommandBatch(msg);
                    }
                    else if (id == MessageID.UNIT_DEATH)
                    {
                        processUnitDeath(msg);
                    }
                    else if (id == MessageID.STRUCTURE_DEATH)
                    {
                        processStructureDeath(msg);
                    }
                    /*else if (id == MessageID.PATH_UPDATE)
                    {
                        processPathUpdate(msg);
                    }*/
                }

                netPeer.Recycle(msg);
            }

            CurrentPing = connection.AverageRoundtripTime;
            //currentScheduleTime = gameClock + currentPing * .6f;
            currentScheduleTime = GameClock + .1f;
        }

        void processUnitMoveCommandBatch(NetIncomingMessage msg)
        {
            float scheduledTime = msg.ReadFloat();
            short team = msg.ReadInt16();
            bool queued = msg.ReadBoolean();
            short numberOfCommands = msg.ReadInt16();

            for (int i = 0; i < numberOfCommands; i++)
            {
                short unitID = msg.ReadInt16();
                Unit unit = Player.Players[team].UnitArray[unitID];
                Vector2 destination = new Vector2(msg.ReadFloat(), msg.ReadFloat());
                //if (unit != null)
                {
                    MoveCommand moveCommand = new MoveCommand(unit, destination);
                    Player.Players[team].ScheduledActions.Add(new ScheduledUnitCommand(scheduledTime, moveCommand, queued));
                    //Rts.pathFinder.AddHighPriorityPathFindRequest(moveCommand, (int)Vector2.DistanceSquared(moveCommand.Unit.CenterPoint, moveCommand.Destination), false);
                }
            }
        }

        void processStructureCommand(NetIncomingMessage msg)
        {
            float scheduledTime = msg.ReadFloat();
            short team = msg.ReadInt16();

            Structure structure = Player.Players[team].StructureArray[msg.ReadInt16()];
            CommandButtonType commandType = CommandButtonType.CommandButtonTypes[msg.ReadInt16()];

            short unitID = msg.ReadInt16();

            Player.Players[team].ScheduledActions.Add(new ScheduledStructureCommand(scheduledTime, structure, commandType, unitID)); 
        }

        void processRallyPointCommand(NetIncomingMessage msg)
        {
            float scheduledTime = msg.ReadFloat();
            short team = msg.ReadInt16();
            bool queued = msg.ReadBoolean();
            short numberOfCommands = msg.ReadInt16();

            for (int i = 0; i < numberOfCommands; i++)
            {
                Structure structure = Player.Players[team].StructureArray[msg.ReadInt16()];
                Resource resource;
                short resourceID = msg.ReadInt16();
                Vector2 point = new Vector2(msg.ReadFloat(), msg.ReadFloat());
                if (resourceID == (short)-1)
                    resource = null;
                else
                    resource = Resource.ResourceArray[resourceID];
                Player.Players[team].ScheduledActions.Add(new ScheduledStructureTargetedCommand(scheduledTime, structure, CommandButtonType.RallyPoint, resource, point, queued));
            }
        }

        void processUnitStatusUpdate(NetIncomingMessage msg)
        {
            short unitID = msg.ReadInt16();
            short team = msg.ReadInt16();
            short hp = msg.ReadInt16();
            Vector2 position = new Vector2(msg.ReadFloat(), msg.ReadFloat());
            float rotation = msg.ReadFloat();
            bool idle = msg.ReadBoolean();

            Unit unit = Player.Players[team].UnitArray[unitID];

            if (unit != null)
            {
                if (hp < unit.Hp && !unit.IsDead)
                {
                    unit.Hp = hp;
                    if (hp <= 0)
                        unit.Die();
                    return;
                }

                //if (unit.IsIdle)
                if (idle)
                {
                    unit.CenterPoint = position;

                    if (!unit.IsIdle)
                        unit.NextCommand();
                }
                else
                {
                    Vector2 expectedPosition = new Vector2(position.X + unit.Speed * connection.AverageRoundtripTime / 2 * (float)Math.Cos(rotation), position.Y + unit.Speed * connection.AverageRoundtripTime / 2 * (float)Math.Sin(rotation));

                    if (Vector2.Distance(expectedPosition, unit.CenterPoint) > unit.Radius)
                    {
                        //unit.CenterPoint -= new Vector2((unit.CenterPoint.X - expectedPosition.X), (unit.CenterPoint.Y - expectedPosition.Y));
                        unit.CenterPoint = expectedPosition;
                    }
                }

                // read current command ID
                int commandID = msg.ReadInt16();
                // if its not the same as our current command, look for it in queue
                if (commandID != -1 && unit.Commands.Count > 0 && commandID != unit.Commands[0].ID)
                {
                    for (int i = 1; i < unit.Commands.Count; i++)
                    {
                        UnitCommand command = unit.Commands[i];
                        if (command.ID == commandID)
                        {
                            // do NextCommand enough times to catch up
                            for (int s = 0; s < i; s++)
                                unit.NextCommand();
                        }
                    }
                }

                // read cargoAmount at end if worker
                WorkerNublet worker = unit as WorkerNublet;
                if (worker != null)
                {
                    short cargoAmount = msg.ReadInt16();

                    worker.CargoAmount = cargoAmount;
                }
            }
        }

        void processUnitHpUpdate(NetIncomingMessage msg)
        {
            short unitID = msg.ReadInt16();
            short team = msg.ReadInt16();
            short hp = msg.ReadInt16();

            Unit unit = Player.Players[team].UnitArray[unitID];
            if (unit != null && hp < unit.Hp)
            {
                unit.Hp = hp;
                if (hp <= 0)
                    unit.Die();
            }
        }

        void processStructureStatusUpdate(NetIncomingMessage msg)
        {
            short structureID = msg.ReadInt16();
            short team = msg.ReadInt16();
            short hp = msg.ReadInt16();

            Structure structure = Player.Players[team].StructureArray[structureID];
            if (structure != null && hp < structure.Hp && structure.HasTakenDamageEver)
            {
                structure.Hp = hp;
                if (hp <= 0)
                    structure.Die();
            }
        }

        void processResourceStatusUpdate(NetIncomingMessage msg)
        {
            short resourceID = msg.ReadInt16();
                        short amount = msg.ReadInt16();

                        Resource resource = Resource.ResourceArray[resourceID];
                        if (resource != null)// && amount > resource.Amount)
                        {
                            resource.Amount = amount;
                        }
        }

        void processUnitAttackCommandBatch(NetIncomingMessage msg)
        {
            float scheduledTime = msg.ReadFloat();
            short team = msg.ReadInt16();
            bool queued = msg.ReadBoolean();
            short count = msg.ReadInt16();

            RtsObject target;
            short targetIsStructure = msg.ReadInt16();
            if (targetIsStructure == 0)
            {
                short targetID = msg.ReadInt16();
                short targetTeam = msg.ReadInt16();
                target = Player.Players[targetTeam].UnitArray[targetID];
            }
            else
            {
                short targetID = msg.ReadInt16();
                short targetTeam = msg.ReadInt16();
                target = Player.Players[targetTeam].StructureArray[targetID];
            }

            for (int i = 0; i < count; i++)
            {
                Unit unit = Player.Players[team].UnitArray[msg.ReadInt16()];
                if (unit != null)
                {
                    AttackCommand attackCommand = new AttackCommand(unit, target, false, false);
                    Player.Players[target.Team].ScheduledActions.Add(new ScheduledUnitCommand(scheduledTime, attackCommand, queued));
                    //Rts.pathFinder.AddHighPriorityPathFindRequest(attackCommand, (int)Vector2.DistanceSquared(attackCommand.Unit.CenterPoint, attackCommand.Destination), false);
                }
            }
        }

        void processUnitAttackMoveCommandBatch(NetIncomingMessage msg)
        {
            float scheduledTime = msg.ReadFloat();
            short team = msg.ReadInt16();
            bool queued = msg.ReadBoolean();
            short count = msg.ReadInt16();

            for (int i = 0; i < count; i++)
            {
                int unitID = msg.ReadInt16();
                Unit unit = Player.Players[team].UnitArray[unitID];
                if (unit != null)
                {
                    AttackMoveCommand attackMoveCommand = new AttackMoveCommand(unit, new Vector2(msg.ReadFloat(), msg.ReadFloat()));
                    Player.Players[team].ScheduledActions.Add(new ScheduledUnitCommand(scheduledTime, attackMoveCommand, queued));
                    //Rts.pathFinder.AddHighPriorityPathFindRequest(attackMoveCommand, (int)Vector2.DistanceSquared(attackMoveCommand.Unit.CenterPoint, attackMoveCommand.Destination), false);
                }
            }
        }

        void processUnitBuildCommand(NetIncomingMessage msg)
        {
            float scheduledTime = msg.ReadFloat();
            short team = msg.ReadInt16();

            Unit unit = Player.Players[team].UnitArray[msg.ReadInt16()];

            StructureType structureType = StructureType.StructureTypes[msg.ReadInt16()];
            Point location = new Point(msg.ReadInt16(), msg.ReadInt16());
            bool queued = msg.ReadBoolean();

            BuildStructureCommand buildStructureCommand = new BuildStructureCommand(unit, structureType, location, new Vector2(location.X * map.TileSize + structureType.Size * map.TileSize / 2, location.Y * map.TileSize + structureType.Size * map.TileSize / 2));
            Player.Players[team].ScheduledActions.Add(new ScheduledUnitBuildCommand(scheduledTime, buildStructureCommand, queued));
            //Rts.pathFinder.AddPathFindRequest(buildStructureCommand, queued, false, false);
        }

        void processUnitHarvestCommandBatch(NetIncomingMessage msg)
        {
            float scheduledTime = msg.ReadFloat();
            short team = msg.ReadInt16();
            bool queued = msg.ReadBoolean();
            short count = msg.ReadInt16();

            for (int i = 0; i < count; i++)
            {
                Unit unit = Player.Players[team].UnitArray[msg.ReadInt16()];
                Resource targetResource = Resource.ResourceArray[msg.ReadInt16()];

                Player.Players[team].ScheduledActions.Add(new ScheduledUnitTargetedCommand(scheduledTime, new HarvestCommand(unit, targetResource), targetResource, queued));
            }
        }

        void processUnitReturnCargoCommandBatch(NetIncomingMessage msg)
        {
            float scheduledTime = msg.ReadFloat();
            short team = msg.ReadInt16();
            short count = msg.ReadInt16();

            for (int i = 0; i < count; i++)
            {
                short unitID = msg.ReadInt16();
                short townHallID = msg.ReadInt16();
                short resourceID = msg.ReadInt16();

                Unit unit = Player.Players[team].UnitArray[unitID];
                Structure townHall = Player.Players[team].StructureArray[townHallID];

                Resource resource;
                if (resourceID == -1)
                    resource = null;
                else
                    resource = Resource.ResourceArray[resourceID];

                Player.Players[team].ScheduledActions.Add(new ScheduledReturnCargoCommand(scheduledTime, new ReturnCargoCommand(unit, townHall, resource)));
            }
        }

        void processUnitStopCommandBatch(NetIncomingMessage msg)
        {
            float scheduledTime = msg.ReadFloat();
            short team = msg.ReadInt16();
            bool queued = msg.ReadBoolean();
            short count = msg.ReadInt16();

            for (int i = 0; i < count; i++)
            {
                short unitID = msg.ReadInt16();
                Unit unit = Player.Players[team].UnitArray[unitID];
                Player.Players[team].ScheduledActions.Add(new ScheduledUnitCommand(scheduledTime, new StopCommand(unit), queued));
            }
        }

        void processUnitHoldPositionCommandBatch(NetIncomingMessage msg)
        {
            float scheduledTime = msg.ReadFloat();
            short team = msg.ReadInt16();
            bool queued = msg.ReadBoolean();
            short count = msg.ReadInt16();

            for (int i = 0; i < count; i++)
            {
                short unitID = msg.ReadInt16();
                Unit unit = Player.Players[team].UnitArray[unitID];
                Player.Players[team].ScheduledActions.Add(new ScheduledUnitCommand(scheduledTime, new HoldPositionCommand(unit), queued));
            }
        }

        void processUnitDeath(NetIncomingMessage msg)
        {
            short unitID = msg.ReadInt16();
            short team = msg.ReadInt16();

            Unit unit = Player.Players[team].UnitArray[unitID];

            if (unit != null && !unit.IsDead)
                unit.Die();
        }

        void processStructureDeath(NetIncomingMessage msg)
        {
            short structureID = msg.ReadInt16();
            short team = msg.ReadInt16();

            Structure structure = Player.Players[team].StructureArray[structureID];

            if (!structure.IsDead)
                structure.Die();
        }

        /*void processPathUpdate(NetIncomingMessage msg)
        {
            //short commandID = msg.ReadInt16();
            short unitID = msg.ReadInt16();
            short team = msg.ReadInt16();
            Point destination = new Point(msg.ReadInt16(), msg.ReadInt16());
            short count = msg.ReadInt16();

            Unit unit = Player.Players[team].UnitArray[unitID];

            if (unit == null)
            {
                return;
            }

            foreach (UnitCommand unitCommand in unit.Commands)
            {
                MoveCommand moveCommand = unitCommand as MoveCommand;
                if (moveCommand != null)
                {
                    if ((int)moveCommand.Destination.X == destination.X && (int)moveCommand.Destination.Y == destination.Y)
                    {
                        List<Vector2> wayPoints = new List<Vector2>();

                        for (int i = 0; i < count; i++)
                        {
                            wayPoints.Add(new Vector2(msg.ReadInt16(), msg.ReadInt16()));
                        }

                        moveCommand.WayPoints = wayPoints;

                        break;
                    }
                }
            }

            /*if (commandID >= Player.Players[team].UnitCommands.Count)
            {
                List<Vector2> wayPoints = new List<Vector2>();

                for (int i = 0; i < count; i++)
                {
                    wayPoints.Add(new Vector2(msg.ReadInt16(), msg.ReadInt16()));
                }

                new DelayedPathUpdate(commandID, team, wayPoints);
            }
            else
            {
                MoveCommand moveCommand = (MoveCommand)Player.Players[team].UnitCommands[commandID];

                List<Vector2> wayPoints = new List<Vector2>();

                for (int i = 0; i < count; i++)
                {
                    wayPoints.Add(new Vector2(msg.ReadInt16(), msg.ReadInt16()));
                }

                moveCommand.WayPoints = wayPoints;
            }*/
        //}
    }
}