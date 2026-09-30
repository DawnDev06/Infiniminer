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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Infiniminer;

public sealed class MouseInfo
{
    public MouseState PreviousState { get; private set; }
    public MouseState CurrentState { get; private set; }

    public Point Position
    {
        get => CurrentState.Position;
        set => Mouse.SetPosition(value.X, value.Y);
    }

    public int X
    {
        get => Position.X;
        set => Position = new Point(value, Position.Y);
    }

    public int Y
    {
        get => Position.Y;
        set => Position = new Point(Position.X, value);
    }

    public bool WasMoved => CurrentState.Position != PreviousState.Position;
    public int DeltaX => PreviousState.X - CurrentState.X;
    public int DeltaY => PreviousState.Y - CurrentState.Y;
    public Point DeltaPosition => new Point(DeltaX, DeltaY);

    public int ScrollWheel => CurrentState.ScrollWheelValue;
    public int ScrollWheelDelta => PreviousState.ScrollWheelValue - CurrentState.ScrollWheelValue;

    public MouseInfo()
    {
        PreviousState = new MouseState();
        CurrentState = Mouse.GetState();
    }

    public void Update()
    {
        PreviousState = CurrentState;
        CurrentState = Mouse.GetState();
    }

    ///////////////////////////////////////////////////////////////////////////
    /// Left Button
    ///////////////////////////////////////////////////////////////////////////
    #region Left Button

    public bool LeftButtonCheck() => CurrentState.LeftButton == ButtonState.Pressed;
    public bool LeftButtonPressed() => CurrentState.LeftButton == ButtonState.Pressed &&
                                       PreviousState.LeftButton == ButtonState.Released;
    public bool LeftButtonReleased() => CurrentState.LeftButton == ButtonState.Released &&
                                        PreviousState.LeftButton == ButtonState.Pressed;

    #endregion Left Button

    ///////////////////////////////////////////////////////////////////////////
    /// Right Button
    ///////////////////////////////////////////////////////////////////////////
    #region Right Button

    public bool RightButtonCheck() => CurrentState.RightButton == ButtonState.Pressed;
    public bool RightButtonPressed() => CurrentState.RightButton == ButtonState.Pressed &&
                                        PreviousState.RightButton == ButtonState.Released;
    public bool RightButtonReleased() => CurrentState.RightButton == ButtonState.Released &&
                                           PreviousState.RightButton == ButtonState.Pressed;

    #endregion Right Button

    ///////////////////////////////////////////////////////////////////////////
    /// Middle Button
    ///////////////////////////////////////////////////////////////////////////
    #region Middle Button

    public bool MiddleButtonCheck() => CurrentState.MiddleButton == ButtonState.Pressed;
    public bool MiddleButtonPressed() => CurrentState.MiddleButton == ButtonState.Pressed &&
                                        PreviousState.MiddleButton == ButtonState.Released;
    public bool MiddleButtonReleased() => CurrentState.MiddleButton == ButtonState.Released &&
                                           PreviousState.MiddleButton == ButtonState.Pressed;

    #endregion Middle Button


    ///////////////////////////////////////////////////////////////////////////
    /// XButton1 Button
    ///////////////////////////////////////////////////////////////////////////
    #region XButton1 Button

    public bool XButton1Check() => CurrentState.XButton1 == ButtonState.Pressed;
    public bool XButton1Pressed() => CurrentState.XButton1 == ButtonState.Pressed &&
                                     PreviousState.XButton1 == ButtonState.Released;
    public bool XButton1Released() => CurrentState.XButton1 == ButtonState.Released &&
                                      PreviousState.XButton1 == ButtonState.Pressed;

    #endregion XButton1 Button

    ///////////////////////////////////////////////////////////////////////////
    /// XButton2 Button
    ///////////////////////////////////////////////////////////////////////////
    #region XButton2 Button

    public bool XButton2Check() => CurrentState.XButton2 == ButtonState.Pressed;
    public bool XButton2Pressed() => CurrentState.XButton2 == ButtonState.Pressed &&
                                     PreviousState.XButton2 == ButtonState.Released;
    public bool XButton2Released() => CurrentState.XButton2 == ButtonState.Released &&
                                      PreviousState.XButton2 == ButtonState.Pressed;

    #endregion XButton2 Button    
}