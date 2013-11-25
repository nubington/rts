using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Lidgren.Network;

namespace rts
{
    public class Structure : RtsObject
    {
        public const int MAX_QUEUE_SIZE = 5;
        const float STARTING_HP_PERCENT = .1f;

        public static List<Structure> structures = new List<Structure>();
        public static List<Structure> DeadStructures = new List<Structure>();

        public static Texture2D[] Explosion1Textures;

        new public int X, Y;
        int size;
        List<RallyPoint> rallyPoints = new List<RallyPoint>();
        protected bool rallyable;

        public bool UnderConstruction { get; private set; }
        public Unit Builder { get; private set; }
        int constructionTimeElapsed = 0;
        float constructionHpPerTick;
        float preciseHpCounter;
        public float PercentDone  { get; private set; }
        public StructureCogWheel CogWheel { get; private set; }

        public List<BuildQueueItem> BuildQueue = new List<BuildQueueItem>();

        public List<PathNode> exitPathNodes = new List<PathNode>();

        StructureType type;
        public short ID;

        public Structure(StructureType type, Point tilePosition, short team)
            : base(new Vector2(tilePosition.X * Rts.map.TileSize, tilePosition.Y * Rts.map.TileSize), type.Size * Rts.map.TileSize, team)
        {
            ID = Player.Players[team].StructureIDCounter++;
            Player.Players[team].StructureArray[ID] = this;

            this.type = type;
            Texture = type.NormalTexture;
            Hp = MaxHp = type.Hp;
            Armor = type.Armor;
            SightRange = type.SightRange;
            rallyable = type.Rallyable;
            SelectionSortValue = type.SelectionSortValue;
            TargetPriority = type.TargetPriority;
            X = tilePosition.X;
            Y = tilePosition.Y;
            this.size = type.Size;
            BuildTime = type.BuildTime;
            UnderConstruction = false;
            PercentDone = 1f;
            setOccupiedPathNodes();
            setExitPathNodes();

            foreach (Structure structure in Structures)
                structure.HasMoved = true;

            foreach (Structure structure in Structures)
                structure.setExitPathNodes();

            foreach (Roks roks in Roks.AllRoks)
                roks.SetExitPathNodes();

            AddStructure(this);
        }

        public Structure(StructureType type, Point tilePosition, Unit builder, short team)
            : this(type, tilePosition, team)
        {
            UnderConstruction = true;
            PercentDone = 0f;
            Builder = builder;
            //preciseHp = MaxHp * STARTING_HP_PERCENT;
            Hp = (int)(MaxHp * STARTING_HP_PERCENT);
            constructionHpPerTick = (MaxHp - (MaxHp * STARTING_HP_PERCENT)) / (BuildTime / 1000) / 8;

            //((Rts)Game1.Game.CurrentGameState).CogWheels.Add(new StructureCogWheel(this, (int)(Type.Size * Map.TileSize / 2.5f)));
            //if (team == Player.Me.Team)
            //{
                CogWheel = new StructureCogWheel(this, (int)(type.Size * Rts.map.TileSize / 2.5f));
                ((Rts)Game1.Game.CurrentGameState).CogWheels.Add(CogWheel);
            //}

                foreach (PathNode pathNode in OccupiedPathNodes)
                {
                    foreach (Unit unit in pathNode.UnitsContained)
                    {
                        if (unit != Builder)
                        {
                            unit.PushSimple((float)(rand.NextDouble() * MathHelper.TwoPi), Radius * .5f);
                            //unit.CheckForPush(true);
                            unit.CheckForWallHit();
                        }
                    }
                }
        }

