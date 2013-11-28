using System.Collections.Generic;
using System.Linq;

namespace rts
{
    public class Selection
    {
        List<RtsObject> objects;
        RtsObjectType mostPopulousType;
        int largestCount = 0;
        RtsObjectType activeType;
        bool activeTypeRallyable;
        public CommandCard ActiveCommandCard;

        Dictionary<RtsObjectType, int> typeCounts = new Dictionary<RtsObjectType, int>();

        public Selection()
        {
            objects = new List<RtsObject>();
            mostPopulousType = null;
            activeType = null;
            ActiveCommandCard = null;
        }
        public Selection(List<RtsObject> list)
        {
            objects = new List<RtsObject>(list.ToArray<RtsObject>());
            sortSelection();

            // add object/count pairs to the objectCount dictionary
            foreach (RtsObject o in objects)
            {
                RtsObjectType type = o.Type;
                if (!typeCounts.ContainsKey(type))
                {
                    typeCounts.Add(type, 1);
                }
                else
                {
                    //int count;
                    //typeCounts.TryGetValue(o.GetType(), out count);
                    //count++;
                    typeCounts[type]++;
                }
            }

            // mostPopulousType = largest count
            foreach (System.Collections.Generic.KeyValuePair<RtsObjectType, int> pair in typeCounts)
            {
                if (pair.Value > largestCount)
                {
                    mostPopulousType = pair.Key;
                    largestCount = pair.Value;
                }
            }

            SetActiveTypeToMostPopulousType();
        }
        /*public Selection(Selection s)
            : this(s.objects)
        {
        }*/

        public void Add(RtsObject o)
        {
            //if (!objects.Contains(o))
                objects.Add(o);
            sortSelection();

            RtsObjectType type = o.Type;

            if (objects.Count == 1)
                ActiveType = type;

            int count;

            if (!typeCounts.ContainsKey(type))
            {
                typeCounts.Add(type, 1);
                count = 1;
            }
            else
            {
                typeCounts[type]++;
                typeCounts.TryGetValue(type, out count);
            }

            if (count > largestCount)
            {
                mostPopulousType = type;
                largestCount = count;
            }
        }
        public void Remove(RtsObject o)
        {
            if (!objects.Contains(o))
                return;

            objects.Remove(o);

            Rts.selectedUnitsChanged = true;

            RtsObjectType type = o.Type;

            if (typeCounts.ContainsKey(type))
            {
                typeCounts[type]--;
                if (typeCounts[type] == 0)
                {
                    typeCounts.Remove(type);
                    if (activeType == type)
                        TabActiveType();
                }
            }

            if (mostPopulousType == type)
            {
                largestCount = 0;
                foreach (KeyValuePair<RtsObjectType, int> pair in typeCounts)
                {
                    if (pair.Value > largestCount)
                    {
                        mostPopulousType = pair.Key;
                        largestCount = pair.Value;
                    }
                }
            }

            if (objects.Count == 0)
            {
                mostPopulousType = null;
                activeType = null;
                largestCount = 0;
            }

            //if (!ContainsType(o))
            //    TabActiveType();
        }
        public void Clear()
        {
            objects.Clear();
            typeCounts.Clear();
            mostPopulousType = null;
            activeType = null;
            largestCount = 0;
        }

        public void TabActiveType()
        {
            List<RtsObjectType> keys = typeCounts.Keys.ToList<RtsObjectType>();

            if (typeCounts.Count > 1)
            {
                RtsObjectType nextPriorityKey = null;
                int lowest = int.MaxValue;

                foreach (RtsObjectType key in keys)
                {
                    if (key.SelectionSortValue > activeType.SelectionSortValue)
                    {
                        if (key.SelectionSortValue < lowest)
                        {
                            nextPriorityKey = key;
                            lowest = key.SelectionSortValue;
                        }
                    }
                }

                if (nextPriorityKey == null)
                    nextPriorityKey = lowestPriorityType(keys);

                ActiveType = nextPriorityKey;
            }

            /*if (typeCounts.Count > 1)
            {
                for (int i = 0; i < keys.Count; i++)
                {
                    if (keys[i] == activeType)
                    {
                        //ActiveType = keys[(i + (keys.Count - 1)) % keys.Count];
                        ActiveType = keys[(i + 1) % keys.Count - 1];
                        break;
                    }
                }
            }*/
            else if (typeCounts.Count == 1)
                ActiveType = keys[0];
            else
                ActiveType = null;
        }
        public void SetActiveTypeToMostPopulousType()
        {
            ActiveType = mostPopulousType;
        }
        RtsObjectType lowestPriorityType(List<RtsObjectType> keys)
        {
            RtsObjectType lowestType = keys[0];

            foreach (RtsObjectType key in keys)
            {
                if (key.SelectionSortValue < lowestType.SelectionSortValue)
                {
                    lowestType = key;
                }
            }

            return lowestType;
        }

        public bool Contains(RtsObject o)
        {
            return objects.Contains(o);
        }
        public bool ContainsType(RtsObject o)
        {
            RtsObjectType type = o.Type;
            return typeCounts.ContainsKey(type);
        }
        public RtsObject[] ToArray()
        {
            return objects.ToArray<RtsObject>();
        }
        public List<RtsObject>.Enumerator GetEnumerator()
        {
            return objects.GetEnumerator();
        }

        public int Count
        {
            get
            {
                return objects.Count;
            }
        }
        public int TypeCount
        {
            get
            {
                return typeCounts.Count;
            }
        }
        public RtsObjectType ActiveType
        {
            get
            {
                return activeType;
            }
            set
            {
                activeType = value;
                activeTypeRallyable = false;

                if (value == null)
                {
                    ActiveCommandCard = null;
                    return;
                }

                ActiveCommandCard = activeType.CommandCard;
            }
        }
        public bool ActiveTypeRallyable
        {
            get
            {
                return activeTypeRallyable;
            }
        }

        public RtsObject this[int i]
        {
            get
            {
                return objects[i];
            }
        }

        public static explicit operator List<RtsObject>(Selection s)
        {
            return s.objects;
        }

        void sortSelection()
        {
            for (int i = 1; i < objects.Count; i++)
            {
                for (int j = i; j >= 1 && objects[j].SelectionSortValue < objects[j - 1].SelectionSortValue; j--)
                {
                    RtsObject tempItem = objects[j];
                    objects.RemoveAt(j);
                    objects.Insert(j - 1, tempItem);
                }
            }
        }
        void sortTypes()
        {
            //typeCounts.ElementAt<RtsObject>();
            for (int i = 2; i < typeCounts.Count; i++)
            {
                for (int j = i; j >= 1 && typeCounts.ElementAt(j).Key.SelectionSortValue < typeCounts.ElementAt(j - 1).Key.SelectionSortValue; j--)
                {
                    RtsObjectType tempKey = typeCounts.ElementAt(j).Key;
                    int tempCount = typeCounts.ElementAt(j).Value;
                    typeCounts.Remove(typeCounts.ElementAt(j).Key);
                    //typeCounts.Insert(j - 1, tempItem);
                }
            }
        }
    }
}
