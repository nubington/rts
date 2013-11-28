using Microsoft.Xna.Framework;

namespace rts
{
    public class MeleeNublet : Unit
    {
        public MeleeNublet(Vector2 position, short team, short id)
            : base(UnitType.MeleeNublet, position, team, id)
        {
        }
    }
}