        void setOccupiedPathNodes()
        {
            for (int x = X; x < X + size; x++)
            {
                for (int y = Y; y < Y + size; y++)
                {
                    PathNode node = Rts.pathFinder.PathNodes[y, x];
                    if (Intersects(node.Tile))
                    {
                        OccupiedPathNodes.Add(node);
                        node.Blocked = true;
                        node.Blocker = this;
                    }
                }
            }

            if (type.CutCorners)
            {
                OccupiedPathNodes.Remove(Rts.pathFinder.PathNodes[Y, X]);
                Rts.pathFinder.PathNodes[Y, X].Blocked = false;
                Rts.pathFinder.PathNodes[Y, X].Blocker = null;
                exitPathNodes.Add(Rts.pathFinder.PathNodes[Y, X]);
                OccupiedPathNodes.Remove(Rts.pathFinder.PathNodes[Y, X + size - 1]);
                Rts.pathFinder.PathNodes[Y, X + size - 1].Blocked = false;
                Rts.pathFinder.PathNodes[Y, X + size - 1].Blocker = null;
                exitPathNodes.Add(Rts.pathFinder.PathNodes[Y, X + size - 1]);
                OccupiedPathNodes.Remove(Rts.pathFinder.PathNodes[Y + size - 1, X]);
                Rts.pathFinder.PathNodes[Y + size - 1, X].Blocked = false;
                Rts.pathFinder.PathNodes[Y + size - 1, X].Blocker = null;
                exitPathNodes.Add(Rts.pathFinder.PathNodes[Y + size - 1, X]);
                OccupiedPathNodes.Remove(Rts.pathFinder.PathNodes[Y + size - 1, X + size - 1]);
                Rts.pathFinder.PathNodes[Y + size - 1, X + size - 1].Blocked = false;
                Rts.pathFinder.PathNodes[Y + size - 1, X + size - 1].Blocker = null;
                exitPathNodes.Add(Rts.pathFinder.PathNodes[Y + size - 1, X + size - 1]);
            }
        }

        void setExitPathNodes()
        {
            for (int x = X - 1; x <= X + type.Size; x++)
            {
                if (x < 0 || x > Rts.map.Width - 1)
                    continue;

                for (int y = Y - 1; y <= Y + type.Size; y++)
                {
                    if (y < 0 || y > Rts.map.Height - 1)
                        continue;

                    if (Rts.pathFinder.PathNodes[y, x].Tile.Walkable && !Rts.pathFinder.PathNodes[y, x].Blocked)
                        exitPathNodes.Add(Rts.pathFinder.PathNodes[y, x]);
                }
            }

            //foreach (PathNode pathNode in exitPathNodes)
            for (int i = 0; i < exitPathNodes.Count; )
            {
                PathNode pathNode = exitPathNodes[i];

                bool good = false;
                foreach (PathNode neighbor in pathNode.Neighbors)
                {
                    if (OccupiedPathNodes.Contains(neighbor))
                        good = true;
                }

                if (!good)
                    exitPathNodes.Remove(pathNode);
                else
                    i++;
            }

            /*for (int x = X - 1; x <= X + type.Size; x++)
            {
                if (x < 0 || x > Map.Width - 1)
                    continue;

                if (!(Y - 1 < 0) && PathFinder.PathNodes[Y - 1, x].Tile.Walkable && !PathFinder.PathNodes[Y - 1, x].Blocked)
                exitPathNodes.Add(PathFinder.PathNodes[Y - 1, x]);

                if (x == X - 1 || x == X + type.Size)
                {
                    for (int y = Y; y <= Y + type.Size; y++)
                    {
                        if (y < 0 || y > Map.Height - 1)
                            continue;

                        if (PathFinder.PathNodes[y, x].Tile.Walkable && !PathFinder.PathNodes[y, x].Blocked)
                            exitPathNodes.Add(PathFinder.PathNodes[y, x]);
                    }
                }

                if (!(Y + type.Size > Map.Height - 1) && PathFinder.PathNodes[Y + type.Size, x].Tile.Walkable && !PathFinder.PathNodes[Y + type.Size, x].Blocked)
                    exitPathNodes.Add(PathFinder.PathNodes[Y + type.Size, x]);
             }*/
        }

        public static void UpdateStructures(GameTime gameTime)
        {
            for (int i = 0; i < Structures.Count; i++)
            {
                Structure structure = Structures[i];
                if (structure.IsDead)
                {
                    RemoveStructure(structure);
                    DeadStructures.Add(structure);
                    i--;
                }
                else
                {
                    structure.Update(gameTime);
                }
            }
        }

