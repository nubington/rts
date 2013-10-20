using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace rts
{
    public class Direction
    {
        private static readonly
            Direction north = new Direction(270),
            south = new Direction(90),
            west = new Direction(180),
            east = new Direction(0),
            northEast = new Direction(315),
            northWest = new Direction(225),
            southEast = new Direction(45),
            southWest = new Direction(135);

        private static readonly Direction[] cardinals = new Direction[] { north, south, west, east };
        private static readonly Direction[] intercardinals = new Direction[] { northEast, northWest, southEast, southWest };
        private static readonly Direction[] directions = new Direction[] { north, south, west, east, northEast, northWest, southEast, southWest };

        float angle, x, y;

        private Direction(float angle)
        {
            this.angle = MathHelper.WrapAngle(MathHelper.ToRadians(angle));
            x = (float)Math.Cos(this.angle);
            y = (float)Math.Sin(this.angle);
        }

        // call at game initialization to load class early
        public static void Init() { }

        public float Angle
        {
            get
            {
                return angle;
            }
        }
        public float X
        {
            get
            {
                return x;
            }
        }
        public float Y
        {
            get
            {
                return y;
            }
        }

        public static Direction North
        {
            get
            {
                return north;
            }
        }
        public static Direction South
        {
            get
            {
                return south;
            }
        }
        public static Direction West
        {
            get
            {
                return west;
            }
        }
        public static Direction East
        {
            get
            {
                return east;
            }
        }
        public static Direction NorthEast
        {
            get
            {
                return northEast;
            }
        }
        public static Direction SouthEast
        {
            get
            {
                return southEast;
            }
        }
        public static Direction NorthWest
        {
            get
            {
                return northWest;
            }
        }
        public static Direction SouthWest
        {
            get
            {
                return southWest;
            }
        }
        public static Direction[] Cardinals
        {
            get
            {
                return cardinals;
            }
        }
        public static Direction[] Intercardinals
        {
            get
            {
                return intercardinals;
            }
        }
        public static Direction[] Directions
        {
            get
            {
                return directions;
            }
        }

        public override bool Equals(object o)
        {
            //if (!(o is Direction))
            //    return false;
            return angle == ((Direction)o).angle;
        }
        public override int GetHashCode()
        {
            return (int)(angle * 100);
        }

        public static bool operator ==(Direction d1, Direction d2)
        {
            return d1.Equals(d2);
        }
        public static bool operator !=(Direction d1, Direction d2)
        {
            return !d1.Equals(d2);
        }
    }
}
