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
using StateMasher;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Lidgren.Network;

namespace Infiniminer.States
{
    public class MainGameState : State
    {
        const float MOVESPEED = 3.5f;
        const float GRAVITY = -8.0f;
        const float JUMPVELOCITY = 4.0f;
        const float CLIMBVELOCITY = 2.5f;
        const float DIEVELOCITY = 15.0f;

        string nextState = null;
        bool mouseInitialized = false;
        KeyMap keyMap;

        public override void OnEnter(string oldState)
        {
            _SM.IsMouseVisible = false;

            keyMap = new KeyMap();
        }

        public override void OnLeave(string newState)
        {
            _P.chatEntryBuffer = "";
            _P.chatMode = ChatMessageType.None;
        }

        public override string OnUpdate(GameTime gameTime, KeyboardState keyState, MouseState mouseState)
        {
            ///////////////////////////////////////////////////////////////////
            /// Update Network stuff
            ///////////////////////////////////////////////////////////////////
            (_SM as InfiniminerGame).UpdateNetwork(gameTime);

            ///////////////////////////////////////////////////////////////////
            /// Update the current screen effect.
            ///////////////////////////////////////////////////////////////////
            _P.screenEffectCounter += gameTime.ElapsedGameTime.TotalSeconds;

            ///////////////////////////////////////////////////////////////////
            /// Update engines
            ///////////////////////////////////////////////////////////////////
            _P.skyplaneEngine.Update(gameTime);
            _P.playerEngine.Update(gameTime);
            _P.interfaceEngine.Update(gameTime);
            _P.particleEngine.Update(gameTime);
            _P.inputEngine.Update(gameTime);

            ///////////////////////////////////////////////////////////////////
            /// Count down the tool cool down
            ///////////////////////////////////////////////////////////////////
            if (_P.playerToolCooldown > 0)
            {
                _P.playerToolCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_P.playerToolCooldown <= 0)
                    _P.playerToolCooldown = 0;
            }

            ///////////////////////////////////////////////////////////////////
            /// Update the camera
            ///     Only update if the window has focus
            ///     Unlock mouse with Ctrl key
            ///////////////////////////////////////////////////////////////////
            bool isCtrlDown = InputManager.Keyboard.CurrentState.IsKeyDown(Keys.LeftControl) || InputManager.Keyboard.CurrentState.IsKeyDown(Keys.RightControl);
            if (!isCtrlDown)
            {
                if (_SM.WindowHasFocus())
                {
                    Rectangle clientBounds = _SM.Window.ClientBounds;
                    if (mouseInitialized)
                    {
                        //  Because the mouse is clamped to the center of the screen in
                        //  a moment, we have to determine if the input type is from
                        //  keyboard/mouse or if it's from gamepad and calculate the
                        //  delta differently based on which
                        float dx;
                        float dy;
                        if (_P.inputEngine.ControlType == ControlType.KeyboardMouse)
                        {
                            dx = InputManager.Mouse.X - clientBounds.Width / 2;
                            dy = InputManager.Mouse.Y - clientBounds.Height / 2;
                        }
                        else
                        {
                            dx = _P.inputEngine.Camera.Value.X * 5.0f;
                            dy = _P.inputEngine.Camera.Value.Y * 5.0f;
                        }

                        if ((_SM as InfiniminerGame).InvertMouseYAxis)
                            dy = -dy;

                        _P.playerCamera.Yaw -= dx * 0.005f;
                        _P.playerCamera.Pitch = (float)Math.Min(Math.PI * 0.49, Math.Max(-Math.PI * 0.49, _P.playerCamera.Pitch - dy * 0.005f));
                    }
                    else
                    {
                        Rectangle windowBounds = new Rectangle(0, 0, clientBounds.Width, clientBounds.Height);
                        if (windowBounds.Contains(InputManager.Mouse.X, InputManager.Mouse.Y) && InputManager.Mouse.LeftButtonPressed())
                            mouseInitialized = true;
                    }
                    if (mouseInitialized)
                        Mouse.SetPosition(clientBounds.Width / 2, clientBounds.Height / 2);
                }
                else
                    mouseInitialized = false;
            }
            _SM.IsMouseVisible = !mouseInitialized || isCtrlDown;

