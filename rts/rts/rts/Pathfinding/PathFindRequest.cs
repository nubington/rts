using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using C5;

namespace rts
{
    public class PathFindRequest
    {
        /*public static Queue<PathFindRequest> PathFindRequests = new Queue<PathFindRequest>();
        public static Object PathFindRequestsLock = new Object();*/
        //public static IntervalHeap<PathFindRequest> PathFindRequests = new IntervalHeap<PathFindRequest>(new PathFindRequestComparer());
        public static IntervalHeap<PathFindRequest> HighPriorityPathFindRequests = new IntervalHeap<PathFindRequest>(new PathFindRequestComparer());
        public static IntervalHeap<PathFindRequest> LowPriorityPathFindRequests = new IntervalHeap<PathFindRequest>(new PathFindRequestComparer());

        public static Queue<PathFindRequest> DonePathFindRequests = new Queue<PathFindRequest>();
        public static Object DonePathFindRequestsLock = new Object();

        public BaseObject Target;
        public MoveCommand Command;
        public PathNode StartNode;
        public bool AvoidUnits;
        public int Priority;

        public List<Vector2> WayPoints;
        //public Object WayPointsLock = new Object();

        public PathFindRequest(MoveCommand command, PathNode startNode, int priority, bool avoidUnits)
        {
            Command = command;
            StartNode = startNode;
            AvoidUnits = avoidUnits;
            Priority = priority;
            AttackCommand attackCommand = command as AttackCommand;
            if (attackCommand != null)
                Target = attackCommand.Target;
            HarvestCommand harvestCommand = command as HarvestCommand;
            if (harvestCommand != null)
                Target = harvestCommand.TargetResource;
            ReturnCargoCommand returnCargoCommand = command as ReturnCargoCommand;
            if (returnCargoCommand != null)
                Target = returnCargoCommand.TargetStructure;
        }
        /*public PathFindRequest(Unit unit, RtsObject attackTarget, MoveCommand command, PathNode startNode, int priority, bool avoidUnits)
            : this ( unit, command, startNode, priority, avoidUnits)
        {
            AttackTarget = attackTarget;
        }*/
    }

    public class PathFindRequestComparer : IComparer<PathFindRequest>
    {
        public int Compare(PathFindRequest p1, PathFindRequest p2)
        {
            //return (int)(p1.DistanceToGoal - p2.DistanceToGoal);
            if (p1.Priority < p2.Priority)
                return -1;
            else if (p1.Priority > p2.Priority)
                return 1;
            else return 0;
        }
    }
}
