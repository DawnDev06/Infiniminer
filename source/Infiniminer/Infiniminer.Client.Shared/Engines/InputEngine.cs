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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Infiniminer;

public enum ControlType
{
    KeyboardMouse,
    GamePad
}

public class InputEngine
{
    ///////////////////////////////////////////////////////////////////////////
    /// Use Tool
    ///     Command used to invoke the action of using a tool
    ///////////////////////////////////////////////////////////////////////////
    public VirtualButton UseTool { get; private set; }

    ///////////////////////////////////////////////////////////////////////////
    /// Ping Teammates
    ///     Command used to send a ping to all members of your team.
    ///////////////////////////////////////////////////////////////////////////
    public VirtualButton PingTeam { get; private set; }

    ///////////////////////////////////////////////////////////////////////////
    /// Tool hotkeys
    ///     Commands used to quickly switch to tools via their number
    ///     This command is only available on keyboard
    ///////////////////////////////////////////////////////////////////////////
    public VirtualButton ToolHotkey1 { get; private set; }
    public VirtualButton ToolHotkey2 { get; private set; }
    public VirtualButton ToolHotkey3 { get; private set; }

    ///////////////////////////////////////////////////////////////////////////
    /// Change Tool
    ///     Command used to change to the next tool
    ///////////////////////////////////////////////////////////////////////////
    public VirtualButton ChangeTool { get; private set; }

    ///////////////////////////////////////////////////////////////////////////
    /// Change Block Type
    ///     Command used to change the block type when using the construction
    ///     gun
    ///////////////////////////////////////////////////////////////////////////
    public VirtualButton ChangeBlockType { get; private set; }

    ///////////////////////////////////////////////////////////////////////////
    /// Deposit Ore
    ///     Command used to deposit ore (at base or bank)
    ///////////////////////////////////////////////////////////////////////////
    public VirtualButton DepositOre { get; private set; }

    ///////////////////////////////////////////////////////////////////////////
    /// Withdraw Ore
    ///     Command used to withdraw ore (at base or bank)
    ///////////////////////////////////////////////////////////////////////////
    public VirtualButton WithdrawOre { get; private set; }

    ///////////////////////////////////////////////////////////////////////////
    /// Movement
    ///     Moved to make player move forward, backwards, strafe left, and
    ///     strafe right.
    ///////////////////////////////////////////////////////////////////////////
    public VirtualJoystick Move { get; private set; }

    ///////////////////////////////////////////////////////////////////////////
    /// Sprint
    ///     Command used to indicate the player is sprinting
    ///////////////////////////////////////////////////////////////////////////
    public VirtualButton Sprint { get; private set; }

    ///////////////////////////////////////////////////////////////////////////
    /// Jump
    ///     Command to make the player jump
    ///////////////////////////////////////////////////////////////////////////
    public VirtualButton Jump { get; private set; }

    ///////////////////////////////////////////////////////////////////////////
    /// Camera Movement
    ///     Moves the camera around
    ///////////////////////////////////////////////////////////////////////////
    public VirtualJoystick Camera { get; private set; }

    ///////////////////////////////////////////////////////////////////////////
    /// Change Team
    ///     Command used to change team
    ///////////////////////////////////////////////////////////////////////////
    public VirtualButton ChangeTeam { get; private set; }

    ///////////////////////////////////////////////////////////////////////////
    /// Change Class
    ///     Command used to change class (at surface)
    ///////////////////////////////////////////////////////////////////////////
    public VirtualButton ChangeClass { get; private set; }

    ///////////////////////////////////////////////////////////////////////////
    /// Say To Team
    ///     Command used to start typing command to say to team.
    ///     This is only available for keyboards
    ///////////////////////////////////////////////////////////////////////////
    public VirtualButton SayToTeam { get; private set; }

    ///////////////////////////////////////////////////////////////////////////
    /// Say To All
    ///     Command used to start typing a command to say to everyone on the
    ///     server.
    ///     This is only available for keyboards
    ///////////////////////////////////////////////////////////////////////////
    public VirtualButton SayToAll { get; private set; }

