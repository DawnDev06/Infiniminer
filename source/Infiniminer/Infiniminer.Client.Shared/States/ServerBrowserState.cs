/* ----------------------------------------------------------------------------
MIT License

Copyright (c) 2009 Zach Barth
Copyright (c) 2023 Christopher Whitley

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
---------------------------------------------------------------------------- */

using System;
using System.Collections.Generic;
using System.Net;
using StateMasher;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Infiniminer.States
{
    public class ServerBrowserState : State
    {
        SpriteBatch spriteBatch;
        BasicEffect uiEffect;
        Texture2D texMenu;
        Rectangle drawRect;
        string nextState = null;
        List<ServerInformation> serverList = new List<ServerInformation>();
        ClickRegion[] clkServers;
        const int TxtPanningX = 15;
        const int TxtPanningY =  3;
        SpriteFont uiFont;
        bool directConnectIPEnter = false;
        string directConnectIP = "";
        KeyMap keyMap;

        ClickRegion[] clkMenuServer = new ClickRegion[3] {
    new ClickRegion(new Rectangle(763,713,243,42), "refresh"),
    new ClickRegion(new Rectangle(0,713,425,42), "direct"),
    new ClickRegion(new Rectangle(474,713,224,42), "settings")
};

        int SelectionsCount { get { return clkServers.Length + clkMenuServer.Length; } }
        int selectionIndex = -1;
        float selectionAlpha = 0;
        Texture2D texMenuDot;
        ClickRegion hoverRegion;

        public override void OnEnter(string oldState)
        {
            _SM.IsMouseVisible = true;
            (_SM as InfiniminerGame).ResetPropertyBag();
            _P = _SM.propertyBag;

            spriteBatch = new SpriteBatch(_SM.GraphicsDevice);
            uiEffect = new BasicEffect(_SM.GraphicsDevice);
            uiEffect.TextureEnabled = true;
            uiEffect.VertexColorEnabled = true;
            texMenu = _SM.Content.Load<Texture2D>("menus/tex_menu_server");

            texMenuDot = new Texture2D(_SM.GraphicsDevice, 1, 1);
            texMenuDot.SetData<Color>(new Color[] { Color.White });

            UpdateUIViewport(_SM.GraphicsDevice.Viewport);

            uiFont = _SM.Content.Load<SpriteFont>("font_04b08");
            keyMap = new KeyMap();

            serverList = (_SM as InfiniminerGame).EnumerateServers(0.5f);
            RefreshClkServers();
        }

        private void RefreshClkServers()
        {
            clkServers = new ClickRegion[serverList.Count];

            for (int i = 0; i < serverList.Count; i++)
            {
                if (i >= 23)
                    break;

                string serverDesc = serverList[i].GetServerDesc();

                Vector2 txtSize = uiFont.MeasureString(serverDesc);
                Rectangle rect = default;
                rect.Width  = (int)txtSize.X + TxtPanningX * 2;
                rect.Height = (int)txtSize.Y + TxtPanningY * 2;
                rect.X = (VWidth / 2) - (rect.Width / 2);
                rect.Y = 80 + i * 25;

                clkServers[i] = new ClickRegion(rect, i.ToString());
            }
        }

        const int VWidth = 1024;
        const int VHeight = 768;
        const float VAspect = (float)VWidth / (float)VHeight;
        private void UpdateUIViewport(Viewport viewport)
        {
            // calculate virtual resolution
            float aspect = viewport.AspectRatio;
            float vWidth = (aspect > VAspect) ? (VHeight * aspect) : VWidth;
            float vHeight = (aspect < VAspect) ? (VWidth / aspect) : VHeight;

            drawRect = new Rectangle((int)vWidth / 2 - VWidth / 2,
                                     (int)vHeight / 2 - VHeight / 2,
                                     1024,
                                     1024);

            Matrix world = Matrix.CreateScale(1f, -1f, -1f) // Flip Y and Depth
                         * Matrix.CreateTranslation(-vWidth / 2f, vHeight / 2f, 0f) // offset center
                         * Matrix.CreateScale(1f / vWidth, 1f / vWidth, 1f); // normalize scale

            if (_SM.propertyBag.playerCamera.UseVrCamera)
            {
                float uiScale = 1f; // scale UI 1meter across.
                world *= Matrix.CreateScale(uiScale, uiScale, 1f);

                // position UI panel
                world *= Matrix.CreateTranslation(0.0f, 0.1f, -1.0f);

                uiEffect.World = world;
                uiEffect.View = _SM.propertyBag.playerCamera.ViewMatrix;
                uiEffect.Projection = _SM.propertyBag.playerCamera.ProjectionMatrix;
            }
            else
            {
                float fov = MathHelper.ToRadians(70);
                float uiScale = ((float)Math.Tan(fov * 0.5)) * aspect * 2f; // scale to fit nearPlane
                world *= Matrix.CreateScale(uiScale, uiScale, 1f);

                world *= Matrix.CreateTranslation(0.0f, 0.0f, -1.0f); // position to near plane

                uiEffect.World = world;
                uiEffect.View = Matrix.Identity;
                uiEffect.Projection = Matrix.CreatePerspectiveFieldOfView(fov, aspect, 1f, 1000.0f);
            }
        }

        public override void OnLeave(string newState)
        {

        }

        public override string OnUpdate(GameTime gameTime, KeyboardState keyState, MouseState mouseState)
        {

            _P.inputEngine.Update(gameTime);

            if (_P.inputEngine.MenuDown.Released())
            {
                selectionIndex = (selectionIndex + 1) % SelectionsCount;
                selectionAlpha = 0;
                _P.PlaySound(InfiniminerSound.ClickLow);
            }
            if (_P.inputEngine.MenuUp.Released())
            {
                selectionIndex = (selectionIndex - 1 + SelectionsCount) % SelectionsCount;
                selectionAlpha = 0;
                _P.PlaySound(InfiniminerSound.ClickLow);
            }
            if (_SM.WindowHasFocus() && _P.inputEngine.MenuConfirm.Released())
            {
                if (selectionIndex >= 0 && selectionIndex < clkServers.Length)
                {
                    int serverIndex = selectionIndex;

                    (_SM as InfiniminerGame).JoinGame(serverList[selectionIndex].ipEndPoint);
                    nextState = "Infiniminer.States.LoadingState";
                    _P.PlaySound(InfiniminerSound.ClickHigh);
                }
                if (selectionIndex >= clkServers.Length)
                {
                    ClickRegion selectedRegion = clkMenuServer[selectionIndex- clkServers.Length];
                    SelectNextState(selectedRegion);
                }
            }

            return nextState;
        }

        public override void OnRenderAtEnter(GraphicsDevice graphicsDevice)
        {

        }

        private void DrawSelection(SpriteBatch spriteBatch, GameTime gameTime)
        {
            ClickRegion menuRegion = null;
            if (hoverRegion != null)
                menuRegion = hoverRegion;
            else if (selectionIndex >= 0 && selectionIndex < clkServers.Length)
                menuRegion = clkServers[selectionIndex];
            else if (selectionIndex >= clkServers.Length)
                menuRegion = clkMenuServer[selectionIndex- clkServers.Length];
            else
                return;

            Rectangle rect = menuRegion.Rectangle;
            rect.X += drawRect.X;
            rect.Y += drawRect.Y;

            selectionAlpha += (1f - selectionAlpha) * 0.1f;
            float timeAlpha = (1f + (float)Math.Cos(Math.Tau * gameTime.TotalGameTime.TotalSeconds)) / 2f;
            float alpha = selectionAlpha * 0.175f + timeAlpha * 0.025f;

            spriteBatch.Draw(texMenuDot, rect, new Rectangle(0, 0, 1, 1), Color.White * alpha);
        }

        public override void OnRenderAtUpdate(GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            UpdateUIViewport(graphicsDevice.Viewport);

            spriteBatch.Begin(sortMode: SpriteSortMode.Deferred, blendState: BlendState.AlphaBlend, effect: uiEffect);
            spriteBatch.Draw(texMenu, drawRect, Color.White);
            DrawSelection(spriteBatch, gameTime);

            for (int i = 0; i < serverList.Count; i++)
            {
                if (i >= 23)
                    break;

                string serverDesc = serverList[i].GetServerDesc();
                Rectangle rect = clkServers[i].Rectangle;

                spriteBatch.DrawString(uiFont, serverDesc, new Vector2(drawRect.X + rect.X + TxtPanningX, drawRect.Y + rect.Y + TxtPanningY), Color.White);
            }

            spriteBatch.DrawString(uiFont, Defines.INFINIMINER_VERSION, new Vector2(10, drawRect.Y * 2 + VHeight - 20), Color.White);

            if (directConnectIPEnter)
                spriteBatch.DrawString(uiFont, "ENTER IP: " + directConnectIP, new Vector2(drawRect.X + 30, drawRect.Y + 690), Color.White);

            spriteBatch.End();
        }

        public override void OnKeyDown(Keys key)
        {
            if (directConnectIPEnter)
            {
                if (key == Keys.Escape)
                {
                    directConnectIPEnter = false;
                    directConnectIP = "";
                }

                if (key == Keys.Back && directConnectIP.Length > 0)
                {
                    directConnectIP = directConnectIP.Substring(0, directConnectIP.Length - 1);
                }

                if (key == Keys.Enter)
                {
                    // Try what was entered first as an IP, and then second as a host name.
                    directConnectIPEnter = false;
                    _P.PlaySound(InfiniminerSound.ClickHigh);
                    IPAddress connectIp = null;
                    if (!IPAddress.TryParse(directConnectIP, out connectIp))
                    {
                        connectIp = null;
                        try
                        {
                            IPAddress[] resolveResults = Dns.GetHostAddresses(directConnectIP);
                            for (int i = 0; i < resolveResults.Length; i++)
                                if (resolveResults[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                {
                                    connectIp = resolveResults[i];
                                    break;
                                }
                        }
                        catch (Exception)
                        {
                            // So, GetHostAddresses() might fail, but we don't really care. Just leave connectIp as null.
                        }
                    }
                    if (connectIp != null)
                    {
                        (_SM as InfiniminerGame).JoinGame(new IPEndPoint(connectIp, 5565));
                        nextState = "Infiniminer.States.LoadingState";
                    }
                    directConnectIP = "";
                }

                if (key == Keys.V && (Keyboard.GetState().IsKeyDown(Keys.LeftControl) || Keyboard.GetState().IsKeyDown(Keys.RightControl)))
                {
                    try
                    {
                        // [MG_PORT_NOTES] System.Windows.Forms is Windows Only
                        // directConnectIP += System.Windows.Forms.Clipboard.GetText();
                    }
                    catch (Exception e)
                    {
                    }
                }
                else if (keyMap.IsKeyMapped(key))
                {
                    directConnectIP += keyMap.TranslateKey(key, false);
                }
            }
            else
            {
                if (key == Keys.Escape)
                {
                    nextState = "Infiniminer.States.TitleState";
                }
            }
        }

        public override void OnKeyUp(Keys key)
        {

        }

        public override void OnMouseDown(MouseButton button, int x, int y)
        {
            ScreenToUI(uiEffect, ref x, ref y);
            x -= drawRect.X;
            y -= drawRect.Y;

            if (directConnectIPEnter == false)
            {
                ClickRegion clkServer = ClickRegion.HitTest(clkServers, new Point(x, y));
                if (clkServer != null)
                {
                    int serverIndex = int.Parse(clkServer.Tag);
                    
                    (_SM as InfiniminerGame).JoinGame(serverList[serverIndex].ipEndPoint);
                    nextState = "Infiniminer.States.LoadingState";
                    _P.PlaySound(InfiniminerSound.ClickHigh);
                }

                ClickRegion selectedRegion = ClickRegion.HitTest(clkMenuServer, new Point(x, y));
                if (selectedRegion != null)
                    SelectNextState(selectedRegion);

                selectionIndex = -1;
                selectionAlpha = 0;
            }
        }

        private void SelectNextState(ClickRegion selectedRegion)
        {
            switch (selectedRegion.Tag)
            {
                case "refresh":
                    _P.PlaySound(InfiniminerSound.ClickHigh);
                    serverList = (_SM as InfiniminerGame).EnumerateServers(0.5f);
                    RefreshClkServers();
                    break;

                case "direct":
                    directConnectIPEnter = true;
                    _P.PlaySound(InfiniminerSound.ClickHigh);
                    break;

                case "settings":
                    _P.PlaySound(InfiniminerSound.ClickHigh);
                    nextState = "Infiniminer.States.SettingsState";
                    break;
            }
        }

        public override void OnMouseUp(MouseButton button, int x, int y)
        {
            ScreenToUI(uiEffect, ref x, ref y);
            x -= drawRect.X;
            y -= drawRect.Y;

        }

        public override void OnMouseScroll(int scrollDelta)
        {
            if (scrollDelta < 0)
            {
                selectionIndex = (selectionIndex + 1) % SelectionsCount;
                selectionAlpha = 0;
                _P.PlaySound(InfiniminerSound.ClickLow);
            }
            else
            {
                selectionIndex = (selectionIndex - 1 + SelectionsCount) % SelectionsCount;
                selectionAlpha = 0;
                _P.PlaySound(InfiniminerSound.ClickLow);
            }
        }

        public override void OnMouseMove(int x, int y)
        {
            ScreenToUI(uiEffect, ref x, ref y);
            x -= drawRect.X;
            y -= drawRect.Y;

            ClickRegion selectedServerRegion = ClickRegion.HitTest(clkServers, new Point(x, y));
            if (selectedServerRegion != null)
            {
                hoverRegion = selectedServerRegion;
                selectionIndex = -1;
                selectionAlpha = 0;
                return;
            }

            hoverRegion = ClickRegion.HitTest(clkMenuServer, new Point(x, y));
            if (hoverRegion != null)
            {
                selectionIndex = -1;
                selectionAlpha = 0;
            }
        }

        // convert mouse screen position to UI world position
        private void ScreenToUI(IEffectMatrices matrices, ref int x, ref int y)
        {
            Viewport vp = _SM.GraphicsDevice.Viewport;

            Vector3 position3 = vp.Unproject(
                            new Vector3(x, y, 0),
                            matrices.Projection,
                            matrices.View,
                            matrices.World);

            x = (int)position3.X;
            y = (int)position3.Y;
        }
    }
}
