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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.XR;

namespace Infiniminer;

public sealed class TouchControllerInfo
{
    private float _leftVibrateStrength;
    private TimeSpan _leftVibrateTimeRemaining;

    private float _rightVibrateStrength;
    private TimeSpan _rightVibrateTimeRemaining;

    GamePadState PreviousState;
    GamePadState CurrentState;

    public bool IsAttached { get; private set; }
    public Vector2 LeftStickThreshold { get; set; }
    public Vector2 RightStickThreshold { get; set; }
    public float LeftTriggerThreshold { get; set; }
    public float RightTriggerThreshold { get; set; }

    public Point DPad
    {
        get
        {
            int x = ButtonCheck(Buttons.DPadLeft) ? -1 :
                    ButtonCheck(Buttons.DPadRight) ? 1 :
                    0;

            int y = ButtonCheck(Buttons.DPadUp) ? -1 :
                    ButtonCheck(Buttons.DPadDown) ? 1 :
                    0;

            return new Point(x, y);
        }
    }

    public Vector2 LeftStick => GetLeftStickWithDeadzone(LeftStickThreshold.X, LeftStickThreshold.Y);
    public Vector2 RightStick => GetRightStickWithDeadzone(RightStickThreshold.X, RightStickThreshold.Y);
    public float LeftTrigger => GetLeftTriggerWithThreshold(LeftTriggerThreshold);
    public float RightTrigger => GetRightTriggerWithThreshold(RightTriggerThreshold);

    public TouchControllerInfo()
    {
    }

    public void Update(GameTime gameTime)
    {
        PreviousState = CurrentState;
        CurrentState = TouchController.GetState(TouchControllerType.Touch);

        //  If there is time remaining for the left motor to vibrate, then reduce
        //  the time by the update cycle delta time. If this bring it below zero
        //  then set the left vibrate to 0.
        if (_leftVibrateTimeRemaining > TimeSpan.Zero)
        {
            _leftVibrateTimeRemaining -= gameTime.ElapsedGameTime;
            if (_leftVibrateTimeRemaining <= TimeSpan.Zero)
            {
                VibrateLeft(0, TimeSpan.Zero);
            }
        }

        //  If there is time remaining for the right motor to vibrate, then reduce
        //  the time by the update cycle delta time. If this bring it below zero
        //  then set the right vibrate to 0.
        if (_rightVibrateTimeRemaining > TimeSpan.Zero)
        {
            _rightVibrateTimeRemaining -= gameTime.ElapsedGameTime;
            if (_rightVibrateTimeRemaining <= TimeSpan.Zero)
            {
                VibrateRight(0, TimeSpan.Zero);
            }
        }
    }

    public void Vibrate(float strength, TimeSpan time)
    {
        VibrateLeft(strength, time);
        VibrateRight(strength, time);
    }

    public void Vibrate(float leftMotorStrength, float rightMotorStrength, TimeSpan time)
    {
        VibrateLeft(leftMotorStrength, time);
        VibrateRight(rightMotorStrength, time);
    }

    public void VibrateLeft(float strength, TimeSpan time)
    {
        _leftVibrateStrength = strength;
        _leftVibrateTimeRemaining = time;
        TouchController.SetVibration(TouchControllerType.LTouch, _leftVibrateStrength);
    }

    public void VibrateRight(float strength, TimeSpan time)
    {
        _rightVibrateStrength = strength;
        _rightVibrateTimeRemaining = time;
        TouchController.SetVibration(TouchControllerType.LTouch, _rightVibrateStrength);
    }

    public void StopVibration()
    {
        _leftVibrateStrength = _rightVibrateStrength = 0;
        _leftVibrateTimeRemaining = _rightVibrateTimeRemaining = TimeSpan.Zero;
        Vibrate(0, TimeSpan.Zero);
    }

    public bool AnyButtonCheck()
    {
        foreach (Buttons button in Enum.GetValues(typeof(Buttons)))
        {
            if (ButtonCheck(button))
            {
                return true;
            }
        }

        return false;
    }

