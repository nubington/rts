using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Lidgren.Network;

namespace rts
{
    public class ResourceType
    {
        public static readonly ResourceType
            Roks;

        static ResourceType()
        {
            Roks = new ResourceType();
            Roks.Name = "Roks";
            Roks.NormalTexture = Game1.Game.Content.Load<Texture2D>("WC2Gold");
            Roks.DepletedTexture = Game1.Game.Content.Load<Texture2D>("WC2Gold");
            Roks.CargoTexture = Game1.Game.Content.Load<Texture2D>("rock");
            Roks.AmountOfResources = 2500;
            Roks.Size = 3;
        }

        public Texture2D NormalTexture, DepletedTexture, CargoTexture;

        public int AmountOfResources { get; private set; }

        public string Name { get; private set; }

        public int Size { get; private set; }
    }

    public abstract class Resource : BaseObject
    {
        public static Resource[] ResourceArray = new Resource[1024];

        public static List<Resource> Resources { get; private set; }
        public ResourceType Type { get; private set; }
        public short ID { get; private set; }
        public bool AmountChanged;

        public List<PathNode> OccupiedPathNodes = new List<PathNode>();

        int amount;
        new public int X, Y;
        int Size;
        public float Radius { get; private set; }
        public bool Depleted { get; protected set; }

        static Resource()
        {
            Resources = new List<Resource>();
        }

        static short idCounter;
        public Resource(ResourceType type, Point location, int size)
            : base(new Rectangle(location.X * Rts.map.TileSize, location.Y * Rts.map.TileSize, size * Rts.map.TileSize, size * Rts.map.TileSize))
        {
            ID = idCounter++;
            ResourceArray[ID] = this;

            Type = type;
            Texture = type.NormalTexture;
            Amount = type.AmountOfResources;
            Size = size;
            Radius = (size * Rts.map.TileSize) / 2f;

            X = location.X;
            Y = location.Y;

            setOccupiedPathNodes();

            Resources.Add(this);
        }

        protected float timeSinceStatusUpdate, statusUpdateDelay = 1f;
        public void checkForStatusUpdate(NetPeer netPeer, NetConnection connection, int team)
        {
            if (AmountChanged && ((team == Player.Me.Team && timeSinceStatusUpdate >= statusUpdateDelay) || Depleted))
            {
                timeSinceStatusUpdate = 0f;
                AmountChanged = false;

                NetOutgoingMessage msg = netPeer.CreateMessage();
                msg.Write(MessageID.RESOURCE_STATUS_UPDATE);
                msg.Write(ID);
                msg.Write((short)Amount);
                netPeer.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);
            }
        }

        void setOccupiedPathNodes()
        {
            for (int x = X; x < X + Size; x++)
            {
                for (int y = Y; y < Y + Size; y++)
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
        }

        protected virtual void deplete()
        {
            Depleted = true;

            foreach (PathNode pathNode in OccupiedPathNodes)
            {
                pathNode.Blocked = false;
                pathNode.Blocker = null;
            }

            ResourceArray[ID] = null;
        }

        public static void UpdateResources(GameTime gameTime)
        {
            for (int i = 0; i < Resources.Count; )
            {
                Resource r = Resources[i];
                if (r.Depleted)
                {
                    RemoveResource(r);
                }
                else
                {
                    r.Update(gameTime);
                    i++;
                }
            }
        }

        protected virtual void Update(GameTime gameTime)
        {
        }

        public static void RemoveResource(Resource r)
        {
            Resources.Remove(r);

            Roks roks = r as Roks;
            if (roks != null)
                Roks.AllRoks.Remove(roks);
        }

        public int Amount
        {
            get
            {
                return amount;
            }
            set
            {
                amount = (int)MathHelper.Max(value, 0);
                if (!Depleted && amount == 0)
                    deplete();

                AmountChanged = true;
            }
        }
        public string Name
        {
            get
            {
                return Type.Name;
            }
        }
    }

    public class Roks : Resource
    {
        public static List<Roks> AllRoks { get; private set; }
        public List<PathNode> exitPathNodes = new List<PathNode>();
        static int allowEntranceDelay = 250, harvestDelay = 2000;

        public const int CARGO_PER_TRIP = 1;

        static Roks()
        {
            AllRoks = new List<Roks>();
        }

        List<WorkerNublet> workersInside = new List<WorkerNublet>();
        List<int> workerTimes = new List<int>();

        public Roks(Point location)
            : base(ResourceType.Roks, location, ResourceType.Roks.Size)
        {
            SetExitPathNodes();

            foreach (Roks roks in Roks.AllRoks)
                roks.SetExitPathNodes();

            AllRoks.Add(this);
        }

