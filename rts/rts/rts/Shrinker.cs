using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace rts
{
    public class Shrinker : BaseObject
    {
        public static List<Shrinker> Shrinkers = new List<Shrinker>();

        int size;
        int shrinkDelay, timeSinceLastShrink;

        public Shrinker(Vector2 position, int size, int shrinkDelay)
            : base(new Rectangle((int)position.X, (int)position.Y, size, size))
        {
            this.size = size;
            this.shrinkDelay = shrinkDelay;
            Shrinkers.Add(this);
        }

        // returns true if reached size
        public bool Update(GameTime gameTime)
        {
            timeSinceLastShrink += (int)(gameTime.ElapsedGameTime.TotalMilliseconds * Rts.GameSpeed);

            if (timeSinceLastShrink >= shrinkDelay)
            {
                timeSinceLastShrink -= shrinkDelay;

                Shrink(1, 1);
            }

            if (Width == 0 && Height == 0)
                return true;
            return false;
        }

        // updates blasts and removes them from list when size reached
        public static void UpdateShrinkers(GameTime gameTime)
        {
            for (int i = 0; i < Shrinkers.Count; i++)
            {
                Shrinker s = Shrinkers[i];
                if (s.Update(gameTime))
                {
                    Shrinkers.Remove(s);
                    i--;
                }
            }
        }
    }
}