using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace rts
{
    public class PathFinderTools
    {
        PathFinder pathFinder;
        Map Map;
        PathNode[,] PathNodes;

        public PathFinderTools(PathFinder pathFinder)
        {
            this.pathFinder = pathFinder;
            Map = pathFinder.Map;
            PathNodes = pathFinder.PathNodes;
        }

        public PathNode PathNodeAt(Vector2 location)
        {
            int y = (int)MathHelper.Clamp(location.Y / pathFinder.Map.TileSize, 0, pathFinder.Map.Height - 1);
            int x = (int)MathHelper.Clamp(location.X / pathFinder.Map.TileSize, 0, pathFinder.Map.Width - 1);

            return pathFinder.PathNodes[y, x];
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
                if (Walkable(path[i], path[i + 2], unit, (int)(Vector2.Distance(path[i], path[i + 2]) / unit.Diameter)))
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
        public void SmoothImmediatePath(List<Vector2> path, Unit unit, Vector2 beginLocation)
        {
            if (path.Count < 2)
                return;

            /*for (int i = path.Count - 1; i >= 1; i--)
            {
                if (Walkable(unit.CenterPoint, path[i], unit, (int)(Vector2.Distance(unit.CenterPoint, path[i]) / unit.Diameter)))
                {
                    path.RemoveRange(0, i);
                    break;
                }
            }*/

            int count = 0;

            for (int i = 1; i < path.Count; i++)
            {
                if (Walkable(beginLocation, path[i], unit, (int)(Vector2.Distance(beginLocation, path[i]) / unit.Diameter)))
                {
                    path.RemoveAt(i - 1);
                }
                else
                {
                    if (++count == 5)
                        return;
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
            y = (int)MathHelper.Clamp(y, 0, Map.Height - 1);
            x = (int)MathHelper.Clamp(x, 0, Map.Width - 1);

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
        public PathNode FindNearestPathNode(int startY, int startX, Unit unit)
        {
            startX = (int)MathHelper.Clamp(startX, 0, Map.Width - 1);
            startY = (int)MathHelper.Clamp(startY, 0, Map.Height - 1);

            if (IsNodeWalkable(startY, startX, unit))
                return PathNodes[startY, startX];

            /*for (int i = 0; ; i++)
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
            }*/

            // searches outward from start node for walkable nodes
            List<PathNode> walkableNodes = new List<PathNode>();
            for (int i = 1; ; i++)
            {
                for (int x = startX - i; x <= startX + i; x += i * 2)
                {
                    if (x < 0 || x > Map.Width - 1)
                        continue;
                    for (int y = startY - i; y <= startY + i; y += i * 2)
                    {
                        if (y < 0 || y > Map.Height - 1)
                            continue;
                        if (IsNodeWalkable(y, x, unit))
                            //return PathNodes[y, x];
                            walkableNodes.Add(PathNodes[y, x]);
                    }
                }
                if (walkableNodes.Count > 0)
                    break;
            }

            // return discovered path node that is closest to unit
            float shortestDistanceToUnit = float.MaxValue;
            int shortestIndex = 0;
            for (int i = 0; i < walkableNodes.Count; i++)
            {
                float distance = Vector2.Distance(walkableNodes[i].Tile.CenterPoint, unit.CenterPoint);
                if (distance < shortestDistanceToUnit)
                {
                    shortestDistanceToUnit = distance;
                    shortestIndex = i;
                }
            }
            return walkableNodes[shortestIndex];
        }

        // find walkable pathnode that is nearest to given RtsObject
        public PathNode FindNearestPathNode(int startY, int startX, RtsObject o)
        {
            // searches outward from start node for walkable nodes
            List<PathNode> walkableNodes = new List<PathNode>();
            for (int i = 1; ; i++)
            {
                for (int x = startX - i; x <= startX + i; x += i * 2)
                {
                    if (x < 0 || x > Map.Width - 1)
                        continue;
                    for (int y = startY - i; y <= startY + i; y += i * 2)
                    {
                        if (y < 0 || y > Map.Height - 1)
                            continue;
                        if (IsNodeWalkable(y, x))
                            //return PathNodes[y, x];
                            walkableNodes.Add(PathNodes[y, x]);
                    }
                }
                if (walkableNodes.Count > 0)
                    break;
            }

            // return discovered path node that is closest to unit
            float shortestDistanceToUnit = float.MaxValue;
            int shortestIndex = 0;
            for (int i = 0; i < walkableNodes.Count; i++)
            {
                float distance = Vector2.Distance(walkableNodes[i].Tile.CenterPoint, o.CenterPoint);
                if (distance < shortestDistanceToUnit)
                {
                    shortestDistanceToUnit = distance;
                    shortestIndex = i;
                }
            }
            return walkableNodes[shortestIndex];
        }
        // uses starting point (for rally points)
        /*public PathNode FindNearestPathNode(int startY, int startX, Vector2 startingPoint)
        {
            startX = (int)MathHelper.Clamp(startX, 0, Map.Width - 1);
            startY = (int)MathHelper.Clamp(startY, 0, Map.Height - 1);

            if (IsNodeWalkable(startY, startX, unit))
                return PathNodes[startY, startX];

            List<PathNode> walkableNodes = new List<PathNode>();
            for (int i = 1; ; i++)
            {
                for (int x = startX - i; x <= startX + i; x += i*2)
                {
                    if (x < 0 || x > Map.Width - 1)
                        continue;
                    for (int y = startY - i; y <= startY + i; y += i*2)
                    {
                        if (y < 0 || y > Map.Height - 1)
                            continue;
                        if (IsNodeWalkable(y, x, unit))
                            //return PathNodes[y, x];
                            walkableNodes.Add(PathNodes[y, x]);
                    }
                }
                if (walkableNodes.Count > 0)
                    break;
            }

            // return discovered path node that is closest to unit
            float shortestDistanceToUnit = float.MaxValue;
            int shortestIndex = 0;
            for (int i = 0; i < walkableNodes.Count; i++)
            {
                float distance = Vector2.Distance(walkableNodes[i].Tile.CenterPoint, unit.CenterPoint);
                if (distance < shortestDistanceToUnit)
                {
                    shortestDistanceToUnit = distance;
                    shortestIndex = i;
                }
            }*/

        public bool IsStructureInLineOfSight(Unit unit, Structure structure)
        {
            float lerpAmountIncrement = 1f / ((int)(Vector2.Distance(unit.CenterPoint, structure.CenterPoint) / unit.Diameter) + 1);

            for (float l = lerpAmountIncrement; l < 1f; l += lerpAmountIncrement)
            //for (float l = 1f - lerpAmountIncrement; l > 0; l -= lerpAmountIncrement)
            {
                Vector2 intermediatePoint = Vector2.Lerp(unit.CenterPoint, structure.CenterPoint, l);

                //if (!IsPointWalkable(intermediatePoint, unit))
                //    return false;

                int y = (int)(intermediatePoint.Y / Map.TileSize);
                int x = (int)(intermediatePoint.X / Map.TileSize);

                //return IsTileWalkable((int)(point.Y / Map.TileSize), (int)(point.X / Map.TileSize), unit);
                if (y < 0 || y >= Map.Height || x < 0 || x >= Map.Width)// || !IsNodeWalkable(PathNodes[y, x], unit))
                    return false;

                if (!PathNodes[y, x].Tile.Walkable || (PathNodes[y, x].Blocked && PathNodes[y, x].Blocker != structure))
                    return false;
            }

            return true;
        }

        public bool Walkable(Vector2 point1, Vector2 point2, Unit unit, int numberOfIntermediatePoints)
        {
            //float radius = unit.Radius;
            float lerpAmountIncrement = 1f / (numberOfIntermediatePoints + 1);

            for (float l = lerpAmountIncrement; l < 1f; l += lerpAmountIncrement)
            //for (float l = 1f - lerpAmountIncrement; l > 0; l -= lerpAmountIncrement)
            {
                Vector2 intermediatePoint = Vector2.Lerp(point1, point2, l);

                if (!IsPointWalkable(intermediatePoint, unit))
                    return false;
            }

            return true;
        }
        public bool Walkable(Vector2 point1, Vector2 point2, int numberOfIntermediatePoints)
        {
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
        public bool IsTileVisible(Vector2 point1, Vector2 point2, RtsObject o, int numberOfIntermediatePoints)
        {
            float lerpAmountIncrement = 1f / (numberOfIntermediatePoints + 1);

            for (float l = lerpAmountIncrement; l < 1f; l += lerpAmountIncrement)
            //for (float l = 1f - lerpAmountIncrement; l > 0; l -= lerpAmountIncrement)
            {
                Vector2 intermediatePoint = Vector2.Lerp(point1, point2, l);

                if (!IsPointVisible(intermediatePoint, o))
                    return false;
            }

            return true;
        }

        public bool IsPointVisible(Vector2 point, RtsObject o)
        {
            int y = (int)(point.Y / Map.TileSize);
            int x = (int)(point.X / Map.TileSize);

            if ((y < 0 || y >= Map.Height || x < 0 || x >= Map.Width))
                return false;

            PathNode pathNode = PathNodes[y, x];
            if (o.OccupiedPathNodes.Contains(pathNode))
                return true;
            else
                return (pathNode.Tile.Walkable && !pathNode.Blocked);
        }

        public bool IsPointWalkable(Vector2 point)
        {
            int y = (int)(point.Y / Map.TileSize);
            int x = (int)(point.X / Map.TileSize);

            //return (y >= 0 && y < Map.Height && x >= 0 && x < Map.Width && PathNodes[y, x].Tile.Walkable);
            return (!(y < 0 || y >= Map.Height || x < 0 || x >= Map.Width) && PathNodes[y, x].Tile.Walkable && !PathNodes[y, x].Blocked);
        }
        public bool IsPointWalkable(Vector2 point, Unit unit)
        {
            /*float radius = unit.Radius;

            if (!IsPointWalkable(new Vector2(point.X - radius, point.Y - radius)))
                return false;
            if (!IsPointWalkable(new Vector2(point.X + radius, point.Y + radius)))
                return false;
            if (!IsPointWalkable(new Vector2(point.X + radius, point.Y - radius)))
                return false;
            if (!IsPointWalkable(new Vector2(point.X - radius, point.Y + radius)))
                return false;

            return true;*/

            int y = (int)(point.Y / Map.TileSize);
            int x = (int)(point.X / Map.TileSize);

            //return IsTileWalkable((int)(point.Y / Map.TileSize), (int)(point.X / Map.TileSize), unit);
            return (!(y < 0 || y >= Map.Height || x < 0 || x >= Map.Width) && IsNodeWalkable(PathNodes[y, x], unit));
        }

        public bool IsNodeWalkable(int tileY, int tileX)
        {
            PathNode pathNode = PathNodes[tileY, tileX];

            return !(!pathNode.Tile.Walkable || pathNode.Blocked);
        }
        // for path smoothing
        public bool IsNodeWalkable(PathNode pathNode, Unit unit)
        {
            if (!pathNode.Tile.Walkable || pathNode.Blocked)
                return false;

            Vector2 unitPosition = pathNode.Tile.CenterPoint - new Vector2(unit.Radius, unit.Radius);

            int posX = (int)MathHelper.Clamp(unitPosition.X / Map.TileSize, 0, Map.Width - 1);
            int posY = (int)MathHelper.Clamp(unitPosition.Y / Map.TileSize, 0, Map.Height - 1);
            int rightBoundX = (int)MathHelper.Min(posX + (int)Math.Ceiling(unit.Width / (float)Map.TileSize), Map.Width - 1);
            int bottomBoundY = (int)MathHelper.Min(posY + (int)Math.Ceiling(unit.Height / (float)Map.TileSize), Map.Height - 1);

            for (int x = posX; x < rightBoundX; x++)
            {
                for (int y = posY; y < bottomBoundY; y++)
                {
                    if (!Map.Tiles[y, x].Walkable || PathNodes[y, x].Blocked)
                        return false;
                }
            }

            /*int howManyTilesToExpand = (int)(unit.Radius / (Map.TileSize + .01f));

            int lowY = (int)MathHelper.Max(tileY - howManyTilesToExpand, 0);
            int lowX = (int)MathHelper.Max(tileX - howManyTilesToExpand, 0);
            int highY = (int)MathHelper.Min(tileY + howManyTilesToExpand, Map.Height);
            int highX = (int)MathHelper.Min(tileX + howManyTilesToExpand, Map.Width);

            for (int y = lowY; y <= highY; y++)
            {
                for (int x = lowX; x <= highX; x++)
                {
                    if (!Map.Tiles[y, x].Walkable || PathNodes[y, x].Blocked)
                        return false;
                }
            }*/

            return true;
        }
        // for mouse position when giving commands
        public bool IsNodeWalkable(int tileY, int tileX, Unit unit)
        {
            if (!Map.Tiles[tileY, tileX].Walkable || PathNodes[tileY, tileX].Blocked)
                return false;

            Vector2 unitPosition = Map.Tiles[tileY, tileX].CenterPoint - new Vector2(unit.Radius, unit.Radius);

            int posX = (int)MathHelper.Clamp(unitPosition.X / Map.TileSize, 0, Map.Width - 1);
            int posY = (int)MathHelper.Clamp(unitPosition.Y / Map.TileSize, 0, Map.Height - 1);
            int rightBoundX = (int)MathHelper.Clamp(posX + (int)Math.Ceiling(unit.Width / (float)Map.TileSize), 0, Map.Width - 1) - 1;
            int bottomBoundY = (int)MathHelper.Clamp(posY + (int)Math.Ceiling(unit.Height / (float)Map.TileSize), 0, Map.Height - 1) - 1;

            for (int x = posX; x <= rightBoundX; x++)
            {
                for (int y = posY; y <= bottomBoundY; y++)
                {
                    if (!Map.Tiles[y, x].Walkable || PathNodes[y, x].Blocked)
                        return false;
                }
            }

            /*int howManyTilesToExpand = (int)(unit.Radius / (Map.TileSize + .01f));

            int lowY = (int)MathHelper.Max(tileY - howManyTilesToExpand, 0);
            int lowX = (int)MathHelper.Max(tileX - howManyTilesToExpand, 0);
            int highY = (int)MathHelper.Min(tileY + howManyTilesToExpand, Map.Height);
            int highX = (int)MathHelper.Min(tileX + howManyTilesToExpand, Map.Width);

            for (int y = lowY; y <= highY; y++)
            {
                for (int x = lowX; x <= highX; x++)
                {
                    if (!Map.Tiles[y, x].Walkable || PathNodes[y, x].Blocked)
                        return false;
                }
            }*/

            return true;
        }

        public bool WillStructureFit(Point location, int size, bool cutCorners)
        {
            List<PathNode> placingStructurePathNodes = new List<PathNode>();
            Circle collisionCircle = new Circle(new Vector2(location.X * Map.TileSize + (size / 2) * Map.TileSize, location.Y * Map.TileSize + (size / 2) * Map.TileSize), size * Map.TileSize);
            //placingStructureCenterPoint = collisionCircle.CenterPoint;

            for (int x = location.X; x < location.X + size; x++)
            {
                for (int y = location.Y; y < location.Y + size; y++)
                {
                    PathNode node = Rts.pathFinder.PathNodes[(int)MathHelper.Clamp(y, 0, Map.Height - 1), (int)MathHelper.Clamp(x, 0, Map.Width - 1)];
                    if (collisionCircle.Intersects(node.Tile))
                    {
                        placingStructurePathNodes.Add(node);
                    }
                }
            }
            // remove corners
            if (cutCorners)
            {
                placingStructurePathNodes.Remove(Rts.pathFinder.PathNodes[location.Y, location.X]);
                if (location.X + size <= Map.Width - 1)
                    placingStructurePathNodes.Remove(Rts.pathFinder.PathNodes[location.Y, location.X + size - 1]);
                else
                    return false;
                if (location.Y + size <= Map.Height - 1)
                    placingStructurePathNodes.Remove(Rts.pathFinder.PathNodes[location.Y + size - 1, location.X]);
                else
                    return false;
                placingStructurePathNodes.Remove(Rts.pathFinder.PathNodes[location.Y + size - 1, location.X + size - 1]);
            }

            foreach (PathNode node in placingStructurePathNodes)
            {
                if (!node.Tile.Walkable || (node.Blocked && node.Blocker is Structure && ((Structure)node.Blocker).Visible) || (node.Blocked && node.Blocker is Resource))
                    return false;
            }

            return true;
        }
        public bool CanStructureBePlaced(Point location, int size, Unit builder, bool cutCorners)
        {
            List<PathNode> placingStructurePathNodes = new List<PathNode>();
            Circle collisionCircle = new Circle(new Vector2(location.X * Map.TileSize + (size / 2) * Map.TileSize, location.Y * Map.TileSize + (size / 2) * Map.TileSize), size * Map.TileSize);
            //placingStructureCenterPoint = collisionCircle.CenterPoint;

            for (int x = location.X; x < location.X + size; x++)
            {
                for (int y = location.Y; y < location.Y + size; y++)
                {
                    PathNode node = Rts.pathFinder.PathNodes[(int)MathHelper.Clamp(y, 0, Map.Height - 1), (int)MathHelper.Clamp(x, 0, Map.Width - 1)];
                    if (collisionCircle.Intersects(node.Tile))
                    {
                        placingStructurePathNodes.Add(node);
                    }
                }
            }
            // remove corners
            if (cutCorners)
            {
                placingStructurePathNodes.Remove(Rts.pathFinder.PathNodes[location.Y, location.X]);
                if (location.X + size <= Map.Width - 1)
                    placingStructurePathNodes.Remove(Rts.pathFinder.PathNodes[location.Y, location.X + size - 1]);
                else
                    return false;
                if (location.Y + size <= Map.Height - 1)
                    placingStructurePathNodes.Remove(Rts.pathFinder.PathNodes[location.Y + size - 1, location.X]);
                else
                    return false;
                placingStructurePathNodes.Remove(Rts.pathFinder.PathNodes[location.Y + size - 1, location.X + size - 1]);
            }

            foreach (PathNode node in placingStructurePathNodes)
            {
                if (!node.Tile.Walkable || node.Blocked)
                    return false;
                if (node.UnitsContained.Count > 0)
                {
                    if (node.UnitsContained.Count == 1 && node.UnitsContained.ElementAt<Unit>(0) == builder)
                        continue;
                    else
                    {
                        bool allow = true;

                        foreach (Unit unit in node.UnitsContained)
                        {
                            if (unit.Team != builder.Team)
                            {
                                allow = false;
                                break;
                            }
                        }
                        if (!allow)
                            return false;
                    }
                }
            }

            return true;
        }

        bool isPathWalkable(List<Vector2> path, Unit unit)
        {
            return Walkable(path[0], path[path.Count - 1], unit, path.Count * 4);
        }
    }
}
