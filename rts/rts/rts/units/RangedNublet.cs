using Microsoft.Xna.Framework;

namespace rts
{
    public class RangedNublet : Unit
    {
        public RangedNublet(Vector2 position, short team, short id)
            : base(UnitType.RangedNublet, position, team, id)
        {
        }
    }
}
