using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace rts
{
    public class TownHall : Structure
    {
        public static List<TownHall> TownHalls = new List<TownHall>();

        public TownHall(Point tilePosition, short team)
            : base(StructureType.TownHall, tilePosition, team)
        {
            TownHalls.Add(this);
        }
        public TownHall(Point tilePosition, Unit builder, short team)
            : base(StructureType.TownHall, tilePosition, builder, team)
        {
            TownHalls.Add(this);
        }

        public override void Die()
        {
            base.Die();

            TownHalls.Remove(this);
        }
    }
}
