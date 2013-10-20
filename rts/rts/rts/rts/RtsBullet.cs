using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace rts
{
    class RtsBullet : BaseObject
    {
        public static List<RtsBullet> RtsBullets = new List<RtsBullet>();

        Unit shooter;
        RtsObject target;

        public RtsBullet(Unit shooter, RtsObject target, Vector2 position, int size, float speed)
            : base(new Rectangle(0, 0, size, size), new Vector2(speed, speed))
        {
            this.shooter = shooter;
            this.target = target;
            CenterPoint = position;
            RtsBullets.Add(this);
        }

        // returns true if bullet is removed
        bool Update(GameTime gameTime)
        {
            moveTowardsPrecise(target.CenterPoint, gameTime, false);

            if (Touches(target.CenterPoint))
            {
                if (!target.IsDead)
                    target.TakeDamage(shooter, shooter.AttackDamage);
                RtsBullets.Remove(this);
                return true;
            }

            turnTowards(target.CenterPoint, float.MaxValue, gameTime);

            return false;
        }

        public static void UpdateAll(GameTime gameTime)
        {
            for (int i = 0; i < RtsBullets.Count; i++)
            {
                RtsBullet b = RtsBullets[i];

                if (b.Update(gameTime))
                    i--;
            }
        }
    }
}
