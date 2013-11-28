using Microsoft.Xna.Framework;

namespace rts
{
    public class Barracks : Structure
    {
        public Barracks(Point tilePosition, short team)
            : base(StructureType.Barracks, tilePosition, team)
        {
        }
        public Barracks(Point tilePosition, Unit builder, short team)
            : base(StructureType.Barracks, tilePosition, builder, team)
        {
        }
    }
}