            ///////////////////////////////////////////////////////////////////
            /// Update Player
            ///////////////////////////////////////////////////////////////////
            if (!_P.playerDead)
            {
                if (_P.chatMode == ChatMessageType.None)
                {
                    ///////////////////////////////////////////////////////////////////
                    /// Use tool if player can
                    ///////////////////////////////////////////////////////////////////
                    if (_P.playerToolCooldown == 0 && _P.inputEngine.UseTool.Check())
                    {
                        switch (_P.playerTools[_P.playerToolSelected])
                        {
                            case PlayerTools.ConstructionGun:
                                _P.FireConstructionGun(_P.playerBlocks[_P.playerBlockSelected]);
                                break;
                            case PlayerTools.DeconstructionGun:
                                _P.FireDeconstructionGun();
                                break;
                            case PlayerTools.Detonator:
                                _P.PlaySound(InfiniminerSound.ClickHigh);
                                _P.FireDetonator();
                                break;
                            case PlayerTools.Pickaxe:
                                _P.FirePickaxe();
                                _P.playerToolCooldown = _P.playerClass == PlayerClass.Miner
                                                         ? _P.GetToolCooldown(PlayerTools.Pickaxe) * 0.4f
                                                         : _P.GetToolCooldown(PlayerTools.Pickaxe);
                                break;
                            case PlayerTools.ProspectingRadar:
                                _P.FireRadar();
                                break;
                        }


                        // Prospector radar stuff.
                        if (_P.playerTools[_P.playerToolSelected] == PlayerTools.ProspectingRadar)
                        {
                            float oldValue = _P.radarValue;
                            _P.ReadRadar(ref _P.radarDistance, ref _P.radarValue);
                            if (_P.radarValue != oldValue)
                            {
                                if (_P.radarValue == 200)
                                    _P.PlaySound(InfiniminerSound.RadarLow);
                                if (_P.radarValue == 1000)
                                    _P.PlaySound(InfiniminerSound.RadarHigh);
                            }
                        }
                    }

                    ///////////////////////////////////////////////////////////////////
                    /// Check if player jumped
                    ///////////////////////////////////////////////////////////////////
                    if (_P.inputEngine.Jump.Pressed())
                    {
                        Vector3 footPosition = _P.playerPosition + new Vector3(0f, -1.5f, 0f);
                        if (_P.blockEngine.SolidAtPointForPlayer(footPosition) && _P.playerVelocity.Y == 0)
                        {
                            _P.playerVelocity.Y = JUMPVELOCITY;
                            float amountBelowSurface = ((ushort)footPosition.Y) + 1 - footPosition.Y;
                            _P.playerPosition.Y += amountBelowSurface + 0.01f;
                        }
                    }

                    ///////////////////////////////////////////////////////////////////
                    /// Check if tool should change
                    ///////////////////////////////////////////////////////////////////
                    int changeTo = -1;
                    if (_P.inputEngine.ToolHotkey1.Pressed() && _P.playerTools.Length > 0)
                    {
                        changeTo = 0;
                    }
                    else if (_P.inputEngine.ToolHotkey2.Pressed() && _P.playerTools.Length > 1)
                    {
                        changeTo = 1;
                    }
                    else if (_P.inputEngine.ToolHotkey3.Pressed() && _P.playerTools.Length > 2)
                    {
                        changeTo = 2;
                    }
                    else if (_P.inputEngine.ChangeTool.Pressed())
                    {
                        changeTo = _P.playerToolSelected + 1;
                    }

                    if (changeTo >= 0 && changeTo != _P.playerToolSelected)
                    {
                        _P.playerToolSelected = changeTo;

                        _P.PlaySound(InfiniminerSound.ClickLow);
                        if (_P.playerToolSelected >= _P.playerTools.Length)
                        {
                            _P.playerToolSelected = 0;
                        }
                    }

                    ///////////////////////////////////////////////////////////////////
                    /// Check if the player is using the construction gun and if they
                    /// want to switch block types
                    ///////////////////////////////////////////////////////////////////
                    if (_P.playerTools[_P.playerToolSelected] == PlayerTools.ConstructionGun && _P.inputEngine.ChangeBlockType.Pressed())
                    {
                        _P.PlaySound(InfiniminerSound.ClickLow);
                        _P.playerBlockSelected += 1;
                        if (_P.playerBlockSelected >= _P.playerBlocks.Length)
                        {
                            _P.playerBlockSelected = 0;
                        }
                    }

                    ///////////////////////////////////////////////////////////////////
                    /// Check if player performed a ping
                    ///////////////////////////////////////////////////////////////////
                    if (_P.inputEngine.PingTeam.Pressed())
                    {
                        NetBuffer msgBuffer = _P.netClient.CreateBuffer();
                        msgBuffer.Write((byte)InfiniminerMessage.PlayerPing);
                        msgBuffer.Write(_P.playerMyId);
                        _P.netClient.SendMessage(msgBuffer, NetChannel.ReliableUnordered);
                    }

                    ///////////////////////////////////////////////////////////////////
                    /// Check if player is depositing or withdrawing from a bank
                    ///////////////////////////////////////////////////////////////////
                    if (_P.AtBankTerminal())
                    {
                        if (_P.inputEngine.DepositOre.Pressed())
                        {
                            _P.DepositOre();
                            _P.PlaySound(InfiniminerSound.ClickHigh);
                        }

                        if (_P.inputEngine.WithdrawOre.Pressed())
                        {
                            _P.WithdrawOre();
                            _P.PlaySound(InfiniminerSound.ClickHigh);
                        }
                    }

                    ///////////////////////////////////////////////////////////////////
                    /// Check if player wants to change class
                    /// (Keyboard only, gamepad check is during options section below)
                    ///////////////////////////////////////////////////////////////////
                    if (_P.inputEngine.ChangeClass.Pressed() && _P.inputEngine.ControlType == ControlType.KeyboardMouse)
                    {
                        nextState = "Infiniminer.States.ClassSelectionState";
                    }

                    ///////////////////////////////////////////////////////////////////
                    /// Check if player wants to change team
                    /// (Keyboard only, gamepad check is during options section below)
                    ///////////////////////////////////////////////////////////////////
                    if (_P.inputEngine.ChangeTeam.Pressed() && _P.inputEngine.ControlType == ControlType.KeyboardMouse)
                    {
                        nextState = "Infiniminer.States.TeamSelectionState";
                    }

                    ///////////////////////////////////////////////////////////////////
                    /// Check if player wants to enter a chat mode
                    ///////////////////////////////////////////////////////////////////
                    if (_P.inputEngine.SayToAll.Pressed())
                    {
                        _P.chatMode = ChatMessageType.SayAll;
                    }

                    if (_P.inputEngine.SayToTeam.Pressed())
                    {
                        _P.chatMode = _P.playerTeam == PlayerTeam.Red ? ChatMessageType.SayRedTeam : ChatMessageType.SayBlueTeam;
                    }

                    ///////////////////////////////////////////////////////////////////
                    /// Check if player want to quit match or commit pixelcide
                    ///////////////////////////////////////////////////////////////////
                    if (_P.inputEngine.ShowOptionsButton.Check())
                    {
                        if (_P.inputEngine.QuitButton.Pressed())
                        {
                            _P.netClient.Disconnect("Client disconnected.");
                            nextState = "Infiniminer.States.ServerBrowserState";
                        }

                        if (_P.inputEngine.PixelcideButton.Pressed())
                        {
                            _P.KillPlayer("HAS COMMITTED PIXELCIDE!");
                        }

                        if (_P.inputEngine.ChangeTeam.Pressed())
                        {
                            nextState = "Infiniminer.States.TeamSelectionState";
                        }

                        if (_P.inputEngine.ChangeClass.Pressed())
                        {
                            nextState = "Infiniminer.States.ClassSelectionState";
                        }
                    }



                    ///////////////////////////////////////////////////////////////////
                    /// Update the players position
                    ///////////////////////////////////////////////////////////////////
                    UpdatePlayerPosition(gameTime, keyState);
                }
                else
                {
                    ///////////////////////////////////////////////////////////////////
                    /// We're in chat mode, so all input should be directed for chat
                    /// and not player actions
                    ///////////////////////////////////////////////////////////////////
                    // Put the characters in the chat buffer.
                    if (InputManager.Keyboard.Pressed(Keys.Enter))
                    {
                        // If we have an actual message to send, fire it off at the server.
                        if (_P.chatEntryBuffer.Length > 0)
                        {
                            NetBuffer msgBuffer = _P.netClient.CreateBuffer();
                            msgBuffer.Write((byte)InfiniminerMessage.ChatMessage);
                            msgBuffer.Write((byte)_P.chatMode);
                            msgBuffer.Write(_P.chatEntryBuffer);
                            _P.netClient.SendMessage(msgBuffer, NetChannel.ReliableInOrder3);
                        }

                        _P.chatEntryBuffer = "";
                        _P.chatMode = ChatMessageType.None;
                    }
                    else if (InputManager.Keyboard.Pressed(Keys.Back))
                    {
                        if (_P.chatEntryBuffer.Length > 0)
                            _P.chatEntryBuffer = _P.chatEntryBuffer.Substring(0, _P.chatEntryBuffer.Length - 1);
                    }
                    else if (InputManager.Keyboard.Pressed(Keys.Escape))
                    {
                        //  TODO: Need to change Pressed to Check and add buffer timer
                        _P.chatEntryBuffer = "";
                        _P.chatMode = ChatMessageType.None;
                    }
                    else if (InputManager.Keyboard.AnyKeyPressed)
                    {
                        if (InputManager.Keyboard.AnyKeyCheck)
                        {
                            // TODO:  Typing too fast causes missed keys, need to fix
                            //  Take first key only
                            Keys key = InputManager.Keyboard.CurrentState.GetPressedKeys()[0];
                            _P.chatEntryBuffer += keyMap.TranslateKey(key, InputManager.Keyboard.Check(Keys.LeftShift) || InputManager.Keyboard.Check(Keys.RightShift));
                        }
                    }
                }
            }
            else
            {
                ///////////////////////////////////////////////////////////////////
                /// Player is dead, check for respawn
                ///////////////////////////////////////////////////////////////////
                if (_P.screenEffectCounter > 2 && _P.inputEngine.UseTool.Pressed())
                {
                    _P.inputEngine.UseTool.ConsumePress();
                    _P.RespawnPlayer();
                }
            }

