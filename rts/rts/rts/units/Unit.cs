using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Lidgren.Network;

namespace rts
{
    /*enum UnitType
    {
        MeleeNublet, RangedNublet
    };*/

    public class Unit : RtsObject
    {
        public static List<Unit> units = new List<Unit>();
        public static List<Unit> unitsSorted = new List<Unit>();
        public static List<Unit> DeadUnits = new List<Unit>();
        public static UnitCollisionSweeper UnitCollisionSweeper = new UnitCollisionSweeper();
        public List<BaseObject> potentialCollisions = new List<BaseObject>();

        public Texture2D BulletTexture;
        public int BulletSize, BulletSpeed;
        public static Texture2D[] Explosion1Textures;

        public short ID;
        UnitType type;
        public int KillCount;
        public bool Busy { get; protected set; }
        public bool IgnoringCollision { get; protected set; }
        public float MaxSpeed { get; private set; }
        public float Speed { get; protected set; }
        protected float acceleration;

        public List<UnitCommand> Commands = new List<UnitCommand>();
        protected Vector2 lastWayPoint, lastMoveDestination;
        //BaseObject followTarget;
        //BaseObject attackTarget;

        //bool isBlocked;
        //bool isIdle = true, isMoving, isFollowing, isAttacking, isAttackMoving;
        bool isWithinRangeOfTarget, avoidingUnits;

        public Unit(UnitType type, Vector2 position, short team, short id)
            : base(position, type.Size, team)
        {
            ID = id;
            //ID = Player.Players[team].UnitIDCounter++;
            Player.Players[team].UnitArray[ID] = this;

            this.type = type;
            //this.speed = new Vector2(type.MoveSpeed, type.MoveSpeed);
            MaxSpeed = type.MoveSpeed;
            acceleration = MaxSpeed * 6f;
            Texture = type.NormalTexture;
            //BulletTexture = type.BulletTexture;
            BulletSize = type.BulletSize;
            BulletSpeed = type.BulletSpeed;
            AttackDamage = type.AttackDamage;
            AttackRange = type.AttackRange;
            AttackDelay = type.AttackDelay;
            Hp = MaxHp = type.Hp;
            Armor = type.Armor;
            SightRange = type.SightRange;
            BuildTime = type.BuildTime;
            SelectionSortValue = type.SelectionSortValue;
            TargetPriority = type.TargetPriority;
            //Player.Players[Team].CurrentSupply += type.SupplyCost;
            
            AddUnit(this);
            //InitializeCurrentPathNode();
        }

        /*public Unit(UnitType type, Vector2 position, int size, float speed, int team)
            : this(type, position, size, team)
        {
            this.speed = new Vector2(speed, speed);
        }*/

        public void InitializeCurrentPathNode()
        {
            /*int y = (int)MathHelper.Clamp(CenterPoint.Y / Map.TileSize, 0, Map.Height - 1);
            int x = (int)MathHelper.Clamp(CenterPoint.X / Map.TileSize, 0, Map.Width - 1);

            try
            {
                CurrentPathNode = PathFinder.PathNodes[y, x];

                if (!CurrentPathNode.Tile.Walkable)
                    CurrentPathNode = PathFinder.FindNearestPathNode(y, x);

                CurrentPathNode.UnitsContained.Add(this);
            }
            catch (Exception)
            {
                throw new Exception("unable to find path node containing unit");
            }*/

            int y = (int)MathHelper.Clamp(CenterPoint.Y / Rts.map.TileSize, 0, Rts.map.Height - 1);
            int x = (int)MathHelper.Clamp(CenterPoint.X / Rts.map.TileSize, 0, Rts.map.Width - 1);

            CurrentPathNode = Rts.pathFinder.PathNodes[y, x];

            // fill OccupiedPathNodes
            //-------------------------------------------------------------------------------
            int posX = (int)MathHelper.Clamp(X / Rts.map.TileSize, 0, Rts.map.Width - 1);
            int posY = (int)MathHelper.Clamp(Y / Rts.map.TileSize, 0, Rts.map.Height - 1);
            int rightBoundX = (int)MathHelper.Clamp(posX + (int)Math.Ceiling(Width / (double)Rts.map.TileSize), 0, Rts.map.Width - 1);
            int bottomBoundY = (int)MathHelper.Clamp(posY + (int)Math.Ceiling(Height / (double)Rts.map.TileSize), 0, Rts.map.Height - 1);

            OccupiedPathNodes.Clear();
            for (x = posX; x <= rightBoundX; x++)
            {
                for (y = posY; y <= bottomBoundY; y++)
                {
                    PathNode neighbor = Rts.pathFinder.PathNodes[y, x];
                    if (Intersects(neighbor.Tile))
                        OccupiedPathNodes.Add(neighbor);
                }
            }
            //-------------------------------------------------------------------------------

            // fill PathNodeBufferSquare
            //-------------------------------------------------------------------------------
            posX = (int)MathHelper.Clamp(X / Rts.map.TileSize - 1, 0, Rts.map.Width - 1);
            posY = (int)MathHelper.Clamp(Y / Rts.map.TileSize - 1, 0, Rts.map.Height - 1);
            rightBoundX = (int)MathHelper.Clamp((int)((X + Width) / (double)Rts.map.TileSize + 1), 0, Rts.map.Width - 1);
            bottomBoundY = (int)MathHelper.Clamp((int)((Y + Height) / (double)Rts.map.TileSize + 1), 0, Rts.map.Height - 1);

            PathNodeBufferSquare.Clear();
            for (x = posX; x <= rightBoundX; x++)
            {
                for (y = posY; y <= bottomBoundY; y++)
                {
                    PathNodeBufferSquare.Add(Rts.pathFinder.PathNodes[y, x]);
                }
            }
            //-------------------------------------------------------------------------------

            foreach (PathNode pathNode in OccupiedPathNodes)
                pathNode.UnitsContained.Add(this);

            /*foreach (PathNode neighbor in CurrentPathNode.Neighbors)
            {
                //if (Intersects(neighbor.Tile))
                if (Vector2.Distance(centerPoint, neighbor.Tile.CenterPoint) < Diameter)
                    OccupiedPathNodes.Add(neighbor);
            }*/

        }

