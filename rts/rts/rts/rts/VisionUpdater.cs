using System;
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
using System.Threading;

namespace rts
{
    class VisionUpdater
    {
        public readonly Thread Thread;
        public Map Map;
        PathFinder PathFinder;
        int Team;

        const int DELAY = 50;

        public VisionUpdater(Map map, PathFinder pathFinder, int team)
        {
            Map = map;
            PathFinder = pathFinder;
            Team = team;

            Thread = new Thread(new ThreadStart(UpdateVision));
            Thread.IsBackground = true;
            Thread.Start();
        }

        void UpdateVision()
        {
            while (true)
            {
                Thread.Sleep(DELAY);

                // visibility array
                bool[,] tiles = new bool[Map.Height, Map.Width];

                lock (Unit.Units)
                {
                    foreach (Unit unit in Unit.Units)
                    {
                        if (unit.Team == Team)
                        {
                            lock (unit.VisibleTiles)
                            {
                                unit.VisibleTiles.Clear();

                                for (int y = unit.CurrentPathNode.Tile.Y - unit.SightRange; y <= unit.CurrentPathNode.Tile.Y + unit.SightRange; y++)
                                {
                                    for (int x = unit.CurrentPathNode.Tile.X - unit.SightRange; x <= unit.CurrentPathNode.Tile.X + unit.SightRange; x++)
                                    {
                                        //if a corner
                                        /*if (y == unit.CurrentPathNode.Tile.Y - unit.SightRange && (x == unit.CurrentPathNode.Tile.X - unit.SightRange || x == unit.CurrentPathNode.Tile.X + unit.SightRange))
                                        {
                                            continue;
                                        }
                                        else if (y == unit.CurrentPathNode.Tile.Y + unit.SightRange && (x == unit.CurrentPathNode.Tile.X - unit.SightRange || x == unit.CurrentPathNode.Tile.X + unit.SightRange))
                                        {
                                            continue;
                                        }*/

                                        // add tile to visibility array if valid, walkable, and within vision range
                                        if (!(x < 0 || x >= Map.Width || y < 0 || y >= Map.Height) && PathFinder.Walkable(unit.CenterPoint, Map.Tiles[y, x].CenterPoint, unit, (int)(Vector2.Distance(unit.CenterPoint, Map.Tiles[y, x].CenterPoint) / Map.TileSize)))
                                        {
                                            if (Vector2.Distance(unit.CenterPoint, Map.Tiles[y, x].CenterPoint) > unit.SightRange * Map.TileSize)
                                                continue;

                                            tiles[y, x] = true;
                                            unit.VisibleTiles.Add(Map.Tiles[y, x]);
                                        }
                                    }
                                }

                                /*int y = unit.CurrentPathNode.Tile.Y;
                                int x = unit.CurrentPathNode.Tile.X;
                                tiles[y, x] = true;
                                unit.VisibleTiles.Add(Map.Tiles[y, x]);

                                for (int i = 0; i < unit.SightRange; i++)
                                {
                                    y = unit.CurrentPathNode.Tile.Y - i;
                                    x = unit.CurrentPathNode.Tile.X;
                                    if (!(x < 0 || x >= Map.Width || y < 0 || y >= Map.Height) && PathFinder.Walkable(unit.CenterPoint, Map.Tiles[y, x].CenterPoint, unit, i))
                                    {
                                        tiles[y, x] = true;
                                        unit.VisibleTiles.Add(Map.Tiles[y, x]);
                                    }

                                    y = unit.CurrentPathNode.Tile.Y + i;
                                    x = unit.CurrentPathNode.Tile.X;
                                    if (!(x < 0 || x >= Map.Width || y < 0 || y >= Map.Height) && PathFinder.Walkable(unit.CenterPoint, Map.Tiles[y, x].CenterPoint, unit, i))
                                    {
                                        tiles[y, x] = true;
                                        unit.VisibleTiles.Add(Map.Tiles[y, x]);
                                    }

                                    y = unit.CurrentPathNode.Tile.Y;
                                    x = unit.CurrentPathNode.Tile.X - i;
                                    if (!(x < 0 || x >= Map.Width || y < 0 || y >= Map.Height) && PathFinder.Walkable(unit.CenterPoint, Map.Tiles[y, x].CenterPoint, unit, i))
                                    {
                                        tiles[y, x] = true;
                                        unit.VisibleTiles.Add(Map.Tiles[y, x]);
                                    }

                                    y = unit.CurrentPathNode.Tile.Y;
                                    x = unit.CurrentPathNode.Tile.X + i;
                                    if (!(x < 0 || x >= Map.Width || y < 0 || y >= Map.Height) && PathFinder.Walkable(unit.CenterPoint, Map.Tiles[y, x].CenterPoint, unit, i))
                                    {
                                        tiles[y, x] = true;
                                        unit.VisibleTiles.Add(Map.Tiles[y, x]);
                                    }
                                }*/
                            }
                        }
                    }
                }

                // apply visibility to map
                for (int y = 0; y < Map.Height; y++)
                {
                    for (int x = 0; x < Map.Width; x++)
                    {
                        Map.Tiles[y, x].Visible = tiles[y, x];
                    }
                }
            }
        }
    }
}