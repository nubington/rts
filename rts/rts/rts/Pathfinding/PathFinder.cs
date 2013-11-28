using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using C5;
using Lidgren.Network;

namespace rts
{
    public class PathFinder
    {
        public Map Map;
        public PathNode[,] PathNodes;
        public PathFinderTools Tools;

        Thread Thread;

        Unit currentUnit;

        //public List<PathNode> OpenList = new List<PathNode>();
        PathNodeFComparer nodeComparer = new PathNodeFComparer();
        public IntervalHeap<PathNode> OpenList = new IntervalHeap<PathNode>(new PathNodeFComparer());
        public List<PathNode> ClosedList = new List<PathNode>();
        List<PathNode> CleanupList = new List<PathNode>();

        public PathFinder(Map m)
        {
            Map = m;
            initializePathNodes();
            Tools = new PathFinderTools(this);

            Thread = new Thread(new ThreadStart(DoPathFindRequests));
            Thread.IsBackground = true;
            Thread.Start();
        }

        void initializePathNodes()
        {
            // create path nodes for every tile
            PathNodes = new PathNode[Map.Height, Map.Width];
            for (int i = 0; i < Map.Height; i++)
            {
                for (int s = 0; s < Map.Width; s++)
                {
                    PathNodes[i, s] = new PathNode(Map.Tiles[i, s]);
                }
            }

            // initialize neighbors of each pathnode
            for (int i = 0; i < Map.Height; i++)
            {
                for (int s = 0; s < Map.Width; s++)
                {
                    PathNode node = PathNodes[i, s];
                    if (node.Tile.Walkable)
                    {
                        if (i - 1 >= 0)
                        {
                            PathNode neighbor = PathNodes[i - 1, s];
                            if (neighbor.Tile.Walkable)
                            {
                                node.Neighbors.Add(neighbor);
                                node.IsNeighborDiagonal.Add(false);
                            }
                        }
                        if (i + 1 < Map.Height)
                        {
                            PathNode neighbor = PathNodes[i + 1, s];
                            if (neighbor.Tile.Walkable)
                            {
                                node.Neighbors.Add(neighbor);
                                node.IsNeighborDiagonal.Add(false);
                            }
                        }
                        if (s - 1 >= 0)
                        {
                            PathNode neighbor = PathNodes[i, s - 1];
                            if (neighbor.Tile.Walkable)
                            {
                                node.Neighbors.Add(neighbor);
                                node.IsNeighborDiagonal.Add(false);
                            }
                        }
                        if (s + 1 < Map.Width)
                        {
                            PathNode neighbor = PathNodes[i, s + 1];
                            if (neighbor.Tile.Walkable)
                            {
                                node.Neighbors.Add(neighbor);
                                node.IsNeighborDiagonal.Add(false);
                            }
                        }
                        /*if (i - 1 >= 0 && s - 1 >= 0)
                        {
                            PathNode neighbor = PathNodes[i - 1, s - 1];
                            if (neighbor.Tile.Walkable)
                            {
                                node.Neighbors.Add(neighbor);
                                node.IsNeighborDiagonal.Add(true);
                            }
                        }
                        if (i - 1 >= 0 && s + 1 < Map.Width)
                        {
                            PathNode neighbor = PathNodes[i - 1, s + 1];
                            if (neighbor.Tile.Walkable)
                            {
                                node.Neighbors.Add(neighbor);
                                node.IsNeighborDiagonal.Add(true);
                            }
                        }
                        if (i + 1 < Map.Height && s - 1 >= 0)
                        {
                            PathNode neighbor = PathNodes[i + 1, s - 1];
                            if (neighbor.Tile.Walkable)
                            {
                                node.Neighbors.Add(neighbor);
                                node.IsNeighborDiagonal.Add(true);
                            }
                        }
                        if (i + 1 < Map.Height && s + 1 < Map.Width)
                        {
                            PathNode neighbor = PathNodes[i + 1, s + 1];
                            if (neighbor.Tile.Walkable)
                            {
                                node.Neighbors.Add(neighbor);
                                node.IsNeighborDiagonal.Add(true);
                            }
                        }*/
                    }
                }
            }
        }

        // estimate distance between two points
        float HeuristicManhattan(Vector2 point1, Vector2 point2)
        {
            return (Math.Abs(point1.X - point2.X) +
                   Math.Abs(point1.Y - point2.Y)) * 10 * 1.001f;
        }
        float HeuristicDiagonal(Vector2 point1, Vector2 point2)
        {
            return 10 * MathHelper.Max(Math.Abs(point1.X - point2.X), Math.Abs(point1.Y - point2.Y));
        }