        protected void updateCurrentPathNode()
        {
            foreach (PathNode pathNode in OccupiedPathNodes)
                pathNode.UnitsContained.Remove(this);

            int y = (int)MathHelper.Clamp(CenterPoint.Y / Rts.map.TileSize, 0, Rts.map.Height - 1);
            int x = (int)MathHelper.Clamp(CenterPoint.X / Rts.map.TileSize, 0, Rts.map.Width - 1);

            CurrentPathNode = Rts.pathFinder.PathNodes[y, x];

            //if (!CurrentPathNode.Tile.Walkable)
            //if (!PathFinder.IsTileWalkable(CurrentPathNode.Tile.Y, CurrentPathNode.Tile.X, this))
                //CurrentPathNode = PathFinder.FindNearestPathNode(y, x);
                //CurrentPathNode = PathFinder.FindNearestPathNode(y, x, this);
                //checkForWallHit();

            //if (type == UnitType.MeleeNublet)
            //    Diameter += .25f;

            // fill OccupiedPathNodes
            //-------------------------------------------------------------------------------
            int posX = (int)MathHelper.Clamp(X / Rts.map.TileSize, 0, Rts.map.Width - 1);
            int posY = (int)MathHelper.Clamp(Y / Rts.map.TileSize, 0, Rts.map.Height - 1);
            int rightBoundX = (int)MathHelper.Clamp(posX + (int)Math.Ceiling(Width / (double)Rts.map.TileSize), 0, Rts.map.Width - 1);
            int bottomBoundY = (int)MathHelper.Clamp(posY + (int)Math.Ceiling(Height / (double)Rts.map.TileSize), 0, Rts.map.Height - 1);

            OccupiedPathNodes.Clear();
            for (x = posX; x <= rightBoundX; x++)
            {
                for (y = posY; y <= bottomBoundY; y++)
                {
                    PathNode neighbor = Rts.pathFinder.PathNodes[y, x];
                    if (Intersects(neighbor.Tile))
                        OccupiedPathNodes.Add(neighbor);
                }
            }
            //-------------------------------------------------------------------------------

            // fill PathNodeBufferSquare
            //-------------------------------------------------------------------------------
            posX = (int)MathHelper.Clamp(X / Rts.map.TileSize - 1, 0, Rts.map.Width - 1);
            posY = (int)MathHelper.Clamp(Y / Rts.map.TileSize - 1, 0, Rts.map.Height - 1);
            //rightBoundX = (int)MathHelper.Clamp(posX + (int)Math.Ceiling(Width / (double)Map.TileSize) + 2, 0, Map.Width - 1);
            //bottomBoundY = (int)MathHelper.Clamp(posY + (int)Math.Ceiling(Height / (double)Map.TileSize) + 2, 0, Map.Height - 1);
            rightBoundX = (int)MathHelper.Clamp((int)((X + Width) / (double)Rts.map.TileSize + 1), 0, Rts.map.Width - 1);
            bottomBoundY = (int)MathHelper.Clamp((int)((Y + Height) / (double)Rts.map.TileSize + 1), 0, Rts.map.Height - 1);

            PathNodeBufferSquare.Clear();
            for (x = posX; x <= rightBoundX; x++)
            {
                for (y = posY; y <= bottomBoundY; y++)
                {
                    PathNodeBufferSquare.Add(Rts.pathFinder.PathNodes[y, x]);
                }
            }
            //-------------------------------------------------------------------------------
            
            /*foreach (PathNode neighbor in CurrentPathNode.Neighbors)
            {
                //if (Vector2.Distance(centerPoint, neighbor.Tile.CenterPoint) < Diameter)
                if (Intersects(neighbor.Tile))
                    OccupiedPathNodes.Add(neighbor);
            }*/

            foreach (PathNode pathNode in OccupiedPathNodes)
                pathNode.UnitsContained.Add(this);

            // prevent getting stuck in walls

            bool stuck = true;
            foreach (PathNode pathNode in OccupiedPathNodes)
            {
                if (pathNode.Tile.Walkable && !pathNode.Blocked)
                {
                    stuck = false;
                    break;
                }
            }

            if (stuck)
            {
                CurrentPathNode = Rts.pathFinder.Tools.FindNearestPathNode(CurrentPathNode.Tile.Y, CurrentPathNode.Tile.X);//, this);
                CenterPoint = CurrentPathNode.Tile.CenterPoint;
            }
            /*if (!CurrentPathNode.Tile.Rectangle.Intersects(Rectangle))
            {
                CurrentPathNode = PathFinder.FindNearestPathNode(y, x);
                CenterPoint = CurrentPathNode.Tile.CenterPoint;
            }*/

            foreach (BoundingBox box in OccupiedBoundingBoxes)
                box.UnitsContained.Remove(this);
            OccupiedBoundingBoxes.Clear();

            x = (int)MathHelper.Clamp(X / Rts.map.TileSize / Map.BOUNDING_BOX_SIZE, 0, Rts.map.Width - 1);
            y = (int)MathHelper.Clamp(Y / Rts.map.TileSize / Map.BOUNDING_BOX_SIZE, 0, Rts.map.Height - 1);
            Rts.map.BigBoundingBoxes[y, x].UnitsContained.Add(this);
            OccupiedBoundingBoxes.Add(Rts.map.BigBoundingBoxes[y, x]);

            x = (int)MathHelper.Clamp((X + Width) / Rts.map.TileSize / Map.BOUNDING_BOX_SIZE, 0, Rts.map.Width - 1);
            y = (int)MathHelper.Clamp(Y / Rts.map.TileSize / Map.BOUNDING_BOX_SIZE, 0, Rts.map.Height - 1);
            Rts.map.BigBoundingBoxes[y, x].UnitsContained.Add(this);
            OccupiedBoundingBoxes.Add(Rts.map.BigBoundingBoxes[y, x]);

            x = (int)MathHelper.Clamp(X / Rts.map.TileSize / Map.BOUNDING_BOX_SIZE, 0, Rts.map.Width - 1);
            y = (int)MathHelper.Clamp((Y + Height) / Rts.map.TileSize / Map.BOUNDING_BOX_SIZE, 0, Rts.map.Height - 1);
            Rts.map.BigBoundingBoxes[y, x].UnitsContained.Add(this);
            OccupiedBoundingBoxes.Add(Rts.map.BigBoundingBoxes[y, x]);

            x = (int)MathHelper.Clamp((X + Width) / Rts.map.TileSize / Map.BOUNDING_BOX_SIZE, 0, Rts.map.Width - 1);
            y = (int)MathHelper.Clamp((Y + Height) / Rts.map.TileSize / Map.BOUNDING_BOX_SIZE, 0, Rts.map.Height - 1);
            Rts.map.BigBoundingBoxes[y, x].UnitsContained.Add(this);
            OccupiedBoundingBoxes.Add(Rts.map.BigBoundingBoxes[y, x]);

        }

        //static int frameCount = 0;
        public static void UpdateUnits(GameTime gameTime, NetPeer netPeer, NetConnection connection)
        {
            //if (++frameCount % 10 == 0)
            //Util.UpdatePotentialCollisions(UnitsSorted);
            //if (++frameCount % 6 == 0)
            //    wallCollisionSweeper.UpdateNearbyWalls(UnitsSorted, Map.Walls);

            //potentialCollisionSweeper.UpdatePotentialCollisions(UnitsSorted);

            Rts.pathFinder.FinalizeAddingPathFindRequests();
            Rts.pathFinder.FulfillDonePathFindRequests(netPeer, connection);
            UnitCollisionSweeper.FulFillCollisionLists();
            //VisionUpdater.FulFillVisionLists();

            /*foreach (Unit unit in Units)
            {
                if (!unit.IsDead)
                    unit.Update(gameTime);
            }*/

            for (int i = 0; i < Units.Count; i++)
            {
                Unit unit = Units[i];
                if (unit.IsDead)
                {
                    RemoveUnit(unit);
                    DeadUnits.Add(unit);
                    i--;
                }
                else
                {
                    unit.Update(gameTime);
                }
            }
        }