    ///////////////////////////////////////////////////////////////////////////
    /// Show Help Button
    ///     This is the command that brings up the overlay screen showing the
    ///     help information and keyboard/controller bindings
    ///////////////////////////////////////////////////////////////////////////
    public VirtualButton ShowHelpButton { get; private set; }

    ///////////////////////////////////////////////////////////////////////////
    /// Show Options Button
    ///     This is the command that brings up the overlay text showing the
    ///     options to quit or pixelcide.  For Gamepad it also displays the
    ///     options for changing team or changing class.
    ///////////////////////////////////////////////////////////////////////////
    public VirtualButton ShowOptionsButton { get; private set; }

    ///////////////////////////////////////////////////////////////////////////
    /// Quit Button
    ///     This is the command used in conjunction with the Select Button
    ///     To allow the player to quit the active game (not the entire game
    ///     client).  
    ///////////////////////////////////////////////////////////////////////////
    public VirtualButton QuitButton { get; private set; }

    ///////////////////////////////////////////////////////////////////////////
    /// Pixelcide Button
    ///     This is the command used in conjunction with the Select Button
    ///     to allow the player to commit pixelcide and restart
    ///////////////////////////////////////////////////////////////////////////
    public VirtualButton PixelcideButton { get; private set; }

    ///////////////////////////////////////////////////////////////////////////
    /// Menu Navigation
    ///     These commands are used during menu screens to move up, down, left 
    ///     or right on the menu  list, confirm menu choices, or back out
    ///     of menu choices
    ///////////////////////////////////////////////////////////////////////////
    public VirtualButton MenuUp { get; private set; }
    public VirtualButton MenuDown { get; private set; }
    public VirtualButton MenuLeft { get; private set; }
    public VirtualButton MenuRight { get; private set; }
    public VirtualButton MenuConfirm { get; private set; }
    public VirtualButton MenuBack { get; private set; }


    public ControlType ControlType { get; private set; }


