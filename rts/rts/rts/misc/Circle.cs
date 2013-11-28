using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace rts
{
    struct Circle
    {
        public float Radius, RadiusSquared, Diameter;

        public Vector2 CenterPoint;

        public Circle(Vector2 centerPoint, float diameter)
        {
            CenterPoint = centerPoint;

            Diameter = diameter;
            Radius = diameter / 2f;
            RadiusSquared = (float)Math.Pow(Radius, 2);
        }

        public bool Contains(Vector2 point)
        {
            return Vector2.DistanceSquared(CenterPoint, point) < (this.RadiusSquared);
        }

        public bool Intersects(BaseObject o)
        {
            float angle = (float)Math.Atan2(o.CenterPoint.Y - CenterPoint.Y, o.CenterPoint.X - CenterPoint.X);
            Vector2 point = CenterPoint + new Vector2(Radius * (float)Math.Cos(angle), Radius * (float)Math.Sin(angle));
            //return o.Touches(point);

            return o.Touches(point) || Vector2.Distance(CenterPoint, o.CenterPoint) < Radius;// || 
            //((Math.Abs(centerPoint.X - o.X) < radius || Math.Abs(centerPoint.X - (o.X  + o.Width)) < radius) &&
            //(Math.Abs(centerPoint.Y - o.Y) < radius || Math.Abs(centerPoint.Y - (o.Y + o.Height)) < radius));
        }
    }
}
