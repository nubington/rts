using System;
using Microsoft.Xna.Framework;

namespace rts
{
    public class WorkerNublet : Unit
    {
        public int CargoAmount;
        public ResourceType CargoType;

        public bool Building;

        public WorkerNublet(Vector2 position, short team, short id)
            : base(UnitType.WorkerNublet, position, team, id)
        {
        }

        // returns true if a special command is being used
        protected override bool DoSpecialCommands(UnitCommand command, GameTime gameTime)
        {
            BuildStructureCommand buildStructureCommand = command as BuildStructureCommand;
            if (buildStructureCommand != null)
            {
                moveToBuildLocation(buildStructureCommand, gameTime);
                return true;
            }

            HarvestCommand harvestCommand = command as HarvestCommand;
            if (harvestCommand != null)
            {
                moveToHarvestLocation(harvestCommand, gameTime);
                return true;
            }

            ReturnCargoCommand returnCargoCommand = command as ReturnCargoCommand;
            if (returnCargoCommand != null)
            {
                moveToReturnCargo(returnCargoCommand, gameTime);
                return true;
            }

            return false;
        }

        void moveToBuildLocation(BuildStructureCommand command, GameTime gameTime)
        {
            if (!Rts.pathFinder.Tools.WillStructureFit(command.StructureLocation, command.StructureType.Size, command.StructureType.CutCorners))
            {
                NextCommand();
                return;
            }

            checkForSmoothPath(command, gameTime);

            clearPushStatus();
            clearHitWallStatus();

            //if (command.WayPoints.Count == 0)
            //    command.WayPoints.Add(centerPoint);

            Vector2 wayPoint = command.WayPoints[0];

            //float moveX = Util.ScaleWithGameTime(speed.X, gameTime);
            //float moveY = Util.ScaleWithGameTime(speed.Y, gameTime);
            Speed = MathHelper.Min(Speed + acceleration, MaxSpeed);
            float moveX = Util.ScaleWithGameTime(Speed, gameTime);
            float moveY = moveX;

            if (command.WayPoints.Count > 1)
            {
                if (Contains(wayPoint))
                {
                    lastWayPoint = wayPoint;
                    //if (command.WayPoints.Count > 2)
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
                    
                    //nextCommand();
                    if (!buildStructure(command))
                    {
                        Player.Players[Team].Roks += command.StructureType.RoksCost;
                        //NextCommand();
                    }

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

            checkForWallHit(command);
            if (checkForPush(command))
                return;

            if (!turnTowards(wayPoint, 120 / Radius, gameTime))
            {
                Speed = MathHelper.Max(Speed - Util.ScaleWithGameTime(acceleration, gameTime), 0);
            }
        }

        bool buildStructure(BuildStructureCommand command)
        {
            bool allowBuild = Rts.pathFinder.Tools.CanStructureBePlaced(command.StructureLocation, command.StructureType.Size, this, command.StructureType.CutCorners);

            if (allowBuild)
            {
                //if (command.StructureType == StructureType.Barracks)
                //    new Barracks(command.StructureLocation, this, Team);

                Structure structure;

                if (command.StructureType == StructureType.TownHall)
                    structure = new TownHall(command.StructureLocation, this, Team);
                else
                    structure = new Structure(command.StructureType, command.StructureLocation, this, Team);

                CenterPoint = structure.CenterPoint;
                //IgnoringCollision = true;
                Targetable = false;
                Busy = true;
                Building = true;
                Unit.RemoveUnit(this);
            }

            return allowBuild;
        }

        public void FinishBuildingStructure()
        {
            //IgnoringCollision = false;
            Targetable = true;
            Busy = false;
            Building = false;
            Unit.AddUnit(this);
            NextCommand();
        }

        void moveToHarvestLocation(HarvestCommand command, GameTime gameTime)
        {
            checkForSmoothPath(command, gameTime);

            clearPushStatus();
            clearHitWallStatus();

            Vector2 wayPoint = command.WayPoints[0];

            //float moveX = Util.ScaleWithGameTime(speed.X, gameTime);
            //float moveY = Util.ScaleWithGameTime(speed.Y, gameTime);
            Speed = MathHelper.Min(Speed + acceleration, MaxSpeed);
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
                //Resource resource = command.TargetResource;
                //if (Vector2.Distance(CenterPoint, resource.CenterPoint) < Radius + resource.Radius)
                {
                    this.CenterPoint = wayPoint;
                    HasMoved = true;

                    lastWayPoint = wayPoint;

                    //if (harvest(command.TargetResource))
                        //nextCommand();
                    Resource targetResource = command.TargetResource;
                    if (targetResource == null || targetResource.Depleted)
                    {
                        Resource nearestResource = findNearestResource(targetResource.Type);
                        if (nearestResource != null)
                            GiveCommand(new HarvestCommand(this, nearestResource));
                        else
                            NextCommand();
                    }
                    else
                        harvest(command.TargetResource);

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

            /*Resource resource = command.TargetResource;
            if (Vector2.Distance(CenterPoint, resource.CenterPoint) <= Radius + resource.Radius)
            {
                //if (harvest(command.TargetResource))
                //    nextCommand();
                harvest(command.TargetResource);
            }*/

            if (checkForWallHit(command))
            {
                //if (harvest(command.TargetResource))
                //    nextCommand();
                /*if (CargoAmount == MAX_CARGO_AMOUNT && command.TargetResource.Type == CargoType)
                {
                    TownHall townHall = findNearestTownHall();
                    if (townHall != null)
                        GiveCommand(new ReturnCargoCommand(townHall, command.TargetResource, 1));
                    else
                        nextCommand();
                }
                else
                {*/
                Resource targetResource = command.TargetResource;
                if (targetResource == null || targetResource.Depleted)
                {
                    Resource nearestResource = findNearestResource(targetResource.Type);
                    if (nearestResource != null)
                        GiveCommand(new HarvestCommand(this, nearestResource));
                    else
                        NextCommand();
                }
                else
                    harvest(command.TargetResource);
                //}
            }

            //if (checkForPush(command))
            //    return;

            if (!turnTowards(wayPoint, 120 / Radius, gameTime))
            {
                Speed = MathHelper.Max(Speed - Util.ScaleWithGameTime(acceleration, gameTime), 0);
            }
        }

        bool harvest(Resource resource)
        {
            Roks roks = resource as Roks;
            if (roks != null)
            {
                if (roks.CheckForEntrance(this))
                {
                    Targetable = false;
                    Busy = true;
                    Unit.RemoveUnit(this);

                    return true;
                }
            }

            return false;
        }

        public void FinishHarvesting()
        {
            Targetable = true;
            Busy = false;
            Unit.AddUnit(this);

            updateCurrentPathNode();
            CheckForWallHit();

            //if (Commands.Count > 1)
            //{
                NextCommand();
                //return;
            //}

            /*TownHall townHall = findNearestTownHall();
            if (townHall != null)
                GiveCommand(new ReturnCargoCommand(townHall, resource, 1));
            else
                nextCommand();*/

            /*if (command != null)
                GiveCommand(command);
            else
                NextCommand();*/
        }

        void moveToReturnCargo(ReturnCargoCommand command, GameTime gameTime)
        {
            checkForSmoothPath(command, gameTime);

            refreshTargetTownHall(command, gameTime);

            clearPushStatus();
            clearHitWallStatus();

            Vector2 wayPoint = command.WayPoints[0];

            //float moveX = Util.ScaleWithGameTime(speed.X, gameTime);
            //float moveY = Util.ScaleWithGameTime(speed.Y, gameTime);
            Speed = MathHelper.Min(Speed + acceleration, MaxSpeed);
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
            /*else
            {
                Vector2 difference = wayPoint - centerPoint;
                if (Math.Abs(difference.X) < moveX && Math.Abs(difference.Y) < moveY)
                {
                    this.CenterPoint = wayPoint;
                    HasMoved = true;

                    lastWayPoint = wayPoint;

                    return;
                }
            }*/

            float angle = (float)Math.Atan2(wayPoint.Y - CenterPoint.Y, wayPoint.X - CenterPoint.X);
            moveX *= (float)Math.Cos(angle);
            moveY *= (float)Math.Sin(angle);

            lastMove.X = moveX;
            lastMove.Y = moveY;
            PrecisePosition += lastMove;
            HasMoved = true;

            if (command.TargetStructure == null || command.TargetStructure.IsDead)
                command.TargetStructure = FindNearestTownHall();

            if (command.TargetStructure == null)
                NextCommand();

            //if (Vector2.Distance(CenterPoint, targetStructure.CenterPoint) <= Radius + targetStructure.Radius)
            if (checkForWallHit(command))
                returnCargo(command);

            //checkForWallHit(command);

            if (!turnTowards(wayPoint, 120 / Radius, gameTime))
            {
                Speed = MathHelper.Max(Speed - Util.ScaleWithGameTime(acceleration, gameTime), 0);
            }
        }

        void returnCargo(ReturnCargoCommand command)
        {
            if (CargoType == ResourceType.Roks)
            {
                Player.Players[Team].Roks += CargoAmount;
                Player.Players[Team].Stats.RoksCounter += CargoAmount;
            }

            //if (Commands.Count > 1)
            //{
            NextCommand();

            if (Commands.Count == 0)
            {
                HarvestCommand harvestCommand = null;

                if (command.Source != null && !command.Source.Depleted)
                    harvestCommand = new HarvestCommand(this, command.Source);
                else
                {
                    Resource nearestResource = findNearestResource(CargoType);
                    if (nearestResource != null && !nearestResource.Depleted)
                        harvestCommand = new HarvestCommand(this, nearestResource);
                }

                if (harvestCommand != null)
                {
                    GiveCommand(harvestCommand);
                    Rts.pathFinder.AddPathFindRequest(harvestCommand, false, false, false);
                }
            }

            CargoAmount = 0;
            CargoType = null;

            //}
            /*else if (command.Source == null || command.Source.Depleted)
            {
                Resource nearestResource = findNearestResource(CargoType);
                if (nearestResource != null)
                    GiveCommand(new HarvestCommand(nearestResource, 1));
                else
                    NextCommand();
            }
            else
                GiveCommand(new HarvestCommand(command.Source, 1));*/
        }

        public void ReturnCargoToNearestTownHall(Resource source)
        {
            if (CargoAmount == 0)
                return;

            TownHall nearestTownHall = FindNearestTownHall();
            if (nearestTownHall != null)
            {
                //if (Commands.Count == 0 || !(Commands[0] is HarvestCommand))
                //{
                //    Resource nearestResource = findNearestResource(CargoType);
                //    if (nearestResource != null)
                //        InsertCommand(new HarvestCommand(nearestResource, 1));
                //}

                stop();

                InsertCommand(new ReturnCargoCommand(this, nearestTownHall, source));
            }
        }

        int refreshTargetTownHallDelay = 500, timeSinceLastRefresh = 0;
        void refreshTargetTownHall(ReturnCargoCommand command, GameTime gameTime)
        {
            timeSinceLastRefresh += (int)(gameTime.ElapsedGameTime.TotalMilliseconds * Rts.GameSpeed);
            if (timeSinceLastRefresh >= refreshTargetTownHallDelay)
            {
                timeSinceLastRefresh = 0;

                TownHall townHall = FindNearestTownHall();

                if (townHall == null)
                {
                    NextCommand();
                    return;
                }

                if (townHall != command.TargetStructure && Rts.pathFinder.Tools.IsStructureInLineOfSight(this, townHall))
                {
                    //command.TargetStructure = townHall;
                    //PathFinder.AddHighPriorityPathFindRequest(this, command, CurrentPathNode, 1, false);

                    Commands.Insert(1, new ReturnCargoCommand(this, townHall, command.Source));
                    NextCommand();
                    //InsertCommand(new ReturnCargoCommand(townHall, command.Source, 1));
                }
            }
        }

        public TownHall FindNearestTownHall()
        {
            TownHall nearestTownHall = null;
            float nearest = int.MaxValue;

            foreach (TownHall townHall in TownHall.TownHalls)
            {
                if (townHall.Team != Team)
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

        Resource findNearestResource(ResourceType resourceType)
        {
            Resource nearestResource = null;
            float nearest = int.MaxValue;

            foreach (Resource resource in Resource.Resources)
            {
                if (resource.Type != resourceType || resource == null || resource.Depleted)
                    continue;

                float distance = Vector2.DistanceSquared(CenterPoint, resource.CenterPoint);
                if (distance < nearest)
                {
                    nearestResource = resource;
                    nearest = distance;
                }
            }

            return nearestResource;
        }

        public override int TargetPriority
        {
            get
            {
                if (IgnoringCollision)
                    return 1;

                return base.TargetPriority;
            }
            set
            {
                base.TargetPriority = value;
            }
        }
    }
}