    public InputEngine()
    {
        //
        // Use Tool
        //
        UseTool = new VirtualButton();
        UseTool.Nodes.Add(new VirtualButton.Mouse.LeftButton());
        UseTool.Nodes.Add(new VirtualButton.GamePad.Button(Buttons.LeftTrigger));
        UseTool.Nodes.Add(new VirtualButton.GamePad.Button(Buttons.RightTrigger));

        //
        //  Ping Team
        //
        PingTeam = new VirtualButton();
        PingTeam.Nodes.Add(new VirtualButton.Keyboard.Key(Keys.Q));
        PingTeam.Nodes.Add(new VirtualButton.GamePad.Button(Buttons.RightStick));

        //
        //  ToolHotkey1
        //
        ToolHotkey1 = new VirtualButton();
        ToolHotkey1.Nodes.Add(new VirtualButton.Keyboard.Key(Keys.D1));
        ToolHotkey1.Nodes.Add(new VirtualButton.GamePad.Button(Buttons.DPadLeft));

        //
        //  ToolHotkey1
        //
        ToolHotkey2 = new VirtualButton();
        ToolHotkey2.Nodes.Add(new VirtualButton.Keyboard.Key(Keys.D2));
        ToolHotkey2.Nodes.Add(new VirtualButton.GamePad.Button(Buttons.DPadUp));

        //
        //  ToolHotkey3
        //
        ToolHotkey3 = new VirtualButton();
        ToolHotkey3.Nodes.Add(new VirtualButton.Keyboard.Key(Keys.D3));
        ToolHotkey3.Nodes.Add(new VirtualButton.GamePad.Button(Buttons.DPadRight));

        //
        //  Change Tool
        //
        ChangeTool = new VirtualButton();
        ChangeTool.Nodes.Add(new VirtualButton.Keyboard.Key(Keys.E));
        ChangeTool.Nodes.Add(new VirtualButton.GamePad.Button(Buttons.RightShoulder));

        //
        //  Change Block Type
        //
        ChangeBlockType = new VirtualButton();
        ChangeBlockType.Nodes.Add(new VirtualButton.Keyboard.Key(Keys.R));
        ChangeBlockType.Nodes.Add(new VirtualButton.GamePad.Button(Buttons.LeftShoulder));

        //
        //  Deposit Ore
        //
        DepositOre = new VirtualButton();
        DepositOre.Nodes.Add(new VirtualButton.Keyboard.Key(Keys.D8));
        DepositOre.Nodes.Add(new VirtualButton.GamePad.Button(Buttons.B));

        //
        //  Withdraw Ore
        //
        WithdrawOre = new VirtualButton();
        WithdrawOre.Nodes.Add(new VirtualButton.Keyboard.Key(Keys.D9));
        WithdrawOre.Nodes.Add(new VirtualButton.GamePad.Button(Buttons.Y));

        //
        //  Move
        //
        Move = new VirtualJoystick();
        Move.Nodes.Add(new VirtualJoystick.Keyboard.Keys(Keys.W, Keys.S, Keys.A, Keys.D, VirtualJoystick.OverlapBehavior.Cancel));
        Move.Nodes.Add(new VirtualJoystick.GamePad.LeftStick());

        //
        //  Sprint
        //
        Sprint = new VirtualButton();
        Sprint.Nodes.Add(new VirtualButton.Keyboard.Key(Keys.LeftShift));
        Sprint.Nodes.Add(new VirtualButton.GamePad.Button(Buttons.LeftStick));

        //
        // Jump
        //
        Jump = new VirtualButton();
        Jump.Nodes.Add(new VirtualButton.Keyboard.Key(Keys.Space));
        Jump.Nodes.Add(new VirtualButton.GamePad.Button(Buttons.A));

        //
        // Camera
        //
        Camera = new VirtualJoystick();
        Camera.Nodes.Add(new VirtualJoystick.GamePad.RightStick());
        Camera.Nodes.Add(new VirtualJoystick.Mouse.Axis());

        //
        //  Change Team
        //
        ChangeTeam = new VirtualButton();
        ChangeTeam.Nodes.Add(new VirtualButton.Keyboard.Key(Keys.N));
        ChangeTeam.Nodes.Add(new VirtualButton.GamePad.Button(Buttons.X));

        //
        //  Change Class
        //
        ChangeClass = new VirtualButton();
        ChangeClass.Nodes.Add(new VirtualButton.Keyboard.Key(Keys.M));
        ChangeClass.Nodes.Add(new VirtualButton.GamePad.Button(Buttons.A));

        //
        //  Say To Team
        //
        SayToTeam = new VirtualButton();
        SayToTeam.Nodes.Add(new VirtualButton.Keyboard.Key(Keys.U));

        //
        //  Say To All
        //
        SayToAll = new VirtualButton();
        SayToAll.Nodes.Add(new VirtualButton.Keyboard.Key(Keys.Y));

        //
        //  Show Help Button
        //
        ShowHelpButton = new VirtualButton();
        ShowHelpButton.Nodes.Add(new VirtualButton.Keyboard.Key(Keys.F1));
        ShowHelpButton.Nodes.Add(new VirtualButton.GamePad.Button(Buttons.Back));

        //
        //  Show Options Button
        //
        ShowOptionsButton = new VirtualButton();
        ShowOptionsButton.Nodes.Add(new VirtualButton.Keyboard.Key(Keys.Escape));
        ShowOptionsButton.Nodes.Add(new VirtualButton.GamePad.Button(Buttons.Start));

        //
        //  Quit Button
        //
        QuitButton = new VirtualButton();
        QuitButton.Nodes.Add(new VirtualButton.Keyboard.Key(Keys.Y));
        QuitButton.Nodes.Add(new VirtualButton.GamePad.Button(Buttons.Y));

        //
        //  Pixelcide Button
        //
        PixelcideButton = new VirtualButton();
        PixelcideButton.Nodes.Add(new VirtualButton.Keyboard.Key(Keys.K));
        PixelcideButton.Nodes.Add(new VirtualButton.GamePad.Button(Buttons.B));

        //
        //  Menu Up
        //
        MenuUp = new VirtualButton();
        MenuUp.Nodes.Add(new VirtualButton.GamePad.Button(Buttons.DPadUp));
        MenuUp.Nodes.Add(new VirtualButton.GamePad.Button(Buttons.LeftThumbstickUp));

        //
        //  Menu Down
        //
        MenuDown = new VirtualButton();
        MenuDown.Nodes.Add(new VirtualButton.GamePad.Button(Buttons.DPadDown));
        MenuDown.Nodes.Add(new VirtualButton.GamePad.Button(Buttons.LeftThumbstickDown));

        //
        //  Menu Right
        //
        MenuRight = new VirtualButton();
        MenuRight.Nodes.Add(new VirtualButton.GamePad.Button(Buttons.DPadRight));
        MenuRight.Nodes.Add(new VirtualButton.GamePad.Button(Buttons.LeftThumbstickRight));

        //
        //  Menu Left
        //
        MenuLeft = new VirtualButton();
        MenuLeft.Nodes.Add(new VirtualButton.GamePad.Button(Buttons.DPadLeft));
        MenuLeft.Nodes.Add(new VirtualButton.GamePad.Button(Buttons.LeftThumbstickLeft));

        //
        //  Menu Confirm
        //
        MenuConfirm = new VirtualButton();
        MenuConfirm.Nodes.Add(new VirtualButton.GamePad.Button(Buttons.A));
        MenuConfirm.Nodes.Add(new VirtualButton.GamePad.Button(Buttons.Start));
        MenuConfirm.Nodes.Add(new VirtualButton.Keyboard.Key(Keys.Enter));
        MenuConfirm.Nodes.Add(new VirtualButton.Mouse.LeftButton());

        //
        //  Menu Back
        //
        MenuBack = new VirtualButton();
        MenuBack.Nodes.Add(new VirtualButton.GamePad.Button(Buttons.B));
        MenuBack.Nodes.Add(new VirtualButton.GamePad.Button(Buttons.Back));
        MenuBack.Nodes.Add(new VirtualButton.Keyboard.Key(Keys.Escape));

#if KNI
        UseTool.Nodes.Add(new VirtualButton.TouchController.Button(Buttons.LeftTrigger));
        UseTool.Nodes.Add(new VirtualButton.TouchController.Button(Buttons.RightTrigger));
        PingTeam.Nodes.Add(new VirtualButton.TouchController.Button(Buttons.RightStick));
        //ToolHotkey1.Nodes.Add(new VirtualButton.TouchController.Button(Buttons.DPadLeft));
        //ToolHotkey2.Nodes.Add(new VirtualButton.TouchController.Button(Buttons.DPadUp));
        //ToolHotkey3.Nodes.Add(new VirtualButton.TouchController.Button(Buttons.DPadRight));
        ChangeTool.Nodes.Add(new VirtualButton.TouchController.Button(Buttons.RightShoulder));
        ChangeBlockType.Nodes.Add(new VirtualButton.TouchController.Button(Buttons.LeftShoulder));
        DepositOre.Nodes.Add(new VirtualButton.TouchController.Button(Buttons.B));
        WithdrawOre.Nodes.Add(new VirtualButton.TouchController.Button(Buttons.Y));
        Move.Nodes.Add(new VirtualJoystick.TouchController.LeftStick());
        Sprint.Nodes.Add(new VirtualButton.TouchController.Button(Buttons.LeftStick));
        Jump.Nodes.Add(new VirtualButton.TouchController.Button(Buttons.A));
        Camera.Nodes.Add(new VirtualJoystick.TouchController.RightStick());
        ChangeTeam.Nodes.Add(new VirtualButton.TouchController.Button(Buttons.X));
        ChangeClass.Nodes.Add(new VirtualButton.TouchController.Button(Buttons.A));
        ShowHelpButton.Nodes.Add(new VirtualButton.TouchController.Button(Buttons.Back));
        ShowOptionsButton.Nodes.Add(new VirtualButton.TouchController.Button(Buttons.Start));
        QuitButton.Nodes.Add(new VirtualButton.TouchController.Button(Buttons.Y));
        PixelcideButton.Nodes.Add(new VirtualButton.TouchController.Button(Buttons.B));
        //MenuUp.Nodes.Add(new VirtualButton.TouchController.Button(Buttons.DPadUp));
        MenuUp.Nodes.Add(new VirtualButton.TouchController.Button(Buttons.LeftThumbstickUp));
        //MenuDown.Nodes.Add(new VirtualButton.TouchController.Button(Buttons.DPadDown));
        MenuDown.Nodes.Add(new VirtualButton.TouchController.Button(Buttons.LeftThumbstickDown));
        //MenuRight.Nodes.Add(new VirtualButton.TouchController.Button(Buttons.DPadRight));
        MenuRight.Nodes.Add(new VirtualButton.TouchController.Button(Buttons.LeftThumbstickRight));
        //MenuLeft.Nodes.Add(new VirtualButton.TouchController.Button(Buttons.DPadLeft));
        MenuLeft.Nodes.Add(new VirtualButton.TouchController.Button(Buttons.LeftThumbstickLeft));
        MenuConfirm.Nodes.Add(new VirtualButton.TouchController.Button(Buttons.A));
        MenuConfirm.Nodes.Add(new VirtualButton.TouchController.Button(Buttons.Start));
        MenuBack.Nodes.Add(new VirtualButton.TouchController.Button(Buttons.B));
        MenuBack.Nodes.Add(new VirtualButton.TouchController.Button(Buttons.Back));
#endif
    }



