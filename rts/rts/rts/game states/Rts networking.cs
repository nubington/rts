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
        float timeSinceLastCheckup, checkupDelay = .5f;
        void checkToCheckup(GameTime gameTime)
        {
            timeSinceLastCheckup += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (timeSinceLastCheckup >= checkupDelay)
            {
                timeSinceLastCheckup = 0f;

                NetOutgoingMessage msg = netPeer.CreateMessage();
                msg.Write(MessageID.CHECKUP);
                netPeer.SendMessage(msg, connection, NetDeliveryMethod.ReliableUnordered);
            }
        }

        float timeSinceLastSync, syncDelay = 1f;
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
                    msg.Write(gameClock);
                    netPeer.SendMessage(msg, connection, NetDeliveryMethod.ReliableSequenced, 0);
                }
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

        float timeSinceMessageReceived, timeOutDelay = .55f;
        float currentPing;
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
                        gameClock = msg.ReadFloat() + connection.AverageRoundtripTime / 2f;
                    }
                    else if (id == MessageID.UNIT_MOVE_COMMAND_BATCH)
                    {
                        float scheduledTime = msg.ReadFloat();
                        short team = msg.ReadInt16();
                        bool queued = msg.ReadBoolean();
                        short numberOfCommands = msg.ReadInt16();

                        for (int i = 0; i < numberOfCommands; i++)
                        {
                            short unitID = msg.ReadInt16();
                            Unit unit = Player.Players[team].UnitArray[unitID];
                            if (unit != null)
                            {
                                MoveCommand moveCommand = new MoveCommand(unit, new Vector2(msg.ReadFloat(), msg.ReadFloat()), 1);
                                Player.Players[team].ScheduledActions.Add(new ScheduledUnitCommand(scheduledTime, moveCommand, queued));
                                //RtsObject.PathFinder.AddHighPriorityPathFindRequest(moveCommand, (int)Vector2.DistanceSquared(moveCommand.Unit.CenterPoint, moveCommand.Destination), false);
                            }
                        }
                    }
                    else if (id == MessageID.STRUCTURE_COMMAND)
                    {
                        float scheduledTime = msg.ReadFloat();
                        short team = msg.ReadInt16();

                        Structure structure = Player.Players[team].StructureArray[msg.ReadInt16()];
                        CommandButtonType commandType = CommandButtonType.CommandButtonTypes[msg.ReadInt16()];

                        short unitID = msg.ReadInt16();

                        Player.Players[team].ScheduledActions.Add(new ScheduledStructureCommand(scheduledTime, structure, commandType, unitID)); 
                    }
                    else if (id == MessageID.RALLY_POINT_COMMAND)
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
                            Player.Players[team].ScheduledActions.Add(new ScheduledStructureTargetedCommand(scheduledTime, structure, CommandButtonType.RallyPoint, resource, point,  queued)); 
                        }
                    }
                    else if (id == MessageID.UNIT_STATUS_UPDATE)
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
                            if (hp < unit.Hp)
                            {
                                unit.Hp = hp;
                                if (hp <= 0)
                                    unit.Die();
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
                                Vector2 expectedPosition = new Vector2(position.X + unit.Speed * currentPing / 2 * (float)Math.Cos(rotation), position.Y + unit.Speed * currentPing / 2 * (float)Math.Sin(rotation));

                                if (Vector2.Distance(expectedPosition, unit.CenterPoint) > unit.Radius / 2)
                                {
                                    unit.CenterPoint -= new Vector2((unit.CenterPoint.X - expectedPosition.X), (unit.CenterPoint.Y - expectedPosition.Y));
                                }
                            }
                        }
                    }
                    else if (id == MessageID.UNIT_HP_UPDATE)
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
                    /*else if (id == MessageID.PATH_UPDATE)
                    {
                        //short commandID = msg.ReadInt16();
                        short unitID = msg.ReadInt16();
                        short team = msg.ReadInt16();
                        Point destination = new Point(msg.ReadInt16(), msg.ReadInt16());
                        short count = msg.ReadInt16();

                        Unit unit = Player.Players[team].UnitArray[unitID];

                        if (unit == null)
                        {
                            netPeer.Recycle(msg);
                            continue;
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
                    else if (id == MessageID.UNIT_ATTACK_COMMAND_BATCH)
                    {
                        float scheduledTime = msg.ReadFloat();
                        short team = msg.ReadInt16();
                        bool queued = msg.ReadBoolean();
                        short count = msg.ReadInt16();

                        RtsObject target;
                        if (msg.ReadInt16() == 0)
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
                                //RtsObject.PathFinder.AddHighPriorityPathFindRequest(attackCommand, (int)Vector2.DistanceSquared(attackCommand.Unit.CenterPoint, attackCommand.Destination), false);
                            }
                        }
                    }
                    else if (id == MessageID.UNIT_ATTACK_MOVE_COMMAND_BATCH)
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
                                AttackMoveCommand attackMoveCommand = new AttackMoveCommand(unit, new Vector2(msg.ReadFloat(), msg.ReadFloat()), 1);
                                Player.Players[team].ScheduledActions.Add(new ScheduledUnitCommand(scheduledTime, attackMoveCommand, queued));
                                //RtsObject.PathFinder.AddHighPriorityPathFindRequest(attackMoveCommand, (int)Vector2.DistanceSquared(attackMoveCommand.Unit.CenterPoint, attackMoveCommand.Destination), false);
                            }
                        }
                    }
                    else if (id == MessageID.UNIT_BUILD_COMMAND)
                    {
                        float scheduledTime = msg.ReadFloat();
                        short team = msg.ReadInt16();

                        Unit unit = Player.Players[team].UnitArray[msg.ReadInt16()];

                        StructureType structureType = StructureType.StructureTypes[msg.ReadInt16()];
                        Point location = new Point(msg.ReadInt16(), msg.ReadInt16());
                        bool queued = msg.ReadBoolean();

                        BuildStructureCommand buildStructureCommand = new BuildStructureCommand(unit, structureType, location, new Vector2(location.X * map.TileSize + structureType.Size * map.TileSize / 2, location.Y * map.TileSize + structureType.Size * map.TileSize / 2), 1);
                        Player.Players[team].ScheduledActions.Add(new ScheduledUnitBuildCommand(scheduledTime, buildStructureCommand, queued));
                        RtsObject.PathFinder.AddHighPriorityPathFindRequest(buildStructureCommand, (int)Vector2.DistanceSquared(buildStructureCommand.Unit.CenterPoint, buildStructureCommand.Destination), false);

                    }
                    else if (id == MessageID.UNIT_HARVEST_COMMAND_BATCH)
                    {
                        float scheduledTime = msg.ReadFloat();
                        short team = msg.ReadInt16();
                        bool queued = msg.ReadBoolean();
                        short count = msg.ReadInt16();

                        for (int i = 0; i < count; i++)
                        {
                            Unit unit = Player.Players[team].UnitArray[msg.ReadInt16()];
                            Resource targetResource = Resource.ResourceArray[msg.ReadInt16()];

                            Player.Players[team].ScheduledActions.Add(new ScheduledUnitTargetedCommand(scheduledTime, new HarvestCommand(unit, targetResource, 1), targetResource, queued));
                        }
                    }
                    else if (id == MessageID.UNIT_RETURN_CARGO_COMMAND_BATCH)
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

                            Player.Players[team].ScheduledActions.Add(new ScheduledReturnCargoCommand(scheduledTime, new ReturnCargoCommand(unit, townHall, resource, 1)));
                        }
                    }
                    else if (id == MessageID.UNIT_STOP_COMMAND_BATCH)
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
                    else if (id == MessageID.UNIT_HOLD_POSITION_COMMAND_BATCH)
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
                    else if (id == MessageID.UNIT_DEATH)
                    {
                        short unitID = msg.ReadInt16();
                        short team = msg.ReadInt16();

                        Unit unit = Player.Players[team].UnitArray[unitID];

                        if (unit != null && !unit.IsDead)
                            unit.Die();
                    }
                    else if (id == MessageID.STRUCTURE_DEATH)
                    {
                        short structureID = msg.ReadInt16();
                        short team = msg.ReadInt16();

                        Structure structure = Player.Players[team].StructureArray[structureID];

                        if (!structure.IsDead)
                            structure.Die();
                    }
                }

                netPeer.Recycle(msg);
            }

            currentPing = connection.AverageRoundtripTime;
            //currentScheduleTime = gameClock + currentPing * .6f;
            currentScheduleTime = gameClock + .1f;
        }
    }

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
            UNIT_HP_UPDATE = 116
            ;
    }

    public class DelayedPathUpdate
    {
        public static List<DelayedPathUpdate> DelayedPathUpdates = new List<DelayedPathUpdate>();

        public short CommandID;
        public short Team;
        public List<Vector2> WayPoints;

        public DelayedPathUpdate(short commandID, short team, List<Vector2> wayPoints)
        {
            CommandID = commandID;
            Team = team;
            WayPoints = wayPoints;

            DelayedPathUpdates.Add(this);
        }
    }

    public abstract class ScheduledAction
    {
        public float ScheduledTime { get; private set; }
        public short Team { get; protected set; }

        public ScheduledAction(float scheduledTime)
        {
            ScheduledTime = scheduledTime;
        }
    }

    public class ScheduledUnitCommand : ScheduledAction
    {
        public UnitCommand UnitCommand { get; private set; }
        public bool Queued { get; private set; }

        public ScheduledUnitCommand(float scheduledTime, UnitCommand unitCommand, bool queued)
            : base(scheduledTime)
        {
            UnitCommand = unitCommand;
            Queued = queued;
            Team = unitCommand.Unit.Team;

            MoveCommand moveCommand = UnitCommand as MoveCommand;
            if (moveCommand != null)
            {
                Unit.PathFinder.AddHighPriorityPathFindRequest(moveCommand, (int)Vector2.DistanceSquared(moveCommand.Unit.CenterPoint, moveCommand.Destination), false);
            }
        }
    }

    public class ScheduledReturnCargoCommand : ScheduledUnitCommand
    {
        public ReturnCargoCommand ReturnCargoCommand { get; private set; }

        public ScheduledReturnCargoCommand(float scheduledTime, ReturnCargoCommand command)
            : base(scheduledTime, command, false)
        {
            ReturnCargoCommand = command;
        }
    }

    public class ScheduledUnitTargetedCommand : ScheduledUnitCommand
    {
        public BaseObject Target { get; private set; }

        public ScheduledUnitTargetedCommand(float scheduledTime, UnitCommand unitCommand, BaseObject target, bool queued)
            : base(scheduledTime, unitCommand, queued)
        {
            Target = target;
        }
    }

    public class ScheduledUnitBuildCommand : ScheduledUnitCommand
    {
        public Point Location { get; private set; }
        public StructureType Type { get; private set; }

        public ScheduledUnitBuildCommand(float scheduledTime, BuildStructureCommand command, bool queued)
            : base(scheduledTime, command, queued)
        {
            Location = command.StructureLocation;
            Type = command.StructureType;
        }
    }

    public class ScheduledStructureCommand : ScheduledAction
    {
        public Structure Structure { get; private set; }
        public CommandButtonType CommandType;
        public short ID { get; private set; }

        public ScheduledStructureCommand(float scheduledTime, Structure structure, CommandButtonType commandType)
            : base(scheduledTime)
        {
            Structure = structure;
            CommandType = commandType;
        }

        public ScheduledStructureCommand(float scheduledTime, Structure structure, CommandButtonType commandType, short id)
            : base(scheduledTime)
        {
            Structure = structure;
            CommandType = commandType;
            ID = id;
        }
    }

    public class ScheduledStructureTargetedCommand : ScheduledStructureCommand
    {
        public BaseObject Target { get; private set; }
        public Vector2 Point { get; private set; }
        public bool Queued { get; private set; }

        public ScheduledStructureTargetedCommand(float scheduledTime, Structure structure, CommandButtonType commandType, BaseObject target, Vector2 point, bool queued)
            : base(scheduledTime, structure, commandType)
        {
            Target = target;
            Point = point;
            Queued = queued;
        }
    }
}