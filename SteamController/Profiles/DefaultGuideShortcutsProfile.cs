using System.Diagnostics;
using System.Runtime.InteropServices;
using ExternalHelpers;
using PowerControl.Helpers;
using WindowsInput;

namespace SteamController.Profiles
{
    public abstract class DefaultGuideShortcutsProfile : DefaultShortcutsProfile
    {
        public static bool SteamModeLizardButtons = false;
        public static bool SteamModeLizardMouse = true;

        public readonly TimeSpan HoldForKill = TimeSpan.FromSeconds(3);
        public readonly TimeSpan HoldForClose = TimeSpan.FromSeconds(1);

        public override Status Run(Context c)
        {
            if (base.Run(c).IsDone)
            {
                return Status.Done;
            }

            if (c.Steam.BtnSteam.Hold(HoldForShorcuts, ShortcutConsumed))
            {
                SteamShortcuts(c);
                return Status.Done;
            }

            return Status.Continue;
        }

        private void SteamShortcuts(Context c)
        {
            c.Steam.LizardButtons = SteamModeLizardButtons;
            c.Steam.LizardMouse = SteamModeLizardMouse;

            EmulateScrollOnLPad(c);
            EmulateMouseOnRPad(c);
            EmulateMouseOnRStick(c);

            if (c.Steam.BtnA.Pressed())
            {
                c.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            }

            if (c.Steam.BtnB.HoldOnce(HoldForKill, ShortcutConsumed))
            {
                // kill application
            }
            else if (c.Steam.BtnB.HoldOnce(HoldForClose, ShortcutConsumed))
            {
                // close application
                c.Keyboard.KeyPress(VirtualKeyCode.LMENU, VirtualKeyCode.F4);
            }

            if (c.Steam.BtnX.Pressed())
            {
                OnScreenKeyboard.Toggle();
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

            if (c.Steam.BtnVirtualLeftThumbUp.HoldRepeat(ShortcutConsumed))
            {
                WindowsSettingsBrightnessController.Increase(5);
            }

            if (c.Steam.BtnVirtualLeftThumbDown.HoldRepeat(ShortcutConsumed))
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
            if (c.Steam.BtnDpadUp.HoldOnce(ShortcutConsumed))
            {
                c.Keyboard.KeyPress(new VirtualKeyCode[] { VirtualKeyCode.LCONTROL, VirtualKeyCode.LMENU }, VirtualKeyCode.VK_U);
            }
        }

        protected void EmulateScrollOnLPad(Context c)
        {
            if (c.Steam.LPadX)
            {
                c.Mouse.HorizontalScroll(c.Steam.LPadX.Scaled(Context.PadToWhellSensitivity, Devices.SteamAxis.ScaledMode.Delta));
            }
            if (c.Steam.LPadY)
            {
                c.Mouse.VerticalScroll(c.Steam.LPadY.Scaled(Context.PadToWhellSensitivity, Devices.SteamAxis.ScaledMode.Delta));
            }
        }

        protected void EmulateMouseOnRStick(Context c)
        {
            if (c.Steam.RightThumbX || c.Steam.RightThumbY)
            {
                c.Mouse.MoveBy(
                    c.Steam.RightThumbX.Scaled(Context.JoystickToMouseSensitivity, Devices.SteamAxis.ScaledMode.AbsoluteTime),
                    -c.Steam.RightThumbY.Scaled(Context.JoystickToMouseSensitivity, Devices.SteamAxis.ScaledMode.AbsoluteTime)
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
                    c.Steam.RPadX.Scaled(Context.PadToMouseSensitivity, Devices.SteamAxis.ScaledMode.Delta),
                    -c.Steam.RPadY.Scaled(Context.PadToMouseSensitivity, Devices.SteamAxis.ScaledMode.Delta)
                );
            }
        }
    }
}
