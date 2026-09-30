using System;
using System.Collections.Generic;
using StateMasher;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Infiniminer.States
{
    public class SettingsState : State
    {
        const int VWidth = 1024;
        const int VHeight = 768;
        const float VAspect = (float)VWidth / (float)VHeight;
        const float TextScale = 2f;
        const int MaxNameLength = 20;

        SpriteBatch spriteBatch;
        BasicEffect uiEffect;
        Texture2D texDot;
        SpriteFont uiFont;
        Rectangle drawRect;
        KeyMap keyMap;
        string nextState = null;
        ClickRegion hoverRegion;

        // Ekranda düzenlenen değerler (Save'e basana kadar oyuna uygulanmaz).
        string handle = "Player";
        bool editingName = false;
        bool fullscreen = false;
        bool soundOn = true;
        float volume = 1f;
        List<Point> resolutions = new List<Point>();
        int resIndex = 0;

        // Tıklanabilir alanlar (1024x768 sanal ekran koordinatları).
        ClickRegion clkName = new ClickRegion(new Rectangle(200, 160, 624, 50), "name");
        ClickRegion clkFullscreen = new ClickRegion(new Rectangle(200, 230, 624, 50), "fullscreen");
        ClickRegion clkSound = new ClickRegion(new Rectangle(200, 300, 624, 50), "sound");
        ClickRegion clkVolDown = new ClickRegion(new Rectangle(520, 370, 60, 50), "voldown");
        ClickRegion clkVolUp = new ClickRegion(new Rectangle(764, 370, 60, 50), "volup");
        ClickRegion clkResPrev = new ClickRegion(new Rectangle(520, 440, 60, 50), "resprev");
        ClickRegion clkResNext = new ClickRegion(new Rectangle(764, 440, 60, 50), "resnext");
        ClickRegion clkSave = new ClickRegion(new Rectangle(200, 560, 300, 50), "save");
        ClickRegion clkCancel = new ClickRegion(new Rectangle(524, 560, 300, 50), "cancel");
        ClickRegion[] regions;

        public override void OnEnter(string oldState)
        {
            _SM.IsMouseVisible = true;
            _P = _SM.propertyBag;
            nextState = null;
            editingName = false;

            spriteBatch = new SpriteBatch(_SM.GraphicsDevice);
            uiEffect = new BasicEffect(_SM.GraphicsDevice);
            uiEffect.TextureEnabled = true;
            uiEffect.VertexColorEnabled = true;

            texDot = new Texture2D(_SM.GraphicsDevice, 1, 1);
            texDot.SetData<Color>(new Color[] { Color.White });

            uiFont = _SM.Content.Load<SpriteFont>("font_04b08");
            keyMap = new KeyMap();

            // Mevcut ayarları oyundan al.
            InfiniminerGame game = _SM as InfiniminerGame;
            handle = game.PlayerHandle;
            fullscreen = game.IsFullScreen;
            soundOn = !game.NoSound;
            volume = game.VolumeLevel;

            resolutions = new List<Point>
            {
                new Point(1024, 768),
                new Point(1280, 720),
                new Point(1280, 800),
                new Point(1366, 768),
                new Point(1600, 900),
                new Point(1920, 1080),
                new Point(2560, 1440),
            };
            Point current = new Point(game.ScreenWidth, game.ScreenHeight);
            resIndex = resolutions.IndexOf(current);
            if (resIndex < 0)
            {
                resolutions.Insert(0, current);
                resIndex = 0;
            }

            regions = new ClickRegion[]
            {
                clkName, clkFullscreen, clkSound,
                clkVolDown, clkVolUp,
                clkResPrev, clkResNext,
                clkSave, clkCancel
            };

            UpdateUIViewport(_SM.GraphicsDevice.Viewport);
        }

        public override void OnLeave(string newState)
        {
        }

        public override string OnUpdate(GameTime gameTime, KeyboardState keyState, MouseState mouseState)
        {
            return nextState;
        }

        public override void OnRenderAtEnter(GraphicsDevice graphicsDevice)
        {
        }

        public override void OnRenderAtUpdate(GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            graphicsDevice.Clear(new Color(20, 20, 28));
            UpdateUIViewport(graphicsDevice.Viewport);

            spriteBatch.Begin(sortMode: SpriteSortMode.Deferred, blendState: BlendState.AlphaBlend, effect: uiEffect);

            DrawTextCenter("SETTINGS", Offset(new Rectangle(0, 60, VWidth, 60)), Color.White, 3f);

            // Düğme arka planları.
            foreach (ClickRegion region in regions)
            {
                float alpha = (region == hoverRegion) ? 0.25f : 0.10f;
                spriteBatch.Draw(texDot, Offset(region.Rectangle), Color.White * alpha);
            }

            // Kullanıcı adı.
            DrawTextLeft("PLAYER NAME", Offset(clkName.Rectangle));
            DrawTextRight(handle + (editingName ? "_" : ""), Offset(clkName.Rectangle));

            // Tam ekran.
            DrawTextLeft("FULLSCREEN", Offset(clkFullscreen.Rectangle));
            DrawTextRight(fullscreen ? "ON" : "OFF", Offset(clkFullscreen.Rectangle));

            // Ses aç/kapat.
            DrawTextLeft("SOUND", Offset(clkSound.Rectangle));
            DrawTextRight(soundOn ? "ON" : "OFF", Offset(clkSound.Rectangle));

            // Ses seviyesi.
            DrawTextLeft("VOLUME", Offset(new Rectangle(200, 370, 320, 50)));
            DrawTextCenter("-", Offset(clkVolDown.Rectangle), Color.White, TextScale);
            DrawTextCenter(((int)Math.Round(volume * 100)).ToString() + "%", Offset(new Rectangle(580, 370, 184, 50)), Color.White, TextScale);
            DrawTextCenter("+", Offset(clkVolUp.Rectangle), Color.White, TextScale);

            // Çözünürlük.
            Point res = resolutions[resIndex];
            DrawTextLeft("RESOLUTION", Offset(new Rectangle(200, 440, 320, 50)));
            DrawTextCenter("<", Offset(clkResPrev.Rectangle), Color.White, TextScale);
            DrawTextCenter(res.X + "x" + res.Y, Offset(new Rectangle(580, 440, 184, 50)), Color.White, TextScale);
            DrawTextCenter(">", Offset(clkResNext.Rectangle), Color.White, TextScale);

            // Alt düğmeler.
            DrawTextCenter("SAVE", Offset(clkSave.Rectangle), Color.White, TextScale);
            DrawTextCenter("CANCEL", Offset(clkCancel.Rectangle), Color.White, TextScale);

            spriteBatch.End();
        }

        public override void OnKeyDown(Keys key)
        {
            if (editingName)
            {
                if (key == Keys.Escape || key == Keys.Enter)
                {
                    editingName = false;
                }
                else if (key == Keys.Back)
                {
                    if (handle.Length > 0)
                        handle = handle.Substring(0, handle.Length - 1);
                }
                else if (keyMap.IsKeyMapped(key) && handle.Length < MaxNameLength)
                {
                    KeyboardState ks = Keyboard.GetState();
                    bool shift = ks.IsKeyDown(Keys.LeftShift) || ks.IsKeyDown(Keys.RightShift);
                    handle += keyMap.TranslateKey(key, shift);
                }
            }
            else if (key == Keys.Escape)
            {
                nextState = "Infiniminer.States.ServerBrowserState";
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

            ClickRegion hit = ClickRegion.HitTest(regions, new Point(x, y));
            editingName = false;
            if (hit == null)
                return;

            _P.PlaySound(InfiniminerSound.ClickHigh);

            switch (hit.Tag)
            {
                case "name":
                    editingName = true;
                    break;

                case "fullscreen":
                    fullscreen = !fullscreen;
                    break;

                case "sound":
                    soundOn = !soundOn;
                    break;

                case "voldown":
                    ChangeVolume(-0.1f);
                    break;

                case "volup":
                    ChangeVolume(0.1f);
                    break;

                case "resprev":
                    resIndex = (resIndex - 1 + resolutions.Count) % resolutions.Count;
                    break;

                case "resnext":
                    resIndex = (resIndex + 1) % resolutions.Count;
                    break;

                case "save":
                    Save();
                    break;

                case "cancel":
                    nextState = "Infiniminer.States.ServerBrowserState";
                    break;
            }
        }

        public override void OnMouseUp(MouseButton button, int x, int y)
        {
        }

        public override void OnMouseScroll(int scrollDelta)
        {
        }

        public override void OnMouseMove(int x, int y)
        {
            ScreenToUI(uiEffect, ref x, ref y);
            x -= drawRect.X;
            y -= drawRect.Y;

            hoverRegion = ClickRegion.HitTest(regions, new Point(x, y));
        }

        private void ChangeVolume(float delta)
        {
            float v = (float)Math.Round(volume, 1) + delta;
            volume = (float)Math.Round(MathHelper.Clamp(v, 0f, 1f), 1);
        }

        private void Save()
        {
            string finalHandle = handle.Trim();
            if (finalHandle.Length == 0)
                finalHandle = "Player";

            Point res = resolutions[resIndex];
            (_SM as InfiniminerGame).ApplySettings(finalHandle, fullscreen, !soundOn, volume, res.X, res.Y);
            nextState = "Infiniminer.States.ServerBrowserState";
        }

        // --- Çizim yardımcıları ---

        private Rectangle Offset(Rectangle r)
        {
            r.X += drawRect.X;
            r.Y += drawRect.Y;
            return r;
        }

        private void DrawTextLeft(string text, Rectangle rect)
        {
            Vector2 size = uiFont.MeasureString(text) * TextScale;
            Vector2 pos = new Vector2(rect.X + 15, rect.Y + (rect.Height - size.Y) / 2f);
            DrawString(text, pos, Color.White, TextScale);
        }

        private void DrawTextRight(string text, Rectangle rect)
        {
            Vector2 size = uiFont.MeasureString(text) * TextScale;
            Vector2 pos = new Vector2(rect.Right - 15 - size.X, rect.Y + (rect.Height - size.Y) / 2f);
            DrawString(text, pos, Color.White, TextScale);
        }

        private void DrawTextCenter(string text, Rectangle rect, Color color, float scale)
        {
            Vector2 size = uiFont.MeasureString(text) * scale;
            Vector2 pos = new Vector2(rect.X + (rect.Width - size.X) / 2f, rect.Y + (rect.Height - size.Y) / 2f);
            DrawString(text, pos, color, scale);
        }

        private void DrawString(string text, Vector2 pos, Color color, float scale)
        {
            spriteBatch.DrawString(uiFont, text, new Vector2((int)pos.X, (int)pos.Y), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        // --- Menü koordinat hesapları (ServerBrowserState ile aynı mantık) ---

        private void UpdateUIViewport(Viewport viewport)
        {
            float aspect = viewport.AspectRatio;
            float vWidth = (aspect > VAspect) ? (VHeight * aspect) : VWidth;
            float vHeight = (aspect < VAspect) ? (VWidth / aspect) : VHeight;

            drawRect = new Rectangle((int)vWidth / 2 - VWidth / 2,
                                     (int)vHeight / 2 - VHeight / 2,
                                     VWidth,
                                     VHeight);

            Matrix world = Matrix.CreateScale(1f, -1f, -1f)
                         * Matrix.CreateTranslation(-vWidth / 2f, vHeight / 2f, 0f)
                         * Matrix.CreateScale(1f / vWidth, 1f / vWidth, 1f);

            float fov = MathHelper.ToRadians(70);
            float uiScale = ((float)Math.Tan(fov * 0.5)) * aspect * 2f;
            world *= Matrix.CreateScale(uiScale, uiScale, 1f);
            world *= Matrix.CreateTranslation(0.0f, 0.0f, -1.0f);

            uiEffect.World = world;
            uiEffect.View = Matrix.Identity;
            uiEffect.Projection = Matrix.CreatePerspectiveFieldOfView(fov, aspect, 1f, 1000.0f);
        }

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