        int recalculatePathDelay = 200, timeSinceLastRecalculatePath = 0;
        int lookForTargetDelay = 100, timeSinceLastLookForTarget = 0;
        int updateCurrentPathNodeDelay = 50, timeSinceUpdateCurrentPathNode = 0;
        void Update(GameTime gameTime)
        {
            int elapsedMilliseconds = (int)(gameTime.ElapsedGameTime.TotalMilliseconds * Rts.GameSpeed);
            timeSinceLastRecalculatePath += elapsedMilliseconds;
            timeSinceLastLookForTarget += elapsedMilliseconds;
            timeSinceLastAttack += elapsedMilliseconds;
            timeSinceUpdateCurrentPathNode += elapsedMilliseconds;

            if (timeSinceUpdateCurrentPathNode >= updateCurrentPathNodeDelay)
            {
                timeSinceUpdateCurrentPathNode = 0;

                updateCurrentPathNode();
                UpdateVisibility();
            }

            //CheckForWallHit();

            // decelerate when idle
            //if (!IsMoving)
            //    Speed = MathHelper.Max(Speed - Util.ScaleWithGameTime(acceleration, gameTime), 0);

            // look for target when idle
            if (Commands.Count == 0)
            {
                CheckForPush(false);

                if (timeSinceLastLookForTarget >= lookForTargetDelay)
                {
                    timeSinceLastLookForTarget = 0;

                    RtsObject target = FindNearestTarget();
                    if (target != null)
                        GiveCommand(new AttackMoveCommand(this, centerPoint, int.MaxValue));
                }
                return;
            }

            UnitCommand command = Commands[0];

            if (DoSpecialCommands(command, gameTime))
                return;
            if (command is StopCommand)
            {
                stop();
                return;
            }
            if (command is HoldPositionCommand)
            {
                if (timeSinceLastLookForTarget >= lookForTargetDelay)
                {
                    timeSinceLastLookForTarget = 0;

                    RtsObject target = FindNearestTarget();
                    if (target != null)
                        GiveCommand(new AttackCommand(this, target, true, true));
                }
                return;
            }
            else if (command is AttackCommand)
            {
                AttackCommand attackCommand = (AttackCommand)command;

                //if (attackCommand.Target.IsDead)
                //{
                //    if (attackCommand.HoldPosition)
                //        GiveCommand(new HoldPositionCommand());
                 //   else
                //        nextCommand();
                //}
                //else
                //{
                    Attack(attackCommand, gameTime);
                    performAttackIfStarted(attackCommand);
                //}
            }
            else if (command is AttackMoveCommand)
            {
                AttackMoveCommand attackMoveCommand = (AttackMoveCommand)command;

                AttackMove(attackMoveCommand, gameTime);
            }
            else if (command is MoveCommand)
            {
                MoveCommand moveCommand = (MoveCommand)command;
                /*if (timeSinceLastRecalculatePath >= recalculatePathDelay && moveCommand.Calculated)
                {
                    timeSinceLastRecalculatePath = 0;
                    PathFinder.AddLowPriorityPathFindRequest(this, moveCommand, CurrentPathNode, (int)Vector2.Distance(centerPoint, moveCommand.Destination), false);
                }*/
                //if (instanceFrameCount % reCalculatePathFrameDelay == 1)
                //    PathFinder.SmoothPath(moveCommand.WayPoints, this);
                Move(moveCommand, gameTime);
            }

            // update attack command destinations
            // and remove attack commands whose targets are dead or out of vision
            for (int i = 0; i < Commands.Count; i++)
            {
                AttackCommand c = Commands[i] as AttackCommand;
                if (c != null)
                {
                    if (c.Target == null || c.Target.IsDead)
                    {
                        Commands.Remove(c);
                        //Player.Players[Team].UnitCommands.Remove(c);
                        i--;
                    }
                    else
                        c.Destination = c.Target.CenterPoint;
                }
            }

            // update queued command starting points
            for (int i = 1; i < Commands.Count; i++)
            {
                MoveCommand c = Commands[i] as MoveCommand;
                MoveCommand previous = Commands[i - 1] as MoveCommand;
                if (c != null && previous != null)
                    c.WayPoints[0] = previous.Destination;
            }

            // recalculate queued paths
            /*if (command is MoveCommand && instanceFrameCount % (reCalculatePathFrameDelay) == 0)
            {
                MoveCommand moveCommand = (MoveCommand)command;
                for (int i = 1; i < Commands.Count; i++)
                {
                    MoveCommand c = Commands[i] as MoveCommand;
                    MoveCommand previousCommand = Commands[i - 1] as MoveCommand;
                    if (c != null && previousCommand != null)
                    {
                        int y = (int)MathHelper.Clamp(previousCommand.Destination.Y / Map.TileSize, 0, Map.Height - 1);
                        int x = (int)MathHelper.Clamp(previousCommand.Destination.X / Map.TileSize, 0, Map.Width - 1);

                        PathNode node = PathFinder.PathNodes[y, x];
                        if (!node.Tile.Walkable)
                            node = PathFinder.FindNearestPathNode(y, x);

                        c.WayPoints = PathFinder.FindPath(node, c.Destination, false);
                    }
                }
            }*/
        }