        void ResetSearchNodes()
        {
            //OpenList.Clear();
            //ClosedList.Clear();
            //OpenList = new List<PathNode>();
            OpenList = new IntervalHeap<PathNode>(nodeComparer);
            //ClosedList = new List<PathNode>();
            CleanupList = new List<PathNode>();

            /*foreach (PathNode node in PathNodes)
            {
                if (node != null)
                {
                    node.InOpenList = false;
                    node.InClosedList = false;

                    //node.DistanceTravelled = float.MaxValue;
                    //node.DistanceToGoal = float.MaxValue;
                }
            }*/

            /*for (int y = 0; y < Map.Height; y++)
            {
                for (int x = 0; x < Map.Width; x++)
                {
                    PathNode node = PathNodes[y, x];

                    if (node == null)
                        continue;

                    node.InOpenList = false;
                    node.InClosedList = false;

                    node.DistanceTravelled = float.MaxValue;
                    node.DistanceToGoal = float.MaxValue;
                }
            }*/
        }

        void Cleanup()
        {
            //pathCount = 0;

            foreach (PathNode node in CleanupList)
            {
                node.InOpenList = false;
                node.InClosedList = false;
            }
        }

        PathNode FindBestNode()
        {
            /*PathNode currentTile = OpenList[0];

            float smallestDistanceToGoal = currentTile.DistanceToGoal;

            // Find the closest node to the goal.
            for (int i = 1; i < OpenList.Count; i++)
            {
                if (OpenList[i].DistanceToGoal < smallestDistanceToGoal)
                {
                    currentTile = OpenList[i];
                    smallestDistanceToGoal = currentTile.DistanceToGoal;
                }
            }
            return currentTile;*/

            return OpenList.DeleteMin();
        }

        private List<Vector2> FindFinalPath(PathNode startNode, PathNode endNode, Vector2 endPoint)
        {
            //ClosedList.Add(endNode);

            PathNode parentNode = endNode.Parent;

            // Trace back through the nodes using the parent fields
            // to find the best path.
            /*while (parentNode != startNode)
            {
                ClosedList.Add(parentNode);
                parentNode = parentNode.Parent;
            }*/

            /*List<Vector2> finalPath = new List<Vector2>();

            // Reverse the path and transform into world space.
            for (int i = ClosedList.Count - 1; i >= 1; i--)
            {
                finalPath.Add(ClosedList[i].Tile.CenterPoint);
            }
            finalPath.Add(endPoint);

            return finalPath;*/

            List<Vector2> finalPath = new List<Vector2>();

            //finalPath.Add(endNode.Tile.CenterPoint);
            while (parentNode != startNode)
            {
                //if (parentNode.Parent.Parent == null ||
                    //!Walkable(parentNode.Tile.CenterPoint, parentNode.Parent.Parent.Tile.CenterPoint, currentUnit, 2))
                    finalPath.Insert(0, parentNode.Tile.CenterPoint);
                parentNode = parentNode.Parent;
            }
            finalPath.Add(endPoint);

            return finalPath;
        }
       
