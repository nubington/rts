using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using System.Windows.Forms;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using Lidgren.Network;
using Lidgren.Network.Xna;

namespace rts
{
    public class HostLobby : GameState
    {
        Viewport viewport;
        Random rand = new Random();

        MouseState mouseState;
        KeyboardState keyboardState;

        PrimitiveLine line = new PrimitiveLine(GraphicsDevice, 1);

        static SpriteFont bigFont;

        NetServer server;

        short myTeam;

        public HostLobby(EventHandler callback)
            : base(callback)
        {
            if (!contentLoaded)
            {
                contentLoaded = true;

                bigFont = Content.Load<SpriteFont>("spritefonts/BigMessage");
            }

            viewport = GraphicsDevice.Viewport;

            NetPeerConfiguration config = new NetPeerConfiguration("rts");
            config.Port = 14242;
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);

            server = new NetServer(config);
            server.Start();
        }

        public override void Update(GameTime gameTime)
        {
            // mute check
            checkForMute();

            // update mouse and keyboard state
            mouseState = Mouse.GetState();
            keyboardState = Keyboard.GetState();

            SimpleButton.UpdateAll(mouseState, keyboardState);

            NetIncomingMessage inc;
            while ((inc = server.ReadMessage()) != null)
            {
                Thread.Sleep(1);

                if (inc.MessageType == NetIncomingMessageType.DiscoveryRequest)
                {
                    // Create a response and write some example data to it
                    NetOutgoingMessage response = server.CreateMessage();
                    myTeam = (short)rand.Next(2);
                    response.Write(myTeam);

                    // Send the response to the sender of the request
                    server.SendDiscoveryResponse(response, inc.SenderEndPoint);
                }
                else if (inc.MessageType == NetIncomingMessageType.StatusChanged)
                {
                    NetConnectionStatus status = (NetConnectionStatus)inc.ReadByte();
                    if (status == NetConnectionStatus.RespondedConnect)
                    {
                        cleanup();
                        returnControlToStartGame();
                        return;
                    }
                }
                /*else// if (inc.MessageType == NetIncomingMessageType.ConnectionApproval)
                {
                    string str = "";
                    foreach (byte b in inc.Data)
                    {
                        str += (char)b;
                    }
                    str += "balls";

                    //networkEndPoint = inc.SenderEndPoint;

                    cleanup();
                    returnControlToStartGame();
                    return;
                }*/
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            string str = "Waiting...";
            Vector2 strSize = bigFont.MeasureString(str);
            spriteBatch.DrawString(bigFont, str, new Vector2((int)(viewport.Width / 2 - strSize.X / 2), (int)(viewport.Height / 2 - strSize.Y / 2)), Color.White);

            spriteBatch.End();
        }

        void cleanup()
        {
        }

        void returnControlToStartGame()
        {
            callback.Invoke(this, new StartGameArgs(server, myTeam));
        }
    }
}