    public bool AnyButtonPressed()
    {
        foreach (Buttons button in Enum.GetValues(typeof(Buttons)))
        {
            if (ButtonPressed(button))
            {
                return true;
            }
        }

        return false;
    }

    public bool AnyButtonReleased()
    {
        foreach (Buttons button in Enum.GetValues(typeof(Buttons)))
        {
            if (ButtonReleased(button))
            {
                return true;
            }
        }

        return false;
    }


    ///////////////////////////////////////////////////////////////////////////
    /// Face Buttons
    ///////////////////////////////////////////////////////////////////////////
    #region Face Buttons

    public bool ButtonCheck(Buttons button) => CurrentState.IsButtonDown(button);
    public bool ButtonPressed(Buttons button) => CurrentState.IsButtonDown(button) && !PreviousState.IsButtonDown(button);
    public bool ButtonReleased(Buttons button) => !CurrentState.IsButtonDown(button) && PreviousState.IsButtonDown(button);

    #endregion Face Buttons

    ///////////////////////////////////////////////////////////////////////////
    /// Left Stick Left
    ///////////////////////////////////////////////////////////////////////////
    #region Left Stick Left

    public bool LeftStickLeftCheck() => LeftStickLeftCheck(LeftStickThreshold.X);
    public bool LeftStickLeftCheck(float deadzone) => CurrentState.ThumbSticks.Left.X < -deadzone;

    public bool LeftStickLeftPressed() => LeftStickLeftPressed(LeftStickThreshold.X);
    public bool LeftStickLeftPressed(float deadzone) => CurrentState.ThumbSticks.Left.X < -deadzone && PreviousState.ThumbSticks.Left.X >= -deadzone;

    public bool LeftStickLeftReleased() => LeftStickLeftReleased(LeftStickThreshold.X);
    public bool LeftStickLeftReleased(float deadzone) => CurrentState.ThumbSticks.Left.X >= -deadzone && PreviousState.ThumbSticks.Left.X < -deadzone;

    #endregion Left Stick Left

    ///////////////////////////////////////////////////////////////////////////
    /// Left Stick Right
    ///////////////////////////////////////////////////////////////////////////
    #region Left Stick Right

    public bool LeftStickRightCheck() => LeftStickRightCheck(LeftStickThreshold.X);
    public bool LeftStickRightCheck(float deadzone) => CurrentState.ThumbSticks.Left.X > deadzone;

    public bool LeftStickRightPressed() => LeftStickRightPressed(LeftStickThreshold.X);
    public bool LeftStickRightPressed(float deadzone) => CurrentState.ThumbSticks.Left.X > deadzone && PreviousState.ThumbSticks.Left.X <= deadzone;

    public bool LeftStickRightReleased() => LeftStickRightReleased(LeftStickThreshold.X);
    public bool LeftStickRightReleased(float deadzone) => CurrentState.ThumbSticks.Left.X <= deadzone && PreviousState.ThumbSticks.Left.X > deadzone;

    #endregion Left Stick Right

    ///////////////////////////////////////////////////////////////////////////
    /// Left Stick Up
    ///////////////////////////////////////////////////////////////////////////
    #region Left Stick Up

    public bool LeftStickUpCheck() => LeftStickUpCheck(LeftStickThreshold.Y);
    public bool LeftStickUpCheck(float deadzone) => CurrentState.ThumbSticks.Left.Y > deadzone;

    public bool LeftStickUpPressed() => LeftStickUpPressed(LeftStickThreshold.Y);
    public bool LeftStickUpPressed(float deadzone) => CurrentState.ThumbSticks.Left.Y > deadzone && PreviousState.ThumbSticks.Left.Y <= deadzone;

    public bool LeftStickUpReleased() => LeftStickUpReleased(LeftStickThreshold.Y);
    public bool LeftStickUpReleased(float deadzone) => CurrentState.ThumbSticks.Left.Y <= deadzone && PreviousState.ThumbSticks.Left.Y > deadzone;

    #endregion Left Stick Up