        int updateVisibilityDelay = 50, timeSinceLastUpdateVisibility = 50;
        void Update(GameTime gameTime)
        {
            // update visibility
            timeSinceLastUpdateVisibility += (int)(gameTime.ElapsedGameTime.TotalMilliseconds * Rts.GameSpeed);
            if (timeSinceLastUpdateVisibility >= updateVisibilityDelay)
            {
                timeSinceLastUpdateVisibility -= updateVisibilityDelay;
                UpdateVisibility();
            }

            // update construction
            if (UnderConstruction)
            {
                updateConstruction(gameTime);
                return;
            }

            // update build queue
            if (BuildQueue.Count > 0)
            {
                updateBuildQueue(gameTime);
            }
        }

        float timeSinceStatusUpdate, statusUpdateDelay = .5f;
        public void CheckForStatusUpdate(GameTime gameTime, NetPeer netPeer, NetConnection connection)
        {
            timeSinceStatusUpdate += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (HpChanged && timeSinceStatusUpdate >= statusUpdateDelay)
            {
                timeSinceStatusUpdate = 0f;
                HpChanged = false;

                NetOutgoingMessage msg = netPeer.CreateMessage();
                msg.Write(MessageID.STRUCTURE_STATUS_UPDATE);
                msg.Write(ID);
                msg.Write(Team);
                msg.Write((short)Hp);
                netPeer.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);
            }
        }

        int timeSinceConstructionHpTick = 0;
        void updateConstruction(GameTime gameTime)
        {
            constructionTimeElapsed += (int)(gameTime.ElapsedGameTime.TotalMilliseconds * Rts.GameSpeed);

            PercentDone = constructionTimeElapsed / (float)BuildTime;

            timeSinceConstructionHpTick += (int)(gameTime.ElapsedGameTime.TotalMilliseconds * Rts.GameSpeed);
            if (timeSinceConstructionHpTick >= 125)
            {
                timeSinceConstructionHpTick -= 125;
                preciseHpCounter += constructionHpPerTick;
                int hpToAdd = (int)preciseHpCounter;
                preciseHpCounter -= hpToAdd;
                Hp += hpToAdd;
            }

            if (constructionTimeElapsed >= BuildTime)
            {
                finishConstruction();
            }
        }

        void finishConstruction()
        {
            Player.Players[Team].MaxSupply += type.Supply;
            UnderConstruction = false;
            PercentDone = 1f;

            if (!HasTakenDamageEver)
                Hp = MaxHp;

            releaseBuilder();
            ((Rts)Game1.Game.CurrentGameState).CheckForResetCommandCardWhenStructureCompletes(this);
            CogWheel = null;
        }

        void releaseBuilder()
        {
            if (IsDead)
                Builder.Position = CenterPoint;
            else
            {
                if (Builder.Commands.Count > 0 && Builder.Commands[0] is MoveCommand)
                {
                    MoveCommand command = Builder.Commands[0] as MoveCommand;
                    float angle = (float)Math.Atan2(command.Destination.Y - CenterPoint.Y, command.Destination.X - CenterPoint.X);
                    angle = Util.ConvertToPositiveRadians(angle);

                    if (angle < MathHelper.TwoPi / 4)
                        Builder.Position = new Vector2(Rectangle.X + Rectangle.Width - Rts.map.TileSize / 2, Rectangle.Y + Rectangle.Height - Rts.map.TileSize / 2);
                    else if (angle < MathHelper.TwoPi / 2)
                        Builder.Position = new Vector2(Rectangle.X + Rts.map.TileSize / 2, Rectangle.Y + Rectangle.Height - Rts.map.TileSize / 2);
                    else if (angle < MathHelper.TwoPi * .75f)
                        Builder.Position = new Vector2(Rectangle.X + Rts.map.TileSize / 2, Rectangle.Y + Rts.map.TileSize / 2);
                    else
                        Builder.Position = new Vector2(Rectangle.X + Rectangle.Width - Rts.map.TileSize / 2, Rectangle.Y + Rts.map.TileSize / 2);
                }
                else
                {
                    Builder.Position = new Vector2(Rectangle.X + Rts.map.TileSize / 2, Rectangle.Y + Rectangle.Height - Rts.map.TileSize / 2);
                }
            }

            ((WorkerNublet)Builder).FinishBuildingStructure();

            Builder = null;
        }

