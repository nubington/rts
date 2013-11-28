using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace rts
{
    public class BoundingBox
    {
        public Rectangle Rectangle;
        public List<MapTile> Tiles = new List<MapTile>();
        bool revealed;
        public bool FullyRevealed { get; private set; }
        public bool Visible;
        public bool FullyRevealedAndNotVisible;

        public System.Collections.Generic.HashSet<Unit> UnitsContained = new System.Collections.Generic.HashSet<Unit>();

        static int counterStart = 0;
        int counter;
        public BoundingBox(Rectangle rectangle)
        {
            Rectangle = rectangle;
            counter = counterStart++ % 1;
        }

        int counterMax = 1;
        public void Update()
        {
            if (++counter >= counterMax)
            {
                counter = 0;

                if (!Revealed)
                {
                    //FullyRevealedAndNotVisible = false;
                    return;
                }

                Visible = false;
                foreach (MapTile tile in Tiles)
                {
                    if (tile.Visible)
                    {
                        Visible = true;
                        break;
                    }
                }

                FullyRevealedAndNotVisible = (FullyRevealed && !Visible);
            }
        }

        int i;
        public bool Revealed
        {
            get
            {
                return revealed;
            }
            set
            {
                revealed = value;

                if (!FullyRevealed && i++ % 2 >= 2)
                {
                    i = 0;
                    FullyRevealed = true;

                    foreach (MapTile tile in Tiles)
                    {
                        if (!tile.Revealed)
                        {
                            FullyRevealed = false;
                            break;
                        }
                    }
                }
            }
        }
    }
}