        int timeSinceLastEntrance = allowEntranceDelay;
        protected override void Update(GameTime gameTime)
        {
            timeSinceLastEntrance += (int)(gameTime.ElapsedGameTime.TotalMilliseconds * Rts.GameSpeed);
            timeSinceStatusUpdate += (float)gameTime.ElapsedGameTime.TotalSeconds;

            for (int i = 0; i < workersInside.Count; )
            {
                workerTimes[i] += (int)(gameTime.ElapsedGameTime.TotalMilliseconds * Rts.GameSpeed);

                if (workerTimes[i] >= harvestDelay)
                {
                    releaseWorker(workersInside[i]);
                    workerTimes.Remove(workerTimes[i]);
                }
                else
                    i++;
            }
        }

        public void SetExitPathNodes()
        {
            for (int x = X - 1; x <= X + Type.Size; x++)
            {
                if (x < 0 || x > Rts.map.Width - 1)
                    continue;

                for (int y = Y - 1; y <= Y + Type.Size; y++)
                {
                    if (y < 0 || y > Rts.map.Height - 1)
                        continue;

                    PathNode pathNode = Rts.pathFinder.PathNodes[y, x];
                    if (pathNode.Tile.Walkable && !pathNode.Blocked)
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
        }

        public bool CheckForEntrance(WorkerNublet worker)
        {
            if (worker.CargoAmount == CARGO_PER_TRIP && worker.CargoType == Type)
            {
                TownHall townHall = findNearestTownHall(worker);
                if (townHall != null)
                    worker.InsertCommand(new ReturnCargoCommand(worker, townHall, this, 1));
                else
                    worker.NextCommand();
                return false;
            }

            if (Amount - (workersInside.Count * CARGO_PER_TRIP) <= 0)
                return false;

            if (timeSinceLastEntrance >= allowEntranceDelay)
            {
                timeSinceLastEntrance = 0;

                letWorkerEnter(worker);

                return true;
            }

            return false;
        }

        void letWorkerEnter(WorkerNublet worker)
        {
            Rts.SelectedUnits.Remove(worker);
            workersInside.Add(worker);
            workerTimes.Add(0);
            worker.CenterPoint = centerPoint;
        }

        void releaseWorker(WorkerNublet worker)
        {
            worker.CargoType = Type;

            int oldAmount = Amount;
            Amount -= CARGO_PER_TRIP;
            worker.CargoAmount = oldAmount - Amount;

            // only decrement amount if worker is on my team,
            // else rely on status updates for amount
            if (worker.Team != Player.Me.Team)
                Amount = oldAmount;

            //int newAmount = (int)(MathHelper.Max(Amount - CARGO_PER_TRIP, 0));
            //worker.CargoAmount = Amount - newAmount;
            //Amount = newAmount;

            workersInside.Remove(worker);

            TownHall townHall = findNearestTownHall(worker);

            float angle;
            if (townHall == null)
                angle = 0;
            else
                angle = (float)Math.Atan2(townHall.Rectangle.Y - CenterPoint.Y, townHall.Rectangle.X - CenterPoint.X);

            PathNode closestPathNode = null;
            if (townHall != null)
            {
                float closest = float.MaxValue;

                foreach (PathNode pathNode in exitPathNodes)
                {
                    float distance = Vector2.Distance(pathNode.Tile.CenterPoint, townHall.CenterPoint);
                    if (distance < closest)
                    {
                        closestPathNode = pathNode;
                        closest = distance;
                    }
                }
            }

            if (closestPathNode != null)
                worker.CenterPoint = closestPathNode.Tile.CenterPoint;
            else if (exitPathNodes.Count > 0)
                worker.CenterPoint = exitPathNodes[0].Tile.CenterPoint;
            else
                worker.CenterPoint = centerPoint;

            //worker.CenterPoint = centerPoint + new Vector2((Radius + worker.Radius) * (float)Math.Cos(angle), (Radius + worker.Radius) * (float)Math.Sin(angle));
            worker.Rotation = angle;

            worker.FinishHarvesting();

            if (townHall != null && worker.Commands.Count <= 0)
            {
                //worker.GiveCommand(new ReturnCargoCommand(townHall, 1));
                //worker.QueueCommand(new HarvestCommand(this, 1));
                worker.ReturnCargoToNearestTownHall(this);
            }

            checkForStatusUpdate(Rts.netPeer, Rts.connection, worker.Team);
        }

        protected override void deplete()
        {
 	        base.deplete();

            //foreach (WorkerNublet worker in workersInside)
            //{
            //
            //}
        }

        TownHall findNearestTownHall(Unit unit)
        {
            TownHall nearestTownHall = null;
            float nearest = int.MaxValue;

            foreach (TownHall townHall in TownHall.TownHalls)
            {
                if (townHall.Team != unit.Team || townHall.UnderConstruction)
                    continue;

                float distance = Vector2.DistanceSquared(CenterPoint, townHall.CenterPoint);
                if (distance < nearest)
                {
                    nearestTownHall = townHall;
                    nearest = distance;
                }
            }

            return nearestTownHall;
        }

        public override Texture2D Texture
        {
            get
            {
                if (!Depleted)
                    return Type.NormalTexture;
                else
                    return Type.DepletedTexture;
            }
            set
            {
                base.Texture = value;
            }
        }
    }
}
