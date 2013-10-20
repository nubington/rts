using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace rts
{
    public class PrimitiveLine
    {
        Texture2D pixel;
        List<Vector2> vectors;

        /// <summary>
        /// Gets/sets the colour of the primitive line object.
        /// </summary>
        public Color Colour;

        public float Alpha = 1f;

        /// <summary>
        /// Gets/sets the position of the primitive line object.
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// Gets/sets the render depth of the primitive line object (0 = front, 1 = back)
        /// </summary>
        public float Depth;

        /// <summary>
        /// Gets the number of vectors which make up the primtive line object.
        /// </summary>
        public int CountVectors
        {
            get
            {
                return vectors.Count;
            }
        }

        public int Size;

        /// <summary>
        /// Creates a new primitive line object.
        /// </summary>
        /// <param name="graphicsDevice">The Graphics Device object to use.</param>
        public PrimitiveLine(GraphicsDevice graphicsDevice, int size)
        {
            this.Size = size;
            // create pixels
            pixel = new Texture2D(graphicsDevice, 1, 1, true, SurfaceFormat.Color);
            Color[] pixels = new Color[1];
            pixels[0] = Color.White;
            //for (int i = 0; i < pixels.Length; i++)
            //pixels[i] = Color.White;
            pixel.SetData<Color>(pixels);

            Colour = Color.White;
            Position = new Vector2(0, 0);
            Depth = 0;

            vectors = new List<Vector2>();
        }

        /// <summary>
        /// Called when the primive line object is destroyed.
        /// </summary>
        ~PrimitiveLine()
        {
        }

        /// <summary>
        /// Adds a vector to the primive live object.
        /// </summary>
        /// <param name="vector">The vector to add.</param>
        public void AddVector(Vector2 vector)
        {
            vectors.Add(vector);
        }

        /// <summary>
        /// Insers a vector into the primitive line object.
        /// </summary>
        /// <param name="index">The index to insert it at.</param>
        /// <param name="vector">The vector to insert.</param>
        public void InsertVector(int index, Vector2 vector)
        {
            vectors.Insert(index, vector);
        }

        /// <summary>
        /// Removes a vector from the primitive line object.
        /// </summary>
        /// <param name="vector">The vector to remove.</param>
        public void RemoveVector(Vector2 vector)
        {
            vectors.Remove(vector);
        }

        /// <summary>
        /// Removes a vector from the primitive line object.
        /// </summary>
        /// <param name="index">The index of the vector to remove.</param>
        public void RemoveVector(int index)
        {
            vectors.RemoveAt(index);
        }

        /// <summary>
        /// Clears all vectors from the primitive line object.
        /// </summary>
        public void ClearVectors()
        {
            vectors.Clear();
        }

        /// <summary>
        /// Renders the primtive line object.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use to render the primitive line object.</param>
        /// public void Render(SpriteBatch spriteBatch, float zoom)
        public void Render(SpriteBatch spriteBatch)
        {
            if (vectors.Count < 2)
                return;

            for (int i = 1; i < vectors.Count; i++)
            {
                Vector2 vector1 = vectors[i - 1];
                Vector2 vector2 = vectors[i];

                // calculate the distance between the two vectors
                float distance = Vector2.Distance(vector1, vector2);

                // calculate the angle between the two vectors
                float angle = (float)Math.Atan2((double)(vector2.Y - vector1.Y),
                    (double)(vector2.X - vector1.X));

                // stretch the pixel between the two vectors
                spriteBatch.Draw(pixel,
                    Position + vector1,
                    //new Vector2(vector1.X, vector1.Y),
                    null,
                    Colour * Alpha,
                    angle,
                    //Vector2.Zero,
                    new Vector2(0, 1),
                    new Vector2(distance, Size),
                    SpriteEffects.None,
                    Depth);
            }
        }
        public void RenderWithZoom(SpriteBatch spriteBatch, float zoom)
        {
            if (vectors.Count < 2)
                return;

            int size = (int)MathHelper.Max(Size * 1.5f / zoom, 1f);

            for (int i = 1; i < vectors.Count; i++)
            {
                Vector2 vector1 = vectors[i - 1];
                Vector2 vector2 = vectors[i];

                // calculate the distance between the two vectors
                float distance = Vector2.Distance(vector1, vector2);

                // calculate the angle between the two vectors
                float angle = (float)Math.Atan2((double)(vector2.Y - vector1.Y),
                    (double)(vector2.X - vector1.X));

                // stretch the pixel between the two vectors
                spriteBatch.Draw(pixel,
                    Position + vector1,
                    //new Vector2(vector1.X, vector1.Y),
                    null,
                    Colour * Alpha,
                    angle,
                    //Vector2.Zero,
                    new Vector2(0, 1),
                    new Vector2(distance, size),
                    SpriteEffects.None,
                    Depth);
            }
        }
        public void RenderWithAlpha(SpriteBatch spriteBatch, float alpha)
        {
            if (vectors.Count < 2)
                return;

            for (int i = 1; i < vectors.Count; i++)
            {
                Vector2 vector1 = vectors[i - 1];
                Vector2 vector2 = vectors[i];

                // calculate the distance between the two vectors
                float distance = Vector2.Distance(vector1, vector2);

                // calculate the angle between the two vectors
                float angle = (float)Math.Atan2((double)(vector2.Y - vector1.Y),
                    (double)(vector2.X - vector1.X));

                // stretch the pixel between the two vectors
                spriteBatch.Draw(pixel,
                    Position + vector1,
                    //new Vector2(vector1.X, vector1.Y),
                    null,
                    Colour * alpha,
                    angle,
                    //Vector2.Zero,
                    new Vector2(0, 1),
                    new Vector2(distance, Size),
                    SpriteEffects.None,
                    Depth);
            }
        }
        public void Render(SpriteBatch spriteBatch, float angle)
        {
            if (vectors.Count < 2)
                return;

            for (int i = 1; i < vectors.Count; i++)
            {
                Vector2 vector1 = (Vector2)vectors[i - 1];
                Vector2 vector2 = (Vector2)vectors[i];

                // calculate the distance between the two vectors
                float distance = Vector2.Distance(vector1, vector2);

                // stretch the pixel between the two vectors
                spriteBatch.Draw(pixel,
                    Position + vector1,
                    //new Vector2(vector1.X, vector1.Y),
                    null,
                    Colour,
                    angle,
                    //Vector2.Zero,
                    new Vector2(0, 1),
                    new Vector2(distance, Size),
                    SpriteEffects.None,
                    Depth);
            }
        }

        /// <summary>
        /// Creates a circle starting from 0, 0.
        /// </summary>
        /// <param name="radius">The radius (half the width) of the circle.</param>
        /// <param name="sides">The number of sides on the circle (the more the detailed).</param>
        /*public void CreateCircle(float radius, int sides)
        {
            vectors.Clear();

            float max = 2 * (float)Math.PI;
            float step = max / (float)sides;

            for (float theta = 0; theta < max; theta += step)
            {
                vectors.Add(new Vector2(radius * (float)Math.Cos((double)theta),
                    radius * (float)Math.Sin((double)theta)));
            }

            // then add the first vector again so it's a complete loop
            vectors.Add(new Vector2(radius * (float)Math.Cos(0),
                    radius * (float)Math.Sin(0)));
        }*/

        public void CreateCircle(Vector2 centerPoint, float radius)
        {
            //float incrementRadians = MathHelper.TwoPi / 32f;
            //float incrementRadians = MathHelper.TwoPi / (radius * 1.25f);
            float incrementRadians = MathHelper.TwoPi / radius;

            for (float r = 0f; r <= MathHelper.TwoPi; r += incrementRadians)
            {
                AddVector(new Vector2(centerPoint.X + radius * (float)Math.Cos(r), centerPoint.Y + radius * (float)Math.Sin(r)));
            }

            AddVector(new Vector2(centerPoint.X + radius, centerPoint.Y));
        }

        public void CreateBox(Rectangle rectangle)
        {
            AddVector(new Vector2(rectangle.X, rectangle.Y));
            AddVector(new Vector2(rectangle.X + rectangle.Width, rectangle.Y));
            AddVector(new Vector2(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height));
            AddVector(new Vector2(rectangle.X, rectangle.Y + rectangle.Height));
            AddVector(new Vector2(rectangle.X, rectangle.Y));
        }
    }
}