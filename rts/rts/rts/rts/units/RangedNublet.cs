using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace rts
{
    class RangedNublet : Unit
    {
        static Texture2D normalTexture = Game1.Game.Content.Load<Texture2D>("unit textures/browncircleguy"),
            selectingTexture = Game1.Game.Content.Load<Texture2D>("unit textures/browncircleguyselected2"),
            selectedTexture = Game1.Game.Content.Load<Texture2D>("unit textures/browncircleguyselecting2"),
            bulletTexture = Game1.Game.Content.Load<Texture2D>("bullet");
        static int bulletSize = 5;

        public static readonly CommandCard CommandCard = CommandCard.DefaultCommandCard;

        static int baseSize = 17;

        static int baseMoveSpeed = 150;

        static int baseAttackDamage = 5,
            baseAttackRange = 50,
            baseAttackDelay = 750;

        static int baseHp = 50;
        static int baseArmor = 0;

        static int baseSightRange = 5;

        public RangedNublet(Vector2 position, int team)
            : base(UnitType.RangedNublet, position, baseSize, baseMoveSpeed)
        {
            Texture = normalTexture;
            BulletTexture = bulletTexture;
            BulletSize = bulletSize;
            AttackDamage = baseAttackDamage;
            AttackRange = baseAttackRange;
            AttackDelay = baseAttackDelay;
            Hp = MaxHp = baseHp;
            Armor = baseArmor;
            SightRange = baseSightRange;
            Team = team;
        }

        public override string UnitName
        {
            get
            {
                return "Ranged Nublet";
            }
        }

        public override Texture2D SelectingTexture
        {
            get
            {
                return selectingTexture;
            }
            set
            {
                selectingTexture = value;
            }
        }
        public override Texture2D SelectedTexture
        {
            get
            {
                return selectedTexture;
            }
            set
            {
                selectedTexture = value;
            }
        }
    }
}
