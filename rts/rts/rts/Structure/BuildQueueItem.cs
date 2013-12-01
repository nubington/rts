using Microsoft.Xna.Framework;

namespace rts
{
    public class BuildQueueItem
    {
        public readonly ProductionButtonType Type;
        public readonly short ID;

        public readonly int BuildTime;
        public bool Started;

        int timeElapsed, percentDone;
        bool done;

        public BuildQueueItem(ProductionButtonType commandType, short id, int buildTime)
        {
            Type = commandType;
            ID = id;
            BuildTime = buildTime;
        }

        public void UpdateTime(GameTime gameTime)
        {
            if (!Started)
                return;

            timeElapsed += (int)(gameTime.ElapsedGameTime.TotalMilliseconds * Rts.GameSpeed);

            if (timeElapsed >= BuildTime)
            {
                done = true;
                percentDone = 100;
            }
            else
            {
                percentDone = (int)(timeElapsed / (float)BuildTime * 100);
            }
        }

        public bool Done
        {
            get
            {
                return done;
            }
        }
        public int PercentDone
        {
            get
            {
                return percentDone;
            }
        }
    }
}
