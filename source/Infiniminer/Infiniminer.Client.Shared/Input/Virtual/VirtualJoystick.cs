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
using Infiniminer.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Infiniminer;

public sealed class VirtualJoystick : VirtualInput
{
    public List<Node> Nodes { get; private set; }

    public bool IsSnapped { get; set; }
    public bool IsNormalized { get; set; }

    public Vector2 Value { get; private set; }
    public Vector2 PreviousValue { get; private set; }
    public Vector2 Delta => Value - PreviousValue;

    public VirtualJoystick() : this(false, false) { }
    public VirtualJoystick(bool snapped, bool normalized) : base()
    {
        Nodes = new List<Node>();
        IsSnapped = snapped;
        IsNormalized = normalized;
    }

    public override void Update()
    {
        for (int i = 0; i < Nodes.Count; i++)
        {
            Nodes[i].Update();
        }

        PreviousValue = Value;
        Value = Vector2.Zero;

        for (int i = 0; i < Nodes.Count; i++)
        {
            Vector2 newValue = Nodes[i].Value;

            if (newValue != Vector2.Zero)
            {
                if (IsNormalized)
                {
                    if (IsSnapped)
                    {
                        newValue = SnapNormal(newValue, 8);
                    }
                    else
                    {
                        newValue.Normalize();
                    }
                }
                else if (IsSnapped)
                {
                    newValue = Snap(newValue, 8);
                }

                Value = newValue;
                break;
            }
        }
    }

    private Vector2 SnapNormal(Vector2 vector, float slices)
    {
        float dividers = MathHelper.TwoPi / slices;
        float angle = (float)Math.Atan2(vector.Y, vector.X);
        float snappedAngle = (float)Math.Floor((angle + dividers / 2.0f) / dividers) * dividers;
        float x = (float)Math.Cos(snappedAngle) * vector.Length();
        float y = (float)Math.Sin(snappedAngle) * vector.Length();
        return new Vector2(x, y);
    }

    private Vector2 Snap(Vector2 vector, float slices)
    {
        float dividers = MathHelper.TwoPi / slices;
        float angle = (float)Math.Atan2(vector.Y, vector.X);
        float snappedAngle = (float)Math.Floor((angle + dividers / 2.0f) / dividers) * dividers;
        float x = (float)Math.Cos(snappedAngle) * vector.Length();
        float y = (float)Math.Sin(snappedAngle) * vector.Length();
        return new Vector2(x, y);
    }

    ///////////////////////////////////////////////////////////////////////////
    /// OverlapBehavior
    ///////////////////////////////////////////////////////////////////////////
    #region Overlap Behavior

    public enum OverlapBehavior
    {
        Cancel,
        Positive,
        Negative
    }

    #endregion Overlap Behavior

    ///////////////////////////////////////////////////////////////////////////
    /// Node
    ///////////////////////////////////////////////////////////////////////////
    #region Node

    public abstract class Node : VirtualNode
    {
        public abstract Vector2 Value { get; }
    }

    #endregion Node

    ///////////////////////////////////////////////////////////////////////////
    /// GamePad
    ///////////////////////////////////////////////////////////////////////////
    #region GamePad

    public sealed class GamePad
    {

        ///////////////////////////////////////////////////////////////////////////
        /// Button
        ///////////////////////////////////////////////////////////////////////////
        #region Button

        public sealed class Button : Node
        {
            private Vector2 _value;
            public override Vector2 Value => _value;

            public OverlapBehavior OverlapBehavior { get; set; }

            public Buttons Up { get; set; }
            public Buttons Down { get; set; }
            public Buttons Left { get; set; }
            public Buttons Right { get; set; }

            public Button(Microsoft.Xna.Framework.Input.Buttons up,
                         Microsoft.Xna.Framework.Input.Buttons down,
                         Microsoft.Xna.Framework.Input.Buttons left,
                         Microsoft.Xna.Framework.Input.Buttons right,
                         OverlapBehavior behavior)
            {
                OverlapBehavior = behavior;
                Up = up;
                Down = down;
                Left = left;
                Right = right;
            }

