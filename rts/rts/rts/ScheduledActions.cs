using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace rts
{
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
}