        public void AddToBuildQueue(ProductionButtonType buttonType, short id)
        {
                BuildQueue.Add(new BuildQueueItem(buttonType, id, buttonType.BuildTime));

                if (BuildQueue.Count == 1)
                {
                    BuildUnitButtonType unitButtonType = BuildQueue[0].Type as BuildUnitButtonType;
                    if (unitButtonType != null)
                    {
                        if (Player.Players[Team].CurrentSupply + unitButtonType.UnitType.SupplyCost <= Player.Players[Team].MaxSupply)
                        {
                            Player.Players[Team].CurrentSupply += unitButtonType.UnitType.SupplyCost;
                            BuildQueue[0].Started = true;
                        }
                    }
                }
        }

        public bool CanAddToBuildQueue(ProductionButtonType buttonType)
        {
            if (UnderConstruction)
                return false;

            if (BuildQueue.Count < MAX_QUEUE_SIZE)
            {
                if (BuildQueue.Count == 1)
                {
                    BuildUnitButtonType unitButtonType = BuildQueue[0].Type as BuildUnitButtonType;
                    if (unitButtonType != null)
                    {
                        //Player.Players[Team].CurrentSupply += unitButtonType.UnitType.SupplyCost;
                        if (Player.Players[Team].CurrentSupply + unitButtonType.UnitType.SupplyCost <= Player.Players[Team].MaxSupply)
                        {
                        }
                    }
                }

                return true;
            }

            return false;
        }

        public void RemoveFromBuildQueue(int index)
        {
            if (BuildQueue.Count <= index)
                return;

            BuildUnitButtonType unitButtonType = BuildQueue[0].Type as BuildUnitButtonType;
            if (unitButtonType != null)
            {
                if (index == 0)
                {
                    if (BuildQueue[0].Started)
                        Player.Players[Team].CurrentSupply -= unitButtonType.UnitType.SupplyCost;

                    if (BuildQueue.Count > 1)
                    {
                        unitButtonType = BuildQueue[1].Type as BuildUnitButtonType;
                        if (unitButtonType != null)
                        {
                            if (Player.Players[Team].CurrentSupply + unitButtonType.UnitType.SupplyCost <= Player.Players[Team].MaxSupply)
                            {
                                Player.Players[Team].CurrentSupply += unitButtonType.UnitType.SupplyCost;
                                BuildQueue[1].Started = true;
                            }
                        }
                        //Player.Players[Team].CurrentSupply += unitButtonType.UnitType.SupplyCost;
                    }
                }

                Player.Players[Team].Roks += unitButtonType.UnitType.RoksCost;
            }
            else if (BuildQueue.Count > 1)
                BuildQueue[1].Started = true;

            BuildQueue.RemoveAt(index);
        }

        void updateBuildQueue(GameTime gameTime)
        {
            BuildQueueItem currentItem = BuildQueue[0];

            /*BuildUnitButtonType unitButtonType = BuildQueue[0].Type as BuildUnitButtonType;
            if (unitButtonType != null)
            {
                if (Player.Players[Team].CurrentSupply + unitButtonType.UnitType.SupplyCost > Player.Players[Team].MaxSupply)
                {
                }
                else
                    currentItem.UpdateTime(gameTime);
            }
            else*/
            if (!currentItem.Started)
            {
                BuildUnitButtonType unitButtonType = currentItem.Type as BuildUnitButtonType;
                if (unitButtonType != null)
                {
                    if (Player.Players[Team].CurrentSupply + unitButtonType.UnitType.SupplyCost <= Player.Players[Team].MaxSupply)
                    {
                        Player.Players[Team].CurrentSupply += unitButtonType.UnitType.SupplyCost;
                        BuildQueue[0].Started = true;
                    }
                }
                else
                    BuildQueue[0].Started = true;
            }

            currentItem.UpdateTime(gameTime);

            if (currentItem.Done)
            {
                completeBuildQueueItem(BuildQueue[0]);
                BuildQueue.RemoveAt(0);

                if (BuildQueue.Count > 0)
                {
                    BuildUnitButtonType unitButtonType = BuildQueue[0].Type as BuildUnitButtonType;
                    if (unitButtonType != null)
                    {
                        if (Player.Players[Team].CurrentSupply + unitButtonType.UnitType.SupplyCost <= Player.Players[Team].MaxSupply)
                        {
                            Player.Players[Team].CurrentSupply += unitButtonType.UnitType.SupplyCost;
                            BuildQueue[0].Started = true;
                        }
                    }
                    else
                        BuildQueue[0].Started = true;
                }
            }
        }

