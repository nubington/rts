using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

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
