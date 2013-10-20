using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace rts
{
    public class BaseObject
    {
        public static GraphicsDeviceManager graphics = Game1.Game.Graphics;
        protected static Random rand = new Random();

        public List<BaseObject> potentialCollisions = new List<BaseObject>();

        protected Rectangle rectangle;
        protected int greaterOfWidthAndHeight;
        private float rotation;
        public Vector2 Origin;
        private Vector2 upperLeftCorner, upperRightCorner, lowerLeftCorner, lowerRightCorner;

        private Texture2D texture;
        public Vector2 speed;
        protected Vector2 lastMove;
        protected Vector2 centerPoint, textureCenterOrigin;
        protected float preciseX, preciseY;
        public int maxX, minX, maxY, minY;

        public BaseObject(Rectangle rectangle, Vector2 speed)
        {
            Rectangle = rectangle;
            preciseX = rectangle.X;
            preciseY = rectangle.Y;
            this.speed = speed;
            maxX = graphics.GraphicsDevice.Viewport.Width - rectangle.Width;
            minX = 0;
            maxY = graphics.GraphicsDevice.Viewport.Height - rectangle.Height;
            minY = 0;
            Rotation = 0f;
            Origin = new Vector2(Width / 2, Height / 2);
            centerPoint = new Vector2(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2);
            greaterOfWidthAndHeight = (rectangle.Width > rectangle.Height ? rectangle.Width : rectangle.Height);
            //CalculateCorners();
        }
        public BaseObject(Rectangle rectangle, float rotation)
            : this(rectangle, new Vector2(0, 0))
        {
            Rotation = rotation;
        }
        public BaseObject(Rectangle rectangle)
            : this(rectangle, 0f)
        { }

        public int X
        {
            set
            {
                rectangle.X = value;
                preciseX = value;
                centerPoint.X = rectangle.X + rectangle.Width / 2;
            }
            get
            {
                return rectangle.X;
            }
        }
        public int Y
        {
            set
            {
                rectangle.Y = value;
                preciseY = value;
                centerPoint.Y = rectangle.Y + rectangle.Height / 2;
            }
            get
            {
                return rectangle.Y;
            }
        }
        public float PreciseX
        {
            get
            {
                return preciseX;
            }
            set
            {
                preciseX = value;
                //rectangle.X = (int)Math.Round(preciseX);
                rectangle.X = (int)preciseX;
                centerPoint.X = preciseX + rectangle.Width / 2;
            }
        }
        public float PreciseY
        {
            get
            {
                return preciseY;
            }
            set
            {
                preciseY = value;
                //rectangle.Y = (int)Math.Round(preciseY);
                rectangle.Y = (int)preciseY;
                centerPoint.Y = preciseY + rectangle.Height / 2;
            }
        }
        public Vector2 Position
        {
            get
            {
                return new Vector2(X, Y);
            }
            set
            {
                rectangle.X = (int)Math.Round(value.X);
                rectangle.Y = (int)Math.Round(value.Y);
                preciseX = value.X;
                preciseY = value.Y;
                centerPoint.X = rectangle.X + rectangle.Width / 2;
                centerPoint.Y = rectangle.Y + rectangle.Height / 2;
            }
        }
        public Vector2 PrecisePosition
        {
            get
            {
                return new Vector2(PreciseX, PreciseY);
            }
            set
            {
                preciseX = value.X;
                preciseY = value.Y;
                //rectangle.X = (int)Math.Round(value.X);
                //rectangle.Y = (int)Math.Round(value.Y);
                rectangle.X = (int)preciseX;
                rectangle.Y = (int)preciseY;
                centerPoint.X = value.X + rectangle.Width / 2;
                centerPoint.Y = value.Y + rectangle.Height / 2;
            }
        }
        public Vector2 LastMove
        {
            get
            {
                return lastMove;
            }
            set
            {
                lastMove = value;
            }
        }
        public bool IsMoving
        {
            get
            {
                return (lastMove.X != 0 || lastMove.Y != 0);
            }
        }
        public Rectangle Rectangle
        {
            set
            {
                rectangle = value;
                preciseX = X;
                preciseY = Y;
                centerPoint.X = rectangle.X + rectangle.Width / 2;
                centerPoint.Y = rectangle.Y + rectangle.Height / 2;
                greaterOfWidthAndHeight = (rectangle.Width > rectangle.Height ? rectangle.Width : rectangle.Height);
            }
            get
            {
                return rectangle;
            }
        }
        public int Width
        {
            get
            {
                return rectangle.Width;
            }
            set
            {
                rectangle.Width = (int)MathHelper.Max(value, 0);
                greaterOfWidthAndHeight = (rectangle.Width > rectangle.Height ? rectangle.Width : rectangle.Height);
                maxX = graphics.GraphicsDevice.Viewport.Width - rectangle.Width;
                centerPoint.X = rectangle.X + rectangle.Width / 2;
                Origin.X = rectangle.Width / 2;
            }
        }
        public int Height
        {
            get
            {
                return rectangle.Height;
            }
            set
            {
                rectangle.Height = (int)MathHelper.Max(value, 0);
                greaterOfWidthAndHeight = (rectangle.Width > rectangle.Height ? rectangle.Width : rectangle.Height);
                maxY = graphics.GraphicsDevice.Viewport.Height - rectangle.Height;
                centerPoint.Y = rectangle.Y + rectangle.Height / 2;
                Origin.Y = rectangle.Height / 2;
            }
        }
        public int GreaterOfWidthAndHeight
        {
            get
            {
                return greaterOfWidthAndHeight;
            }
        }
        public virtual Texture2D Texture
        {
            get
            {
                return texture;
            }
            set
            {
                texture = value;
                textureCenterOrigin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            }
        }
        public Vector2 CenterPoint
        {
            get
            {
                return centerPoint;
            }
            set
            {
                centerPoint = value;
                preciseX = centerPoint.X - rectangle.Width / 2;
                preciseY = centerPoint.Y - rectangle.Height / 2;
                rectangle.X = (int)(preciseX + .5f);
                rectangle.Y = (int)(preciseY + .5f);
            }
        }
        public float CenterPointX
        {
            get
            {
                return centerPoint.X;
            }
            set
            {
                centerPoint.X = value;
                preciseX = centerPoint.X - rectangle.Width / 2;
                rectangle.X = (int)(preciseX + .5f);
            }
        }
        public float CenterPointY
        {
            get
            {
                return centerPoint.Y;
            }
            set
            {
                centerPoint.Y = value;
                preciseY = centerPoint.Y - rectangle.Height / 2;
                rectangle.Y = (int)(preciseY + .5f);
            }
        }
        public float Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                //rotation = MathHelper.WrapAngle(value);
                rotation = Util.ConvertToPositiveRadians(value);
            }
        }
        public Vector2 UpperLeftCorner
        {
            get
            {
                return upperLeftCorner;
            }
        }
        public Vector2 UpperRightCorner
        {
            get
            {
                return upperRightCorner;
            }
        }
        public Vector2 LowerLeftCorner
        {
            get
            {
                return lowerLeftCorner;
            }
        }
        public Vector2 LowerRightCorner
        {
            get
            {
                return lowerRightCorner;
            }
        }
        public virtual float LeftBound
        {
            get
            {
                return centerPoint.X - greaterOfWidthAndHeight;
            }
        }
        public virtual float RightBound
        {
            get
            {
                return centerPoint.X + greaterOfWidthAndHeight;
            }
        }
        public virtual float TopBound
        {
            get
            {
                return centerPoint.Y - greaterOfWidthAndHeight;
            }
        }
        public virtual float BottomBound
        {
            get
            {
                return centerPoint.Y + greaterOfWidthAndHeight;
            }
        }
        public Vector2 TextureCenterOrigin
        {
            get
            {
                return textureCenterOrigin;
            }
        }
        public static implicit operator Rectangle(BaseObject o)
        {
            return o.Rectangle;
        }

        List<int> aRectangleAScalars = new List<int>();
        List<int> aRectangleBScalars = new List<int>();
        private bool IsAxisCollision(BaseObject theRectangle, Vector2 aAxis)
        {
            //List<int> aRectangleAScalars = new List<int>();
            aRectangleAScalars.Clear();
            aRectangleAScalars.Add(GenerateScalar(theRectangle.UpperLeftCorner, aAxis));
            aRectangleAScalars.Add(GenerateScalar(theRectangle.UpperRightCorner, aAxis));
            aRectangleAScalars.Add(GenerateScalar(theRectangle.LowerLeftCorner, aAxis));
            aRectangleAScalars.Add(GenerateScalar(theRectangle.LowerRightCorner, aAxis));

            //List<int> aRectangleBScalars = new List<int>();
            aRectangleBScalars.Clear();
            aRectangleBScalars.Add(GenerateScalar(UpperLeftCorner, aAxis));
            aRectangleBScalars.Add(GenerateScalar(UpperRightCorner, aAxis));
            aRectangleBScalars.Add(GenerateScalar(LowerLeftCorner, aAxis));
            aRectangleBScalars.Add(GenerateScalar(LowerRightCorner, aAxis));

            int aRectangleAMinimum = aRectangleAScalars.Min();
            int aRectangleAMaximum = aRectangleAScalars.Max();
            int aRectangleBMinimum = aRectangleBScalars.Min();
            int aRectangleBMaximum = aRectangleBScalars.Max();

            if (aRectangleBMinimum <= aRectangleAMaximum && aRectangleBMaximum >= aRectangleAMaximum)
            {
                return true;
            }
            else if (aRectangleAMinimum <= aRectangleBMaximum && aRectangleAMaximum >= aRectangleBMaximum)
            {
                return true;
            }

            return false;
        }
        private bool IsAxisCollision(Vector2 v, Vector2 aAxis)
        {
            //List<int> aRectangleAScalars = new List<int>();
            aRectangleAScalars.Clear();
            aRectangleAScalars.Add(GenerateScalar(v, aAxis));
            aRectangleAScalars.Add(GenerateScalar(v, aAxis));
            aRectangleAScalars.Add(GenerateScalar(v, aAxis));
            aRectangleAScalars.Add(GenerateScalar(v, aAxis));

            //List<int> aRectangleBScalars = new List<int>();
            aRectangleBScalars.Clear();
            aRectangleBScalars.Add(GenerateScalar(UpperLeftCorner, aAxis));
            aRectangleBScalars.Add(GenerateScalar(UpperRightCorner, aAxis));
            aRectangleBScalars.Add(GenerateScalar(LowerLeftCorner, aAxis));
            aRectangleBScalars.Add(GenerateScalar(LowerRightCorner, aAxis));

            int aRectangleAMinimum = aRectangleAScalars.Min();
            int aRectangleAMaximum = aRectangleAScalars.Max();
            int aRectangleBMinimum = aRectangleBScalars.Min();
            int aRectangleBMaximum = aRectangleBScalars.Max();

            if (aRectangleBMinimum <= aRectangleAMaximum && aRectangleBMaximum >= aRectangleAMaximum)
            {
                return true;
            }
            else if (aRectangleAMinimum <= aRectangleBMaximum && aRectangleAMaximum >= aRectangleBMaximum)
            {
                return true;
            }

            return false;
        }

        Vector2 aCornerProjected = new Vector2();
        private int GenerateScalar(Vector2 theRectangleCorner, Vector2 theAxis)
        {
            float aNumerator = (theRectangleCorner.X * theAxis.X) + (theRectangleCorner.Y * theAxis.Y);
            float aDenominator = (theAxis.X * theAxis.X) + (theAxis.Y * theAxis.Y);
            float aDivisionResult = aNumerator / aDenominator;
            //Vector2 aCornerProjected = new Vector2(aDivisionResult * theAxis.X, aDivisionResult * theAxis.Y);
            aCornerProjected.X = aDivisionResult * theAxis.X;
            aCornerProjected.Y = aDivisionResult * theAxis.Y;

            float aScalar = (theAxis.X * aCornerProjected.X) + (theAxis.Y * aCornerProjected.Y);
            return (int)aScalar;
        }
        Vector2 aTranslatedPoint = new Vector2();
        private Vector2 RotatePoint(Vector2 thePoint, Vector2 theOrigin, float theRotation)
        {
            aTranslatedPoint.X = (float)(theOrigin.X + (thePoint.X - theOrigin.X) * Math.Cos(theRotation)
                - (thePoint.Y - theOrigin.Y) * Math.Sin(theRotation));
            aTranslatedPoint.Y = (float)(theOrigin.Y + (thePoint.Y - theOrigin.Y) * Math.Cos(theRotation)
                + (thePoint.X - theOrigin.X) * Math.Sin(theRotation));
            return aTranslatedPoint;
        }

        Vector2 aUpperLeft = new Vector2();
        public Vector2 CalculateUpperLeftCorner()
        {
            aUpperLeft.X = rectangle.Left;
            aUpperLeft.Y = rectangle.Top;
            //Vector2 aUpperLeft = new Vector2(Rectangle.Left, Rectangle.Top);
            aUpperLeft = RotatePoint(aUpperLeft, aUpperLeft + Origin, Rotation);
            return aUpperLeft;
        }
        Vector2 aUpperRight = new Vector2();
        public Vector2 CalculateUpperRightCorner()
        {
            aUpperRight.X = rectangle.Right;
            aUpperRight.Y = rectangle.Top;
            //Vector2 aUpperRight = new Vector2(Rectangle.Right, Rectangle.Top);
            aUpperRight = RotatePoint(aUpperRight, aUpperRight + new Vector2(-Origin.X, Origin.Y), Rotation);
            return aUpperRight;
        }
        Vector2 aLowerLeft = new Vector2();
        public Vector2 CalculateLowerLeftCorner()
        {
            aLowerLeft.X = rectangle.Left;
            aLowerLeft.Y = rectangle.Bottom;
            //Vector2 aLowerLeft = new Vector2(Rectangle.Left, Rectangle.Bottom);
            aLowerLeft = RotatePoint(aLowerLeft, aLowerLeft + new Vector2(Origin.X, -Origin.Y), Rotation);
            return aLowerLeft;
        }
        Vector2 aLowerRight = new Vector2();
        public Vector2 CalculateLowerRightCorner()
        {
            aLowerRight.X = rectangle.Right;
            aLowerRight.Y = rectangle.Bottom;
            //Vector2 aLowerRight = new Vector2(Rectangle.Right, Rectangle.Bottom);
            aLowerRight = RotatePoint(aLowerRight, aLowerRight + new Vector2(-Origin.X, -Origin.Y), Rotation);
            return aLowerRight;
        }
        public void CalculateCorners()
        {
            upperLeftCorner = CalculateUpperLeftCorner();
            upperRightCorner = CalculateUpperRightCorner();
            lowerLeftCorner = CalculateLowerLeftCorner();
            lowerRightCorner = CalculateLowerRightCorner();
        }

        public bool Touches(Vector2 point)
        {
            return (point.X >= preciseX && point.X <= preciseX + rectangle.Width &&
                point.Y >= preciseY && point.Y <= preciseY + rectangle.Height);
        }
        public bool TouchesRotated(Vector2 v)
        {
            List<Vector2> aRectangleAxis = new List<Vector2>();
            aRectangleAxis.Add(UpperRightCorner - UpperLeftCorner);
            aRectangleAxis.Add(UpperRightCorner - LowerRightCorner);
            aRectangleAxis.Add(v - v);
            aRectangleAxis.Add(v - v);

            foreach (Vector2 aAxis in aRectangleAxis)
            {
                if (!IsAxisCollision(v, aAxis))
                {
                    return false;
                }
            }

            return true;
        }

        public bool Intersects(Rectangle theRectangle)
        {
            BaseObject o = new BaseObject(theRectangle, 0.0f);
            o.CalculateCorners();
            return Intersects(o);
        }
        public virtual bool Intersects(BaseObject theRectangle)
        {
            List<Vector2> aRectangleAxis = new List<Vector2>();
            aRectangleAxis.Add(UpperRightCorner - UpperLeftCorner);
            aRectangleAxis.Add(UpperRightCorner - LowerRightCorner);
            aRectangleAxis.Add(theRectangle.UpperLeftCorner - theRectangle.LowerLeftCorner);
            aRectangleAxis.Add(theRectangle.UpperLeftCorner - theRectangle.UpperRightCorner);

            foreach (Vector2 aAxis in aRectangleAxis)
            {
                if (!IsAxisCollision(theRectangle, aAxis))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsNear(Vector2 point)
        {
            Rectangle inflatedRectangle = new Rectangle(X, Y, Width, Height);
            inflatedRectangle.Inflate((int)(Width), (int)(Height));
            return inflatedRectangle.Contains((int)point.X, (int)point.Y);
        }
        public bool IsNear(Vector2 point, float factor)
        {
            /*int factorWidth = (int)(this.Width * factor),
                factorHeight = (int)(this.Height * factor);
            return (point.X > this.X - factorWidth && point.X < this.X + this.Width + factorWidth &&
                point.Y > this.Y - factorHeight && point.Y < this.Y + this.Height + factorHeight);*/
            Rectangle inflatedRectangle = new Rectangle(X, Y, Width, Height);
            inflatedRectangle.Inflate((int)(Width * factor), (int)(Height * factor));
            return inflatedRectangle.Contains((int)point.X, (int)point.Y);
        }
        public bool IsNear(Rectangle r, float factor)
        {
            /*Vector2 point1 = new Vector2(r.X, r.Y);
            Vector2 point2 = new Vector2(r.X + r.Width, r.Y);
            Vector2 point3 = new Vector2(r.X + r.Width, r.Y + r.Height);
            Vector2 point4 = new Vector2(r.X, r.Y + r.Height);
            return IsNear(point1, factor) || IsNear(point2, factor) || IsNear(point3, factor) || IsNear(point4, factor);*/

            //int adjustedWidth = (int)(this.Width + this.Width * factor * 2);
            //int adjustedHeight = (int)(this.Width + this.Height * factor * 2);
            //return new Rectangle((int)this.CenterPoint.X - adjustedWidth / 2, (int)this.CenterPoint.Y - adjustedHeight / 2, adjustedWidth, adjustedHeight).Intersects(r);
            Rectangle inflatedRectangle = new Rectangle(X, Y, Width, Height);
            inflatedRectangle.Inflate((int)(Width * factor), (int)(Height * factor));
            return inflatedRectangle.Intersects(r);
        }
        public bool IsNearRotated(BaseObject r, float factor)
        {
            Rectangle inflatedRectangle = new Rectangle(X, Y, Width, Height);
            inflatedRectangle.Inflate((int)(Width * factor), (int)(Height * factor));
            return r.Intersects(inflatedRectangle);
        }

        public void move(GameTime gameTime, bool restrictToScreen)
        {
            move(speed, gameTime, restrictToScreen);
        }
        public void move(float angle, GameTime gameTime, bool restrictToScreen)
        {
            move(new Vector2(speed.X * (float)Math.Cos(angle), speed.Y * (float)Math.Sin(angle)), gameTime, restrictToScreen);
        }
        public void move(Vector2 movement, GameTime gameTime, bool restrictToScreen)
        {
            lastMove.X = (float)Math.Round(Util.ScaleWithGameTime(movement.X, gameTime));
            lastMove.Y = (float)Math.Round(Util.ScaleWithGameTime(movement.Y, gameTime));

            Position += lastMove;

            if (restrictToScreen)
            {
                X = (int)MathHelper.Clamp(X, minX, maxX);
                Y = (int)MathHelper.Clamp(Y, minY, maxY);
            }
        }

        public void movePrecise(GameTime gameTime, bool restrictToScreen)
        {
            movePrecise(speed, gameTime, restrictToScreen);
        }
        public void movePrecise(float angle, GameTime gameTime, bool restrictToScreen)
        {
            movePrecise(new Vector2(speed.X * (float)Math.Cos(angle), speed.Y * (float)Math.Sin(angle)), gameTime, restrictToScreen);
        }
        public void movePrecise(Vector2 movement, GameTime gameTime, bool restrictToScreen)
        {
            lastMove.X = Util.ScaleWithGameTime(movement.X, gameTime);
            lastMove.Y = Util.ScaleWithGameTime(movement.Y, gameTime);

            PrecisePosition += lastMove;

            if (restrictToScreen)
            {
                PreciseX = MathHelper.Clamp(PreciseX, minX, maxX);
                PreciseY = MathHelper.Clamp(PreciseY, minY, maxY);
            }
        }

        public void moveTowards(Vector2 destination, GameTime gameTime, bool restrictToScreen)
        {
            lastMove.X = Util.ScaleWithGameTime(speed.X, gameTime);
            lastMove.Y = Util.ScaleWithGameTime(speed.Y, gameTime);

            Vector2 difference = destination - CenterPoint;
            if (Math.Abs(difference.X) < lastMove.X && Math.Abs(difference.Y) < lastMove.Y)
            {
                this.CenterPoint = destination;
                return;
            }

            float angle = (float)Math.Atan2((double)(destination.Y - CenterPoint.Y), (double)(destination.X - CenterPoint.X));

            lastMove.X *= (float)Math.Cos(angle);
            lastMove.Y *= (float)Math.Sin(angle);

            Position += lastMove;

            if (restrictToScreen)
            {
                X = (int)MathHelper.Clamp(X, minX, maxX);
                Y = (int)MathHelper.Clamp(Y, minY, maxY);
            }
        }
        public void moveTowardsPrecise(Vector2 destination, GameTime gameTime, bool restrictToScreen)
        {
            lastMove.X = Util.ScaleWithGameTime(speed.X, gameTime);
            lastMove.Y = Util.ScaleWithGameTime(speed.Y, gameTime);

            Vector2 difference = destination - CenterPoint;
            if (difference == Vector2.Zero)
                return;
            if (Math.Abs(difference.X) < lastMove.X && Math.Abs(difference.Y) < lastMove.Y)
            {
                this.CenterPoint = destination;
                return;
            }

            float angle = (float)Math.Atan2((double)(destination.Y - CenterPoint.Y), (double)(destination.X - CenterPoint.X));

            lastMove.X *= (float)Math.Cos(angle);
            lastMove.Y *= (float)Math.Sin(angle);

            PrecisePosition += lastMove;

            if (restrictToScreen)
            {
                PreciseX = MathHelper.Clamp(PreciseX, minX, maxX);
                PreciseY = MathHelper.Clamp(PreciseY, minY, maxY);
            }
        }
        public void moveTowardsWeird(Vector2 destination, GameTime gameTime, bool restrictToScreen)
        {
            Vector2 difference = destination - new Vector2(CenterPoint.X, CenterPoint.Y);

            float ratio = Math.Abs(difference.X) / Math.Abs(difference.Y);

            int moveX = (int)Math.Round(Util.ScaleWithGameTime(speed.X, gameTime));
            int moveY = (int)Math.Round(Util.ScaleWithGameTime(speed.Y, gameTime));

            if (Math.Abs(difference.X) < moveX && Math.Abs(difference.Y) < moveY)
            {
                this.CenterPoint = destination;
                return;
            }
            else if (Math.Abs(difference.X) < moveX)
            {
                moveX = 0;
                if (difference.Y < 0)
                    moveY = -moveY;
            }
            else if (Math.Abs(difference.Y) < moveY)
            {
                moveY = 0;
                if (difference.X < 0)
                    moveX = -moveX;
            }
            else if (ratio > 1.0f)
            {
                moveY = (int)MathHelper.Min(moveX / ratio, moveY);

                if (difference.X < 0)
                    moveX = -moveX;
                if (difference.Y < 0)
                    moveY = -moveY;
            }
            else if (ratio < 1.0f)
            {
                moveX = (int)MathHelper.Min(moveX * ratio, moveX);

                if (difference.X < 0)
                    moveX = -moveX;
                if (difference.Y < 0)
                    moveY = -moveY;
            }
            else
            {
                if (difference.X < 0)
                    moveX = -moveX;
                if (difference.Y < 0)
                    moveY = -moveY;
            }

            X += moveX;
            Y += moveY;

            if (restrictToScreen)
            {
                X = (int)MathHelper.Clamp(X, minX, maxX);
                Y = (int)MathHelper.Clamp(Y, minY, maxY);
            }
        }

        public bool turnTowards(Vector2 target, float turnSpeed, GameTime gameTime)
        {
            return turnTowards(target, centerPoint, turnSpeed, gameTime);
        }
        public bool turnTowards(Vector2 target, Vector2 origin, float turnSpeed, GameTime gameTime)
        {
            float targetAngle = (float)Math.Atan2(target.Y - origin.Y, target.X - origin.X);

            if (Rotation == targetAngle)
                return true;
            
            //float targetX = (float)Math.Cos(targetAngle);
            //float targetY = (float)Math.Sin(targetAngle);

            Vector3 oldAngleVector = new Vector3((float)Math.Cos(Rotation), (float)Math.Sin(Rotation), 0);
            //Vector3 newAngleVector = new Vector3(targetX, targetY, 0);
            Vector3 newAngleVector = new Vector3(target.X - origin.X, target.Y - origin.Y, 0);

            Vector3 crossProduct = Vector3.Cross(oldAngleVector, newAngleVector);

            float actualTurnSpeed = Util.ScaleWithGameTime(turnSpeed, gameTime);

            if (crossProduct.Z > 0)
                Rotation += actualTurnSpeed;
            else if (crossProduct.Z < 0)
                Rotation -= actualTurnSpeed;
            else
            {
                int n = rand.Next(2);
                if (n == 0)
                    Rotation += actualTurnSpeed;
                else
                    Rotation -= actualTurnSpeed;
            }

            if (Util.AngleDifference(Rotation, targetAngle) < actualTurnSpeed)
            {
                Rotation = targetAngle;
                return true;
            }

            return false;
        }

        public void Shrink(int x, int y)
        {
            rectangle.Width = (int)Math.Max(0, Width - x);
            rectangle.Height = (int)Math.Max(0, Height - y);
            CenterPoint = centerPoint;
        }
        public void Grow(int x, int y)
        {
            rectangle.Width += x;
            rectangle.Height += y;
            CenterPoint = centerPoint;
        }
        public void Grow(int x, int y, int maxX, int maxY)
        {
            rectangle.Width = (int)Math.Min(maxX, Width + x);
            rectangle.Height = (int)Math.Min(maxY, Height + y);
            CenterPoint = centerPoint;
        }

        public bool IsOffScreen(Viewport viewport, Camera camera)
        {
            if (camera == null)
                return (X < 0 - Width || X > viewport.Width || Y < 0 - Height || Y > viewport.Height);
            else
            {
                Vector2 screenPosition = Vector2.Transform(PrecisePosition, camera.get_transformation(viewport));
                return (screenPosition.X < 0 - Width || screenPosition.X > viewport.Width ||
                    screenPosition.Y < 0 - Height || screenPosition.Y > viewport.Height);
            }
        }

        //public static Object PotentialCollisionsLock = new Object();
        public List<BaseObject> PotentialCollisions
        {
            get
            {
                //lock (PotentialCollisionsLock)
                //{
                return potentialCollisions;
                //}
            }
        }
        public void AddPotentialCollision(BaseObject o)
        {
            //lock (PotentialCollisions)
            //{
                potentialCollisions.Add(o);
            //}
        }
        public void ClearPotentialCollisions()
        {
            //lock (PotentialCollisions)
            //{
                potentialCollisions.Clear();
            //}
        }
    }

    /*class BaseObjectPair
    {
        BaseObject o1, o2;

        public BaseObjectPair(BaseObject o1, BaseObject o2)
        {
            this.o1 = o1;
            this.o2 = o2;
        }

        public BaseObject O1
        {
            get
            {
                return o1;
            }
            set
            {
                o1 = value;
            }
        }
        public BaseObject O2
        {
            get
            {
                return o2;
            }
            set
            {
                o2 = value;
            }
        }

        public bool Equals(object o)
        {
            return (o is BaseObjectPair && Equals((BaseObjectPair)o));
        }
        public bool Equals(BaseObjectPair b)
        {
            return (o1 == b.o1 && o2 == b.o2) || (o1 == b.o2 && o2 == b.o1);
        }
        public static bool operator ==(BaseObjectPair b1, BaseObjectPair b2)
        {
            return b1.Equals(b2);
        }
        public static bool operator !=(BaseObjectPair b1, BaseObjectPair b2)
        {
            return !b1.Equals(b2);
        }
    }*/
}