        void completeBuildQueueItem(BuildQueueItem item)
        {
            BuildUnitButtonType buttonType = item.Type as BuildUnitButtonType;
            if (buttonType != null)
            {
                Unit unit;
                if (buttonType.UnitType == UnitType.MeleeNublet)
                    unit = new MeleeNublet(new Vector2(), Team, item.ID);
                else if (buttonType.UnitType == UnitType.RangedNublet)
                    unit = new RangedNublet(new Vector2(), Team, item.ID);
                else
                    unit = new WorkerNublet(new Vector2(), Team, item.ID);

                float angle = 0;

                Vector2 spawnLocation;
                if (rallyPoints.Count > 0)
                {
                    angle = (float)Math.Atan2(rallyPoints[0].Point.Y - CenterPoint.Y, rallyPoints[0].Point.X - CenterPoint.X);
                    angle = Util.ConvertToPositiveRadians(angle);

                    /*if (angle < MathHelper.TwoPi / 4)
                        spawnLocation = new Vector2(Rectangle.X + Rectangle.Width - map.TileSize, Rectangle.Y + Rectangle.Height - map.TileSize / 2);
                    else if (angle < MathHelper.TwoPi / 2)
                        spawnLocation = new Vector2(Rectangle.X + map.TileSize, Rectangle.Y + Rectangle.Height - map.TileSize / 2);
                    else if (angle < MathHelper.TwoPi * .75f)
                        spawnLocation = new Vector2(Rectangle.X + map.TileSize, Rectangle.Y + map.TileSize / 2);
                    else
                        spawnLocation = new Vector2(Rectangle.X + Rectangle.Width - map.TileSize, Rectangle.Y + map.TileSize / 2);*/

                    PathNode closestPathNode = null;
                    float closest = float.MaxValue;

                    foreach (PathNode pathNode in exitPathNodes)
                    {
                        float distance = Vector2.Distance(pathNode.Tile.CenterPoint, RallyPoints[0].Point);
                        if (distance < closest)
                        {
                            closestPathNode = pathNode;
                            closest = distance;
                        }
                    }

                    if (closestPathNode != null)
                        spawnLocation = closestPathNode.Tile.CenterPoint;
                    else if (exitPathNodes.Count > 0)
                        spawnLocation = exitPathNodes[0].Tile.CenterPoint;
                    else
                        spawnLocation = new Vector2(Rectangle.X + Rts.map.TileSize, Rectangle.Y + Rectangle.Height - Rts.map.TileSize / 2);
                }
                else
                {
                    spawnLocation = new Vector2(Rectangle.X + Rts.map.TileSize, Rectangle.Y + Rectangle.Height - Rts.map.TileSize / 2);
                }

                unit.CenterPoint = new Vector2(spawnLocation.X, spawnLocation.Y);
                unit.Rotation = angle;
                unit.InitializeCurrentPathNode();

                if (rallyPoints.Count == 0)
                {
                    unit.CheckForWallHit();
                    unit.CheckForPush();
                }
                else
                {
                    MoveCommand command = null;

                    if (rallyPoints[0].Resource != null && unit is WorkerNublet)
                        command = new HarvestCommand(unit, rallyPoints[0].Resource, 1);
                    else
                        command = new MoveCommand(unit, RallyPoints[0].Point, 1);

                    if (command != null)
                    {
                        unit.GiveCommand(command);
                        Rts.pathFinder.AddHighPriorityPathFindRequest(command, (int)Vector2.DistanceSquared(centerPoint, command.Destination), false);
                    }

                    for (int i = 1; i < RallyPoints.Count; i++)
                    {
                        if (rallyPoints[i].Resource != null && unit is WorkerNublet)
                            unit.QueueCommand(new HarvestCommand(unit, rallyPoints[i].Resource, 1));
                        else
                            unit.QueueCommand(new MoveCommand(unit, RallyPoints[i].Point, 1));
                    }

                    unit.CheckForWallHit();
                }
            }
        }