            ///////////////////////////////////////////////////////////////////
            /// Update the camera regardless of if we"re alive or not.
            ///////////////////////////////////////////////////////////////////
            _P.UpdateCamera(gameTime);

            return nextState;
        }

        private void UpdatePlayerPosition(GameTime gameTime, KeyboardState keyState)
        {
            // Double-speed move flag, set if we're on road.
            bool movingOnRoad = false;

            // Apply "gravity".
            _P.playerVelocity.Y += GRAVITY * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector3 footPosition = _P.playerPosition + new Vector3(0f, -1.5f, 0f);
            Vector3 headPosition = _P.playerPosition + new Vector3(0f, 0.1f, 0f);
            if (_P.blockEngine.SolidAtPointForPlayer(footPosition) || _P.blockEngine.SolidAtPointForPlayer(headPosition))
            {
                BlockType standingOnBlock = _P.blockEngine.BlockAtPoint(footPosition);
                BlockType hittingHeadOnBlock = _P.blockEngine.BlockAtPoint(headPosition);

                // If we"re hitting the ground with a high velocity, die!
                if (standingOnBlock != BlockType.None && _P.playerVelocity.Y < 0)
                {
                    float fallDamage = Math.Abs(_P.playerVelocity.Y) / DIEVELOCITY;
                    if (fallDamage >= 1)
                    {
                        _P.PlaySoundForEveryone(InfiniminerSound.GroundHit, _P.playerPosition);
                        _P.KillPlayer("WAS KILLED BY GRAVITY!");
                        return;
                    }
                    else if (fallDamage > 0.5)
                    {
                        // Fall damage of 0.5 maps to a screenEffectCounter value of 2, meaning that the effect doesn"t appear.
                        // Fall damage of 1.0 maps to a screenEffectCounter value of 0, making the effect very strong.
                        _P.screenEffect = ScreenEffect.Fall;
                        _P.screenEffectCounter = 2 - (fallDamage - 0.5) * 4;
                        _P.PlaySoundForEveryone(InfiniminerSound.GroundHit, _P.playerPosition);
                    }
                }

                // If the player has their head stuck in a block, push them down.
                if (_P.blockEngine.SolidAtPointForPlayer(headPosition))
                {
                    int blockIn = (int)(headPosition.Y);
                    _P.playerPosition.Y = (float)(blockIn - 0.15f);
                }

                // If the player is stuck in the ground, bring them out.
                // This happens because we"re standing on a block at -1.5, but stuck in it at -1.4, so -1.45 is the sweet spot.
                if (_P.blockEngine.SolidAtPointForPlayer(footPosition))
                {
                    int blockOn = (int)(footPosition.Y);
                    _P.playerPosition.Y = (float)(blockOn + 1 + 1.45);
                }

                _P.playerVelocity.Y = 0;

                // Logic for standing on a block.
                switch (standingOnBlock)
                {
                    case BlockType.Jump:
                        _P.playerVelocity.Y = 2.5f * JUMPVELOCITY;
                        _P.PlaySoundForEveryone(InfiniminerSound.Jumpblock, _P.playerPosition);
                        break;

                    case BlockType.Road:
                        movingOnRoad = true;
                        break;

                    //case BlockType.Teleporter:
                    //    if (!_P.playerDead)
                    //    {
                    //        _P.Teleport();
                    //        _P.PlaySoundForEveryone(InfiniminerSound.Teleporter, _P.playerPosition);
                    //    }
                    //    break;

                    //case BlockType.Shock:
                    //    _P.KillPlayer("WAS ELECTROCUTED!");
                    //    return;

                    case BlockType.Lava:
                        _P.KillPlayer("WAS INCINERATED BY LAVA!");
                        return;
                }

                // Logic for bumping your head on a block.
                switch (hittingHeadOnBlock)
                {
                    case BlockType.Shock:
                        _P.KillPlayer("WAS ELECTROCUTED!");
                        return;

                    case BlockType.Lava:
                        _P.KillPlayer("WAS INCINERATED BY LAVA!");
                        return;
                }
            }
            _P.playerPosition += _P.playerVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Death by falling off the map.
            if (_P.playerPosition.Y < -30)
            {
                _P.KillPlayer("WAS KILLED BY MISADVENTURE!");
                return;
            }

            // Pressing forward moves us in the direction we"re looking.
            Vector3 moveVector = Vector3.Zero;

            if (_P.chatMode == ChatMessageType.None)
            {
                Vector2 direction = _P.inputEngine.Move.Value;
                if (direction.Y < 0)
                    moveVector += _P.playerCamera.GetLookVector();
                if (direction.Y > 0)
                    moveVector -= _P.playerCamera.GetLookVector();
                if (direction.X > 0)
                    moveVector += _P.playerCamera.GetRightVector();
                if (direction.X < 0)
                    moveVector -= _P.playerCamera.GetRightVector();
            }

            if (moveVector.X != 0 || moveVector.Z != 0)
            {
                // "Flatten" the movement vector so that we don"t move up/down.
                moveVector.Y = 0;
                moveVector.Normalize();

                //  Check if sprinting
                if (_P.inputEngine.Sprint.Check())
                {
                    moveVector *= 1.5f;
                }

                moveVector *= MOVESPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (movingOnRoad)
                    moveVector *= 2;

                // Attempt to move, doing collision stuff.
                if (TryToMoveTo(moveVector, gameTime)) { }
                else if (!TryToMoveTo(new Vector3(0, 0, moveVector.Z), gameTime)) { }
                else if (!TryToMoveTo(new Vector3(moveVector.X, 0, 0), gameTime)) { }
            }
        }