        public List<Vector2> FindPath(PathNode startNode, Vector2 endPoint, Unit unit, BaseObject target, bool avoidUnits)
        {
            // Only try to find a path if the start and end points are different.
            if (Vector2.Distance(startNode.Tile.CenterPoint, endPoint) <= Map.TileSize)
            {
                List<Vector2> list = new List<Vector2>();
                list.Add(endPoint);
                Cleanup();
                return list;
            }

            /*int startPointY = (int)(startPoint.Y / Map.TILESIZE);
            int startPointX = (int)(startPoint.X / Map.TILESIZE);

            if ((startPointY < 0 || startPointY >= Map.Height || startPointX < 0 || startPointX >= Map.Width)
                || (PathNodes[startPointY, startPointX] == null))
                offset = Vector2.Zero;
            else
                offset = startNode.Tile.CenterPoint - startPoint;*/

            Vector2 startPoint = startNode.Tile.CenterPoint;
            PathNode endNode = PathNodes[(int)(endPoint.Y / Map.TileSize), (int)(endPoint.X / Map.TileSize)];
            currentUnit = unit;

            /////////////////////////////////////////////////////////////////////
            // Step 1 : Clear the Open and Closed Lists and reset each node’s F 
            //          and G values in case they are still set from the last 
            //          time we tried to find a path. 
            /////////////////////////////////////////////////////////////////////
            ResetSearchNodes();

            /////////////////////////////////////////////////////////////////////
            // Step 2 : Set the start node’s G value to 0 and its F value to the 
            //          estimated distance between the start node and goal node 
            //          (this is where our H function comes in) and add it to the 
            //          Open List. 
            /////////////////////////////////////////////////////////////////////
            startNode.InOpenList = true;

            //if (!avoidUnits)
                //startNode.DistanceToGoal = HeuristicManhattan(startPoint, endPoint);
            //else
                startNode.DistanceToGoal = HeuristicManhattan(startPoint, endPoint);
            startNode.DistanceTravelled = 0;

            OpenList.Add(startNode);
            CleanupList.Add(startNode);

            /////////////////////////////////////////////////////////////////////
            // Setp 3 : While there are still nodes to look at in the Open list : 
            /////////////////////////////////////////////////////////////////////
            while (OpenList.Count > 0)
            {
                /////////////////////////////////////////////////////////////////
                // a) : Loop through the Open List and find the node that 
                //      has the smallest F value.
                /////////////////////////////////////////////////////////////////
                PathNode currentNode = FindBestNode();
                //pathCount++;

                /////////////////////////////////////////////////////////////////
                // c) : If the Active Node is the goal node, we will 
                //      find and return the final path.
                /////////////////////////////////////////////////////////////////
                //if (pathCount >= MaxPathSize)
                //{
                //    Cleanup();
                //    return FindFinalPath(startNode, currentNode, endPoint);
                //}
                if (currentNode == endNode)
                {
                    // Trace our path back to the start.
                    Cleanup();
                    return FindFinalPath(startNode, currentNode, endPoint);
                }

                /////////////////////////////////////////////////////////////////
                // d) : Else, for each of the Active Node’s neighbours :
                /////////////////////////////////////////////////////////////////
                for (int i = 0; i < currentNode.Neighbors.Count; i++)
                {
                    PathNode neighbor = currentNode.Neighbors[i];

                    //////////////////////////////////////////////////
                    // i) : Make sure that the neighbouring node can 
                    //      be walked across. 
                    //////////////////////////////////////////////////
                    if (neighbor.Blocked)
                    {
                        if (target == null || neighbor.Blocker != target)
                            continue;
                    }
                    if (!neighbor.Tile.Walkable)
                        continue;
                    //if (avoidUnits && neighbor.UnitsContained.Count > 0)
                    //    continue;

                    //////////////////////////////////////////////////
                    // ii) Calculate a new G value for the neighbouring node.
                    //////////////////////////////////////////////////
                    float distanceTravelled;
                    if (currentNode.IsNeighborDiagonal[i])
                    {
                        if (avoidUnits)
                            distanceTravelled = currentNode.DistanceTravelled + 14;
                        else
                            continue;
                    }
                    else
                        distanceTravelled = currentNode.DistanceTravelled + 10;

                    // An estimate of the distance from this node to the end node.
                    float heuristic;
                    if (!avoidUnits)
                        heuristic = HeuristicManhattan(neighbor.Tile.CenterPoint, endPoint);
                    else
                    {
                        if (currentNode.IsNeighborDiagonal[i])
                            heuristic = HeuristicManhattan(neighbor.Tile.CenterPoint, endPoint) + (neighbor.UnitsContained.Count * 14);
                        else
                            heuristic = HeuristicManhattan(neighbor.Tile.CenterPoint, endPoint) + (neighbor.UnitsContained.Count * 10);
                    }

                    //////////////////////////////////////////////////
                    // iii) If the neighbouring node is not in either the Open 
                    //      List or the Closed List : 
                    //////////////////////////////////////////////////
                    if (!neighbor.InOpenList && !neighbor.InClosedList)
                    {
                        // (1) Set the neighbouring node’s G value to the G value 
                        //     we just calculated.
                        if (avoidUnits)
                        {
                            if (currentNode.IsNeighborDiagonal[i])
                                neighbor.DistanceTravelled = distanceTravelled + (neighbor.UnitsContained.Count * 1400000);
                            else
                                neighbor.DistanceTravelled = distanceTravelled + (neighbor.UnitsContained.Count * 1000000);
                        }
                        else
                            neighbor.DistanceTravelled = distanceTravelled;
                        // (2) Set the neighbouring node’s F value to the new G value + 
                        //     the estimated distance between the neighbouring node and
                        //     goal node.

                        // normal A*
                        neighbor.DistanceToGoal = neighbor.DistanceTravelled + heuristic;
                        // best-first
                        //neighbor.DistanceToGoal = heuristic;

                        // (3) Set the neighbouring node’s Parent property to point at the Active 
                        //     Node.
                        neighbor.Parent = currentNode;
                        // (4) Add the neighbouring node to the Open List.
                        neighbor.InOpenList = true;
                        OpenList.Add(neighbor);
                        CleanupList.Add(neighbor);
                    }
                    //////////////////////////////////////////////////
                    // iv) Else if the neighbouring node is in either the Open 
                    //     List or the Closed List :
                    //////////////////////////////////////////////////
                    else if (neighbor.InOpenList || neighbor.InClosedList)
                    {
                        // (1) If our new G value is less than the neighbouring 
                        //     node’s G value, we basically do exactly the same 
                        //     steps as if the nodes are not in the Open and 
                        //     Closed Lists except we do not need to add this node 
                        //     the Open List again.
                        if (neighbor.DistanceTravelled > distanceTravelled)
                        {
                            neighbor.DistanceTravelled = distanceTravelled;
                            neighbor.DistanceToGoal = distanceTravelled + heuristic;
                            //neighbor.DistanceToGoal = heuristic;

                            neighbor.Parent = currentNode;
                        }
                    }
                }

                /////////////////////////////////////////////////////////////////
                // e) Remove the Active Node from the Open List and add it to the 
                //    Closed List
                /////////////////////////////////////////////////////////////////
                //OpenList.Remove(currentNode);
                currentNode.InClosedList = true;
            }

            // No path could be found.
            List<Vector2> l = new List<Vector2>();
            l.Add(endPoint);
            Cleanup();
            return l;
        }

