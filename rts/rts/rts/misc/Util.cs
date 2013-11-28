using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Forms;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Reflection;

namespace rts
{
    public class Util
    {
        public static float ScaleWithGameTime(float value, GameTime gameTime)
        {
            return value * (float)gameTime.ElapsedGameTime.TotalSeconds * Rts.GameSpeed;
        }

        /*public static bool ClampBaseObjectToScreen(BaseObject o, GraphicsDeviceManager graphics)
        {}*/

        public static Vector2 ClampVectorToScreen(Vector2 v, GraphicsDeviceManager graphics)
        {
            return ClampVectorToScreen(v, 0, graphics);
        }
        public static Vector2 ClampVectorToScreen(Vector2 v, float spacing, GraphicsDeviceManager graphics)
        {
            v.X = MathHelper.Clamp(v.X, 0 + spacing, graphics.GraphicsDevice.Viewport.Width - spacing);
            v.Y = MathHelper.Clamp(v.Y, 0 + spacing, graphics.GraphicsDevice.Viewport.Height - spacing);
            return v;
        }

        public static Rectangle ClampRectangleToScreen(Rectangle rect, GraphicsDeviceManager graphics)
        {
            return ClampRectangleToScreen(rect, 0, graphics);
        }
        public static Rectangle ClampRectangleToScreen(Rectangle rect, float spacing, GraphicsDeviceManager graphics)
        {
            rect.X = (int)MathHelper.Clamp(rect.X, 0 + spacing, graphics.GraphicsDevice.Viewport.Width - rect.Width - spacing);
            rect.Y = (int)MathHelper.Clamp(rect.Y, 0 + spacing, graphics.GraphicsDevice.Viewport.Height - rect.Height - spacing);
            return rect;
        }

