using Microsoft.Xna.Framework;

namespace rts
{
    public class RallyPoint
    {
        public Resource Resource { get; private set; }
        public Vector2 Point { get; private set; }

        public RallyPoint(Vector2 point, Resource resource)
        {
            Point = point;
            Resource = resource;
        }
    }
}
