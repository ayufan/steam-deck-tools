using System.Diagnostics;
using ExternalHelpers;
using PowerControl.Helpers;
using WindowsInput;

namespace SteamController.Profiles.Default
{
    public abstract class GuideShortcutsProfile : ShortcutsProfile
    {
        public readonly TimeSpan HoldForKill = TimeSpan.FromSeconds(3);
        public readonly TimeSpan HoldForClose = TimeSpan.FromSeconds(1);

        protected override bool SteamShortcuts(Context c)
        {
            if (base.SteamShortcuts(c))
                return true;

            c.Steam.LizardButtons = SettingsDebug.Default.LizardButtons;
            c.Steam.LizardMouse = SettingsDebug.Default.LizardMouse;

            EmulateScrollOnLPad(c);
            EmulateMouseOnRPad(c);
            EmulateMouseOnRStick(c);

            if (c.Steam.BtnA.Pressed())
            {
                c.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            }

            if (c.Steam.BtnB.HoldOnce(HoldForClose, ShortcutConsumed))
            {
                Helpers.ForegroundProcess.Store();

                // close application
                c.Keyboard.KeyPress(VirtualKeyCode.LMENU, VirtualKeyCode.F4);
            }
            else if (c.Steam.BtnB.HoldChain(HoldForKill, ShortcutConsumed, "KillProcess"))
            {
                // We want to KILL only the process that
                // was foreground last time
                Helpers.ForegroundProcess.Kill(true);
            }

            if (c.Steam.BtnX.Pressed())
            {
                switch (Settings.Default.KeyboardStyle)
                {
                    case Settings.KeyboardStyles.CTRL_WIN_O:
                        c.Keyboard.KeyPress(new VirtualKeyCode[] { VirtualKeyCode.LCONTROL, VirtualKeyCode.LWIN }, VirtualKeyCode.VK_O);
                        break;

                    case Settings.KeyboardStyles.WindowsTouch:
                        if (!OnScreenKeyboard.Toggle())
                        {
                            // Fallback to CTRL+WIN+O
                            c.Keyboard.KeyPress(new VirtualKeyCode[] { VirtualKeyCode.LCONTROL, VirtualKeyCode.LWIN }, VirtualKeyCode.VK_O);
                        }
                        break;
                }
            }

            if (c.Steam.BtnL1.Pressed())
            {
                if (Process.GetProcessesByName("Magnify").Any())
                {
                    // close magnifier
                    c.Keyboard.KeyPress(VirtualKeyCode.LWIN, VirtualKeyCode.ESCAPE);
                }
                else
                {
                    // enable magnifier
                    c.Keyboard.KeyPress(VirtualKeyCode.LWIN, VirtualKeyCode.OEM_PLUS);
                }
            }

            if (c.Steam.BtnR1.Pressed())
            {
                // take screenshot
                c.Keyboard.KeyPress(VirtualKeyCode.LWIN, VirtualKeyCode.SNAPSHOT);
            }

            if (c.Steam.BtnVirtualLeftThumbUp.JustPressed() || c.Steam.BtnVirtualLeftThumbUp.HoldRepeat(ShortcutConsumed))
            {
                WindowsSettingsBrightnessController.Increase(5);
            }

            if (c.Steam.BtnVirtualLeftThumbDown.JustPressed() || c.Steam.BtnVirtualLeftThumbDown.HoldRepeat(ShortcutConsumed))
            {
                WindowsSettingsBrightnessController.Increase(-5);
            }

            if (c.Steam.BtnDpadRight.Pressed())
            {
                c.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            }

            if (c.Steam.BtnDpadDown.Pressed())
            {
                c.Keyboard.KeyPress(VirtualKeyCode.TAB);
            }

            if (c.Steam.BtnDpadLeft.Pressed())
            {
                c.Keyboard.KeyPress(VirtualKeyCode.ESCAPE);
            }

            // Additional binding for tool hotkeys (Lossless Fullscreen is nice)
            if (c.Steam.BtnDpadUp.Pressed())
            {
                c.Keyboard.KeyPress(new VirtualKeyCode[] { VirtualKeyCode.LCONTROL, VirtualKeyCode.LMENU }, VirtualKeyCode.VK_U);
            }

            return true;
        }

        protected void EmulateScrollOnLPad(Context c)
        {
            if (c.Steam.LPadX)
            {
                c.Mouse.HorizontalScroll(c.Steam.LPadX.DeltaValue * Context.PadToWhellSensitivity);
            }
            if (c.Steam.LPadY)
            {
                c.Mouse.VerticalScroll(c.Steam.LPadY.DeltaValue * Context.PadToWhellSensitivity * (double)Settings.Default.ScrollDirection);
            }
        }

        protected void EmulateMouseOnRStick(Context c)
        {
            if (c.Steam.RightThumbX || c.Steam.RightThumbY)
            {
                c.Mouse.MoveBy(
                    c.Steam.RightThumbX.DeltaValue * Context.JoystickToMouseSensitivity,
                    -c.Steam.RightThumbY.DeltaValue * Context.JoystickToMouseSensitivity
                );
            }
        }

        protected void EmulateMouseOnRPad(Context c, bool useButtonTriggers = true)
        {
            if (useButtonTriggers)
            {
                c.Mouse[Devices.MouseController.Button.Right] = c.Steam.BtnL2 || c.Steam.BtnLPadPress;
                c.Mouse[Devices.MouseController.Button.Left] = c.Steam.BtnR2 || c.Steam.BtnRPadPress;
            }
            else
            {
                c.Mouse[Devices.MouseController.Button.Right] = c.Steam.BtnLPadPress;
                c.Mouse[Devices.MouseController.Button.Left] = c.Steam.BtnRPadPress;
            }

            if (c.Steam.RPadX || c.Steam.RPadY)
            {
                c.Mouse.MoveBy(
                    c.Steam.RPadX.DeltaValue * Context.PadToMouseSensitivity,
                    -c.Steam.RPadY.DeltaValue * Context.PadToMouseSensitivity
                );
            }
        }
    }
}
