using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace rts
{
    class UnitButton : SimpleButton
    {
        public static new Texture2D NormalTexture = Game1.Game.Content.Load<Texture2D>("whitebox"), 
            MousedOverTexture = null, PressTexture = null;
        public RtsObject Unit;
        //bool active;

        public UnitButton(Rectangle rectangle, RtsObject unit)
            : base(rectangle)
        {
            Unit = unit;
            //this.active = active;
        }

        public override Texture2D Texture
        {
            get
            {
                if (PressTexture != null && pressing && buttonBeingPressed == this)
                    return PressTexture;
                else if (MousedOverTexture != null && Rectangle.Contains(mouseState.X, mouseState.Y))
                    return MousedOverTexture;
                else
                    return NormalTexture;
            }
        }

        /*public bool Active
        {
            get
            {
                return active;
            }
        }*/
    }
}