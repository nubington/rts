using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace rts
{
    public class CommandCard
    {
        public static readonly CommandCard 
            BlankCommandCard,
            DefaultCommandCard,
            TestCommandCard,
            BarracksCommandCard,
            TownHallCommandCard,
            WorkerCommandCard,
            BuildMenuCommandCard,
            UnderConstructionCommandCard;

        static CommandCard()
        {
            BlankCommandCard = new CommandCard(new CommandButton[3, 4]);

            DefaultCommandCard = new CommandCard(new CommandButton[3, 4]);
            DefaultCommandCard.Buttons[0, 0] = new CommandButton(CommandButtonType.Attack);
            DefaultCommandCard.Buttons[0, 1] = new CommandButton(CommandButtonType.HoldPosition);
            DefaultCommandCard.Buttons[2, 2] = new CommandButton(CommandButtonType.Stop);

            TestCommandCard = new CommandCard(new CommandButton[3, 4]);
            TestCommandCard.Buttons[0, 0] = new CommandButton(CommandButtonType.Attack);
            TestCommandCard.Buttons[0, 1] = new CommandButton(CommandButtonType.HoldPosition);
            TestCommandCard.Buttons[0, 2] = new CommandButton(CommandButtonType.Move);

            WorkerCommandCard = new CommandCard(new CommandButton[3, 4]);
            WorkerCommandCard.Buttons[0, 0] = new CommandButton(CommandButtonType.Attack);
            WorkerCommandCard.Buttons[0, 1] = new CommandButton(CommandButtonType.HoldPosition);
            //WorkerCommandCard.Buttons[1, 0] = new CommandButton(CommandButtonType.Harvest);
            WorkerCommandCard.Buttons[1, 1] = new CommandButton(CommandButtonType.ReturnCargo);
            WorkerCommandCard.Buttons[2, 2] = new CommandButton(CommandButtonType.Stop);
            WorkerCommandCard.Buttons[2, 0] = new CommandButton(CommandButtonType.Build);

            BuildMenuCommandCard = new CommandCard(new CommandButton[3, 4]);
            BuildMenuCommandCard.Buttons[0, 0] = new CommandButton(CommandButtonType.BuildTownHall);
            BuildMenuCommandCard.Buttons[1, 0] = new CommandButton(CommandButtonType.BuildBarracks);
            BuildMenuCommandCard.Buttons[0, 1] = new CommandButton(CommandButtonType.BuildFarm);
            BuildMenuCommandCard.Buttons[2, 2] = new CommandButton(CommandButtonType.Cancel);

            UnderConstructionCommandCard = new CommandCard(new CommandButton[3, 4]);
            UnderConstructionCommandCard.Buttons[2, 2] = new CommandButton(CommandButtonType.Cancel);

            BarracksCommandCard = new CommandCard(new CommandButton[3, 4]);
            BarracksCommandCard.Buttons[2, 0] = new CommandButton(CommandButtonType.RallyPoint);
            BarracksCommandCard.Buttons[0, 0] = new CommandButton(CommandButtonType.BuildMeleeNublet);
            BarracksCommandCard.Buttons[0, 1] = new CommandButton(CommandButtonType.BuildRangedNublet);
            BarracksCommandCard.Buttons[2, 2] = new CommandButton(CommandButtonType.Cancel);

            TownHallCommandCard = new CommandCard(new CommandButton[3, 4]);
            TownHallCommandCard.Buttons[2, 0] = new CommandButton(CommandButtonType.RallyPoint);
            TownHallCommandCard.Buttons[0, 0] = new CommandButton(CommandButtonType.BuildWorkerNublet);
            TownHallCommandCard.Buttons[2, 2] = new CommandButton(CommandButtonType.Cancel);

            UnitType.SetCommandCards();
            StructureType.SetCommandCards();
        }

        public CommandButton[,] Buttons;// = new CommandButton[4, 4];

        public CommandCard(CommandButton[,] buttons)
        {
            Buttons = buttons;
        }

        //call at game initialization to load class early
        public static void Init() { }
    }

    public class CommandButtonType
    {
        public static List<CommandButtonType> CommandButtonTypes = new List<CommandButtonType>();

        public static readonly CommandButtonType
            // normal commands
            Move = new CommandButtonType(Game1.Game.Content.Load<Texture2D>("whitebox"), Keys.Kana),
            Stop = new CommandButtonType(Game1.Game.Content.Load<Texture2D>("stopsign"), Keys.S),
            Attack = new CommandButtonType(Game1.Game.Content.Load<Texture2D>("simplesword"), Keys.A),
            HoldPosition = new CommandButtonType(Game1.Game.Content.Load<Texture2D>("crossedswords"), Keys.H),
            RallyPoint = new CommandButtonType(Game1.Game.Content.Load<Texture2D>("redflag"), Keys.R),
            Build = new CommandButtonType(Game1.Game.Content.Load<Texture2D>("hammer"), Keys.B),
            Cancel = new CommandButtonType(Game1.Game.Content.Load<Texture2D>("redx"), Keys.Escape),

            // worker commands
            Harvest = new CommandButtonType(Game1.Game.Content.Load<Texture2D>("whitebox"), Keys.H),
            ReturnCargo = new CommandButtonType(Game1.Game.Content.Load<Texture2D>("whitebox"), Keys.C),
            
            // build unit commands
            BuildMeleeNublet = new BuildUnitButtonType(Game1.Game.Content.Load<Texture2D>("unit textures/MeleeNubletButton"), Keys.M, UnitType.MeleeNublet),
            BuildRangedNublet = new BuildUnitButtonType(Game1.Game.Content.Load<Texture2D>("unit textures/RangedNubletButton"), Keys.A, UnitType.RangedNublet),
            BuildWorkerNublet = new BuildUnitButtonType(Game1.Game.Content.Load<Texture2D>("unit textures/WorkerNubletButton"), Keys.W, UnitType.WorkerNublet),

            // build structure commands
            BuildTownHall = new BuildStructureButtonType(Game1.Game.Content.Load<Texture2D>("structure textures/HumanTownhall"), Keys.C, StructureType.TownHall),
            BuildBarracks = new BuildStructureButtonType(Game1.Game.Content.Load<Texture2D>("structure textures/HumanBarracks"), Keys.B, StructureType.Barracks),
            BuildFarm = new BuildStructureButtonType(Game1.Game.Content.Load<Texture2D>("structure textures/HumanFarm"), Keys.F, StructureType.Farm)
            ;

        public readonly Texture2D Texture;
        public readonly Keys Hotkey;
        public readonly short ID;

        static short idCounter;
        protected CommandButtonType(Texture2D texture, Keys hotkey)
        {
            Texture = texture;
            Hotkey = hotkey;

            ID = idCounter++;
            CommandButtonTypes.Add(this);
        }
    }

    public class CommandButton : SimpleButton
    {
        public readonly CommandButtonType Type;

        public CommandButton(CommandButtonType type) 
            : base(Rectangle.Empty)
        {
            NormalTexture = type.Texture;
            Type = type;
            Hotkey = type.Hotkey;
        }
    }

    public class ProductionButtonType : CommandButtonType
    {
        public readonly int BuildTime;

        public ProductionButtonType(Texture2D texture, Keys hotkey, int buildTime)
            : base(texture, hotkey)
        {
            BuildTime = buildTime;
        }

        public virtual string Name
        {
            get
            {
                return null;
            }
        }
    }

    public class BuildUnitButtonType : ProductionButtonType
    {
        public readonly UnitType UnitType;

        public BuildUnitButtonType(Texture2D texture, Keys hotkey, UnitType unitType)
            : base(texture, hotkey, unitType.BuildTime)
        {
            UnitType = unitType;
        }

        public override string Name
        {
            get
            {
                return UnitType.Name;
            }
        }
    }

    public class BuildStructureButtonType : ProductionButtonType
    {
        public readonly StructureType StructureType;

        public BuildStructureButtonType(Texture2D texture, Keys hotkey, StructureType structureType)
            : base(texture, hotkey, structureType.BuildTime)
        {
            StructureType = structureType;
        }

        public override string Name
        {
            get
            {
                return StructureType.Name;
            }
        }
    }
}