            public override void Update()
            {
                bool isUp = InputManager.GamePad.ButtonCheck(Up);
                bool isDown = InputManager.GamePad.ButtonCheck(Down);
                bool isLeft = InputManager.GamePad.ButtonCheck(Left);
                bool isRight = InputManager.GamePad.ButtonCheck(Right);

                if (isUp)
                {
                    if (isDown)
                    {
                        //  Both Up and Down are pressed so the value is determiend
                        //  by the overlap behavior.
                        switch (OverlapBehavior)
                        {
                            default:
                            case OverlapBehavior.Cancel:
                                _value.Y = 0;
                                break;
                            case OverlapBehavior.Positive:
                                _value.Y = 1;
                                break;
                            case OverlapBehavior.Negative:
                                _value.Y = -1;
                                break;
                        }
                    }
                    else
                    {
                        _value.Y = 1;
                    }
                }
                else if (isDown)
                {
                    _value.Y = -1;
                }
                else
                {
                    _value.Y = 0;
                }


                if (isLeft)
                {
                    if (isRight)
                    {
                        //  Both Left and Right are pressed so the value is determiend
                        //  by the overlap behavior.
                        switch (OverlapBehavior)
                        {
                            default:
                            case OverlapBehavior.Cancel:
                                _value.X = 0;
                                break;
                            case OverlapBehavior.Positive:
                                _value.X = 1;
                                break;
                            case OverlapBehavior.Negative:
                                _value.X = -1;
                                break;

                        }
                    }
                    else
                    {
                        _value.X = -1;
                    }
                }
                else if (isRight)
                {
                    _value.X = 1;
                }
                else
                {
                    _value.X = 0;
                }
            }
        }

        #endregion Button

        ///////////////////////////////////////////////////////////////////////////
        /// Left Stick
        ///////////////////////////////////////////////////////////////////////////
        #region Left Stick

        public sealed class LeftStick : Node
        {
            public Vector2 Deadzone { get; set; }
            public bool UseGlobalDeadzone { get; set; }

            public override Vector2 Value => UseGlobalDeadzone ? InputManager.GamePad.LeftStick : InputManager.GamePad.GetLeftStickWithDeadzone(Deadzone);

            public LeftStick()
            {
                Deadzone = Vector2.Zero;
                UseGlobalDeadzone = true;
            }

            public LeftStick(float deadzone)
                : this(new Vector2(deadzone, deadzone)) { }

            public LeftStick(Vector2 deadzone)
            {
                Deadzone = deadzone;
                UseGlobalDeadzone = false;
            }
        }

        #endregion Left Stick

        ///////////////////////////////////////////////////////////////////////////
        /// Right Stick
        ///////////////////////////////////////////////////////////////////////////
        #region Right Stick

        public sealed class RightStick : Node
        {
            public Vector2 Deadzone { get; set; }
            public bool UseGlobalDeadzone { get; set; }

            public override Vector2 Value => UseGlobalDeadzone ? InputManager.GamePad.RightStick : InputManager.GamePad.GetRightStickWithDeadzone(Deadzone);

            public RightStick()
            {
                Deadzone = Vector2.Zero;
                UseGlobalDeadzone = true;
            }

            public RightStick(float deadzone)
                : this(new Vector2(deadzone, deadzone)) { }

            public RightStick(Vector2 deadzone)
            {
                Deadzone = deadzone;
                UseGlobalDeadzone = false;
            }
        }

        #endregion Right Stick
    }

    #endregion GamePad

    ///////////////////////////////////////////////////////////////////////////
    /// Keyboard
    ///////////////////////////////////////////////////////////////////////////             
    #region Keyboard