    public void Update(GameTime gameTime)
    {
        InputManager.Update(gameTime);
        if (ControlType == ControlType.KeyboardMouse && InputManager.GamePad.AnyButtonCheck())
        {
            ControlType = ControlType.GamePad;
        }
        else if (ControlType == ControlType.GamePad && InputManager.Keyboard.AnyKeyCheck || InputManager.Mouse.WasMoved)
        {
            ControlType = ControlType.KeyboardMouse;
        }
#if KNI
        if (ControlType == ControlType.KeyboardMouse && InputManager.TouchController.AnyButtonCheck())
        {
            ControlType = ControlType.GamePad;
        }
#endif


        // ControlType = InputManager.GamePad.AnyButtonCheck() ? ControlType.GamePad : ControlType.KeyboardMouse;
    }

    public void VibrateGamepad(float strength, TimeSpan time)
    {
        if (ControlType != ControlType.GamePad) { return; }
        InputManager.GamePad.Vibrate(strength, time);
#if KNI
        InputManager.TouchController.Vibrate(strength, time);
#endif
    }

    public void Register()
    {
        UseTool.Register();
        PingTeam.Register();
        ToolHotkey1.Register();
        ToolHotkey2.Register();
        ToolHotkey3.Register();
        ChangeTool.Register();
        ChangeBlockType.Register();
        DepositOre.Register();
        WithdrawOre.Register();
        Move.Register();
        Sprint.Register();
        Jump.Register();
        Camera.Register();
        ChangeTeam.Register();
        ChangeClass.Register();
        SayToTeam.Register();
        SayToAll.Register();
        ShowHelpButton.Register();
        MenuUp.Register();
        MenuDown.Register();
        MenuLeft.Register();
        MenuRight.Register();
        MenuConfirm.Register();
        MenuBack.Register();
    }

    public void Unregister()
    {
        UseTool.Unregister();
        PingTeam.Unregister();
        ToolHotkey1.Unregister();
        ToolHotkey2.Unregister();
        ToolHotkey3.Unregister();
        ChangeTool.Unregister();
        ChangeBlockType.Unregister();
        DepositOre.Unregister();
        WithdrawOre.Unregister();
        Move.Unregister();
        Sprint.Unregister();
        Jump.Unregister();
        Camera.Unregister();
        ChangeTeam.Unregister();
        ChangeClass.Unregister();
        SayToTeam.Unregister();
        SayToAll.Unregister();
        ShowHelpButton.Unregister();
        MenuUp.Unregister();
        MenuDown.Unregister();
        MenuLeft.Unregister();
        MenuRight.Unregister();
        MenuConfirm.Unregister();
        MenuBack.Unregister();
    }
}