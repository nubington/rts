using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace rts
{
    public class StructureType : RtsObjectType
    {
        public static readonly StructureType
            Barracks, 
            TownHall,
            Farm;

        public static List<StructureType> StructureTypes = new List<StructureType>();

        static StructureType()
        {
            Barracks = new StructureType();
            Barracks.CommandCard = CommandCard.BarracksCommandCard;
            Barracks.NormalTexture = Game1.Game.Content.Load<Texture2D>("structure textures/HumanBarracks");
            Barracks.BuildingTexture = Game1.Game.Content.Load<Texture2D>("structure textures/HumanBarracks");
            Barracks.PlacingTexture = Game1.Game.Content.Load<Texture2D>("structure textures/HumanBarracks");
            Barracks.Name = "Nub School";
            Barracks.Size = 4;
            Barracks.Hp = 750;
            Barracks.Armor = 2;
            Barracks.SightRange = 7;
            Barracks.BuildTime = 15000;// 25000;
            Barracks.SelectionSortValue = 1;
            Barracks.TargetPriority = 0;
            Barracks.Rallyable = true;
            Barracks.RoksCost = 25;
            Barracks.CutCorners = true;

            TownHall = new StructureType();
            TownHall.CommandCard = CommandCard.TownHallCommandCard;
            TownHall.NormalTexture = Game1.Game.Content.Load<Texture2D>("structure textures/HumanTownhall");
            TownHall.BuildingTexture = Game1.Game.Content.Load<Texture2D>("structure textures/HumanTownhall");
            TownHall.PlacingTexture = Game1.Game.Content.Load<Texture2D>("structure textures/HumanTownhall");
            TownHall.Name = "Nub City";
            TownHall.Size = 5;
            TownHall.Hp = 1000;
            TownHall.Armor = 2;
            TownHall.SightRange = 10;
            TownHall.BuildTime = 2500;// 25000;
            TownHall.SelectionSortValue = 0;
            TownHall.TargetPriority = 0;
            TownHall.Rallyable = true;
            TownHall.Supply = 8;
            TownHall.RoksCost = 75;
            TownHall.CutCorners = true;

            Farm = new StructureType();
            Farm.CommandCard = CommandCard.BlankCommandCard;
            Farm.NormalTexture = Game1.Game.Content.Load<Texture2D>("structure textures/HumanFarm");
            Farm.BuildingTexture = Game1.Game.Content.Load<Texture2D>("structure textures/HumanFarm");
            Farm.PlacingTexture = Game1.Game.Content.Load<Texture2D>("structure textures/HumanFarm");
            Farm.Name = "Nub Farm";
            Farm.Size = 3;
            Farm.Hp = 500;
            Farm.Armor = 1;
            Farm.SightRange = 7;
            Farm.BuildTime = 5000;// 25000;
            Farm.SelectionSortValue = 0;
            Farm.TargetPriority = 0;
            Farm.Rallyable = false;
            Farm.Supply = 6;
            Farm.RoksCost = 15;
            Farm.CutCorners = false;
        }

        public short ID { get; private set; }

        public Texture2D BuildingTexture, PlacingTexture;

        public int Size { get; private set; }
        public int Hp { get; private set; }
        public int Armor { get; private set; }
        public int SightRange { get; private set; }
        public int BuildTime { get; private set; }
        public bool Rallyable { get; private set;}
        public int TargetPriority { get; private set; }
        public int Supply { get; private set; }
        public int RoksCost { get; private set; }
        public bool CutCorners { get; private set; }

        StructureType()
        {
            StructureTypes.Add(this);
            ID = (short)(StructureTypes.Count - 1);
        }

        //call at game initialization to load class early
        public static void Init() { }

        public static void SetCommandCards()
        {
            Barracks.CommandCard = CommandCard.BarracksCommandCard;
            TownHall.CommandCard = CommandCard.TownHallCommandCard;
        }
    }
}
