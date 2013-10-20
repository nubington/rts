using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace rts
{
    class CommandCard
    {
        public static readonly CommandCard 
            DefaultCommandCard,
            TestCommandCard;

        static CommandCard()
        {
            DefaultCommandCard = new CommandCard(new CommandButton[3, 4]);
            DefaultCommandCard.Buttons[0, 0] = new CommandButton(CommandButtonType.Attack);

            TestCommandCard = new CommandCard(new CommandButton[3, 4]);
            TestCommandCard.Buttons[0, 0] = new CommandButton(CommandButtonType.Attack);
            TestCommandCard.Buttons[0, 1] = new CommandButton(CommandButtonType.Move);
        }

        public CommandButton[,] Buttons;// = new CommandButton[4, 4];

        public CommandCard(CommandButton[,] buttons)
        {
            Buttons = buttons;
        }

        //call at game initialization to load class early
        public static void Init() { }
    }

    class CommandButtonType
    {
        public static readonly CommandButtonType
            Move = new CommandButtonType(Game1.Game.Content.Load<Texture2D>("whitebox"), Keys.Kana),
            Attack = new CommandButtonType(Game1.Game.Content.Load<Texture2D>("simplesword"), Keys.A);

        public readonly Texture2D Texture;
        public readonly Keys Hotkey;

        CommandButtonType(Texture2D texture, Keys hotkey)
        {
            Texture = texture;
            Hotkey = hotkey;
        }
    }

    class CommandButton : SimpleButton
    {
        static int idCounter = 0;

        public readonly CommandButtonType Type;
        int id;

        public CommandButton(CommandButtonType type) 
            : base(Rectangle.Empty)
        {
            id = idCounter++;
            NormalTexture = type.Texture;
            Type = type;
            Hotkey = type.Hotkey;
        }

        public override bool Equals(object o)
        {
            //if (!(o is Direction))
            //    return false;
            //if (o == null)
            //    return false;
            return  (o is CommandButton) && (id == ((CommandButton)o).id);
        }
        public override int GetHashCode()
        {
            return (int)(id * 100);
        }

        public static bool operator ==(CommandButton b1, CommandButton b2)
        {
            return b1.Equals(b2);
        }
        public static bool operator !=(CommandButton b1, CommandButton b2)
        {
            return !b1.Equals(b2);
        }
    }
}
