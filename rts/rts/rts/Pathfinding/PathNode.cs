using System;
using System.Collections.Generic;

namespace rts
{
    public class PathNode
    {
        public MapTile Tile;
        public List<PathNode> Neighbors = new List<PathNode>();
        public List<bool> IsNeighborDiagonal = new List<bool>();
        public System.Collections.Generic.HashSet<Unit> UnitsContained = new System.Collections.Generic.HashSet<Unit>();
        public bool Blocked = false;
        public BaseObject Blocker = null;

        public PathNode Parent;

        public bool InOpenList, InClosedList;

        public float DistanceToGoal, DistanceTravelled;

        public PathNode(MapTile tile)
        {
            Tile = tile;
        }
    }

    public class PathNodeFComparer : IComparer<PathNode>
    {
        public int Compare(PathNode p1, PathNode p2)
        {
            //return (int)(p1.DistanceToGoal - p2.DistanceToGoal);
            if (p1.DistanceToGoal < p2.DistanceToGoal)
                return -1;
            else if (p1.DistanceToGoal > p2.DistanceToGoal)
                return 1;
            else return 0;
        }
    }
}