        private bool TryToMoveTo(Vector3 moveVector, GameTime gameTime)
        {
            // Build a "test vector" that is a little longer than the move vector.
            float moveLength = moveVector.Length();
            Vector3 testVector = moveVector;
            testVector.Normalize();
            testVector = testVector * (moveLength + 0.1f);

            // Apply this test vector.
            Vector3 movePosition = _P.playerPosition + testVector;
            Vector3 midBodyPoint = movePosition + new Vector3(0, -0.7f, 0);
            Vector3 lowerBodyPoint = movePosition + new Vector3(0, -1.4f, 0);

            if (!_P.blockEngine.SolidAtPointForPlayer(movePosition) && !_P.blockEngine.SolidAtPointForPlayer(lowerBodyPoint) && !_P.blockEngine.SolidAtPointForPlayer(midBodyPoint))
            {
                _P.playerPosition = _P.playerPosition + moveVector;
                return true;
            }

            // It"s solid there, so while we can"t move we have officially collided with it.
            BlockType lowerBlock = _P.blockEngine.BlockAtPoint(lowerBodyPoint);
            BlockType midBlock = _P.blockEngine.BlockAtPoint(midBodyPoint);
            BlockType upperBlock = _P.blockEngine.BlockAtPoint(movePosition);

            //// It"s solid there, so see if it"s a spike block. If so, touching it will kill us!
            //if (upperBlock == BlockType.Shock || lowerBlock == BlockType.Shock || midBlock == BlockType.Shock)
            //{
            //    _P.KillPlayer("WAS ELECTROCUTED!");
            //    return true;
            //}

            // It"s solid there, so see if it"s a lava block. If so, touching it will kill us!
            if (upperBlock == BlockType.Lava || lowerBlock == BlockType.Lava || midBlock == BlockType.Lava)
            {
                _P.KillPlayer("WAS INCINERATED BY LAVA!");
                return true;
            }

            //// If it"s our home block, deposit our money.
            //if ((upperBlock == BlockType.HomeRed || lowerBlock == BlockType.HomeRed || midBlock == BlockType.HomeRed) && _P.playerTeam == PlayerTeam.Red)
            //    _P.DepositLoot();
            //if ((upperBlock == BlockType.HomeBlue || lowerBlock == BlockType.HomeBlue || midBlock == BlockType.HomeBlue) && _P.playerTeam == PlayerTeam.Blue)
            //    _P.DepositLoot();

            // If it"s a ladder, move up.
            if (upperBlock == BlockType.Ladder || lowerBlock == BlockType.Ladder || midBlock == BlockType.Ladder)
            {
                _P.playerVelocity.Y = CLIMBVELOCITY;
                Vector3 footPosition = _P.playerPosition + new Vector3(0f, -1.5f, 0f);
                if (_P.blockEngine.SolidAtPointForPlayer(footPosition))
                    _P.playerPosition.Y += 0.1f;
                return true;
            }

            return false;
        }

