using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace rts
{
    class SimpleButton : BaseObject
    {
        static protected bool pressing;
        static protected SimpleButton buttonBeingPressed;
        static protected MouseState mouseState;
        static int idCounter = 0;

        bool triggered;
        Texture2D normalTexture, mousedOverTexture, pressTexture;
        int id;

        Keys hotkey;

        public SimpleButton(Rectangle rectangle)
            : base(rectangle)
        {
            id = idCounter++;
        }
        public SimpleButton(Rectangle rectangle, Texture2D normalTexture, Texture2D mousedOverTexture, Texture2D pressTexture)
            : this(rectangle)
        {
            this.normalTexture = normalTexture;
            this.mousedOverTexture = mousedOverTexture;
            this.pressTexture = pressTexture;
        }

        static List<SimpleButton> allButtons = new List<SimpleButton>();

        public static void AddButton(SimpleButton button)
        {
            allButtons.Add(button);
        }
        public static void AddButtons<T>(List<T> buttons) where T : SimpleButton
        {
            foreach (SimpleButton button in buttons)
            {
                allButtons.Add(button);
            }
        }
        public static void RemoveButton(SimpleButton button)
        {
            allButtons.Remove(button);
        }
        public static void RemoveButtons<T>(List<T> buttons) where T : SimpleButton
        {
            foreach (SimpleButton button in buttons)
            {
                allButtons.Remove(button);
            }
        }
        public static void RemoveAllButtons()
        {
            allButtons.Clear();
        }

        static bool allowPress;
        public static void UpdateAll(MouseState mouseState, KeyboardState keyboardState)
        {
            foreach (SimpleButton button in allButtons)
                button.triggered = false;

            if (!pressing && allowPress)
            {
                foreach (SimpleButton button in allButtons)
                {

                    if ((button.Rectangle.Contains(mouseState.X, mouseState.Y) && mouseState.LeftButton == ButtonState.Pressed))
                    {
                        pressing = true;
                        buttonBeingPressed = button;
                        break;
                    }
                }
            }

            allowPress = (!pressing && mouseState.LeftButton == ButtonState.Released);

            //if ((Object)buttonBeingPressed != null && keyboardState.IsKeyDown(buttonBeingPressed.hotkey))
            //    return;

            if (pressing && mouseState.LeftButton == ButtonState.Released)
            {
                pressing = false;

                foreach (SimpleButton button in allButtons)
                {
                    if (buttonBeingPressed == button && button.Rectangle.Contains(mouseState.X, mouseState.Y))
                    {
                        button.triggered = true;
                        break;
                    }
                }
            }
        }

        /*public void Update(MouseState mState)
        {
            mouseState = mState;
            triggered = false;

            if (!pressing && Rectangle.Contains(mouseState.X, mouseState.Y) && mouseState.LeftButton == ButtonState.Pressed)
            {
                pressing = true;
                buttonBeingPressed = this;
            }
            if (pressing && mouseState.LeftButton == ButtonState.Released)
            {
                pressing = false;

                if (buttonBeingPressed == this && Rectangle.Contains(mouseState.X, mouseState.Y))
                    triggered = true;
            }
        }*/

        //public void static setPressing

        public Keys Hotkey
        {
            get
            {
                return hotkey;
            }
            set
            {
                hotkey = value;
            }
        }
        public bool Pressing
        {
            get
            {
                return (pressing && buttonBeingPressed == this);
            }
        }
        public bool Triggered
        {
            get
            {
                return triggered;
            }
        }
        public Texture2D NormalTexture
        {
            get
            {
                return normalTexture;
            }
            set
            {
                normalTexture = value;
            }
        }
        public Texture2D MousedOverTexture
        {
            get
            {
                return mousedOverTexture;
            }
            set
            {
                mousedOverTexture = value;
            }
        }
        public Texture2D PressTexture
        {
            get
            {
                return pressTexture;
            }
            set
            {
                pressTexture = value;
            }
        }
        public override Texture2D Texture
        {
            get
            {
                if (pressTexture != null && pressing && buttonBeingPressed == this)
                    return pressTexture;
                else if (mousedOverTexture != null && Rectangle.Contains(mouseState.X, mouseState.Y))
                    return mousedOverTexture;
                else
                    return normalTexture;
            }
        }

        public override bool Equals(object o)
        {
            //if (!(o is Direction))
            //    return false;
            return id == ((SimpleButton)o).id;
        }
        public override int GetHashCode()
        {
            return (int)(id * 100);
        }

        public static bool operator ==(SimpleButton d1, SimpleButton d2)
        {
            return d1.Equals(d2);
        }
        public static bool operator !=(SimpleButton d1, SimpleButton d2)
        {
            return !d1.Equals(d2);
        }
    }
}