    public sealed class Keyboard
    {
        public sealed class Keys : Node
        {
            private Vector2 _value;
            public override Vector2 Value => _value;

            public OverlapBehavior OverlapBehavior { get; set; }

            public Microsoft.Xna.Framework.Input.Keys Up { get; set; }
            public Microsoft.Xna.Framework.Input.Keys Down { get; set; }
            public Microsoft.Xna.Framework.Input.Keys Left { get; set; }
            public Microsoft.Xna.Framework.Input.Keys Right { get; set; }

            public Keys(Microsoft.Xna.Framework.Input.Keys up,
                        Microsoft.Xna.Framework.Input.Keys down,
                        Microsoft.Xna.Framework.Input.Keys left,
                        Microsoft.Xna.Framework.Input.Keys right,
                        OverlapBehavior behavior)
            {
                OverlapBehavior = behavior;
                Up = up;
                Down = down;
                Left = left;
                Right = right;
            }

            public override void Update()
            {
                bool isUp = InputManager.Keyboard.Check(Up);
                bool isDown = InputManager.Keyboard.Check(Down);
                bool isLeft = InputManager.Keyboard.Check(Left);
                bool isRight = InputManager.Keyboard.Check(Right);

                Console.WriteLine($"{isUp}, {isDown}, {isLeft}, {isRight}");

                if (isUp)
                {
                    if (isDown)
                    {
                        //  Both Up and Down are pressed so the value is determiend
                        //  by the overlap behavior.
                        switch (OverlapBehavior)
                        {
                            default:
                            case OverlapBehavior.Cancel:
                                _value.Y = 0;
                                break;
                            case OverlapBehavior.Positive:
                                _value.Y = -1;
                                break;
                            case OverlapBehavior.Negative:
                                _value.Y = 1;
                                break;
                        }
                    }
                    else
                    {
                        _value.Y = -1;
                    }
                }
                else if (isDown)
                {
                    _value.Y = 1;
                }
                else
                {
                    _value.Y = 0;
                }


                if (isLeft)
                {
                    if (isRight)
                    {
                        //  Both Left and Right are pressed so the value is determiend
                        //  by the overlap behavior.
                        switch (OverlapBehavior)
                        {
                            default:
                            case OverlapBehavior.Cancel:
                                _value.X = 0;
                                break;
                            case OverlapBehavior.Positive:
                                _value.X = 1;
                                break;
                            case OverlapBehavior.Negative:
                                _value.X = -1;
                                break;

                        }
                    }
                    else
                    {
                        _value.X = -1;
                    }
                }
                else if (isRight)
                {
                    _value.X = 1;
                }
                else
                {
                    _value.X = 0;
                }
            }
        }
    }

    #endregion Keyboard


    ///////////////////////////////////////////////////////////////////////////
    /// Mouse
    ///////////////////////////////////////////////////////////////////////////             
    #region Mouse
    public sealed class Mouse
    {
        public sealed class Axis : Node
        {
            private Vector2 _value;
            public override Vector2 Value => _value;

            public override void Update()
            {
                _value.X = -Math.Sign(InputManager.Mouse.DeltaX);
                _value.Y = -Math.Sign(InputManager.Mouse.DeltaY);
            }
        }
    }
    #endregion Mouse


    ///////////////////////////////////////////////////////////////////////////
    /// TouchController
    ///////////////////////////////////////////////////////////////////////////
    #region TouchController
#if KNI
    public sealed class TouchController
    {

        ///////////////////////////////////////////////////////////////////////////
        /// Button
        ///////////////////////////////////////////////////////////////////////////
        #region Button

        public sealed class Button : Node
        {
            private Vector2 _value;
            public override Vector2 Value => _value;

            public OverlapBehavior OverlapBehavior { get; set; }

            public Buttons Up { get; set; }
            public Buttons Down { get; set; }
            public Buttons Left { get; set; }
            public Buttons Right { get; set; }

