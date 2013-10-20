using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;

namespace rts
{
    public class ColorTexture
    {

        public static Texture2D White, Black, Gray, Red, Green, Yellow, LightGray, DarkGray, Beige;

        /// <summary>
        /// Creates a 1x1 pixel black texture.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device to use.</param>
        /// <returns>The newly created texture.</returns>
        public static Texture2D Create(GraphicsDevice graphicsDevice)
        {
            return Create(graphicsDevice, 1, 1, new Color());
        }

        /// <summary>
        /// Creates a 1x1 pixel texture of the specified color.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device to use.</param>
        /// <param name="color">The color to set the texture to.</param>
        /// <returns>The newly created texture.</returns>
        public static Texture2D Create(GraphicsDevice graphicsDevice, Color color)
        {
            return Create(graphicsDevice, 1, 1, color);
        }

        /// <summary>
        /// Creates a texture of the specified color.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device to use.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="color">The color to set the texture to.</param>
        /// <returns>The newly created texture.</returns>
        public static Texture2D Create(GraphicsDevice graphicsDevice, int width, int height, Color color)
        {
            // create the rectangle texture without colors
            Texture2D texture = new Texture2D(
                graphicsDevice,
                width,
                height,
                false,
                SurfaceFormat.Color);

            // Create a color array for the pixels
            Color[] colors = new Color[width * height];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = new Color(color.ToVector3());
            }

            // Set the color data for the texture
            texture.SetData(colors);

            return texture;
        }

        public static readonly int TextureSize = 16;
        public static readonly Vector2 CenterVector = new Vector2(8, 8);

        public static void Initialize(GraphicsDevice graphicsDevice)
        {
            White = ColorTexture.Create(graphicsDevice, TextureSize, TextureSize, Color.White);
            Black = ColorTexture.Create(graphicsDevice, TextureSize, TextureSize, Color.Black);
            Red = ColorTexture.Create(graphicsDevice, TextureSize, TextureSize, Color.Red);
            Green = ColorTexture.Create(graphicsDevice, TextureSize, TextureSize, Color.Green);
            Yellow = ColorTexture.Create(graphicsDevice, TextureSize, TextureSize, Color.Yellow);
            Gray = ColorTexture.Create(graphicsDevice, TextureSize, TextureSize, Color.Gray);
            LightGray = ColorTexture.Create(graphicsDevice, TextureSize, TextureSize, Color.LightGray);
            DarkGray = ColorTexture.Create(graphicsDevice, TextureSize, TextureSize, Color.DarkGray);
            Beige = ColorTexture.Create(graphicsDevice, TextureSize, TextureSize, Color.Beige);
        }
    }
}