    ///////////////////////////////////////////////////////////////////////////
    /// Left Stick Down
    ///////////////////////////////////////////////////////////////////////////
    #region Left Stick Down
    public bool LeftStickDownCheck() => LeftStickDownCheck(LeftStickThreshold.Y);
    public bool LeftStickDownCheck(float deadzone) => CurrentState.ThumbSticks.Left.Y < -deadzone;

    public bool LeftStickDownPressed() => LeftStickDownPressed(LeftStickThreshold.Y);
    public bool LeftStickDownPressed(float deadzone) => CurrentState.ThumbSticks.Left.Y < -deadzone && PreviousState.ThumbSticks.Left.Y >= -deadzone;

    public bool LeftStickDownReleased() => LeftStickDownReleased(LeftStickThreshold.Y);
    public bool LeftStickDownReleased(float deadzone) => CurrentState.ThumbSticks.Left.Y >= -deadzone && PreviousState.ThumbSticks.Left.Y < -deadzone;

    #endregion Left Stick Down

    ///////////////////////////////////////////////////////////////////////////
    /// Right Stick Right
    ///////////////////////////////////////////////////////////////////////////
    #region Right Stick Left

    public bool RightStickLeftCheck() => RightStickLeftCheck(RightStickThreshold.X);
    public bool RightStickLeftCheck(float deadzone) => CurrentState.ThumbSticks.Right.X < -deadzone;

    public bool RightStickLeftPressed() => RightStickPressed(RightStickThreshold.X);
    public bool RightStickPressed(float deadzone) => CurrentState.ThumbSticks.Right.X < -deadzone && PreviousState.ThumbSticks.Right.X >= -deadzone;

    public bool RightStickReleased() => RightStickReleased(RightStickThreshold.X);
    public bool RightStickReleased(float deadzone) => CurrentState.ThumbSticks.Right.X >= -deadzone && PreviousState.ThumbSticks.Right.X < -deadzone;

    #endregion Right Stick Right

    ///////////////////////////////////////////////////////////////////////////
    /// Right Stick Right
    ///////////////////////////////////////////////////////////////////////////
    #region Right Stick Right

    public bool RightStickRightCheck() => RightStickRightCheck(RightStickThreshold.X);
    public bool RightStickRightCheck(float deadzone) => CurrentState.ThumbSticks.Right.X > deadzone;

    public bool RightStickRightPressed() => RightStickRightPressed(RightStickThreshold.X);
    public bool RightStickRightPressed(float deadzone) => CurrentState.ThumbSticks.Right.X > deadzone && PreviousState.ThumbSticks.Right.X <= deadzone;

    public bool RightStickRightReleased() => RightStickRightReleased(RightStickThreshold.X);
    public bool RightStickRightReleased(float deadzone) => CurrentState.ThumbSticks.Right.X <= deadzone && PreviousState.ThumbSticks.Right.X > deadzone;

    #endregion Right Stick Right

    ///////////////////////////////////////////////////////////////////////////
    /// Right Stick Up
    ///////////////////////////////////////////////////////////////////////////
    #region Right Stick Up

    public bool RightStickUpCheck() => RightStickUpCheck(RightStickThreshold.Y);
    public bool RightStickUpCheck(float deadzone) => CurrentState.ThumbSticks.Right.Y > deadzone;

    public bool RightStickUpPressed() => RightStickUpPressed(RightStickThreshold.Y);
    public bool RightStickUpPressed(float deadzone) => CurrentState.ThumbSticks.Right.Y > deadzone && PreviousState.ThumbSticks.Right.Y <= deadzone;

    public bool RightStickUpReleased() => RightStickUpReleased(RightStickThreshold.Y);
    public bool RightStickUpReleased(float deadzone) => CurrentState.ThumbSticks.Right.Y <= deadzone && PreviousState.ThumbSticks.Right.Y > deadzone;

    #endregion Right Stick Up

    ///////////////////////////////////////////////////////////////////////////
    /// Right Stick Down
    ///////////////////////////////////////////////////////////////////////////
    #region Right Stick Down
    public bool RightStickDownCheck() => RightStickDownCheck(RightStickThreshold.Y);
    public bool RightStickDownCheck(float deadzone) => CurrentState.ThumbSticks.Right.Y < -deadzone;