            public Button(Microsoft.Xna.Framework.Input.Buttons up,
                          Microsoft.Xna.Framework.Input.Buttons down,
                          Microsoft.Xna.Framework.Input.Buttons left,
                          Microsoft.Xna.Framework.Input.Buttons right,
                          OverlapBehavior behavior)
            {
                OverlapBehavior = behavior;
                Up = up;
                Down = down;
                Left = left;
                Right = right;
            }

            public override void Update()
            {
                bool isUp = InputManager.TouchController.ButtonCheck(Up);
                bool isDown = InputManager.TouchController.ButtonCheck(Down);
                bool isLeft = InputManager.TouchController.ButtonCheck(Left);
                bool isRight = InputManager.TouchController.ButtonCheck(Right);

                if (isUp)
                {
                    if (isDown)
                    {
                        //  Both Up and Down are pressed so the value is determiend
                        //  by the overlap behavior.
                        switch (OverlapBehavior)
                        {
                            default:
                            case OverlapBehavior.Cancel:
                                _value.Y = 0;
                                break;
                            case OverlapBehavior.Positive:
                                _value.Y = 1;
                                break;
                            case OverlapBehavior.Negative:
                                _value.Y = -1;
                                break;
                        }
                    }
                    else
                    {
                        _value.Y = 1;
                    }
                }
                else if (isDown)
                {
                    _value.Y = -1;
                }
                else
                {
                    _value.Y = 0;
                }


                if (isLeft)
                {
                    if (isRight)
                    {
                        //  Both Left and Right are pressed so the value is determiend
                        //  by the overlap behavior.
                        switch (OverlapBehavior)
                        {
                            default:
                            case OverlapBehavior.Cancel:
                                _value.X = 0;
                                break;
                            case OverlapBehavior.Positive:
                                _value.X = 1;
                                break;
                            case OverlapBehavior.Negative:
                                _value.X = -1;
                                break;

                        }
                    }
                    else
                    {
                        _value.X = -1;
                    }
                }
                else if (isRight)
                {
                    _value.X = 1;
                }
                else
                {
                    _value.X = 0;
                }
            }
        }

        #endregion Button

        ///////////////////////////////////////////////////////////////////////////
        /// Left Stick
        ///////////////////////////////////////////////////////////////////////////
        #region Left Stick

        public sealed class LeftStick : Node
        {
            public Vector2 Deadzone { get; set; }
            public bool UseGlobalDeadzone { get; set; }

            public override Vector2 Value => UseGlobalDeadzone ? InputManager.TouchController.LeftStick : InputManager.TouchController.GetLeftStickWithDeadzone(Deadzone);

            public LeftStick()
            {
                Deadzone = Vector2.Zero;
                UseGlobalDeadzone = true;
            }

            public LeftStick(float deadzone)
                : this(new Vector2(deadzone, deadzone)) { }

            public LeftStick(Vector2 deadzone)
            {
                Deadzone = deadzone;
                UseGlobalDeadzone = false;
            }
        }

        #endregion Left Stick

        ///////////////////////////////////////////////////////////////////////////
        /// Right Stick
        ///////////////////////////////////////////////////////////////////////////
        #region Right Stick

        public sealed class RightStick : Node
        {
            public Vector2 Deadzone { get; set; }
            public bool UseGlobalDeadzone { get; set; }

            public override Vector2 Value => UseGlobalDeadzone ? InputManager.TouchController.RightStick : InputManager.TouchController.GetRightStickWithDeadzone(Deadzone);

            public RightStick()
            {
                Deadzone = Vector2.Zero;
                UseGlobalDeadzone = true;
            }

            public RightStick(float deadzone)
                : this(new Vector2(deadzone, deadzone)) { }

            public RightStick(Vector2 deadzone)
            {
                Deadzone = deadzone;
                UseGlobalDeadzone = false;
            }
        }

        #endregion Right Stick
    }
#endif
    #endregion TouchController
}