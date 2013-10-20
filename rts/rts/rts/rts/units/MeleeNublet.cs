using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace rts
{
    class MeleeNublet : Unit
    {
        static Texture2D normalTexture = Game1.Game.Content.Load<Texture2D>("unit textures/redcircleguy"),
            selectingTexture = Game1.Game.Content.Load<Texture2D>("unit textures/redcircleguyselected2"),
            selectedTexture = Game1.Game.Content.Load<Texture2D>("unit textures/redcircleguyselecting2"),
            bulletTexture = Game1.Game.Content.Load<Texture2D>("boxingglove");
        static int bulletSize = 15;

        public static readonly CommandCard CommandCard = CommandCard.TestCommandCard;

        static int baseSize = 19;

        static int baseMoveSpeed = 150;

        static int baseAttackDamage = 5,
            baseAttackRange = 5,
            baseAttackDelay = 1000;

        static int baseHp = 60;
        static int baseArmor = 1;

        static int baseSightRange = 4;

        public MeleeNublet(Vector2 position, int team)
            : base(UnitType.MeleeNublet, position, baseSize, baseMoveSpeed)
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
                return "Melee Nublet";
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
