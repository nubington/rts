using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

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
