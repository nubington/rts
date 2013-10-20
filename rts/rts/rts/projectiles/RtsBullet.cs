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
    public class RtsBullet : BaseObject
    {
        public static List<RtsBullet> RtsBullets = new List<RtsBullet>();

        Unit shooter;
        RtsObject target;
        public BulletType Type { get; private set; }

        Animation animation;

        public RtsBullet(BulletType type, Unit shooter, RtsObject target, Vector2 position, int size, float speed)
            : base(new Rectangle(0, 0, size, size), new Vector2(speed, speed))
        {
            Type = type;

            /*if (type.Animated)
                animation = new Animation(0, 1 * Rts.GameSpeed, Util.SplitTexture(type.Texture, type.SheetWidth, type.SheetHeight));
            else
                Texture = type.Texture;*/
            if (type.Textures.Length > 1)
                animation = new Animation(0, 10 * Rts.GameSpeed, type.Textures);
            else
                Texture = type.Textures[0];

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

            if (animation != null)
            {
                animation.Update(gameTime);
            }

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

        //public void SetAnimation(

        public override Texture2D Texture
        {
            get
            {
                if (animation == null)
                    return base.Texture;

                return animation;
            }
            set
            {
                base.Texture = value;
            }
        }
    }

    public class BulletType
    {
        public static BulletType WorkerBullet, RangedNubletBullet, MeleeBullet;

        static BulletType()
        {
            WorkerBullet = new BulletType();
            WorkerBullet.Textures = Util.SplitTexture(Game1.Game.Content.Load<Texture2D>("projectile textures/gray ball sheet"), 64, 72);

            RangedNubletBullet = new BulletType();
            RangedNubletBullet.Textures = Util.SplitTexture(Game1.Game.Content.Load<Texture2D>("projectile textures/3 frame fireball"), 32, 29);

            MeleeBullet = new BulletType();
            MeleeBullet.Textures = new Texture2D[1];
            MeleeBullet.Textures[0] = Game1.Game.Content.Load<Texture2D>("boxingglove");
        }

        public Texture2D[] Textures { get; private set; }
    }
}