        public TimeSpan TimeSpentPathFinding = TimeSpan.Zero;
        public Object TimeSpentPathFindingLock = new Object();
        void DoPathFindRequests()
        {
            PathFindRequest request;

            while (true)
            {
                // get a request or sleep 1 ms and try again
                if (PathFindRequest.HighPriorityPathFindRequests.Count > 0)
                {
                    lock (PathFindRequest.HighPriorityPathFindRequests)
                    {
                        request = PathFindRequest.HighPriorityPathFindRequests.DeleteMax();
                    }
                }
                else if (PathFindRequest.LowPriorityPathFindRequests.Count > 0)
                {
                    lock (PathFindRequest.LowPriorityPathFindRequests)
                    {
                        request = PathFindRequest.LowPriorityPathFindRequests.DeleteMax();
                    }
                }
                else
                {
                    Thread.Sleep(1);
                    continue;
                }

                // if request is has been deactivated dont do it
                if (!request.Command.Active)
                    continue;

                DateTime startTime = DateTime.Now;

                // calculate path
                request.WayPoints = FindPath(request.StartNode, request.Command.Destination, request.Command.Unit, request.Target, request.AvoidUnits);

                // smooth path
                if (!request.AvoidUnits)
                {
                    //if (!(request.Command is AttackCommand))
                    {
                        //if (!(request.Command is AttackCommand))

                        Tools.SmoothPath(request.WayPoints, request.Command.Unit);
                        Tools.SmoothImmediatePath(request.WayPoints, request.Command.Unit, request.StartNode.Tile.CenterPoint);
                    }
                }

                // add finished request to the done queue
                lock (PathFindRequest.DonePathFindRequestsLock)
                {
                    PathFindRequest.DonePathFindRequests.Enqueue(request);
                }

                lock (TimeSpentPathFindingLock)
                {
                    TimeSpentPathFinding += (DateTime.Now - startTime);
                }
            }
        }


        //************************************************************************************
        // request queue stuff
        //************************************************************************************

        Queue<PathFindRequest> highPriorityRequestsToAdd = new Queue<PathFindRequest>();
        Queue<PathFindRequest> lowPriorityRequestsToAdd = new Queue<PathFindRequest>();

