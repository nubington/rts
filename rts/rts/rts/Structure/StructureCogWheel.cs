using Microsoft.Xna.Framework;

namespace rts
{
    public class StructureCogWheel
    {
        public Structure Structure { get; private set; }
        public float Rotation = 0;
        public Rectangle Rectangle { get; private set; }

        public StructureCogWheel(Structure structure, int size)
        {
            Structure = structure;
            //Rectangle = new Rectangle((int)(structure.CenterPointX - size / 2), (int)(structure.CenterPointY - size / 2), size, size);
            Rectangle = new Rectangle((int)(structure.CenterPointX), (int)(structure.CenterPointY), size, size);
        }
    }
}
