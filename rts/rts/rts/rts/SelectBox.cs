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
    class SelectBox
    {
        static Rectangle selectPoint1, selectPoint2, drawPoint1, drawPoint2;
        static Rectangle drawBox;
        static BaseObject selectBox = new BaseObject(new Rectangle(0, 0, 1, 1));
        static bool selecting = false, enabled;

        public static void Update(Viewport viewport, Camera camera)
        {
            if (!enabled)
                return;

            //Vector2 halfOfViewPort = new Vector2(Game1.Game.GraphicsDevice.Viewport.Width / 2f, Game1.Game.GraphicsDevice.Viewport.Height / 2f);
            //Vector2 mousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y) + (cameraOffset);

            Vector2 mousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);

            Vector2 mouseWorldPosition = Vector2.Transform(mousePosition, Matrix.Invert(camera.get_transformation(viewport)));

            if (Mouse.GetState().LeftButton == ButtonState.Released)
            {
                selecting = false;
                return;
            }
            else if (selecting == false)// && graphics.GraphicsDevice.Viewport.Bounds.Contains(new Point(Mouse.GetState().X, Mouse.GetState().Y)))
            {
                selecting = true;
                selectPoint1 = selectPoint2 = new Rectangle((int)mouseWorldPosition.X, (int)mouseWorldPosition.Y, 1, 1);
                drawPoint1 = drawPoint2 = new Rectangle((int)mousePosition.X, (int)mousePosition.Y, 1, 1);
            }
            else
            {
                selectPoint2 = new Rectangle((int)mouseWorldPosition.X, (int)mouseWorldPosition.Y, 1, 1);
                drawPoint2 = new Rectangle((int)mousePosition.X, (int)mousePosition.Y, 1, 1);
            }

            /*selectBox.X = selectPoint1.X;
            selectBox.Y = selectPoint1.Y;
            selectBox.Width = (int)Vector2.Distance(new Vector2(selectPoint1.X, 0), new Vector2(selectPoint2.X, 0));
            selectBox.Height = (int)Vector2.Distance(new Vector2(0, selectPoint1.Y), new Vector2(0, selectPoint2.Y));
            selectBox.Rotation = camera.Rotation + (float)Math.PI;*/

            selectBox.Rectangle = Rectangle.Union(selectPoint1, selectPoint2);
            selectBox.Rotation = camera.Rotation + (float)Math.PI;
            drawBox = Rectangle.Union(drawPoint1, drawPoint2);
        }

        static PrimitiveLine selectBoxLine;
        public static void InitializeSelectBoxLine(GraphicsDevice graphicsDevice, Color color)
        {
            selectBoxLine = new PrimitiveLine(graphicsDevice, 1);
            selectBoxLine.Colour = color;
        }

        public static void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            if (!enabled || !selecting)
                return;

            /*selectBoxLine.ClearVectors();
            selectBoxLine.AddVector(selectBox.UpperLeftCorner);
            selectBoxLine.AddVector(selectBox.UpperRightCorner);
            selectBoxLine.Render(spriteBatch);

            selectBoxLine.ClearVectors();
            selectBoxLine.AddVector(selectBox.UpperLeftCorner);
            selectBoxLine.AddVector(selectBox.LowerLeftCorner);
            selectBoxLine.Render(spriteBatch);

            selectBoxLine.ClearVectors();
            selectBoxLine.AddVector(selectBox.LowerLeftCorner);
            selectBoxLine.AddVector(selectBox.LowerRightCorner);
            selectBoxLine.Render(spriteBatch);

            selectBoxLine.ClearVectors();
            selectBoxLine.AddVector(selectBox.UpperRightCorner);
            selectBoxLine.AddVector(selectBox.LowerRightCorner);
            selectBoxLine.Render(spriteBatch);
            */
            //

            selectBoxLine.ClearVectors();
            selectBoxLine.AddVector(new Vector2(drawBox.X, drawBox.Y));
            selectBoxLine.AddVector(new Vector2(drawBox.X + drawBox.Width, drawBox.Y));
            selectBoxLine.Render(spriteBatch);

            selectBoxLine.ClearVectors();
            selectBoxLine.AddVector(new Vector2(drawBox.X, drawBox.Y));
            selectBoxLine.AddVector(new Vector2(drawBox.X, drawBox.Y + drawBox.Height));
            selectBoxLine.Render(spriteBatch);

            selectBoxLine.ClearVectors();
            selectBoxLine.AddVector(new Vector2(drawBox.X, drawBox.Y + drawBox.Height));
            selectBoxLine.AddVector(new Vector2(drawBox.X + drawBox.Width, drawBox.Y + drawBox.Height));
            selectBoxLine.Render(spriteBatch);

            selectBoxLine.ClearVectors();
            selectBoxLine.AddVector(new Vector2(drawBox.X + drawBox.Width, drawBox.Y));
            selectBoxLine.AddVector(new Vector2(drawBox.X + drawBox.Width, drawBox.Y + drawBox.Height));
            selectBoxLine.Render(spriteBatch);
        }

        public static void Clear()
        {
            selecting = false;
        }

        public static Color Color
        {
            get
            {
                return selectBoxLine.Colour;
            }
            set
            {
                selectBoxLine.Colour = value;
            }
        }

        public static bool IsSelecting
        {
            get
            {
                return selecting;
            }
        }
        public static bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;
                if (!enabled)
                    Clear();
            }
        }
        public static BaseObject Box
        {
            get
            {
                return selectBox;
            }
        }
    }
}