        // public method for adding pathfinding requests
        public void AddPathFindRequest(MoveCommand command, bool queued, bool recalculatingCurrentPath, bool avoidUnits)
        {
            int distancePriority;

            if (!queued)
            {
                distancePriority = (int)Vector2.DistanceSquared(command.Unit.CenterPoint, command.Destination);

                if (!recalculatingCurrentPath)
                    addHighPriorityPathFindRequest(command, distancePriority, avoidUnits);
                else
                    addLowPriorityPathFindRequest(command, command.Unit.CurrentPathNode, distancePriority, avoidUnits);
            }
            else
            {
                Vector2 beginLocation = command.Unit.FinalMoveDestination;

                distancePriority = (int)Vector2.DistanceSquared(beginLocation, command.Destination);

                addLowPriorityPathFindRequest(command, Tools.PathNodeAt(beginLocation), distancePriority, avoidUnits);
            }
        }

        // without attack target
        void addHighPriorityPathFindRequest(MoveCommand command, int priority, bool avoidUnits)
        {
            command.Calculated = false;
            //highPriorityRequestsToAdd.Enqueue(new PathFindRequest(command, command.Unit.CurrentPathNode, priority, avoidUnits));

            highPriorityRequestsToAdd.Enqueue(new PathFindRequest(command, command.Unit.CurrentPathNode, priority, avoidUnits));
        }
        /*// with attack target
        public void AddHighPriorityPathFindRequest(Unit unit, RtsObject attackTarget, MoveCommand command, PathNode startNode, int priority, bool avoidUnits)
        {
            command.Calculated = false;
            highPriorityRequestsToAdd.Enqueue(new PathFindRequest(unit, attackTarget, command, startNode, priority, avoidUnits));
        }*/

        // without attack target
        void addLowPriorityPathFindRequest(MoveCommand command, PathNode startNode, int priority, bool avoidUnits)
        {
            command.Calculated = false;
            lowPriorityRequestsToAdd.Enqueue(new PathFindRequest(command, startNode, priority, avoidUnits));
        }
        /*// with attack target
        public void AddLowPriorityPathFindRequest(Unit unit, RtsObject attackTarget, MoveCommand command, PathNode startNode, int priority, bool avoidUnits)
        {
            command.Calculated = false;
            lowPriorityRequestsToAdd.Enqueue(new PathFindRequest(unit, attackTarget, command, startNode, priority, avoidUnits));
        }
        */
        public void FinalizeAddingPathFindRequests()
        {
            /*lock (PathFindRequest.PathFindRequests)
            {
                while (requestsToAdd.Count > 0)
                {
                    PathFindRequest.PathFindRequests.Add(requestsToAdd.Dequeue());
                }
            }*/
            lock (PathFindRequest.HighPriorityPathFindRequests)
            {
                while (highPriorityRequestsToAdd.Count > 0)
                {
                    PathFindRequest.HighPriorityPathFindRequests.Add(highPriorityRequestsToAdd.Dequeue());
                }
            }
            lock (PathFindRequest.LowPriorityPathFindRequests)
            {
                while (lowPriorityRequestsToAdd.Count > 0)
                {
                    PathFindRequest.LowPriorityPathFindRequests.Add(lowPriorityRequestsToAdd.Dequeue());
                }
            }
        }

        public void FulfillDonePathFindRequests(NetPeer netPeer, NetConnection connection)
        {
            lock (PathFindRequest.DonePathFindRequestsLock)
            {
                while (PathFindRequest.DonePathFindRequests.Count > 0)
                {
                    PathFindRequest request = PathFindRequest.DonePathFindRequests.Dequeue();
                    request.Command.WayPoints = request.WayPoints;
                    request.Command.Calculated = true;
                    //if (!request.AvoidUnits)
                    //    pathfinder.SmoothPath(request.Command.WayPoints, request.Unit);

                    /*NetOutgoingMessage msg = netPeer.CreateMessage();
                    msg.Write(MessageID.PATH_UPDATE);
                    //msg.Write(request.Command.ID);
                    msg.Write(request.Command.Unit.ID);
                    msg.Write(request.Command.Unit.Team);
                    msg.Write((short)request.Command.Destination.X);
                    msg.Write((short)request.Command.Destination.Y);
                    msg.Write((short)request.Command.WayPoints.Count);

                    foreach (Vector2 wayPoint in request.Command.WayPoints)
                    {
                        msg.Write((short)wayPoint.X);
                        msg.Write((short)wayPoint.Y);
                    }
                    netPeer.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);*/
                }
            }
        }

        //************************************************************************************


        //************************************************************************************

        // thread stuff

        public void SuspendThread()
        {
            Thread.Suspend();
        }
        public void ResumeThread()
        {
            if (!Thread.IsAlive)
                Thread.Resume();
        }
        public void AbortThread()
        {
            Thread.Abort();
        }

        //************************************************************************************
    }
}