        public override void OnRenderAtUpdate(GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            // Set posteffects target.
            if (_P.blockEngine.bloomPosteffect != null)
                _P.blockEngine.bloomPosteffect.SetRenderTarget(graphicsDevice);

            _P.skyplaneEngine.Render(graphicsDevice);
            _P.particleEngine.Render(graphicsDevice);
            _P.playerEngine.Render(graphicsDevice);
            _P.blockEngine.Render(graphicsDevice, gameTime);
            
            // Apply posteffects.
            if (_P.blockEngine.bloomPosteffect != null)
                _P.blockEngine.bloomPosteffect.Draw(graphicsDevice);

            _P.playerEngine.RenderPlayerNames(graphicsDevice);
            _P.interfaceEngine.Render(graphicsDevice);

            _SM.Window.Title = "Infiniminer";
        }

        public override void OnKeyDown(Keys key)
        {
            if (_P.chatMode != ChatMessageType.None)
            {

                return;
            }
        }

        public override void OnMouseScroll(int scrollDelta)
        {
            if (_P.playerDead)
                return;

            if (scrollDelta == 120 && _P.playerTools[_P.playerToolSelected] == PlayerTools.ConstructionGun)
            {
                _P.PlaySound(InfiniminerSound.ClickLow);
                _P.playerBlockSelected += 1;
                if (_P.playerBlockSelected >= _P.playerBlocks.Length)
                    _P.playerBlockSelected = 0;
            }

            if (scrollDelta == -120 && _P.playerTools[_P.playerToolSelected] == PlayerTools.ConstructionGun)
            {
                _P.PlaySound(InfiniminerSound.ClickLow);
                _P.playerBlockSelected -= 1;
                if (_P.playerBlockSelected < 0)
                    _P.playerBlockSelected = _P.playerBlocks.Length - 1;
            }
        }
    }
}
