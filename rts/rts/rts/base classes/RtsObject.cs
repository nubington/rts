using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;


namespace rts
{
    public abstract class RtsObjectType
    {
        public CommandCard CommandCard;

        public Texture2D NormalTexture;

        public string Name { get; protected set; }

        public int SelectionSortValue { get; protected set; }
    }

    public abstract class RtsObject : BaseObject
    {
        static List<RtsObject> rtsObjects = new List<RtsObject>();

        public static PathFinder PathFinder;

        public List<MapTile> VisibleTiles = new List<MapTile>();

        public PathNode CurrentPathNode;
        public List<PathNode> OccupiedPathNodes = new List<PathNode>();
        public HashSet<BoundingBox> OccupiedBoundingBoxes = new HashSet<BoundingBox>();
        public List<PathNode> PathNodeBufferSquare = new List<PathNode>();
        public bool Visible;
        public bool Revealed;
        public bool HasMoved = true;
        float radius, radiusSquared, diameter;
        int hp, maxHp;
        decimal percentHp;
        int armor;
        protected int attackDamage, attackRange, attackDelay, initialAttackDelay, timeSinceLastAttack = int.MaxValue / 2;//500;
        int sightRange;
        int buildTime;
        bool isDead, targetable;
        public short Team { get; private set; }
        int targetPriority;

        public int SelectionSortValue { get; protected set; }

        public RtsObject(Vector2 position, int size, short team)
            : base(new Rectangle(0, 0, size, size))
        {
            PrecisePosition = position;
            diameter = size;
            radius = size / 2f;
            radiusSquared = (float)Math.Pow(radius, 2);
            Team = team;
            targetable = true;
        }

        public void TakeDamage(Unit attacker, int damage)
        {
            int actualDamage = (int)MathHelper.Max(damage - armor, 0);
            Hp -= actualDamage;
            if (Hp == 0)
            {
                Die();
                if (Team != attacker.Team)
                    attacker.KillCount++;
            }
        }

        public virtual void Die()
        {
            IsDead = true;
        }

        protected void UpdateVisibility()
        {
            foreach (MapTile tile in VisibleTiles)
            {

            }

            Visible = false;

            foreach (PathNode pathNode in OccupiedPathNodes)
            {
                if (pathNode.Tile.Visible)
                {
                    Visible = true;
                    Revealed = true;
                    return;
                }
            }
        }

        public bool Contains(Vector2 point)
        {
            return Vector2.Distance(centerPoint, point) < (this.Radius);
        }

        public override bool Intersects(BaseObject o)
        {
            float angle = (float)Math.Atan2(o.CenterPoint.Y - centerPoint.Y, o.CenterPoint.X - centerPoint.X);
            Vector2 point = centerPoint + new Vector2(Radius * (float)Math.Cos(angle), Radius * (float)Math.Sin(angle));
            //return o.Touches(point);

            return o.Touches(point) || Vector2.DistanceSquared(centerPoint, o.CenterPoint) < RadiusSquared;// || 
            //((Math.Abs(centerPoint.X - o.X) < radius || Math.Abs(centerPoint.X - (o.X  + o.Width)) < radius) &&
            //(Math.Abs(centerPoint.Y - o.Y) < radius || Math.Abs(centerPoint.Y - (o.Y + o.Height)) < radius));
        }

        public float Radius
        {
            get
            {
                return radius;
            }
        }
        public float RadiusSquared
        {
            get
            {
                return radiusSquared;
            }
        }
        public float Diameter
        {
            get
            {
                return diameter;
            }
            set
            {
                diameter = value;
                radius = value / 2f;
                Vector2 center = CenterPoint;
                Width = (int)Math.Round(value);
                Height = (int)Math.Round(value);
                CenterPoint = center;
            }
        }
        public int Hp
        {
            get
            {
                return hp;
            }
            set
            {
                hp = (int)MathHelper.Clamp(value, 0, maxHp);
                percentHp = hp / (decimal)maxHp;
            }
        }
        public int MaxHp
        {
            get
            {
                return maxHp;
            }
            set
            {
                maxHp = (int)MathHelper.Max(0, value);
                percentHp = hp / (decimal)maxHp;
            }
        }
        public decimal PercentHp
        {
            get
            {
                return percentHp;
            }
        }
        public int Armor
        {
            get
            {
                return armor;
            }
            set
            {
                armor = value;
            }
        }
        public int SightRange
        {
            get
            {
                return sightRange;
            }
            set
            {
                sightRange = value;
            }
        }
        public int BuildTime
        {
            get
            {
                return buildTime;
            }
            set
            {
                buildTime = value;
            }
        }
        public bool IsDead
        {
            get
            {
                return isDead;
            }
            set
            {
                isDead = value;
            }
        }
        public bool Targetable
        {
            get
            {
                return targetable;
            }
            set
            {
                targetable = value;
            }
        }
        public virtual int TargetPriority
        {
            get
            {
                return targetPriority;
            }
            set
            {
                targetPriority = value;
            }
        }

        public int AttackDamage
        {
            get
            {
                return attackDamage;
            }
            set
            {
                attackDamage = (int)MathHelper.Max(0, value);
            }
        }
        public int AttackRange
        {
            get
            {
                return attackRange;
            }
            set
            {
                attackRange = value;
            }
        }
        public int AttackDelay
        {
            get
            {
                return attackDelay;
            }
            set
            {
                attackDelay = value;
                initialAttackDelay = value / 5;
            }
        }
        /*public float Speed
        {
            get
            {
                return speed.X;
            }
            set
            {
                speed.X = value;
                speed.Y = value;
            }
        }*/

        public virtual string Name
        {
            get
            {
                return "base object";
            }
        }
        public virtual RtsObjectType Type
        {
            get
            {
                return null;
            }
        }

        public static List<RtsObject> RtsObjects
        {
            get
            {
                return rtsObjects;
            }
        }
        public static void AddObject(RtsObject o)
        {
            rtsObjects.Add(o);
        }
        public static void RemoveObject(RtsObject o)
        {
            lock (RtsObjects)
            {
                rtsObjects.Remove(o);
            }
        }
    }
}
