using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

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
