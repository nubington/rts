using Microsoft.Xna.Framework;

namespace rts
{
    public class Farm : Structure
    {
        public Farm(Point tilePosition, short team)
            : base(StructureType.Farm, tilePosition, team)
        {
        }
        public Farm(Point tilePosition, Unit builder, short team)
            : base(StructureType.Farm, tilePosition, builder, team)
        {
        }
    }
}