        public static decimal DecimalClamp(decimal value, decimal min, decimal max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        public static float ConvertToPositiveRadians(float radians)
        {
            if (radians >= 0)
                return radians;

            //return (MathHelper.TwoPi - Math.Abs(radians));

            return MathHelper.TwoPi + radians;
        }

        public static float AngleDifference(float a1, float a2)
        {
            /*float dif = (float)Math.Abs(a1 - a2) % MathHelper.TwoPi;

            if (dif > 180)
                dif = MathHelper.TwoPi - dif;

            return dif;*/

            return Math.Abs(MathHelper.WrapAngle(a2 - a1));
        }

        public static void SortByX<T>(List<T> list) where T : BaseObject
        {
            for (int i = 2; i < list.Count; i++)
            {
                for (int j = i; j > 1 && list[j].X < list[j - 1].X; j--)
                {
                    T tempItem = list[j];
                    list.RemoveAt(j);
                    list.Insert(j - 1, tempItem);
                }
            }
        }
        public static void SortByY<T>(List<T> list) where T : BaseObject
        {
            for (int i = 2; i < list.Count; i++)
            {
                for (int j = i; j > 1 && list[j].Y < list[j - 1].Y; j--)
                {
                    T tempItem = list[j];
                    list.RemoveAt(j);
                    list.Insert(j - 1, tempItem);
                }
            }
        }

        public static Texture2D[] SplitTexture(Texture2D original, int partWidth, int partHeight)
        {
            int yCount = original.Height / partHeight + (partHeight % original.Height == 0 ? 0 : 1);//The number of textures in each horizontal row
            int xCount = original.Height / partHeight + (partHeight % original.Height == 0 ? 0 : 1);//The number of textures in each vertical column
            Texture2D[] r = new Texture2D[xCount * yCount];//Number of parts = (area of original) / (area of each part).
            int dataPerPart = partWidth * partHeight;//Number of pixels in each of the split parts

            //Get the pixel data from the original texture:
            Color[] originalData = new Color[original.Width * original.Height];
            original.GetData<Color>(originalData);

            int index = 0;
            for (int y = 0; y < yCount * partHeight; y += partHeight)
                for (int x = 0; x < xCount * partWidth; x += partWidth)
                {
                    //The texture at coordinate {x, y} from the top-left of the original texture
                    Texture2D part = new Texture2D(original.GraphicsDevice, partWidth, partHeight);
                    //The data for part
                    Color[] partData = new Color[dataPerPart];

                    //Fill the part data with colors from the original texture
                    for (int py = 0; py < partHeight; py++)
                        for (int px = 0; px < partWidth; px++)
                        {
                            int partIndex = px + py * partWidth;
                            //If a part goes outside of the source texture, then fill the overlapping part with Color.Transparent
                            if (y + py >= original.Height || x + px >= original.Width)
                                partData[partIndex] = Color.Transparent;
                            else
                                partData[partIndex] = originalData[(x + px) + (y + py) * original.Width];
                        }

                    //Fill the part with the extracted data
                    part.SetData<Color>(partData);
                    //Stick the part in the return array:                    
                    r[index++] = part;
                }
            //Return the array of parts.
            return r;
        }

        public static void DrawStringAtCenterOfRectangle(SpriteBatch spriteBatch, SpriteFont font, string str, Rectangle rectangle, Color color)
        {
            Vector2 stringSize = font.MeasureString(str);

            spriteBatch.DrawString(font, str, new Vector2((int)(rectangle.X + rectangle.Width / 2f - stringSize.X / 2), (int)(rectangle.Y + rectangle.Height / 2f - stringSize.Y / 2)), color);
        }

        public static Cursor LoadCustomCursor(string path)
        {
            IntPtr hCurs = LoadCursorFromFile(path);
            if (hCurs == IntPtr.Zero) throw new Win32Exception();
            var curs = new Cursor(hCurs);
            // Note: force the cursor to own the handle so it gets released properly
            var fi = typeof(Cursor).GetField("ownHandle", BindingFlags.NonPublic | BindingFlags.Instance);
            fi.SetValue(curs, true);
            return curs;
        }
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadCursorFromFile(string path);


        /*public static void UpdatePotentialCollisions<T>(List<T> objects) where T : BaseObject
        {
            if (objects.Count == 0)
                return;

            Util.SortByX(objects);

            List<int[]> pairs = new List<int[]>();

            for (int i = 0; i < objects.Count; i++)
            {
                T object1 = objects[i];
                object1.PotentialCollisions.Clear();

                for (int s = i + 1; s < objects.Count; s++)
                {
                    T object2 = objects[s];

                    if (object2.RightBound < object1.LeftBound)
                        continue;

                    if (object2.LeftBound > object1.RightBound)
                        break;

                    if (object2.TopBound <= object1.BottomBound &&
                        object2.BottomBound >= object1.TopBound)
                        pairs.Add(new int[2] { i, s });
                }
            }

            foreach (int[] pair in pairs)
            {
                objects[pair[0]].PotentialCollisions.Add(objects[pair[1]]);
                objects[pair[1]].PotentialCollisions.Add(objects[pair[0]]);
            }
        }


        static int persistentIndex = 0;
        public static List<BaseObject> Objects = new List<BaseObject>();
        //static List<int[]> pairs = new List<int[]>();
        static List<BaseObject[]> pairs = new List<BaseObject[]>();
        public static int PotentialCollisionsDivideBy = 8;

        public static void UpdatePotentialCollisionsDivided<T>(List<T> list) where T : BaseObject
        {
            if (persistentIndex == 0)
            {
                Util.SortByX(list);
                Objects = new List<BaseObject>(list);
                //Objects = Objects.Union(list).ToList();
                //Util.SortByX(Objects);
                pairs.Clear();
            }

            if (Objects.Count == 0)
                return;

            int numberToDo = (int)(Objects.Count / (float)PotentialCollisionsDivideBy + .5f);
            int count = 0;

            for (; persistentIndex < Objects.Count; persistentIndex++)
            {
                BaseObject object1 = Objects[persistentIndex];
                //object1.PotentialCollisions.Clear();

                for (int s = persistentIndex + 1; s < Objects.Count; s++)
                {
                    BaseObject object2 = Objects[s];

                    if (object2.RightBound < object1.LeftBound)
                        continue;

                    if (object2.LeftBound > object1.RightBound)
                        break;

                    if (object2.TopBound <= object1.BottomBound &&
                        object2.BottomBound >= object1.TopBound)
                        //pairs.Add(new int[2] { persistentIndex, s });
                        pairs.Add(new BaseObject[2] { Objects[persistentIndex], Objects[s] });
                }

                if (++count >= numberToDo && persistentIndex + numberToDo < Objects.Count)
                    return;
            }

            persistentIndex = 0;

            foreach (BaseObject o in Objects)
                o.PotentialCollisions.Clear();

            /*foreach (int[] pair in pairs)
            {
                Objects[pair[0]].PotentialCollisions.Add(Objects[pair[1]]);
                Objects[pair[1]].PotentialCollisions.Add(Objects[pair[0]]);
            }*/
        /*foreach (BaseObject[] pair in pairs)
        {
            if (Vector2.Distance(pair[0].CenterPoint, pair[1].CenterPoint) < (pair[0].GreaterOfWidthAndHeight + pair[1].GreaterOfWidthAndHeight) * .8f)
            {
                pair[0].PotentialCollisions.Add(pair[1]);
                pair[1].PotentialCollisions.Add(pair[0]);
            }
        }
    }

    public static void UpdatePotentialCollisions<T>(T object1, List<T> objects) where T : BaseObject
    {
        if (objects.Count == 0)
            return;

        object1.PotentialCollisions.Clear();

        Util.SortByX(objects);

        for (int i = 0; i < objects.Count; i++)
        {
            T object2 = objects[i];

            if (object2.RightBound < object1.LeftBound)
                continue;

            if (object2.LeftBound > object1.RightBound)
                break;

            if (object2.TopBound <= object1.BottomBound &&
                object2.BottomBound >= object1.TopBound)
                object1.PotentialCollisions.Add(object2);
        }
    }

    public static void UpdatePotentialCollisions<T>(List<T> objects1, List<T> objects2) where T : BaseObject
    {
        if (objects1.Count == 0 || objects2.Count == 0)
            return;

        Util.SortByX(objects1);
        Util.SortByX(objects2);

        List<int[]> pairs = new List<int[]>();

        for (int i = 0; i < objects1.Count; i++)
        {
            T object1 = objects1[i];
            object1.PotentialCollisions.Clear();

            for (int s = 0; s < objects2.Count; s++)
            {
                T object2 = objects2[s];

                if (object2.RightBound < object1.LeftBound)
                    continue;

                if (object2.LeftBound > object1.RightBound)
                    break;

                if (object2.TopBound <= object1.BottomBound &&
                    object2.BottomBound >= object1.TopBound)
                    pairs.Add(new int[2] { i, s });
            }
        }

        foreach (int[] pair in pairs)
        {
            objects1[pair[0]].PotentialCollisions.Add(objects2[pair[1]]);
            objects2[pair[1]].PotentialCollisions.Add(objects1[pair[0]]);
        }
    }*/
    }

    public class ObjectLink<T1, T2>
    {
        public T1 Object1 { get; private set; }
        public T2 Object2 { get; private set; }

        public ObjectLink(T1 o1, T2 o2)
        {
            Object1 = o1;
            Object2 = o2;
        }
    }

    public class PlacedStructure : ObjectLink<StructureType, Rectangle>
    {
        public int Team { get; private set; }

        public PlacedStructure(StructureType structureType, Rectangle rectangle, int team)
            : base(structureType, rectangle)
        {
            Team = team;
        }
    }
}

// update fog texture
                /*Color color;

                for (int x = 0; x < Map.Width; x++)
                {
                    for (int y = 0; y < Map.Height; y++)
                    {
                        if (Map.Tiles[y, x].Visible)
                            color = Color.Transparent;
                        else
                            color = FogColor;

                        int xx = x * Map.TileSize;
                        int yy = y * Map.TileSize;

                        for (int py = 0; py < Map.TileSize; py++)
                        {
                            for (int px = 0; px < Map.TileSize; px++)
                            {
                                //int partIndex = px + py * Map.TileSize;
                                Fog[(xx + px) + (yy + py) * FogTexture.Width] = color;
                            }
                        }
                    }
                }
*/