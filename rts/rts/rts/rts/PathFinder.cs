using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;
using C5;

namespace rts
{
    class PathFinder
    {
        public Map Map;
        public PathNode[,] PathNodes;

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

            Thread = new Thread(new ThreadStart(DoPathFindRequests));
            Thread.IsBackground = true;
            Thread.Start();
        }

        void initializePathNodes()
        {
            // create path nodes for every walkable tile
            PathNodes = new PathNode[Map.Width, Map.Height];
            for (int i = 0; i < Map.Height; i++)
            {
                for (int s = 0; s < Map.Width; s++)
                {
                    //if (Map.Tiles[i, s].Walkable)
                    //{
                    PathNodes[i, s] = new PathNode(Map.Tiles[i, s]);
                    //}
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
                        if (i - 1 >= 0 && s - 1 >= 0)
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
                        }
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
        //public int MaxPathSize = int.MaxValue;
        //int pathCount = 0;
        public List<Vector2> FindPath(PathNode startNode, Vector2 endPoint, Unit unit, bool avoidUnits)
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
                    if (!neighbor.Tile.Walkable)
                        continue;

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
                    //if (!avoidUnits)
                    //    heuristic = HeuristicManhattan(neighbor.Tile.CenterPoint, endPoint);
                    //else
                    //{
                        if (currentNode.IsNeighborDiagonal[i])
                            heuristic = HeuristicManhattan(neighbor.Tile.CenterPoint, endPoint) + (neighbor.UnitsContained.Count * 14);
                        else
                            heuristic = HeuristicManhattan(neighbor.Tile.CenterPoint, endPoint) + (neighbor.UnitsContained.Count * 10);
                    //}

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
                                neighbor.DistanceTravelled = distanceTravelled + (neighbor.UnitsContained.Count * 1400);
                            else
                                neighbor.DistanceTravelled = distanceTravelled + (neighbor.UnitsContained.Count * 1000);
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

        public void SmoothPath(List<Vector2> path, Unit unit)
        {
            if (path.Count < 2)
                return;

            /*int i;
            if (Walkable(unit.CenterPoint, path[0], unit, 2))
                i = 0;
            else
                i = 1;

            for (; i < path.Count - 1; i++)
            {
                if (Walkable(path[i], path[i + 1], unit, 2))
                {
                    path.RemoveAt(i);
                    i--;
                }
                else
                    return;
            }*/

            /*for (int i = 0; i < path.Count - 1; )
            {
                if (Walkable(unit.CenterPoint, path[i + 1], unit, (int)(Vector2.Distance(unit.CenterPoint, path[i + 1]) / unit.Radius) * 2))
                {
                    path.RemoveAt(i);
                }
                else
                    break;
            }*/

            for (int i = 0; i < path.Count - 2; i++)
            {
                if (Walkable(path[i], path[i + 2], unit, (int)(Vector2.Distance(path[i], path[i + 2]) / unit.Diameter) * 2))
                {
                    path.RemoveAt(i + 1);
                    i--;
                }
            }

            /*for (int i = path.Count - 1; i >= 2; i--)
            {
                if (Walkable(path[i], path[i - 2], unit, (int)(Vector2.Distance(path[i], path[i + 2]) / unit.Radius) * 2))
                {
                    path.RemoveAt(i - 1);
                    i++;
                }
            }*/
        }
        public void SmoothImmediatePath(List<Vector2> path, Unit unit)
        {
            if (path.Count < 2)
                return;

            for (int i = path.Count - 1; i >= 1; i--)
            {
                if (Walkable(unit.CenterPoint, path[i], unit, (int)(Vector2.Distance(unit.CenterPoint, path[i]) / unit.Diameter)))
                {
                    path.RemoveRange(0, i);
                    break;
                }
            }

            /*for (int i = 0; i < path.Count; i++)
            {
                if (!Walkable(unit.CenterPoint, path[i], unit, (int)(Vector2.Distance(unit.CenterPoint, path[i]) / unit.Radius)))
                {
                    if (i != 0)
                        path.RemoveRange(0, i - 1);
                    break;
                }
            }*/

            /*for (int i = 0; i < path.Count - 1; )
            {
                if (Walkable(unit.CenterPoint, path[i + 1], unit, (int)(Vector2.Distance(unit.CenterPoint, path[i + 1]) / unit.Radius) * 2))
                {
                    path.RemoveAt(i);
                }
                else
                    break;
            }*/
        }
        public void SmoothPathEnd(List<Vector2> path, Unit unit)
        {
            if (path.Count == 2)
            {
                Vector2 point = path[path.Count - 1];
                if (Walkable(unit.CenterPoint, point, unit, (int)(Vector2.Distance(unit.CenterPoint, point) / unit.Diameter)))
                {
                    path.RemoveAt(path.Count - 2);
                }
            }
            else
            {
                Vector2 point1 = path[path.Count - 3];
                Vector2 point2 = path[path.Count - 1];
                if (Walkable(point1, point2, unit, (int)(Vector2.Distance(point1, point2) / unit.Diameter)))
                {
                    path.RemoveAt(path.Count - 2);
                }
            }
        }

        int numberOfCurvePoints = 100;
        public List<Vector2> CurvePath(List<Vector2> path, Unit unit)
        {
            /*if (path.Count < 2)
                return path;

            path.Insert(0, path[0]);

            if (path.Count < 4)
                path.Add(path[2]);*/

            if (path.Count < 4)
                return path;

            //path.Insert(0, unit.CenterPoint);

            List<Vector2> curvedPath = new List<Vector2>();

            //curvedPath.Add(unit.CenterPoint);
            //curvedPath.Add(path[path.Count - 1]);

            List<Vector2> curvePoints1 = new List<Vector2>();
            List<Vector2> curvePoints2 = new List<Vector2>();

            float weightIncrement = 1f / numberOfCurvePoints;

            for (float s = weightIncrement; s < 1f; s += weightIncrement)
            {
                //curvedPath.Add(Vector2.CatmullRom(unit.CenterPoint, path[0], path[1], path[2], s));
                curvePoints1.Add(Vector2.CatmullRom(unit.CenterPoint, path[0], path[1], path[2], s));
            }

            for (float s = weightIncrement; s < 1f; s += weightIncrement)
            {
                //curvedPath.Add(Vector2.CatmullRom(unit.CenterPoint, path[0], path[1], path[2], s));
                curvePoints2.Add(Vector2.CatmullRom(path[0], path[1], path[2], path[3], s));
            }
            //path.InsertRange(0, curvePoints1);
            path.InsertRange(1, curvePoints2);
            //curvedPath.Add(path[0]);

            //curvedPath.Add(path[0]);
            /*for (int i = 1; i < path.Count - 2; i++)
            {
                Vector2 point1 = path[i - 1];
                Vector2 point2 = path[i];
                Vector2 point3 = path[i + 1];
                Vector2 point4 = path[i + 2];

                curvedPath.Add(point2);
                for (float s = 0; s < 1f; s += weightIncrement)
                {
                    curvedPath.Add(Vector2.CatmullRom(point1, point2, point3, point4, s));
                }
                curvedPath.Add(point3);
            }

            for (float s = 0; s < 1f; s += weightIncrement)
            {
                curvedPath.Add(Vector2.CatmullRom(path[path.Count - 3], path[path.Count - 2], path[path.Count - 1], path[path.Count - 1], s));
            }

            curvedPath.Add(path[path.Count - 1]);

            return curvedPath;*/

            return path;
        }

        // find nearest walkable path node
        public PathNode FindNearestPathNode(int y, int x)
        {
            PathNode node = PathNodes[y, x];

            if (node.Tile.Walkable)
                return node;

            PathNode neighbor;

            for (int i = 0; ; i++)
            {
                if (y - i >= 0)
                {
                    neighbor = PathNodes[y - i, x];
                    if (neighbor.Tile.Walkable)
                        return neighbor;
                }
                if (y + i < Map.Height)
                {
                    neighbor = PathNodes[y + i, x];
                    if (neighbor.Tile.Walkable)
                        return neighbor;
                }
                if (x - i >= 0)
                {
                    neighbor = PathNodes[y, x - i];
                    if (neighbor.Tile.Walkable)
                        return neighbor;
                }
                if (x + i < Map.Width)
                {
                    neighbor = PathNodes[y, x + i];
                    if (neighbor.Tile.Walkable)
                        return neighbor;
                }
                if (y - i >= 0 && x - i >= 0)
                {
                    neighbor = PathNodes[y - i, x - i];
                    if (neighbor.Tile.Walkable)
                        return neighbor;
                }
                if (y - i >= 0 && x + i < Map.Width)
                {
                    neighbor = PathNodes[y - i, x + i];
                    if (neighbor.Tile.Walkable)
                        return neighbor;
                }
                if (y + i < Map.Height && x - i >= 0)
                {
                    neighbor = PathNodes[y + i, x - i];
                    if (neighbor.Tile.Walkable)
                        return neighbor;
                }
                if (y + i < Map.Height && x + i < Map.Width)
                {
                    neighbor = PathNodes[y + i, x + i];
                    if (neighbor.Tile.Walkable)
                        return neighbor;
                }
            }
        }
        // find nearest walkable path node (considers unit size)
        public PathNode FindNearestPathNode(int y, int x, Unit unit)
        {
            if (IsTileWalkable(y, x, unit))
                return PathNodes[y, x];

            for (int i = 0; ; i++)
            {
                if (y - i >= 0)
                {
                    if (IsTileWalkable(y - i, x, unit))
                        return PathNodes[y - i, x];
                }
                if (y + i < Map.Height)
                {
                    if (IsTileWalkable(y + i, x, unit))
                        return PathNodes[y + i, x];
                }
                if (x - i >= 0)
                {
                    if (IsTileWalkable(y, x - i, unit))
                        return PathNodes[y, x - i];
                }
                if (x + i < Map.Width)
                {
                    if (IsTileWalkable(y, x + i, unit))
                        return PathNodes[y, x + i];
                }
                if (y - i >= 0 && x - i >= 0)
                {
                    if (IsTileWalkable(y - i, x - i, unit))
                        return PathNodes[y - i, x - i];
                }
                if (y - i >= 0 && x + i < Map.Width)
                {
                    if (IsTileWalkable(y - i, x + i, unit))
                        return PathNodes[y - i, x + i];
                }
                if (y + i < Map.Height && x - i >= 0)
                {
                    if (IsTileWalkable(y + i, x - i, unit))
                        return PathNodes[y + i, x - i];
                }
                if (y + i < Map.Height && x + i < Map.Width)
                {
                    if (IsTileWalkable(y + i, x + i, unit))
                        return PathNodes[y + i, x + i];
                }
            }
        }

        public bool Walkable(Vector2 point1, Vector2 point2, Unit unit, int numberOfIntermediatePoints)
        {
            float radius = unit.Radius;
            float lerpAmountIncrement = 1f / (numberOfIntermediatePoints + 1);

            for (float l = lerpAmountIncrement; l < 1f; l += lerpAmountIncrement)
            //for (float l = 1f - lerpAmountIncrement; l > 0; l -= lerpAmountIncrement)
            {
                Vector2 intermediatePoint = Vector2.Lerp(point1, point2, l);

                if (!IsPointWalkable(intermediatePoint))
                    return false;
            }

            return true;
        }

        public bool IsPointWalkable(Vector2 point)
        {
            int y = (int)(point.Y / Map.TileSize);
            int x = (int)(point.X / Map.TileSize);

            //return (y >= 0 && y < Map.Height && x >= 0 && x < Map.Width && PathNodes[y, x].Tile.Walkable);
            return (!(y < 0 || y >= Map.Height || x < 0 || x >= Map.Width) && PathNodes[y, x].Tile.Walkable);
        }
        public bool IsPointWalkable(Vector2 point, Unit unit)
        {
            float radius = unit.Radius;

            if (!IsPointWalkable(new Vector2(point.X - radius, point.Y - radius)))
                return false;
            if (!IsPointWalkable(new Vector2(point.X + radius, point.Y + radius)))
                return false;
            if (!IsPointWalkable(new Vector2(point.X + radius, point.Y - radius)))
                return false;
            if (!IsPointWalkable(new Vector2(point.X - radius, point.Y + radius)))
                return false;

            return true;
        }

        public bool IsTileWalkable(int tileY, int tileX, Unit unit)
        {
            if (!Map.Tiles[tileY, tileX].Walkable)
                return false;

            int howManyTilesToExpand = (int)(unit.Diameter / (Map.TileSize + .01f));

            int lowY = (int)MathHelper.Max(tileY - howManyTilesToExpand, 0);
            int lowX = (int)MathHelper.Max(tileX - howManyTilesToExpand, 0);
            int highY = (int)MathHelper.Min(tileY + howManyTilesToExpand, Map.Height);
            int highX = (int)MathHelper.Min(tileX + howManyTilesToExpand, Map.Width);

            for (int y = lowY; y <= highY; y++)
            {
                for (int x = lowX; x <= highX; x++)
                {
                    if (!Map.Tiles[y, x].Walkable)
                        return false;
                }
            }

            return true;
        }

        bool isPathWalkable(List<Vector2> path, Unit unit)
        {
            return Walkable(path[0], path[path.Count - 1], unit, path.Count * 4);
        }

        public TimeSpan TimeSpentPathFinding = TimeSpan.Zero;
        public Object TimeSpentPathFindingLock = new Object();
        public bool Go;
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
                request.WayPoints = FindPath(request.Unit.CurrentPathNode, request.Command.Destination, request.Unit, request.AvoidUnits);

                // smooth path
                if (!request.AvoidUnits)
                {
                    SmoothPath(request.WayPoints, request.Unit);
                    SmoothImmediatePath(request.WayPoints, request.Unit);
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

        //Queue<PathFindRequest> requestsToAdd = new Queue<PathFindRequest>();
        Queue<PathFindRequest> highPriorityRequestsToAdd = new Queue<PathFindRequest>();
        Queue<PathFindRequest> lowPriorityRequestsToAdd = new Queue<PathFindRequest>();
        /*public void AddPathFindRequest(Unit unit, MoveCommand command, PathNode startNode, int priority, bool avoidUnits)
        {
            command.Calculated = false;

            //requestsToAdd.Enqueue(new PathFindRequest(unit, command, startNode, priority, avoidUnits));

            if (priority == 0)
                highPriorityRequestsToAdd.Enqueue(new PathFindRequest(unit, command, startNode, (int)Vector2.Distance(unit.CenterPoint, command.Destination), avoidUnits));
            else
                lowPriorityRequestsToAdd.Enqueue(new PathFindRequest(unit, command, startNode, (int)Vector2.Distance(unit.CenterPoint, command.Destination), avoidUnits));
        }*/

        public void AddHighPriorityPathFindRequest(Unit unit, MoveCommand command, PathNode startNode, int priority, bool avoidUnits)
        {
            command.Calculated = false;
            highPriorityRequestsToAdd.Enqueue(new PathFindRequest(unit, command, startNode, priority, avoidUnits));
        }
        public void AddLowPriorityPathFindRequest(Unit unit, MoveCommand command, PathNode startNode, int priority, bool avoidUnits)
        {
            command.Calculated = false;
            lowPriorityRequestsToAdd.Enqueue(new PathFindRequest(unit, command, startNode, priority, avoidUnits));
        }

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

        public void FulfillDonePathFindRequests()
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
                }
            }
        }

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
    }

    class PathNode
    {
        public MapTile Tile;
        public List<PathNode> Neighbors = new List<PathNode>();
        public List<bool> IsNeighborDiagonal = new List<bool>();
        public List<Unit> UnitsContained = new List<Unit>();

        public PathNode Parent;

        public bool InOpenList, InClosedList;

        public float DistanceToGoal, DistanceTravelled;

        public PathNode(MapTile tile)
        {
            Tile = tile;
        }
    }

    class PathNodeFComparer : IComparer<PathNode>
    {
        public int Compare(PathNode p1, PathNode p2)
        {
            //return (int)(p1.DistanceToGoal - p2.DistanceToGoal);
            if (p1.DistanceToGoal < p2.DistanceToGoal)
                return -1;
            else if (p1.DistanceToGoal > p2.DistanceToGoal)
                return 1;
            else return 0;
        }
    }

    class PathFindRequest
    {
        /*public static Queue<PathFindRequest> PathFindRequests = new Queue<PathFindRequest>();
        public static Object PathFindRequestsLock = new Object();*/
        //public static IntervalHeap<PathFindRequest> PathFindRequests = new IntervalHeap<PathFindRequest>(new PathFindRequestComparer());
        public static IntervalHeap<PathFindRequest> HighPriorityPathFindRequests = new IntervalHeap<PathFindRequest>(new PathFindRequestComparer());
        public static IntervalHeap<PathFindRequest> LowPriorityPathFindRequests = new IntervalHeap<PathFindRequest>(new PathFindRequestComparer());

        public static Queue<PathFindRequest> DonePathFindRequests = new Queue<PathFindRequest>();
        public static Object DonePathFindRequestsLock = new Object();

        public Unit Unit;
        public MoveCommand Command;
        public PathNode StartNode;
        public bool AvoidUnits;
        public int Priority;

        public List<Vector2> WayPoints;
        //public Object WayPointsLock = new Object();

        public PathFindRequest(Unit unit, MoveCommand command, PathNode startNode, int priority, bool avoidUnits)
        {
            Unit = unit;
            Command = command;
            StartNode = startNode;
            AvoidUnits = avoidUnits;
            Priority = priority;
        }
    }

    class PathFindRequestComparer : IComparer<PathFindRequest>
    {
        public int Compare(PathFindRequest p1, PathFindRequest p2)
        {
            //return (int)(p1.DistanceToGoal - p2.DistanceToGoal);
            if (p1.Priority < p2.Priority)
                return -1;
            else if (p1.Priority > p2.Priority)
                return 1;
            else return 0;
        }
    }
}
