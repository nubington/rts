using System;
using System.Collections.Generic;
using System.Linq;

namespace rts
{
    class Selection
    {
        List<RtsObject> objects;
        Type mostPopulousType;
        int largestCount = 0;
        Type activeType;
        public CommandCard ActiveCommandCard;

        Dictionary<Type, int> typeCounts = new Dictionary<Type, int>();

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

            // add object/count pairs to the objectCount dictionary
            foreach (RtsObject o in objects)
            {
                if (!typeCounts.ContainsKey(o.GetType()))
                {
                    typeCounts.Add(o.GetType(), 1);
                }
                else
                {
                    //int count;
                    //typeCounts.TryGetValue(o.GetType(), out count);
                    //count++;
                    typeCounts[o.GetType()]++;
                }
            }

            // mostPopulousType = largest count
            foreach (KeyValuePair<Type, int> pair in typeCounts)
            {
                if (pair.Value > largestCount)
                {
                    mostPopulousType = pair.Key;
                    largestCount = pair.Value;
                }
            }

            SetActiveTypeToMostPopulousType();
        }
        public Selection(Selection s)
            : this(s.objects)
        {
        }

        public void Add(RtsObject o)
        {
            objects.Add(o);

            if (objects.Count == 1)
                activeType = o.GetType();

            int count;

            if (!typeCounts.ContainsKey(o.GetType()))
            {
                typeCounts.Add(o.GetType(), 1);
                count = 1;
            }
            else
            {
                typeCounts[o.GetType()]++;
                typeCounts.TryGetValue(o.GetType(), out count);
            }

            if (count > largestCount)
            {
                mostPopulousType = o.GetType();
                largestCount = count;
            }
        }
        public void Remove(RtsObject o)
        {
            objects.Remove(o);

            if (typeCounts.ContainsKey(o.GetType()))
            {
                int count;
                typeCounts.TryGetValue(o.GetType(), out count);
                count--;
                if (count == 0)
                    typeCounts.Remove(o.GetType());
            }

            if (mostPopulousType == o.GetType())
            {
                largestCount = 0;
                foreach (KeyValuePair<Type, int> pair in typeCounts)
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
            if (typeCounts.Count > 1)
            {
                List<Type> keys = typeCounts.Keys.ToList<Type>();

                for (int i = 0; i < keys.Count; i++)
                {
                    if (keys[i] == activeType)
                    {
                        activeType = keys[(i + 1) % keys.Count];
                        break;
                    }
                }
            }
        }
        public void SetActiveTypeToMostPopulousType()
        {
            activeType = mostPopulousType;
        }

        public bool Contains(RtsObject o)
        {
            return objects.Contains(o);
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
        public Type ActiveType
        {
            get
            {
                return activeType;
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
    }
}
