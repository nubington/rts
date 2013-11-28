using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;

namespace rts
{
    public class VisionUpdater
    {
        public readonly Thread Thread;
        public Map Map;
        PathFinder PathFinder;
        int Team;
        Color[] Fog;
        Color FogColor;
        public Texture2D FogTexture;

        const int DELAY = 90;
        
        List<ObjectLink<RtsObject, List<MapTile>>> visionLists = new List<ObjectLink<RtsObject, List<MapTile>>>();
        public static List<ObjectLink<RtsObject, List<MapTile>>> PublicVisionLists = new List<ObjectLink<RtsObject, List<MapTile>>>();// = null;
        public static Object PublicLock = new Object();

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


                // update VisibleTiles of all units
                RtsObject[] objects;
                lock (RtsObject.RtsObjects)
                {
                    objects = RtsObject.RtsObjects.ToArray<RtsObject>();
                }
                //lock (RtsObject.RtsObjects)
                //{
                    //foreach (RtsObject unit in RtsObject.RtsObjects)
                foreach (RtsObject unit in objects)
                    {
                        if (!unit.HasMoved)
                            continue;

                            //lock (unit.VisibleTiles)
                            //{
                                //unit.VisibleTiles.Clear();
                                List<MapTile> visibleTiles = new List<MapTile>();

                                bool myTeam = (unit.Team == Team);

                                int posX = (int)MathHelper.Clamp(unit.CenterPointX / Map.TileSize - unit.SightRange, 0, Map.Width - 1);
                                int posY = (int)MathHelper.Clamp(unit.CenterPointY / Map.TileSize - unit.SightRange, 0, Map.Height - 1);
                                int rightBoundX = (int)MathHelper.Clamp(posX + (int)Math.Ceiling((double)(unit.SightRange * 2)), 0, Map.Width - 1);
                                int bottomBoundY = (int)MathHelper.Clamp(posY + (int)Math.Ceiling((double)(unit.SightRange * 2)), 0, Map.Height - 1);

                                for (int x = posX; x <= rightBoundX; x++)
                                {
                                    for (int y = posY; y <= bottomBoundY; y++)
                                    {
                                        // add tile to visibility array if valid, walkable, and within vision range
                                        //if (!(x < 0 || x >= Map.Width || y < 0 || y >= Map.Height))
                                        {
                                            if (Vector2.Distance(unit.CenterPoint, Map.Tiles[y, x].CenterPoint) > unit.SightRange * Map.TileSize)
                                                continue;
                                            //if (!PathFinder.Walkable(unit.CenterPoint, Map.Tiles[y, x].CenterPoint, (int)(Vector2.Distance(unit.CenterPoint, Map.Tiles[y, x].CenterPoint) / Map.TileSize)))
                                            //    continue;
                                            if (!PathFinder.Tools.IsTileVisible(unit.CenterPoint, Map.Tiles[y, x].CenterPoint, unit, (int)(Vector2.Distance(unit.CenterPoint, Map.Tiles[y, x].CenterPoint) / Map.TileSize)))
                                                continue;

                                            if (myTeam)
                                                tiles[y, x] = true;
                                            //unit.VisibleTiles.Add(Map.Tiles[y, x]);
                                            visibleTiles.Add(Map.Tiles[y, x]);
                                        }
                                    }
                                }
                            //}

                            //visionLists.Add(new ObjectLink<RtsObject, List<MapTile>>(unit, visibleTiles));
                                lock (unit.VisibleTiles)
                                {
                                    //foreach (MapTile tile in unit.VisibleTiles)
                                     //   tile.Visible = false;

                                    unit.VisibleTiles = visibleTiles;

                                   // foreach (MapTile tile in unit.VisibleTiles)
                                    //    tile.Visible = true;

                                    unit.HasMoved = false;
                                }
                    //}
                }



                // apply visibility to map
                /*for (int y = 0; y < Map.Height; y++)
                {
                    for (int x = 0; x < Map.Width; x++)
                    {
                        Map.Tiles[y, x].Visible = tiles[y, x];
                    }
                }*/
                /*foreach (MapTile tile in Map.Tiles)
                    tile.Visible = false;

                foreach (RtsObject o in objects)
                {
                    foreach (MapTile tile in o.VisibleTiles)
                        tile.Visible = true;
                }*/

                /*lock (PublicLock)
                {
                    PublicVisionLists = new List<ObjectLink<RtsObject, List<MapTile>>>(visionLists);
                    visionListsFulfilled = false;
                }*/

                /*foreach (ObjectLink<RtsObject, List<MapTile>> objectLink in visionLists)
                {
                    lock (objectLink.Object1.VisibleTiles)
                    {
                        objectLink.Object1.VisibleTiles.Clear();

                        foreach (MapTile tile in objectLink.Object2)
                        {
                            objectLink.Object1.VisibleTiles.Add(tile);
                        }
                    }
                }*/
            }
        }

        static bool visionListsFulfilled = false;
        public static void FulFillVisionLists()
        {
            lock (PublicLock)
            {
                if (visionListsFulfilled)
                    return;

                foreach (ObjectLink<RtsObject, List<MapTile>> objectLink in PublicVisionLists)
                {
                    objectLink.Object1.VisibleTiles.Clear();

                    foreach (MapTile tile in objectLink.Object2)
                    {
                        objectLink.Object1.VisibleTiles.Add(tile);
                    }
                }

                visionListsFulfilled = true;
            }
        }
    }
}