        public override void Die()
        {
            base.Die();

            if (!UnderConstruction)
                Player.Players[Team].MaxSupply -= type.Supply;

            foreach (Structure structure in Structures)
                structure.HasMoved = true;

            UnitAnimation a = new UnitAnimation(this, Width, .5f, true, Structure.Explosion1Textures);
            a.Start();

            foreach (PathNode node in OccupiedPathNodes)
            {
                node.Blocked = false;
                node.Blocker = null;
            }

            if (Builder != null)
            {
                releaseBuilder();
                ((Rts)Game1.Game.CurrentGameState).CheckForResetCommandCardWhenStructureCompletes(this);
                CogWheel = null;
            }

            NetOutgoingMessage msg = Rts.netPeer.CreateMessage();
            msg.Write(MessageID.STRUCTURE_DEATH);
            msg.Write(ID);
            msg.Write(Team);
            Rts.netPeer.SendMessage(msg, Rts.connection, NetDeliveryMethod.ReliableUnordered);
        }

        public void Cancel()
        {
            Die();
            Player.Players[Team].Roks += type.RoksCost;
        }

        public static List<Structure> Structures
        {
            get
            {
                return structures;
            }
        }
        static void AddStructure(Structure s)
        {
            structures.Add(s);

            lock (RtsObject.RtsObjects)
            {
                RtsObject.AddObject(s);
            }
        }
        public static void RemoveStructure(Structure s)
        {
            structures.Remove(s);

            RtsObject.RemoveObject(s);
        }

        /*new public int X
        {
            get
            {
                return x;
            }
        }
        new public int Y
        {
            get
            {
                return y;
            }
        }*/
        public override RtsObjectType Type
        {
            get
            {
                return type;
            }
        }
        public List<RallyPoint> RallyPoints
        {
            get
            {
                return rallyPoints;
            }
        }
        public bool Rallyable
        {
            get
            {
                return rallyable;
            }
        }

        public override string Name
        {
            get
            {
                return type.Name;
            }
        }
    }

    public class BuildQueueItem
    {
        public readonly ProductionButtonType Type;
        public readonly short ID;

        public readonly int BuildTime;
        public bool Started;

        int timeElapsed, percentDone;
        bool done;

        public BuildQueueItem(ProductionButtonType commandType, short id, int buildTime)
        {
            Type = commandType;
            ID = id;
            BuildTime = buildTime;
        }

        public void UpdateTime(GameTime gameTime)
        {
            if (!Started)
                return;

            timeElapsed += (int)(gameTime.ElapsedGameTime.TotalMilliseconds * Rts.GameSpeed);

            if (timeElapsed >= BuildTime)
            {
                done = true;
                percentDone = 100;
            }
            else
            {
                percentDone = (int)(timeElapsed / (float)BuildTime * 100);
            }
        }

        public bool Done
        {
            get
            {
                return done;
            }
        }
        public int PercentDone
        {
            get
            {
                return percentDone;
            }
        }
    }

    public class StructureCogWheel
    {
        public Structure Structure { get; private set; }
        public float Rotation = 0;
        public Rectangle Rectangle { get; private set; }

        public StructureCogWheel(Structure structure, int size)
        {
            Structure = structure;
            //Rectangle = new Rectangle((int)(structure.CenterPointX - size / 2), (int)(structure.CenterPointY - size / 2), size, size);
            Rectangle = new Rectangle((int)(structure.CenterPointX), (int)(structure.CenterPointY), size, size);
        }
    }

    public class RallyPoint
    {
        public Resource Resource { get; private set; }
        public Vector2 Point { get; private set; }

        public RallyPoint(Vector2 point, Resource resource)
        {
            Point = point;
            Resource = resource;
        }
    }
}
