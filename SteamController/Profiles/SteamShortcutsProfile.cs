using System.Diagnostics;
using ExternalHelpers;
using PowerControl.Helpers;
using WindowsInput;

namespace SteamController.Profiles
{
    public sealed class SteamShortcutsProfile : Profile
    {
        public const bool LizardButtons = true;
        public const bool LizardMouse = false;

        public const String Consumed = "SteamShortcutsProfile";
        public readonly TimeSpan HoldForShorcuts = TimeSpan.FromMilliseconds(200);
        public readonly TimeSpan HoldForKill = TimeSpan.FromSeconds(3);
        public readonly TimeSpan HoldForClose = TimeSpan.FromSeconds(1);
        public readonly TimeSpan HoldToSwitchDesktop = TimeSpan.FromSeconds(1);

        public SteamShortcutsProfile()
        {
            RunAlways = true;
        }

        public override Status Run(Context c)
        {
            // Steam + 3 dots simulate CTRL+ALT+DELETE
            if (c.Steam.BtnSteam.Hold(HoldForShorcuts, Consumed) && c.Steam.BtnQuickAccess.HoldOnce(HoldForShorcuts, Consumed))
            {
                c.Keyboard.KeyPress(new VirtualKeyCode[] { VirtualKeyCode.LCONTROL, VirtualKeyCode.LMENU }, VirtualKeyCode.DELETE);
            }

            if (c.Steam.BtnSteam.Hold(HoldForShorcuts, Consumed))
            {
                c.Steam.LizardButtons = LizardButtons;
                c.Steam.LizardMouse = LizardMouse;
                SteamShortcuts(c);
                AdditionalShortcuts(c);
                return Status.Stop;
            }

            if (c.Steam.BtnOptions.HoldOnce(HoldToSwitchDesktop, Consumed))
            {
                c.RequestDesktopMode = !c.RequestDesktopMode;
            }

            if (c.Steam.BtnQuickAccess.Hold(HoldForShorcuts, Consumed))
            {
                // nothing there, just consume
                return Status.Stop;
            }

            return Status.Continue;
        }

        private void SteamShortcuts(Context c)
        {
            c.Steam.LizardButtons = false;
            c.Steam.LizardMouse = true;

            if (c.Steam.BtnA.Pressed())
            {
                c.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            }

            if (c.Steam.BtnB.HoldOnce(HoldForKill, Consumed))
            {
                // kill application
            }
            else if (c.Steam.BtnB.HoldOnce(HoldForClose, Consumed))
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

            c.Mouse[Devices.MouseController.Button.Right] = c.Steam.BtnL2 || c.Steam.BtnLPadPress;
            c.Mouse[Devices.MouseController.Button.Left] = c.Steam.BtnR2 || c.Steam.BtnRPadPress;

            if (c.Steam.BtnRStickTouch && (c.Steam.RightThumbX || c.Steam.RightThumbY))
            {
                c.Mouse.MoveBy(
                    c.Steam.RightThumbX.Scaled(Context.JoystickToMouseSensitivity, Devices.SteamController.SteamAxis.ScaledMode.AbsoluteTime),
                    -c.Steam.RightThumbY.Scaled(Context.JoystickToMouseSensitivity, Devices.SteamController.SteamAxis.ScaledMode.AbsoluteTime)
                );
            }

            if (c.Steam.LPadX)
            {
                c.Mouse.HorizontalScroll(c.Steam.LPadX.Scaled(Context.PadToWhellSensitivity, Devices.SteamController.SteamAxis.ScaledMode.Delta));
            }
            if (c.Steam.LPadY)
            {
                c.Mouse.VerticalScroll(c.Steam.LPadY.Scaled(Context.PadToWhellSensitivity, Devices.SteamController.SteamAxis.ScaledMode.Delta));
            }

            EmulateLizardMouse(c);

            if (c.Steam.BtnVirtualLeftThumbUp.HoldRepeat(Consumed))
            {
                WindowsSettingsBrightnessController.Increase(5);
            }

            if (c.Steam.BtnVirtualLeftThumbDown.HoldRepeat(Consumed))
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
        }

        private void AdditionalShortcuts(Context c)
        {
            if (c.Steam.BtnMenu.Pressed())
            {
                c.Keyboard.KeyPress(VirtualKeyCode.LWIN, VirtualKeyCode.TAB);
            }

            if (c.Steam.BtnOptions.Pressed())
            {
                c.Keyboard.KeyPress(VirtualKeyCode.F11);
            }
        }

        private void EmulateLizardMouse(Context c)
        {
            if (c.Steam.RPadX || c.Steam.RPadY)
            {
                c.Mouse.MoveBy(
                    c.Steam.RPadX.Scaled(Context.PadToMouseSensitivity, Devices.SteamController.SteamAxis.ScaledMode.Delta),
                    -c.Steam.RPadY.Scaled(Context.PadToMouseSensitivity, Devices.SteamController.SteamAxis.ScaledMode.Delta)
                );
            }
        }
    }
}
