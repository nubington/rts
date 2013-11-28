using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace rts
{
    public class MapTile : BaseObject
    {
        new public readonly int X, Y;
        public readonly int Type;
        public bool Walkable;
        public readonly float CollisionRadius;
        public bool Visible;
        public bool Revealed;
        public BoundingBox BoundingBox;

        public List<MapTile> Neighbors = new List<MapTile>();

        public MapTile(int x, int y, int width, int height, int typeCode, int pathingCode)
            : base(new Rectangle(x * width, y * height, width, height))
        {
            X = x;
            Y = y;
            Type = typeCode;
            Walkable = (pathingCode == 0);
            CollisionRadius = width / 2f;
        }

        public bool IntersectsUnit(Unit u)
        {
            return Vector2.Distance(centerPoint, u.CenterPoint) < (CollisionRadius + u.Radius);
        }
    }
}
