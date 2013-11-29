using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace rts
{
    public class UnitCommand
    {
        public Unit Unit { get; private set; }
        public short ID;
        public bool Active = true;

        protected UnitCommand(Unit unit)
        {
            Player.Players[unit.Team].UnitCommands.Add(this);
            ID = (short)(Player.Players[unit.Team].UnitCommands.Count - 1);
            //ID = Player.Players[unit.Team].CommandIDCounter++;
            Unit = unit;
        }
    }

    public class StopCommand : UnitCommand
    {
        public StopCommand(Unit unit)
            : base(unit)
        {
        }
    }

    public class HoldPositionCommand : UnitCommand
    {
        public HoldPositionCommand(Unit unit)
            : base(unit)
        {
        }
    }

    public class MoveCommand : UnitCommand
    {
        private Vector2 destination;
        public List<Vector2> WayPoints;
        public bool Calculated;

        /*public MoveCommand(List<Vector2> wayPoints, int priority)
        {
            WayPoints = wayPoints;
            Priority = priority;
        }
        public MoveCommand(Vector2 wayPoint, int priority)
        {
            WayPoints = new List<Vector2>();
            WayPoints.Add(wayPoint);
            Priority = priority;
        }*/
        public MoveCommand(Unit unit, Vector2 destination) 
            : base(unit)
        {
            this.destination = destination;
            WayPoints = new List<Vector2>();
            WayPoints.Add(destination);
        }

        public void NextWayPoint(Unit unit, PathFinder pathFinder)
        {
            WayPoints.RemoveAt(0);
            //pathFinder.SmoothImmediatePath(WayPoints, unit);
        }

        public Vector2 Destination
        {
            get
            {
                return destination;
            }
            set
            {
                destination = value;
                WayPoints[WayPoints.Count - 1] = value;
            }
        }
    }

    public class AttackCommand : MoveCommand
    {
        public RtsObject Target { get; set; }
        bool willingToChangeTarget;
        bool holdPosition;

        /*public AttackCommand(List<Vector2> wayPoints, RtsObject target) 
            : base(wayPoints, 1)
        {
            this.target = target;
        }*/
        public AttackCommand(Unit unit, RtsObject target, bool willingToChangeTarget, bool holdPosition)
            : base(unit, target.CenterPoint)
        {
            Target = target;
            this.willingToChangeTarget = willingToChangeTarget;
            this.holdPosition = holdPosition;
        }

        public bool WillingToChangeTarget
        {
            get
            {
                return willingToChangeTarget;
            }
        }
        public bool HoldPosition
        {
            get
            {
                return holdPosition;
            }
        }
    }

    public class AttackMoveCommand : MoveCommand
    {
        public AttackMoveCommand(Unit unit, Vector2 destination)
            : base(unit, destination)
        { }
    }

    public class BuildStructureCommand : MoveCommand
    {
        public StructureType StructureType { get; private set; }
        public Point StructureLocation { get; private set; }

        public BuildStructureCommand(Unit unit, StructureType structureType, Point structureLocation, Vector2 moveDestination)
            : base(unit, moveDestination)
        {
            StructureType = structureType;
            StructureLocation = structureLocation;
        }
    }

    public class HarvestCommand : MoveCommand
    {
        public Resource TargetResource { get; private set; }

        public HarvestCommand(Unit unit, Resource targetResource)
            : base(unit, targetResource.CenterPoint)
        {
            TargetResource = targetResource;
        }
    }

    public class ReturnCargoCommand : MoveCommand
    {
        public Structure TargetStructure;
        public Resource Source { get; private set; }

        public ReturnCargoCommand(Unit unit, Structure targetStructure, Resource source)
            : base(unit, targetStructure.CenterPoint)
        {
            TargetStructure = targetStructure;
            Source = source;
        }
    }
}