    public bool RightStickDownPressed() => RightStickDownPressed(RightStickThreshold.Y);
    public bool RightStickDownPressed(float deadzone) => CurrentState.ThumbSticks.Right.Y < -deadzone && PreviousState.ThumbSticks.Right.Y >= -deadzone;

    public bool RightStickDownReleased() => RightStickDownReleased(RightStickThreshold.Y);
    public bool RightStickDownReleased(float deadzone) => CurrentState.ThumbSticks.Right.Y >= -deadzone && PreviousState.ThumbSticks.Right.Y < -deadzone;

    #endregion Right Stick Down    



    public Vector2 GetLeftStickWithDeadzone(Vector2 deadzone) => GetLeftStickWithDeadzone(deadzone.X, deadzone.Y);
    public Vector2 GetLeftStickWithDeadzone(float deadzoneX, float deadzoneY)
    {
        float x = CurrentState.ThumbSticks.Left.X;
        float y = CurrentState.ThumbSticks.Left.Y;

        x = Math.Abs(x) >= deadzoneX ? x : 0;
        y = Math.Abs(y) >= deadzoneY ? y : 0;

        return new Vector2(x, -y);
    }

    public Vector2 GetRightStickWithDeadzone(Vector2 deadzone) => GetRightStickWithDeadzone(deadzone.X, deadzone.Y);
    public Vector2 GetRightStickWithDeadzone(float deadzoneX, float deadzoneY)
    {
        float x = CurrentState.ThumbSticks.Right.X;
        float y = CurrentState.ThumbSticks.Right.Y;

        x = Math.Abs(x) >= deadzoneX ? x : 0;
        y = Math.Abs(y) >= deadzoneY ? y : 0;

        return new Vector2(x, -y);
    }

    ///////////////////////////////////////////////////////////////////////////
    /// Left Trigger
    ///////////////////////////////////////////////////////////////////////////
    #region Left Trigger

    public float GetLeftTriggerWithThreshold(float threshold)
    {
        float value = CurrentState.Triggers.Left;
        return Math.Abs(value) >= threshold ? value : 0.0f;
    }

    public bool LeftTriggerCheck() => LeftTriggerCheck(LeftTriggerThreshold);
    public bool LeftTriggerCheck(float threshold) => CurrentState.Triggers.Left > threshold;

    public bool LeftTriggerPressed() => LeftTriggerPressed(LeftTriggerThreshold);
    public bool LeftTriggerPressed(float threshold) => CurrentState.Triggers.Left > threshold && PreviousState.Triggers.Left <= threshold;

    public bool LeftTriggerReleased() => LeftTriggerReleased(LeftTriggerThreshold);
    public bool LeftTriggerReleased(float threshold) => CurrentState.Triggers.Left <= threshold && PreviousState.Triggers.Left > threshold;

    #endregion Left Trigger

    ///////////////////////////////////////////////////////////////////////////
    /// Right Trigger
    ///////////////////////////////////////////////////////////////////////////
    #region Right Trigger

    public float GetRightTriggerWithThreshold(float threshold)
    {
        float value = CurrentState.Triggers.Right;

        return Math.Abs(value) >= threshold ? value : 0.0f;
    }

    public bool RightTriggerCheck() => RightTriggerCheck(RightTriggerThreshold);
    public bool RightTriggerCheck(float threshold) => CurrentState.Triggers.Right > threshold;

    public bool RightTriggerPressed() => RightTriggerPressed(RightTriggerThreshold);
    public bool RightTriggerPressed(float threshold) => CurrentState.Triggers.Right > threshold && PreviousState.Triggers.Right <= threshold;

    public bool RightTriggerReleased() => RightTriggerReleased(RightTriggerThreshold);
    public bool RightTriggerReleased(float threshold) => CurrentState.Triggers.Right <= threshold && PreviousState.Triggers.Right > threshold;

    #endregion Right Trigger
}