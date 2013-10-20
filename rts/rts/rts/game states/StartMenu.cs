using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
//using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using System.Diagnostics;
using System.Windows.Forms;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using Lidgren.Network;
using Lidgren.Network.Xna;

namespace rts
{
    public class StartMenu : GameState
    {
        Viewport viewport;

        MouseState mouseState;
        KeyboardState keyboardState;

        SimpleButton createButton, joinLanButton, joinIPButton, exitButton;

        PrimitiveLine line = new PrimitiveLine(GraphicsDevice, 1);

        static SpriteFont buttonFont;

        StartMenuState state = StartMenuState.Normal;

        NetClient client;
        //System.Net.IPEndPoint networkEndPoint;

        short myTeam;

        public StartMenu(EventHandler callback)
            : base(callback)
        {
            if (!contentLoaded)
            {
                contentLoaded = true;

                buttonFont = Content.Load<SpriteFont>("spritefonts/StartMenuButtonFont");
            }

            viewport = GraphicsDevice.Viewport;

            //MediaPlayer.Play(rtsMusic);
            //MediaPlayer.Volume = 0;// .25f;
            //MediaPlayer.IsRepeating = true;

            initializeButtons();
        }

        int buttonWidth = 100, buttonHeight = 50;
        void initializeButtons()
        {
            createButton = new SimpleButton(new Rectangle((int)(viewport.Width * .25f - buttonWidth / 2), (int)(viewport.Height * .75f - buttonHeight / 2), buttonWidth, buttonHeight));
            SimpleButton.AddButton(createButton);

            joinLanButton = new SimpleButton(new Rectangle((int)(viewport.Width * .5f - buttonWidth / 2), (int)(viewport.Height * .75f - buttonHeight / 2), buttonWidth, buttonHeight));
            SimpleButton.AddButton(joinLanButton);

            joinIPButton = new SimpleButton(new Rectangle((int)(viewport.Width * .5f - buttonWidth / 2), (int)(viewport.Height * .75f - buttonHeight / 2 + buttonHeight + 1), buttonWidth, buttonHeight));
            SimpleButton.AddButton(joinIPButton);

            exitButton = new SimpleButton(new Rectangle((int)(viewport.Width * .75f - buttonWidth / 2), (int)(viewport.Height * .75f - buttonHeight / 2), buttonWidth, buttonHeight));
            SimpleButton.AddButton(exitButton);
        }

        public override void Update(GameTime gameTime)
        {
            // mute check
            checkForMute();

            // update mouse and keyboard state
            mouseState = Mouse.GetState();
            keyboardState = Keyboard.GetState();

            if (state == StartMenuState.Joining)
            {
                NetIncomingMessage inc;
                while ((inc = client.ReadMessage()) != null)
                {
                    Thread.Sleep(1);

                    if (inc.MessageType == NetIncomingMessageType.DiscoveryResponse)
                    {
                        short enemyTeam = inc.ReadInt16();
                        myTeam = (short)((enemyTeam + 1) % 2);

                        client.Connect(inc.SenderEndPoint);

                        //NetOutgoingMessage msg = client.CreateMessage();
                       // msg.Write("bowlop");
                        //client.SendMessage(msg, NetDeliveryMethod.ReliableUnordered);

                        cleanup();
                        returnControlToStartGame();
                        return;
                    }
                    /*else// if (inc.MessageType == NetIncomingMessageType.Data)
                    {
                        cleanup();
                        returnControlToStartGame();
                        return;
                    }*/
                }
            }
            else if (state == StartMenuState.Normal)
            {

                SimpleButton.UpdateAll(mouseState, keyboardState);

                if (checkButtons())
                    return;
            }
        }

