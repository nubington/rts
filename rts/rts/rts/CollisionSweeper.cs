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
    public class PotentialCollisionSweeper
    {
        public bool DoSpread;
        public int SpreadDivisor = 1;
        int persistentIndex = 0;
        List<BaseObject> objects = new List<BaseObject>();
        List<BaseObject[]> pairs = new List<BaseObject[]>();

        public PotentialCollisionSweeper()
        { }
        public PotentialCollisionSweeper(int spreadDivisor)
        {
            SpreadDivisor = spreadDivisor;
            DoSpread = true;
        }

        // one list of objects that collide with each other
        public void UpdatePotentialCollisions<T>(List<T> objects) where T : BaseObject
        {
            if (DoSpread)
                UpdatePotentialCollisionsDivided(objects);
            else
                UpdatePotentialCollisionsAggregate(objects);
        }

        public void UpdatePotentialCollisionsAggregate<T>(List<T> objects) where T : BaseObject
        {
            if (objects.Count == 0)
                return;

            Util.SortByX(objects);

            List<int[]> pairs = new List<int[]>();

            for (int i = 0; i < objects.Count; i++)
            {
                T object1 = objects[i];
                object1.PotentialCollisions.Clear();

                for (int s = i + 1; s < objects.Count; s++)
                {
                    T object2 = objects[s];

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
                objects[pair[0]].PotentialCollisions.Add(objects[pair[1]]);
                objects[pair[1]].PotentialCollisions.Add(objects[pair[0]]);
            }
        }

        // one list of objects that collide with each other
        // divide calculation over multiple frames
        public void UpdatePotentialCollisionsDivided<T>(List<T> list) where T : BaseObject
        {
            if (persistentIndex == 0)
            {
                Util.SortByX(list);
                objects = new List<BaseObject>(list);
                pairs.Clear();
            }

            if (objects.Count == 0)
                return;

            int numberToDo;
            if (objects.Count <= SpreadDivisor)
                numberToDo = objects.Count;
            else
                numberToDo = (int)(objects.Count / (float)SpreadDivisor + .5f);

            int count = 0;

            for (; persistentIndex < objects.Count; )// persistentIndex++)
            {
                BaseObject object1 = objects[persistentIndex];

                for (int s = persistentIndex + 1; s < objects.Count; s++)
                {
                    BaseObject object2 = objects[s];

                    if (object2.RightBound < object1.LeftBound)
                        continue;

                    if (object2.LeftBound > object1.RightBound)
                        break;

                    if (object2.TopBound <= object1.BottomBound &&
                        object2.BottomBound >= object1.TopBound)
                        pairs.Add(new BaseObject[2] { objects[persistentIndex], objects[s] });
                }

                persistentIndex++;

                if (++count >= numberToDo && persistentIndex + numberToDo < objects.Count)
                    return;
            }

            persistentIndex = 0;

            foreach (BaseObject o in objects)
                o.PotentialCollisions.Clear();

            foreach (BaseObject[] pair in pairs)
            {
                if (Vector2.Distance(pair[0].CenterPoint, pair[1].CenterPoint) < (pair[0].GreaterOfWidthAndHeight + pair[1].GreaterOfWidthAndHeight) * .8f)
                {
                    pair[0].PotentialCollisions.Add(pair[1]);
                    pair[1].PotentialCollisions.Add(pair[0]);
                }
            }
        }

        // one object and a list of objects that it can collide with
        public void UpdatePotentialCollisionsAggregate<T>(T object1, List<T> objects, bool doSort) where T : BaseObject
        {
            if (objects.Count == 0)
                return;

            object1.PotentialCollisions.Clear();

            if (doSort)
                Util.SortByX(objects);

            for (int i = 0; i < objects.Count; i++)
            {
                T object2 = objects[i];

                if (object2.RightBound < object1.LeftBound)
                    continue;

                if (object2.LeftBound > object1.RightBound)
                    break;

                if (object2.TopBound <= object1.BottomBound &&
                    object2.BottomBound >= object1.TopBound)
                    object1.PotentialCollisions.Add(object2);
            }
        }

        List<BaseObject> potentials = new List<BaseObject>();

        // one object and a list of objects that it can collide with
        // divide calculation over multiple frames
        public void UpdatePotentialCollisionsDivided<T>(T object1, List<T> list) where T : BaseObject
        {
            if (persistentIndex == 0)
            {
                Util.SortByX(list);
                objects = new List<BaseObject>(list);
                potentials.Clear();
            }

            if (objects.Count == 0)
                return;

            int numberToDo = (int)(objects.Count / (float)SpreadDivisor + .5f);
            int count = 0;

            for (; persistentIndex < objects.Count; persistentIndex++)
            {
                BaseObject object2 = objects[persistentIndex];

                if (object2.RightBound < object1.LeftBound)
                    continue;

                if (object2.LeftBound > object1.RightBound)
                    break;

                if (object2.TopBound <= object1.BottomBound &&
                    object2.BottomBound >= object1.TopBound)
                    potentials.Add(object2);

                if (++count >= numberToDo && persistentIndex + numberToDo < objects.Count)
                    return;
            }

            persistentIndex = 0;

            object1.PotentialCollisions.Clear();

            foreach (BaseObject o in potentials)
                object1.PotentialCollisions.Add(o);
        }

        // two lists of objects that can collide with each other
        public void UpdatePotentialCollisions<T>(List<T> objects1, List<T> objects2) where T : BaseObject
        {
            if (objects1.Count == 0 || objects2.Count == 0)
                return;

            Util.SortByX(objects1);
            Util.SortByX(objects2);

            List<int[]> pairs = new List<int[]>();

            for (int i = 0; i < objects1.Count; i++)
            {
                T object1 = objects1[i];
                object1.PotentialCollisions.Clear();

                for (int s = 0; s < objects2.Count; s++)
                {
                    T object2 = objects2[s];

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
                objects1[pair[0]].PotentialCollisions.Add(objects2[pair[1]]);
                objects2[pair[1]].PotentialCollisions.Add(objects1[pair[0]]);
            }
        }
    }

    public class UnitCollisionSweeper
    {
        //int delay = 25;//, timeSinceLastSweep = 50;
        //List<Unit> units;
        public readonly Thread Thread;
        List<Unit> Units = new List<Unit>();

        public Unit[] PublicUnits;
        public List<Unit>[] PublicCollisionLists;
        public Object PublicLock = new Object();
        //public Array PublicPairs;

        public UnitCollisionSweeper()
        {
            //units = Unit.UnitsSorted;

            Thread = new Thread(new ThreadStart(UpdatePotentialCollisions));
            Thread.IsBackground = true;
            //Thread.Priority = ThreadPriority.BelowNormal;
            Thread.Start();
        }

        void UpdatePotentialCollisions()
        {
            while (true)
            {
                //Thread.Sleep(1);
                Thread.Sleep(25);

                if (Unit.UnitsSorted.Count == 0)
                    continue;

                //List<Unit> units;
                lock (Unit.UnitsSorted)
                {
                    Units = new List<Unit>(Unit.UnitsSorted.ToArray<Unit>());
                }

                //lock (units)
                //{
                    SortByX(Units);

                    List<int[]> pairs = new List<int[]>();
                    //List<Unit[]> pairs = new List<Unit[]>();

                    for (int i = 0; i < Units.Count; i++)
                    {
                        Unit object1 = Units[i];
                        if (object1.IgnoringCollision)
                            continue;
                        //object1.ClearPotentialCollisions();

                        for (int s = i + 1; s < Units.Count; s++)
                        {
                            Unit object2 = Units[s];
                            if (object2.IgnoringCollision)
                                continue;

                            if (object2.RightBound < object1.LeftBound)
                                continue;

                            if (object2.LeftBound > object1.RightBound)
                                break;

                            if (object2.TopBound <= object1.BottomBound &&
                                object2.BottomBound >= object1.TopBound)
                                pairs.Add(new int[2] { i, s });
                            //pairs.Add(new Unit[2] { object1, object2 });
                        }
                    }

                    List<List<Unit>> collisionLists = new List<List<Unit>>();

                    //lock (Unit.UnitsSorted)
                    //{
                    for (int i = 0; i < Units.Count; i++)
                        collisionLists.Add(new List<Unit>());

                    foreach (int[] pair in pairs)
                    {
                        collisionLists[pair[0]].Add(Units[pair[1]]);
                        collisionLists[pair[1]].Add(Units[pair[0]]);
                    }

                    lock (PublicLock)
                    {
                        PublicUnits = Units.ToArray<Unit>();
                        PublicCollisionLists = collisionLists.ToArray<List<Unit>>();
                    }

                    /*for (int i = 0; i < Units.Count; i++)
                    {
                        Unit unit = Units[i];
                        lock (unit.PotentialCollisions)
                        {
                            unit.ClearPotentialCollisions();

                            foreach (Unit u in collisionLists[i])
                            {
                                unit.AddPotentialCollision(u);
                            }
                        }
                    }*/


                    //}
                //}

                /*lock (BaseObject.PotentialCollisionsLock)
                {
                    lock (Unit.UnitsSortedLock)
                    {
                        foreach (Unit u in units)
                            u.ClearPotentialCollisions();
                    }

                    /*foreach (Unit[] pair in pairs)
                    {
                        pair[0].AddPotentialCollision(pair[1]);
                        pair[1].AddPotentialCollision(pair[0]);
                    }*/
                //}
                /*foreach (int[] pair in pairs)
                {
                    units[pair[0]].AddPotentialCollision(units[pair[1]]);
                    units[pair[1]].AddPotentialCollision(units[pair[0]]);
                }*/
            }
        }

        public void FulFillCollisionLists()
        {
            if (PublicUnits == null)
                return;

            lock (PublicLock)
            {
                for (int i = 0; i < PublicUnits.Length; i++)
                {
                    Unit unit = PublicUnits[i];
                    //lock (unit.PotentialCollisions)
                    //{
                        unit.ClearPotentialCollisions();

                        foreach (Unit u in PublicCollisionLists[i])
                        {
                            unit.AddPotentialCollision(u);
                        }
                   // }
                }
                PublicUnits = null;
            }
        }

        static void SortByX(List<Unit> list)
        {
            //lock (Unit.UnitsSorted)
            //{
                for (int i = 1; i < list.Count; i++)
                {
                    for (int j = i; j > 1 && list[j].X < list[j - 1].X; j--)
                    {
                        Unit tempItem = list[j];
                        list.RemoveAt(j);
                        list.Insert(j - 1, tempItem);
                    }
                }
            //}
        }
    }
}