        float timeSinceStatusUpdate, statusUpdateDelay = .5f;
        public void CheckForStatusUpdate(GameTime gameTime, NetPeer netPeer, NetConnection connection)
        {
            timeSinceStatusUpdate += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (timeSinceStatusUpdate >= statusUpdateDelay)
            {
                timeSinceStatusUpdate = 0f;

                if (Team == Player.Me.Team)
                {
                    NetOutgoingMessage msg = netPeer.CreateMessage();
                    msg.Write(MessageID.UNIT_STATUS_UPDATE);
                    msg.Write(ID);
                    msg.Write(Team);
                    msg.Write((short)Hp);
                    msg.Write(centerPoint.X);
                    msg.Write(centerPoint.Y);
                    msg.Write(Rotation);
                    msg.Write(IsIdle);

                    // send cargoAmount, 0 if not worker
                    WorkerNublet worker = this as WorkerNublet;
                    if (worker != null)
                        msg.Write(worker.CargoAmount);
                    else
                        msg.Write((short)0);

                    netPeer.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);
                }
                else if (HpChanged)
                {
                    HpChanged = false;

                    NetOutgoingMessage msg = netPeer.CreateMessage();
                    msg.Write(MessageID.UNIT_HP_UPDATE);
                    msg.Write(ID);
                    msg.Write(Team);
                    msg.Write((short)Hp);
                    netPeer.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);
                }
            }
        }

        virtual protected bool DoSpecialCommands(UnitCommand command, GameTime gameTime)
        {
            return false;
        }

        public void NextCommand()
        {
            if (Commands.Count == 0)
                return;

            UnitCommand lastCommand = Commands[0];
            lastCommand.Active = false;
            Commands.RemoveAt(0);
            //Player.Players[Team].UnitCommands.Remove(lastCommand);

            IgnoringCollision = false;
            
            MoveCommand lastMoveCommand = lastCommand as MoveCommand;
            if (lastMoveCommand != null)
            {
                lastMoveDestination = lastWayPoint;

                if (Commands.Count > 0 && Commands[0] is MoveCommand)
                {
                    MoveCommand newMoveCommand = (MoveCommand)Commands[0];
                    //if (Team == Player.Me.Team)
                    Rts.pathFinder.AddPathFindRequest(newMoveCommand, false, false, false);
                    //newMoveCommand.WayPoints = PathFinder.FindPath(CurrentPathNode, newMoveCommand.Destination, false);
                    IgnoringCollision = (newMoveCommand is HarvestCommand || newMoveCommand is ReturnCargoCommand);
                    return;
                }
            }

            // look for new target immediately if on attack move command
            if (Commands.Count > 0 && Commands[0] is AttackMoveCommand)
            {
                AttackMoveCommand attackMoveCommand = (AttackMoveCommand)Commands[0];

                RtsObject target = FindNearestTarget();
                if (target != null)
                {
                    Commands[0] = new AttackCommand(this, target, true, false);
                    Commands.Insert(1, attackMoveCommand);

                    //IgnoringCollision = false;
                    //return;
                }
            }

            timeSinceLastRecalculatePath = 0;
            attackStarted = false;
        }

        public void GiveCommand(UnitCommand command)
        {
            if (Busy)
            {
                if (Commands.Count > 1)
                {
                    //for (int i = 1; i < Commands.Count - 1; i++)
                     //   Player.Players[Team].UnitCommands.Remove(Commands[i]);
                    Commands.RemoveRange(1, Commands.Count - 1);
                }
                return;
            }

            IgnoringCollision = (command is HarvestCommand || command is ReturnCargoCommand);

            deactivateAllCommands();
            clearCommands();
            Commands.Add(command);
        }
        public void GiveCommand(MoveCommand command)
        {
            if (Busy)
            {
                if (Commands.Count > 1)
                {
                    //for (int i = 1; i < Commands.Count - 1; i++)
                    //    Player.Players[Team].UnitCommands.Remove(Commands[i]);
                    Commands.RemoveRange(1, Commands.Count - 1);
                }
                return;
            }

            IgnoringCollision = (command is HarvestCommand || command is ReturnCargoCommand);

            // use previous path with new destination tacked on until new path is calculated
            // (LoL style)
            int commandPriority = -1;
            if (Commands.Count > 0 && !command.Calculated)
            {
                MoveCommand lastMoveCommand = Commands[0] as MoveCommand;
                if (lastMoveCommand != null)
                {
                    if (lastMoveCommand.Destination == command.Destination)
                        return;

                    command.WayPoints = lastMoveCommand.WayPoints;
                    command.WayPoints.Add(command.Destination);
                    Rts.pathFinder.Tools.SmoothPathEnd(command.WayPoints, this);

                    commandPriority = ((int)Vector2.DistanceSquared(lastMoveCommand.Destination, command.Destination) + (int)Vector2.DistanceSquared(centerPoint, command.Destination)) / 2;
                }
            }

            deactivateAllCommands();
            clearCommands();
            Commands.Add(command);
            lastWayPoint = centerPoint;
            lastMoveDestination = command.Destination;
            timeSinceLastRecalculatePath = 0;
            //command.WayPoints = PathFinder.FindPath(CurrentPathNode, command.Destination, false);

            if (command is ReturnCargoCommand || command is HarvestCommand)
                commandPriority = int.MaxValue / 2 + (int)Vector2.DistanceSquared(centerPoint, command.Destination);
            else if (commandPriority == -1)
                commandPriority = (int)Vector2.DistanceSquared(centerPoint, command.Destination);

            //AttackCommand attackCommand = command as AttackCommand;
            //if (attackCommand != null)
                //PathFinder.AddHighPriorityPathFindRequest(this, command, CurrentPathNode, commandPriority, false);
            //else

            //if (Team == Player.Me.Team)
                //PathFinder.AddHighPriorityPathFindRequest(command, commandPriority, false);

            // look for target if attack move command
            if (command is AttackMoveCommand)
            {
                RtsObject target = FindNearestTarget();

                if (target != null)
                {
                    Commands[0] = new AttackCommand(this, target, true, false);
                    Commands.Insert(1, command);
                    return;
                }

                timeSinceLastLookForTarget = lookForTargetDelay;
            }
        }

        public void QueueCommand(UnitCommand command)
        {
            Commands.Add(command);
        }
        public void QueueCommand(MoveCommand command)
        {
            if (Commands.Count == 0)
                GiveCommand(command);
            else
            {
                MoveCommand previousMoveCommand = Commands[Commands.Count - 1] as MoveCommand;
                if (previousMoveCommand != null)
                    command.WayPoints.Insert(0, previousMoveCommand.Destination);
                Commands.Add(command);
            }
        }

        public void InsertCommand(MoveCommand command)
        {
            lastWayPoint = centerPoint;
            lastMoveDestination = command.Destination;
            timeSinceLastRecalculatePath = 0;
            IgnoringCollision = (command is HarvestCommand || command is ReturnCargoCommand);

            /*int commandPriority;
            if (command is ReturnCargoCommand || command is HarvestCommand)
                commandPriority = int.MaxValue / 2 + (int)Vector2.DistanceSquared(centerPoint, command.Destination);
            else
                commandPriority = (int)Vector2.DistanceSquared(centerPoint, command.Destination);*/

            //if (Team == Player.Me.Team)
            Rts.pathFinder.AddPathFindRequest(command, false, false, false);

            Commands.Insert(0, command);
        }

        void clearCommands()
        {
            checkForBuildingRefund();

            Commands.Clear();
        }

        void checkForBuildingRefund()
        {
            foreach (UnitCommand command in Commands)
            {
                BuildStructureCommand structureCommand = command as BuildStructureCommand;
                if (structureCommand != null)
                    Player.Players[Team].Roks += structureCommand.StructureType.RoksCost;
            }
        }

        int pushCount;
        static protected void clearPushStatus()
        {
            foreach (Unit unit in Units)
                unit.pushCount = 0;
        }
        int hitWall;
        static protected void clearHitWallStatus()
        {
            foreach (Unit unit in Units)
                unit.hitWall = 0;
        }

        /*void Move(Vector2 target, GameTime gameTime)
        {
            clearPushStatus();

            float moveX = Util.ScaleWithGameTime(speed.X, gameTime);
            float moveY = Util.ScaleWithGameTime(speed.Y, gameTime);

            Vector2 difference = target - centerPoint;
            if (Math.Abs(difference.X) < moveX && Math.Abs(difference.Y) < moveY)
            {
                this.CenterPoint = target;

                nextWayPoint();
                return;
            }

            float angle = (float)Math.Atan2((double)(target.Y - CenterPoint.Y), (double)(target.X - CenterPoint.X));

            moveX *= (float)Math.Cos(angle);
            moveY *= (float)Math.Sin(angle);

            lastMove.X = moveX;
            lastMove.Y = moveY;

            PrecisePosition += lastMove;

            checkForWallHit();

            //foreach (Unit unit in Units)
            foreach (Unit unit in PotentialCollisions)
            {
                if (unit != this && Intersects(unit))
                {
                    if (isMoving)
                    {
                        if (unit.isIdle && unit.lastWayPoint == target)
                        {
                            nextWayPoint();
                            //Stop();
                        }
                        else if (Contains(MoveTarget))// || unit.Contains(moveTarget))
                        {
                            nextWayPoint();
                            //Stop();
                        }
                    }

                    angle = (float)Math.Atan2(unit.centerPoint.Y - centerPoint.Y, unit.centerPoint.X - centerPoint.X);

                    float distance = Radius + unit.Radius;
                    float force = distance - Vector2.Distance(unit.centerPoint, centerPoint);


                    if (unit.isFollowing && unit.followTarget == this)
                    {
                        unit.Push(this, angle, force);
                    }
                    else if (isFollowing && unit == followTarget)
                    {
                        //Push(angle + (float)Math.PI, force);
                        PushSimple(angle + (float)Math.PI, force);
                    }
                    else
                    {
                        isPushing = true;
                        unit.Push(this, angle, force * .1f);
                        PushSimple(angle + (float)Math.PI, force * .9f);

                        //Push(angle + (float)Math.PI, force * .5f);
                        //unit.Push(angle, force);
                    }
                }
            }

            //if (instanceFrameCount % 6 == 0)
            //    checkIfCurrentPathNodeChanged();
            checkIfCloserToNextWayPointThanCurrentWayPoint();
            //if (instanceFrameCount % 120 == 0)
            //    checkIfCloserToGoalThanCurrentWayPoint();

            turnTowards(target, 100 / radius, gameTime);
        }*/
        
        void Move(MoveCommand command, GameTime gameTime)
        {
            checkForSmoothPath(command, gameTime);

            clearPushStatus();
            clearHitWallStatus();

            Vector2 wayPoint = command.WayPoints[0];

            //float moveX = Util.ScaleWithGameTime(speed.X, gameTime);
            //float moveY = Util.ScaleWithGameTime(speed.Y, gameTime);
            Speed = MathHelper.Min(Speed + Util.ScaleWithGameTime(acceleration, gameTime), MaxSpeed);
            float moveX = Util.ScaleWithGameTime(Speed, gameTime);
            float moveY = moveX;

            if (command.WayPoints.Count > 1)
            {
                if (Contains(wayPoint))
                {
                    lastWayPoint = wayPoint;
                    command.NextWayPoint(this, Rts.pathFinder);
                    return;
                }
            }
            else
            {
                Vector2 difference = wayPoint - centerPoint;
                if (Math.Abs(difference.X) < moveX && Math.Abs(difference.Y) < moveY)
                {
                    this.CenterPoint = wayPoint;
                    HasMoved = true;

                    lastWayPoint = wayPoint;
                    //command.NextWayPoint(this, PathFinder);
                    //if (command.WayPoints.Count == 0)
                    NextCommand();
                    return;
                }
            }

            float angle = (float)Math.Atan2(wayPoint.Y - CenterPoint.Y, wayPoint.X - CenterPoint.X);
            moveX *= (float)Math.Cos(angle);
            moveY *= (float)Math.Sin(angle);

            lastMove.X = moveX;
            lastMove.Y = moveY;
            PrecisePosition += lastMove;
            HasMoved = true;

            if (checkForWallHit(command) && Vector2.Distance(centerPoint, command.Destination) < Radius)
            {
                NextCommand();
                return;
            }
            if (checkForPush(command))
                return;

            if (!turnTowards(wayPoint, 120 / Radius, gameTime))
            {
                Speed = MathHelper.Max(Speed - Util.ScaleWithGameTime(acceleration, gameTime), 0);
            }
        }

        protected int smoothPathDelay = 150, timeSinceLastSmoothPath = 0;
        protected void checkForSmoothPath(MoveCommand command, GameTime gameTime)
        {
            timeSinceLastSmoothPath += (int)(gameTime.ElapsedGameTime.TotalMilliseconds * Rts.GameSpeed);
            if (timeSinceLastSmoothPath >= smoothPathDelay)
            {
                timeSinceLastSmoothPath = 0;
                Rts.pathFinder.Tools.SmoothImmediatePath(command.WayPoints, this, centerPoint);
            }
        }

        bool attackStarted;
        //int initialAttackDelay = 0;// = 150;
        void Attack(AttackCommand command, GameTime gameTime)
        {
            avoidingUnits = false;
            isWithinRangeOfTarget = false;

            // if original target is gone, stop attack
            if (command.Target == null || command.Target.IsDead)
            {
                attackStarted = false;

                if (command.HoldPosition)
                    GiveCommand(new HoldPositionCommand(this));
                else
                    NextCommand();
                return;
            }
            
            clearPushStatus();
            clearHitWallStatus();

            //float angle = (float)Math.Atan2(command.Target.CenterPoint.Y - centerPoint.Y, command.Target.CenterPoint.X - centerPoint.X);

            float distanceToTarget = Vector2.Distance(centerPoint, command.Target.CenterPoint) - (Radius + command.Target.Radius);

            if (distanceToTarget <= attackRange)
            {
                isWithinRangeOfTarget = true;

                // begin attack animation
                if (timeSinceLastAttack >= attackDelay)
                {
                    timeSinceLastAttack = 0;
                    attackStarted = true;
                }

                turnTowards(command.Target.CenterPoint, 120 / Radius, gameTime);
            }
            else if (attackStarted)
            {
                isWithinRangeOfTarget = false;
                attackStarted = false;
            }
            else if (!attackStarted)
            {
                Vector2 wayPoint = command.WayPoints[0];

                if (!command.HoldPosition)
                {
                    //float moveX = Util.ScaleWithGameTime(speed.X, gameTime);
                    //float moveY = Util.ScaleWithGameTime(speed.Y, gameTime);
                    Speed = MathHelper.Min(Speed + acceleration, MaxSpeed);
                    float moveX = Util.ScaleWithGameTime(Speed, gameTime);
                    float moveY = moveX;

                    Vector2 difference = wayPoint - centerPoint;
                    if (Math.Abs(difference.X) < moveX && Math.Abs(difference.Y) < moveY)
                    {
                        this.CenterPoint = wayPoint;
                        HasMoved = true;

                        lastWayPoint = wayPoint;
                        if (command.WayPoints.Count > 1)
                            command.NextWayPoint(this, Rts.pathFinder);
                        return;
                    }

                    float angle = (float)Math.Atan2(wayPoint.Y - centerPoint.Y, wayPoint.X - centerPoint.X);
                    moveX *= (float)Math.Cos(angle);
                    moveY *= (float)Math.Sin(angle);

                    lastMove.X = moveX;
                    lastMove.Y = moveY;
                    PrecisePosition += lastMove;
                    HasMoved = true;
                }

                checkForWallHit(command);
                // checkForPush sets avoidingUnits
                checkForPush(command);

                command.Destination = command.Target.CenterPoint;

                if (!turnTowards(wayPoint, 120 / Radius, gameTime))
                {
                    Speed = MathHelper.Max(Speed - Util.ScaleWithGameTime(acceleration, gameTime), 0);
                }

                if (!command.HoldPosition)
                {
                    if (timeSinceLastRecalculatePath >= recalculatePathDelay)// && command.Calculated)
                    {
                        timeSinceLastRecalculatePath = 0;

                        Rts.pathFinder.AddPathFindRequest(command, false, true, avoidingUnits);
                    }
                    // repath to avoid units
                    /*if (avoidingUnits)
                    {
                        if (timeSinceLastRecalculatePath >= recalculatePathDelay)// && command.Calculated)
                        {
                            timeSinceLastRecalculatePath = 0;

                            PathFinder.AddLowPriorityPathFindRequest(this, command, CurrentPathNode, (int)Vector2.DistanceSquared(centerPoint, command.Destination), true);
                        }
                    }
                    // normal repathing
                    else if (timeSinceLastRecalculatePath >= recalculatePathDelay)// && command.Calculated)
                    {
                        timeSinceLastRecalculatePath = 0;

                        PathFinder.AddLowPriorityPathFindRequest(this, command, CurrentPathNode, (int)Vector2.DistanceSquared(centerPoint, command.Destination), false);
                    }*/
                }
            }

            // periodically switch or lose target
            if (command.Target.Team != Team && timeSinceLastLookForTarget >= lookForTargetDelay)
            {
                timeSinceLastLookForTarget = 0;

                // switch to closer target if possible
                if (command.WillingToChangeTarget)
                {
                    RtsObject newTarget = FindNearestTarget();

                    if (newTarget == null)
                    {
                        if (command.HoldPosition)
                            GiveCommand(new HoldPositionCommand(this));
                        else
                            NextCommand();
                    }
                    else
                    {
                        command.Target = newTarget;
                        return;
                    }
                }
                //else if (command.WillingToChangeTarget)
                //    command.Target = newTarget;

                // lose target if out of vision
                //if (!command.Target.CurrentPathNode.Tile.Visible)
                if (!command.Target.Visible)
                {
                    //if (command.HoldPosition)
                    //    GiveCommand(new HoldPositionCommand());
                    //else
                    //    nextCommand();
                    command.Target = null;
                }
            }
        }

        void AttackMove(AttackMoveCommand command, GameTime gameTime)
        {

            /*List<RtsObject> targets = new List<RtsObject>();

            lock (VisibleTiles)
            {
                foreach (MapTile tile in VisibleTiles)
                {
                    foreach (Unit unit in PathFinder.PathNodes[tile.Y, tile.X].UnitsContained)
                    {
                        if (unit.Team != Team)
                        {
                            Commands[0] = new AttackCommand(unit);
                            Commands.Insert(1, command);
                            return;
                        }
                    }
                }
            }*/

            if (timeSinceLastLookForTarget >= lookForTargetDelay)
            {
                timeSinceLastLookForTarget = 0;

                RtsObject target = FindNearestTarget();

                if (target != null)
                {
                    Commands[0] = new AttackCommand(this, target, true, false);
                    Commands.Insert(1, command);
                    return;
                }
            }

            Move(command, gameTime);
        }

        RtsObject FindNearestTarget()
        {
            List<RtsObject> targets = new List<RtsObject>();

            int highestPriority = 0;

            lock (VisibleTiles)
            {
                foreach (MapTile tile in VisibleTiles)
                {
                    PathNode pathNode = Rts.pathFinder.PathNodes[tile.Y, tile.X];
                    foreach (Unit unit in pathNode.UnitsContained)
                    {
                        if (unit.Targetable && unit.Team != Team)
                        {
                            targets.Add(unit);
                            if (unit.TargetPriority > highestPriority)
                                highestPriority = unit.TargetPriority;
                        }
                    }
                    if (pathNode.Blocked)
                    {
                        Structure s = pathNode.Blocker as Structure;
                        if (s != null && s.Targetable && s.Team != Team)
                        {
                            targets.Add(s);
                        }
                    }
                }
            }

            for (int i = 0; i < targets.Count; )
            {
                RtsObject target = targets[i];
                if (target.TargetPriority < highestPriority)
                    targets.Remove(target);
                else
                    i++;
            }

            float shortestDistance = float.MaxValue;
            RtsObject closestTarget = null;

            foreach (RtsObject target in targets)
            {
                float distance = Vector2.DistanceSquared(centerPoint, target.CenterPoint) - target.Radius;
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closestTarget = target;
                }
            }

            return closestTarget;
        }

        void IdleLookForTarget()
        {
        }

        void LookForNewTarget(AttackCommand command)
        {
        }

        void performAttackIfStarted(AttackCommand command)
        {
            if (attackStarted)
            {
                if (timeSinceLastAttack >= initialAttackDelay)
                {
                    attackStarted = false;

                    RtsBullet b = new RtsBullet(type.BulletType, this, command.Target, centerPoint, BulletSize, BulletSpeed);
                }
            }
        }

        // for move command. returns true if command ended
        protected bool checkForPush(MoveCommand command)
        {
            //lock (PotentialCollisions)
            {
                foreach (Unit unit in PotentialCollisions)
                {
                    if (!unit.IgnoringCollision && Intersects(unit))
                    {
                        if (timeSinceLastRecalculatePath >= recalculatePathDelay && command.Calculated)
                        {
                            timeSinceLastRecalculatePath = 0;
                            Rts.pathFinder.AddPathFindRequest(command, false, true, false);
                        }

                        float angle = (float)Math.Atan2(unit.centerPoint.Y - centerPoint.Y, unit.centerPoint.X - centerPoint.X);

                        float distance = Radius + unit.Radius;
                        float force = distance - Vector2.Distance(unit.centerPoint, centerPoint);

                        if (unit.Team != Team)
                        {
                            PushSimple(angle + (float)Math.PI, force);
                        }
                        else if (unit.IsIdle && (unit.lastMoveDestination == command.Destination || unit.Contains(command.Destination)))
                        {
                            PushSimple(angle + (float)Math.PI, force);
                            lastWayPoint = command.WayPoints[0];
                            if (!(command is BuildStructureCommand))
                            {
                                command.NextWayPoint(this, Rts.pathFinder);
                                if (command.WayPoints.Count == 0)
                                {
                                    NextCommand();
                                    return true;
                                }
                            }
                        }
                        else if (unit.lastMoveDestination == command.Destination && unit.Contains(command.Destination))
                        {
                            PushSimple(angle + (float)Math.PI, force);
                            lastWayPoint = command.WayPoints[0];
                            if (!(command is BuildStructureCommand))
                            {
                                command.NextWayPoint(this, Rts.pathFinder);
                                if (command.WayPoints.Count == 0)
                                {
                                    NextCommand();
                                    return true;
                                }
                            }
                            command.NextWayPoint(this, Rts.pathFinder);
                            if (command.WayPoints.Count == 0 && !(command is BuildStructureCommand))
                            {
                                NextCommand();
                                return true;
                            }
                        }
                        else if (Contains(command.Destination))
                        {
                            PushSimple(angle + (float)Math.PI, force);
                            lastWayPoint = command.WayPoints[0];
                            if (!(command is BuildStructureCommand))
                            {
                                command.NextWayPoint(this, Rts.pathFinder);
                                if (command.WayPoints.Count == 0)
                                {
                                    NextCommand();
                                    return true;
                                }
                            }
                        }

                        //if (unit.isFollowing && unit.followTarget == this)
                        //{
                        //    unit.Push(this, angle, force);
                        //}
                        //else if (isFollowing && unit == followTarget)
                        //{
                        //    PushSimple(angle + (float)Math.PI, force);
                        //}
                       // else
                        {
                            //pushCount++;

                            float sizeRatio = this.Diameter / unit.Diameter;
                            //float pushForce = force * (.05f * sizeRatio);
                            float pushForce;
                            if (unit.IsMoving)
                                pushForce = force * (.05f * sizeRatio);
                            else
                                pushForce = force * (.3f * sizeRatio);

                            unit.Push(this, angle, pushForce);
                            PushSimple(angle + (float)Math.PI, force - pushForce);
                            //PushSimple(angle + (float)Math.PI, force * .90f);


                            //unit.Push(this, angle, force * .1f);
                            //PushSimple(angle + (float)Math.PI, force * .9f);
                        }
                    }
                }
            }

            /*foreach (PathNode pathNode in OccupiedPathNodes)
            {
                foreach (Unit unit in pathNode.UnitsContained)
                {
                        if (unit != this && Intersects(unit))
                        {
                            if (timeSinceLastRecalculatePath >= recalculatePathDelay && command.Calculated)
                            {
                                timeSinceLastRecalculatePath = 0;
                                PathFinder.AddLowPriorityPathFindRequest(this, command, CurrentPathNode, (int)Vector2.DistanceSquared(centerPoint, command.Destination), false);
                            }

                            float angle = (float)Math.Atan2(unit.centerPoint.Y - centerPoint.Y, unit.centerPoint.X - centerPoint.X);

                            float distance = Radius + unit.Radius;
                            float force = distance - Vector2.Distance(unit.centerPoint, centerPoint);

                            if (unit.IsIdle && (unit.lastMoveDestination == command.Destination || unit.Contains(command.Destination)))
                            {
                                PushSimple(angle + (float)Math.PI, force);
                                lastWayPoint = command.WayPoints[0];
                                command.NextWayPoint(this, PathFinder);
                                if (command.WayPoints.Count == 0)
                                {
                                    nextCommand();
                                    return true;
                                }
                            }
                            else if (unit.lastMoveDestination == command.Destination && unit.Contains(command.Destination))
                            {
                                PushSimple(angle + (float)Math.PI, force);
                                lastWayPoint = command.WayPoints[0];
                                command.NextWayPoint(this, PathFinder);
                                if (command.WayPoints.Count == 0)
                                {
                                    nextCommand();
                                    return true;
                                }
                            }
                            else if (Contains(command.Destination))
                            {
                                PushSimple(angle + (float)Math.PI, force);
                                lastWayPoint = command.WayPoints[0];
                                command.NextWayPoint(this, PathFinder);
                                if (command.WayPoints.Count == 0)
                                {
                                    nextCommand();
                                    return true;
                                }
                            }

                            //if (unit.isFollowing && unit.followTarget == this)
                            //{
                            //    unit.Push(this, angle, force);
                            //}
                            //else if (isFollowing && unit == followTarget)
                            //{
                            //    PushSimple(angle + (float)Math.PI, force);
                            //}
                            // else
                            {
                                //pushCount++;

                                float sizeRatio = this.Diameter / unit.Diameter;
                                float pushForce = force * (.1f * sizeRatio);
                                unit.Push(this, angle, pushForce);
                                PushSimple(angle + (float)Math.PI, force - pushForce);
                                //unit.Push(this, angle, force * .1f);
                                //PushSimple(angle + (float)Math.PI, force * .9f);
                            }
                        }
                    
                }
            }*/

            return false;
        }
        // when attacking
        protected void checkForPush(AttackCommand command)
        {
            //lock (PotentialCollisions)
            {
                foreach (Unit unit in PotentialCollisions)
                {
                    if (!unit.IgnoringCollision && Intersects(unit))
                    {
                        float angle = (float)Math.Atan2(unit.centerPoint.Y - centerPoint.Y, unit.centerPoint.X - centerPoint.X);

                        float distance = Radius + unit.Radius;
                        float force = distance - Vector2.Distance(unit.centerPoint, centerPoint);

                        if (unit.Team != Team || unit == command.Target)
                        {
                            PushSimple(angle + (float)Math.PI, force);
                        }
                        //else if (unit.IsAttacking && ((AttackCommand)unit.Commands[0]).Target == command.Target)
                        //{
                        //    PushSimple(angle + (float)Math.PI, force);
                        //}
                        else if (unit.IsAttacking && unit.isWithinRangeOfTarget)
                        {
                            PushSimple(angle + (float)Math.PI, force);

                            avoidingUnits = true;
                            isWithinRangeOfTarget = true;
                        }
                        else
                        {
                            //pushCount++;

                            float sizeRatio = this.Diameter / unit.Diameter;
                            float pushForce = force * (.1f * sizeRatio);
                            unit.Push(this, angle, pushForce);
                            PushSimple(angle + (float)Math.PI, force - pushForce);
                            //unit.Push(this, angle, force * (.1f);
                            //PushSimple(angle + (float)Math.PI, force * .9f);
                        }
                    }
                }
            }
        }
        // just check for collision
        public void CheckForPush(bool wimpy)
        {
            foreach (Unit unit in PotentialCollisions)
            {
                if (!unit.IgnoringCollision && Intersects(unit))
                {
                    float angle;
                    if (unit.CenterPoint == centerPoint)
                        angle = (float)(rand.NextDouble() * MathHelper.TwoPi);
                    else
                        angle = (float)Math.Atan2(unit.centerPoint.Y - centerPoint.Y, unit.centerPoint.X - centerPoint.X);

                    float distance = Radius + unit.Radius;
                    float force = distance - Vector2.Distance(unit.centerPoint, centerPoint);

                    if (unit.Team != Team)
                    {
                        PushSimple(angle + (float)Math.PI, force);
                        return;
                    }

                    if (!wimpy)
                    {
                        float sizeRatio = this.Diameter / unit.Diameter;

                        float pushForce;
                        if (unit.IsMoving)
                            pushForce = force * (.05f * sizeRatio);
                        else
                            pushForce = force * (.3f * sizeRatio);

                        unit.Push(this, angle, pushForce);
                        PushSimple(angle + (float)Math.PI, force - pushForce);
                    }
                    else
                    {
                        PushSimple(angle + (float)Math.PI, force);
                        return;
                    }
                }
            }
        }

        // just checks for units
        public void CheckForPush()
        {
            /*foreach (Unit unit in PotentialCollisions)
            {
                if (Intersects(unit))
                {
                    float angle = (float)Math.Atan2(unit.centerPoint.Y - centerPoint.Y, unit.centerPoint.X - centerPoint.X);

                    float distance = Radius + unit.Radius;
                    float force = distance - Vector2.Distance(unit.centerPoint, centerPoint);

                    float sizeRatio = this.Diameter / unit.Diameter;
                    
                    float pushForce;
                    if (unit.IsMoving)
                        pushForce = force * (.05f * sizeRatio);
                    else
                        pushForce = force * (.3f * sizeRatio);

                    unit.Push(this, angle, pushForce);
                    PushSimple(angle + (float)Math.PI, force - pushForce);
                }
            }*/

            foreach (PathNode pathNode in OccupiedPathNodes)
            {
                foreach (Unit unit in pathNode.UnitsContained)
                {
                    if (!unit.IgnoringCollision && Intersects(unit))
                    {
                        float angle = (float)Math.Atan2(unit.centerPoint.Y - centerPoint.Y, unit.centerPoint.X - centerPoint.X);

                        float distance = Radius + unit.Radius;
                        float force = distance - Vector2.Distance(unit.centerPoint, centerPoint);

                        float sizeRatio = this.Diameter / unit.Diameter;

                        float pushForce;
                        if (unit.IsMoving)
                            pushForce = force * (.05f * sizeRatio);
                        else
                            pushForce = force * (.3f * sizeRatio);

                        unit.Push(this, angle, pushForce);
                        PushSimple(angle + (float)Math.PI, force - pushForce);
                    }
                }
            }
        }

        public void Push(Unit pusher, float angle, float force)
        {
            pushCount++;
            float angleX = (float)Math.Cos(angle);
            float angleY = (float)Math.Sin(angle);

            CenterPoint += new Vector2(force * angleX, force * angleY);
            HasMoved = true;

            checkForWallHit(null);

            //if (isFollowing)
            //    return;

            //lock (PotentialCollisions)
            {
                foreach (Unit unit in PotentialCollisions)
                {
                    if (!unit.IgnoringCollision && unit.pushCount < 1 && Intersects(unit))
                    {
                        angle = (float)Math.Atan2(unit.centerPoint.Y - centerPoint.Y, unit.centerPoint.X - centerPoint.X);

                        //if (unit.Team != Team)
                        //    PushSimple(angle, force);
                        //else if (unit.pushCount < 1)
                            unit.Push(this, angle, force);

                        //unit.Push(this, angle, force * .1f);
                        //PushSimple(angle + (float)Math.PI, force * .9f);
                    }
                }
            }
        }

        public void PushSimple(float angle, float force)
        {
            float angleX = (float)Math.Cos(angle);
            float angleY = (float)Math.Sin(angle);

            CenterPoint += new Vector2(force * angleX, force * angleY);
            HasMoved = true;

            checkForWallHit(null);
        }

        protected bool checkForWallHit(MoveCommand command)
        {
            if (hitWall >= 2)
                return false;

            bool hit = false;

            List<PathNode> pathNodesHit = new List<PathNode>();

            //foreach (MapTile tile in CurrentPathNode.Tile.Neighbors)
            foreach (PathNode pathNode in PathNodeBufferSquare)
            {
                MapTile tile = pathNode.Tile;

                //if (!tile.Walkable && Intersects(tile))
                if ((!tile.Walkable || pathNode.Blocked) && tile.IntersectsUnit(this))
                {
                    //if (harvestCommand != null)
                    //{
                    pathNodesHit.Add(pathNode);
                        //return (pathNode.Blocker == ((HarvestCommand)command).TargetResource);
                    //}

                    if (command != null && timeSinceLastRecalculatePath >= recalculatePathDelay && command.Calculated)
                    {
                        timeSinceLastRecalculatePath = 0;
                        Rts.pathFinder.AddPathFindRequest(command, false, true, false);
                    }

                    float angle = (float)Math.Atan2(centerPoint.Y - tile.CenterPoint.Y, centerPoint.X - tile.CenterPoint.X);

                    float distance = Radius + tile.CollisionRadius;
                    float force = distance - Vector2.Distance(tile.CenterPoint, centerPoint);

                    hitWall++;
                    hit = true;
                    //isBlocked = true;
                    PushSimple(angle, force);
                    //Push(null, angle, force);
                }

                /*MapTile tile = pathNode.Tile;

                //if (!tile.Walkable && Intersects(tile))
                if (!tile.Walkable)
                {
                    hitWall++;

                    if (centerPoint.X < tile.CenterPoint.X)
                    {
                        CenterPointX = tile.Rectangle.X - Radius;
                        checkForWallHit();
                    }
                    else if (centerPoint.X >= tile.CenterPoint.X)
                    {
                        CenterPointX = tile.Rectangle.X + tile.Rectangle.Width + Radius;
                        checkForWallHit();
                    }
                    else if (centerPoint.Y < tile.CenterPoint.Y)
                    {
                        CenterPointY = tile.Rectangle.Y - Radius;
                        checkForWallHit();
                    }
                    else if (centerPoint.Y >= tile.CenterPoint.Y)
                    {
                        CenterPointY = tile.Rectangle.Y + tile.Rectangle.Height + Radius;
                        checkForWallHit();
                    }
                }*/
            }

            HarvestCommand harvestCommand = command as HarvestCommand;
            if (harvestCommand != null)
            {
                foreach (PathNode pathNode in pathNodesHit)
                {
                    if (pathNode.Blocker == harvestCommand.TargetResource)
                        return true;
                }
                //return (pathNodeHit != null && pathNodeHit.Blocker is Resource);
                return false;
            }

            ReturnCargoCommand returnCargoCommand = command as ReturnCargoCommand;
            if (returnCargoCommand != null)
            {
                foreach (PathNode pathNode in pathNodesHit)
                {
                    if (pathNode.Blocker == returnCargoCommand.TargetStructure)
                        return true;
                }

                return false;
            }

            return hit;
        }

        // no command
        public void CheckForWallHit()
        {
            foreach (PathNode pathNode in PathNodeBufferSquare)
            {
                MapTile tile = pathNode.Tile;

                //if (!tile.Walkable && Intersects(tile))
                if ((!tile.Walkable || pathNode.Blocked) && tile.IntersectsUnit(this))
                {
                    //if (harvestCommand != null)
                    //{
                    //pathNodesHit.Add(pathNode);
                    //return (pathNode.Blocker == ((HarvestCommand)command).TargetResource);
                    //}

                    //if (command != null && timeSinceLastRecalculatePath >= recalculatePathDelay && command.Calculated)
                    //{
                    //    timeSinceLastRecalculatePath = 0;
                    //   PathFinder.AddLowPriorityPathFindRequest(this, command, CurrentPathNode, (int)Vector2.DistanceSquared(centerPoint, command.Destination), false);
                    //}

                    float angle = (float)Math.Atan2(centerPoint.Y - tile.CenterPoint.Y, centerPoint.X - tile.CenterPoint.X);

                    float distance = Radius + tile.CollisionRadius;
                    float force = distance - Vector2.Distance(tile.CenterPoint, centerPoint);

                    //hitWall++;
                    //hit = true;
                    //isBlocked = true;
                    PushSimple(angle, force);
                    //Push(null, angle, force);
                }
            }
        }

        protected void stop()
        {
            //moveTarget = null;
            //attackTarget = null;
            //if (WayPoints.Count > 0)
            //    lastWayPoint = WayPoints[WayPoints.Count - 1];
            //WayPoints.Clear();
            deactivateAllCommands();
            clearCommands();
            //Commands.Clear();
            //IsIdle = true;
        }

        // flags commands as inactive so pathfinder will skip them
        void deactivateAllCommands()
        {
            foreach (UnitCommand command in Commands)
                command.Active = false;
        }

        public override void Die()
        {
            base.Die();

            Player.Players[Team].CurrentSupply -= type.SupplyCost;

            UnitAnimation a = new UnitAnimation(this, Width, .5f, true, Unit.Explosion1Textures);
            a.Start();

            CurrentPathNode.UnitsContained.Remove(this);
            foreach (PathNode pathNode in OccupiedPathNodes)
                pathNode.UnitsContained.Remove(this);

            clearCommands();

            Player.Players[Team].UnitArray[ID] = null;

            NetOutgoingMessage msg = Rts.netPeer.CreateMessage();
            msg.Write(MessageID.UNIT_DEATH);
            msg.Write(ID);
            msg.Write(Team);
            Rts.netPeer.SendMessage(msg, Rts.connection, NetDeliveryMethod.ReliableUnordered);
        }

        public bool Intersects(Unit u)
        {
            return Vector2.Distance(centerPoint, u.centerPoint) < (this.Radius + u.Radius);
            //return Vector2.DistanceSquared(centerPoint, u.centerPoint) < Math.Pow(this.Radius + u.Radius, 2);
        }

        public override RtsObjectType Type
        {
            get
            {
                return type;
            }
        }

        public Vector2 CurrentMoveDestination
        {
            get
            {
                for (int i = 0; i < Commands.Count; i++)
                    if (Commands[i] is MoveCommand)
                        return ((MoveCommand)Commands[i]).Destination;
                return centerPoint;
            }
        }
        public Vector2 FinalMoveDestination
        {
            get
            {
                for (int i = Commands.Count - 1; i >= 0; i--)
                    if (Commands[i] is MoveCommand)
                        return ((MoveCommand)Commands[i]).Destination;
                return centerPoint;
            }
        }
        /*public BaseObject FollowTarget
        {
            get
            {
                return followTarget;
            }
            set
            {
                followTarget = value;
                IsFollowing = true;
            }
        }
        public BaseObject AttackTarget
        {
            get
            {
                return attackTarget;
            }
            set
            {
                attackTarget = value;
                IsAttacking = true;
            }
        }*/

        public bool IsIdle
        {
            get
            {
                //return isIdle;
                return (Commands.Count == 0);
            }
            //set
            //{
            //AllFlags = false;
            //isIdle = value;
            //}
        }
        public bool IsHoldingPosition
        {
            get
            {
                return (Commands.Count > 0 && (Commands[0] is HoldPositionCommand || (Commands[0] is AttackCommand && ((AttackCommand)Commands[0]).HoldPosition)));
            }
        }
        new public bool IsMoving
        {
            get
            {
                //return isMoving;
                return (Commands.Count > 0 && Commands[0] is MoveCommand);
            }
            //set
            //{
            //    AllFlags = false;
            //    isMoving = value;
            //}
        }
        public bool IsAttacking
        {
            get
            {
                return (Commands.Count > 0 && Commands[0] is AttackCommand);
            }
        }
        /*public bool IsFollowing
        {
            get
            {
                return isFollowing;
            }
            set
            {
                AllFlags = false;
                isFollowing = true;
            }
        }
        public bool IsAttackMoving
        {
            get
            {
                return isAttackMoving;
            }
            set
            {
                AllFlags = false;
                isAttackMoving = value;
            }
        }*/
        //public bool AllFlags
        //{
        //    set
        //    {
        //        isIdle = isMoving = isFollowing = isAttacking = isAttackMoving = value;
        //    }
        //}

        public override float LeftBound
        {
            get
            {
                return centerPoint.X - Radius * 1.5f;
            }
        }
        public override float RightBound
        {
            get
            {
                return centerPoint.X + Radius * 1.5f;
            }
        }
        public override float TopBound
        {
            get
            {
                return centerPoint.Y - Radius * 1.5f;
            }
        }
        public override float BottomBound
        {
            get
            {
                return centerPoint.Y + Radius * 1.5f;
            }
        }

        public static List<Unit> Units
        {
            get
            {
                //lock (UnitsLock)
                //{
                return units;
                //}
            }
        }
        public static List<Unit> UnitsSorted
        {
            get
            {
                //lock (UnitsSortedLock)
                //{
                return unitsSorted;
                //}
            }
        }
        public static void AddUnit(Unit u)
        {
            lock (Units)
            {
                units.Add(u);
            }
            lock (UnitsSorted)
            {
                unitsSorted.Add(u);
            }
            lock (RtsObject.RtsObjects)
            {
                RtsObject.AddObject(u);
            }
        }
        public static void RemoveUnit(Unit u)
        {
            lock (Units)
            {
                units.Remove(u);
            }
            lock (UnitsSorted)
            {
                unitsSorted.Remove(u);
            }
            RtsObject.RemoveObject(u);
        }

        public Texture2D SelectingTexture
        {
            get
            {
                return type.SelectingTexture;
            }
            set
            {
            }
        }
        public Texture2D SelectedTexture
        {
            get
            {
                return type.SelectedTexture;
            }
            set
            {
            }
        }

        //public static Object PotentialCollisionsLock = new Object();
        public List<BaseObject> PotentialCollisions
        {
            get
            {
                //lock (PotentialCollisionsLock)
                //{
                return potentialCollisions;
                //}
            }
        }
        public void AddPotentialCollision(BaseObject o)
        {
            //lock (PotentialCollisions)
            //{
            potentialCollisions.Add(o);
            //}
        }
        public void ClearPotentialCollisions()
        {
            //lock (PotentialCollisions)
            //{
            potentialCollisions.Clear();
            //}
        }

        public override string Name
        {
            get
            {
                return type.Name;
            }
        }
    }

    /*class UnitWallCollisionSweeper
    {
        // assumes walls are already sorted by x
        public void UpdateNearbyWalls(List<Unit> units, List<MapTile> walls)
        {
            if (units.Count == 0 || walls.Count == 0)
                return;

            Util.SortByX(units);

            List<int[]> pairs = new List<int[]>();

            for (int i = 0; i < units.Count; i++)
            {
                Unit object1 = units[i];
                object1.NearbyWalls.Clear();

                for (int s = 0; s < walls.Count; s++)
                {
                    MapTile object2 = walls[s];

                    if (object2.RightBound < object1.LeftBound)
                        continue;

                    if (object2.LeftBound > object1.RightBound)
                        break;

                    if (object2.TopBound <= object1.BottomBound &&
                        object2.BottomBound >= object1.TopBound)
                        pairs.Add(new int[2] { i, s });
                }
            }

            foreach (int[] pair in pairs)
            {
                units[pair[0]].NearbyWalls.Add(walls[pair[1]]);
            }
        }
    }*/
}