        bool checkButtons()
        {
            if (createButton.Triggered)
            {
                cleanup();
                returnControl("create");
                return true;
            }
            else if (joinLanButton.Triggered)
            {
                NetPeerConfiguration config = new NetPeerConfiguration("rts");
                config.Port = 14243;

                client = new NetClient(config);
                client.Start();

                client.DiscoverLocalPeers(14242);

                state = StartMenuState.Joining;
            }
            else if (joinIPButton.Triggered)
            {
                if (!File.Exists("C:\\rts hosts.txt"))
                    File.Create("C:\\rts hosts.txt");

                string[] hosts = File.ReadAllLines("C:\\rts hosts.txt");

                NetPeerConfiguration config = new NetPeerConfiguration("rts");
                config.Port = 14243;

                client = new NetClient(config);
                client.Start();

                foreach (string host in hosts)
                    client.DiscoverKnownPeer(host, 14242);

                state = StartMenuState.Joining;
            }
            else if (exitButton.Triggered)
            {
                cleanup();
                returnControl("exit");
                return true;
            }

            return false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            if (state == StartMenuState.Joining)
            {
                string str = "Joining...";
                Vector2 strSize = buttonFont.MeasureString(str);
                spriteBatch.DrawString(buttonFont, str, new Vector2((int)(viewport.Width / 2 - strSize.X / 2), (int)(viewport.Height / 2 - strSize.Y / 2)), Color.White);
            }
            else if (state == StartMenuState.Normal)
            {
                drawButtons(spriteBatch);
            }

            spriteBatch.End();
        }

        void drawButtons(SpriteBatch spriteBatch)
        {
            // create button
            line.ClearVectors();
            line.CreateBox(createButton.Rectangle);
            line.Render(spriteBatch);
            if (createButton.Pressing)
                spriteBatch.Draw(ColorTexture.White, createButton.Rectangle, Color.White * .15f);
            string str = "Create";
            Vector2 strSize = buttonFont.MeasureString(str);
            spriteBatch.DrawString(buttonFont, str, new Vector2((int)(createButton.X + createButton.Width / 2 - strSize.X / 2), (int)(createButton.Y + createButton.Height / 2 - strSize.Y / 2)), Color.White);

            // join lan button
            line.ClearVectors();
            line.CreateBox(joinLanButton.Rectangle);
            line.Render(spriteBatch);
            if (joinLanButton.Pressing)
                spriteBatch.Draw(ColorTexture.White, joinLanButton.Rectangle, Color.White * .15f);
            str = "LAN Join";
            strSize = buttonFont.MeasureString(str);
            spriteBatch.DrawString(buttonFont, str, new Vector2((int)(joinLanButton.X + joinLanButton.Width / 2 - strSize.X / 2), (int)(joinLanButton.Y + joinLanButton.Height / 2 - strSize.Y / 2)), Color.White);

            // join ip button
            line.ClearVectors();
            line.CreateBox(joinIPButton.Rectangle);
            line.Render(spriteBatch);
            if (joinIPButton.Pressing)
                spriteBatch.Draw(ColorTexture.White, joinIPButton.Rectangle, Color.White * .15f);
            str = "IP Join";
            strSize = buttonFont.MeasureString(str);
            spriteBatch.DrawString(buttonFont, str, new Vector2((int)(joinIPButton.X + joinIPButton.Width / 2 - strSize.X / 2), (int)(joinIPButton.Y + joinIPButton.Height / 2 - strSize.Y / 2)), Color.White);

            // exit button
            line.ClearVectors();
            line.CreateBox(exitButton.Rectangle);
            line.Render(spriteBatch);
            if (exitButton.Pressing)
                spriteBatch.Draw(ColorTexture.White, exitButton.Rectangle, Color.White * .15f);
            str = "Exit";
            strSize = buttonFont.MeasureString(str);
            spriteBatch.DrawString(buttonFont, str, new Vector2((int)(exitButton.X + exitButton.Width / 2 - strSize.X / 2), (int)(exitButton.Y + exitButton.Height / 2 - strSize.Y / 2)), Color.White);
        }

        void cleanup()
        {
            SimpleButton.RemoveAllButtons();
        }

        void returnControlToStartGame()
        {
            callback.Invoke(this, new StartGameArgs(client, myTeam));
        }
    }

    public enum StartMenuState
    {
        Normal, Joining
    }
}