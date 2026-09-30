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

using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Infiniminer;

public static class InputManager
{
    internal static List<VirtualInput> VirtualInputs { get; private set; }
    public static KeyboardInfo Keyboard { get; private set; }
    public static MouseInfo Mouse { get; private set; }
    public static GamePadInfo GamePad { get; private set; }
#if KNI
    public static TouchControllerInfo TouchController { get; private set; }
# endif

    static  InputManager()
    {
        Keyboard = new();
        Mouse = new();
        GamePad = new(PlayerIndex.One);
#if KNI
        TouchController = new();
        TouchController.LeftStickThreshold = new Vector2(0.3f);
        TouchController.RightStickThreshold = new Vector2(0.3f);
#endif
        VirtualInputs = new List<VirtualInput>();
    }

    public static void Update(GameTime gameTime)
    {
        Keyboard.Update();
        Mouse.Update();
        GamePad.Update(gameTime);
#if KNI
        TouchController.Update(gameTime);
#endif

        for (int i = 0; i < VirtualInputs.Count; i++)
        {
            VirtualInputs[i].Update();
        }
    }
}