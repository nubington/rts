using Microsoft.Xna.Framework.Graphics;

namespace rts
{
    public class UnitType : RtsObjectType
    {
        public static readonly UnitType
            MeleeNublet,
            RangedNublet,
            WorkerNublet;

        static UnitType()
        {
            MeleeNublet = new UnitType();
            MeleeNublet.NormalTexture = Game1.Game.Content.Load<Texture2D>("unit textures/redcircleguy");
            MeleeNublet.SelectingTexture = Game1.Game.Content.Load<Texture2D>("unit textures/redcircleguyselected2");
            MeleeNublet.SelectedTexture = Game1.Game.Content.Load<Texture2D>("unit textures/redcircleguyselecting2");
            //MeleeNublet.BulletTexture = Game1.Game.Content.Load<Texture2D>("boxingglove");
            MeleeNublet.BulletType = BulletType.MeleeBullet;
            MeleeNublet.Name = "Melee Nublet";
            MeleeNublet.Size = 20;// 38;
            MeleeNublet.MoveSpeed = 115;// 140;
            MeleeNublet.AttackDamage = 6;
            MeleeNublet.AttackRange = 5;
            MeleeNublet.AttackDelay = 1000;
            MeleeNublet.BulletSize = 15;
            MeleeNublet.BulletSpeed = 200;
            MeleeNublet.Hp = 100;// 0000;
            MeleeNublet.Armor = 1;
            MeleeNublet.SightRange = 5;
            MeleeNublet.BuildTime = 1250;
            MeleeNublet.SelectionSortValue = 101;
            MeleeNublet.TargetPriority = 2;
            MeleeNublet.SupplyCost = 2;
            MeleeNublet.RoksCost = 15;

            RangedNublet = new UnitType();
            RangedNublet.NormalTexture = Game1.Game.Content.Load<Texture2D>("unit textures/browncircleguy");
            RangedNublet.SelectingTexture = Game1.Game.Content.Load<Texture2D>("unit textures/browncircleguyselected2");
            RangedNublet.SelectedTexture = Game1.Game.Content.Load<Texture2D>("unit textures/browncircleguyselecting2");
            //RangedNublet.BulletTexture = Game1.Game.Content.Load<Texture2D>("bullet");
            RangedNublet.BulletType = BulletType.RangedNubletBullet;
            RangedNublet.Name = "Ranged Nublet";
            RangedNublet.Size = 18;
            RangedNublet.MoveSpeed = 125;// 150;
            RangedNublet.AttackDamage = 5;
            RangedNublet.AttackRange = 50;
            RangedNublet.AttackDelay = 750;
            RangedNublet.BulletSize = 7;
            RangedNublet.BulletSpeed = 200;
            RangedNublet.Hp = 50;
            RangedNublet.Armor = 0;
            RangedNublet.SightRange = 6;
            RangedNublet.BuildTime = 10000;
            RangedNublet.SelectionSortValue = 102;
            RangedNublet.TargetPriority = 2;
            RangedNublet.SupplyCost = 2;
            RangedNublet.RoksCost = 10;

            WorkerNublet = new UnitType();
            //WorkerNublet.NormalTexture = Game1.Game.Content.Load<Texture2D>("unit textures/ants/worker ant");
            //WorkerNublet.SelectingTexture = Game1.Game.Content.Load<Texture2D>("unit textures/ants/worker ant");
            //WorkerNublet.SelectedTexture = Game1.Game.Content.Load<Texture2D>("unit textures/ants/worker ant");
            WorkerNublet.NormalTexture = Game1.Game.Content.Load<Texture2D>("unit textures/graycircleguy");
            WorkerNublet.SelectingTexture = Game1.Game.Content.Load<Texture2D>("unit textures/graycircleguyselected");
            WorkerNublet.SelectedTexture = Game1.Game.Content.Load<Texture2D>("unit textures/graycircleguyselecting");
            //WorkerNublet.BulletTexture = Game1.Game.Content.Load<Texture2D>("bullet");
            WorkerNublet.BulletType = BulletType.WorkerBullet;
            WorkerNublet.Name = "Worker Nublet";
            WorkerNublet.Size = 16;
            WorkerNublet.MoveSpeed = 90;// 110;
            WorkerNublet.AttackDamage = 4;
            WorkerNublet.AttackRange = 5;
            WorkerNublet.AttackDelay = 1000;
            WorkerNublet.BulletSize = 5;
            WorkerNublet.BulletSpeed = 200;
            WorkerNublet.Hp = 30;
            WorkerNublet.Armor = 0;
            WorkerNublet.SightRange = 5;
            WorkerNublet.BuildTime = 1000;// 8000;
            WorkerNublet.SelectionSortValue = 100;
            WorkerNublet.TargetPriority = 2;
            WorkerNublet.SupplyCost = 1;
            WorkerNublet.RoksCost = 1;//8;
        }

        public Texture2D SelectingTexture, SelectedTexture;

        public BulletType BulletType { get; private set; }

        public int Size { get; private set; }
        public int MoveSpeed { get; private set; }
        public int AttackDamage { get; private set; }
        public int AttackRange { get; private set; }
        public int AttackDelay { get; private set; }
        public int BulletSize { get; private set; }
        public int BulletSpeed { get; private set; }
        public int Hp { get; private set; }
        public int Armor { get; private set; }
        public int SightRange { get; private set; }
        public int BuildTime { get; private set; }
        public int TargetPriority { get; private set; }
        public int SupplyCost { get; private set; }
        public int RoksCost { get; private set; }

        UnitType()
        {
        }

        //call at game initialization to load class early
        public static void Init() { }

        public static void SetCommandCards()
        {
            MeleeNublet.CommandCard = CommandCard.DefaultCommandCard;
            RangedNublet.CommandCard = CommandCard.DefaultCommandCard;
            WorkerNublet.CommandCard = CommandCard.WorkerCommandCard;
        }
    }
}
