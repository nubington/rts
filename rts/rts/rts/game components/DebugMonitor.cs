using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace rts
{
    public class DebugMonitor : Microsoft.Xna.Framework.DrawableGameComponent
    {
        SpriteBatch spriteBatch;
        SpriteFont font;

        List<string> lines;

        PrimitiveLine boxLine;
        Vector2[] boxVectors;

        Direction position;

        bool enabled, drawBox;

        public DebugMonitor(Game game)
            : base(game)
        {
            position = Direction.NorthEast;
        }

        public override void Initialize()
        {
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);

            lines = new List<string>();

            boxLine = new PrimitiveLine(Game.GraphicsDevice, 1);
            boxLine.Colour = Color.Black;
            boxVectors = new Vector2[4];

            base.Initialize();
        }

        protected override void LoadContent()
        {
            font = Game.Content.Load<SpriteFont>("spritefonts/debugfont");

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (!enabled || lines.Count == 0)
                return;

            boxVectors = new Vector2[4];

            int biggestLineX = 0;
            foreach (string line in lines)
            {
                int lineX = (int)font.MeasureString(line).X;
                if (lineX > biggestLineX)
                    biggestLineX = lineX;
            }
            int lineSizeY = (int)font.MeasureString(lines[0]).Y;

            spriteBatch.Begin();

            int spacing = 2;

            if (position == Direction.North)
            {
            }
            else if (position == Direction.NorthEast)
            {
                int posX = Game.GraphicsDevice.Viewport.Width - biggestLineX;
                int posY = 2;

                for (int i = 0; i < lines.Count; i++)
                {
                    string line = lines[i];

                    if (i == 0)
                    {
                        boxVectors[0] = new Vector2(posX - 6, posY);
                        boxVectors[1] = new Vector2(Game.GraphicsDevice.Viewport.Width, posY);
                    }
                    else if (i == lines.Count - 1)
                    {
                        boxVectors[3] = new Vector2(posX - 6, posY + lineSizeY);
                        boxVectors[2] = new Vector2(Game.GraphicsDevice.Viewport.Width, posY + lineSizeY);
                    }

                    spriteBatch.DrawString(font, line, new Vector2((int)(Game.GraphicsDevice.Viewport.Width - biggestLineX - 2), posY), Color.Black);
                    posY += (int)lineSizeY + spacing;
                }
            }
            else if (position == Direction.East)
            {
            }
            else if (position == Direction.SouthEast)
            {
                int posX = Game.GraphicsDevice.Viewport.Width - biggestLineX;
                int posY = Game.GraphicsDevice.Viewport.Height - lineSizeY * lines.Count - 2;

                for (int i = 0; i < lines.Count; i++)
                {
                    string line = lines[i];

                    if (i == 0)
                    {
                        boxVectors[0] = new Vector2(posX - 5, posY);
                        boxVectors[1] = new Vector2(Game.GraphicsDevice.Viewport.Width - 1, posY);
                    }
                    if (i == lines.Count - 1)
                    {
                        boxVectors[2] = new Vector2(Game.GraphicsDevice.Viewport.Width - 1, posY + lineSizeY);
                        boxVectors[3] = new Vector2(posX - 5, posY + lineSizeY);
                    }

                    spriteBatch.DrawString(font, line, new Vector2((int)(Game.GraphicsDevice.Viewport.Width - biggestLineX - 2), posY), Color.Black);
                    posY += lineSizeY;
                }
            }
            else if (position == Direction.South)
            {
            }
            else if (position == Direction.SouthWest)
            {
            }
            else if (position == Direction.West)
            {
            }
            else if (position == Direction.NorthWest)
            {
            }

            if (drawBox)
            {
                boxLine.ClearVectors();
                foreach (Vector2 v in boxVectors)
                    boxLine.AddVector(v);
                boxLine.AddVector(boxVectors[0]);
                boxLine.Render(spriteBatch);
            }

            spriteBatch.End();

            lines.Clear();

            base.Draw(gameTime);
        }

        public void AddLine(string str)
        {
            lines.Add(str);
        }
        public void InsertLine(int index, string str)
        {
            lines.Insert(index, str);
        }

        new public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;
            }
        }
        public bool DrawBox
        {
            get
            {
                return drawBox;
            }
            set
            {
                drawBox = value;
            }
        }

        public Direction Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }
    }
}
