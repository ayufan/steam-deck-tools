using PowerControl.Helpers;
using WindowsInput;

namespace SteamController.Profiles
{
    public sealed class DesktopProfile : Profile
    {
        public const bool LizardButtons = false;
        public const bool LizardMouse = true;

        public const String Consumed = "DesktopProfileOwner";

        public DesktopProfile()
        {
        }

        public override Status Run(Context c)
        {
            if (!c.DesktopMode)
            {
                return Status.Continue;
            }

            if (!c.Mouse.Valid)
            {
                // Failed to acquire secure context
                // Enable emergency Lizard
                c.Steam.LizardButtons = true;
                c.Steam.LizardMouse = true;
                return Status.Continue;
            }

            c.Steam.LizardButtons = LizardButtons;
            c.Steam.LizardMouse = LizardMouse;

            EmulateLizardButtons(c);
            EmulateLizardMouse(c);

            if (c.Steam.LPadX)
            {
                c.Mouse.HorizontalScroll(c.Steam.LPadX.Scaled(Context.PadToWhellSensitivity, Devices.SteamController.SteamAxis.ScaledMode.Delta));
            }
            if (c.Steam.LPadY)
            {
                c.Mouse.VerticalScroll(c.Steam.LPadY.Scaled(Context.PadToWhellSensitivity, Devices.SteamController.SteamAxis.ScaledMode.Delta));
            }

            if (c.Steam.BtnVirtualLeftThumbUp.HoldRepeat(Context.ThumbToWhellFirstRepeat, Context.ThumbToWhellRepeat, Consumed))
            {
                c.Mouse.VerticalScroll(Context.ThumbToWhellSensitivity);
            }
            else if (c.Steam.BtnVirtualLeftThumbDown.HoldRepeat(Context.ThumbToWhellFirstRepeat, Context.ThumbToWhellRepeat, Consumed))
            {
                c.Mouse.VerticalScroll(-Context.ThumbToWhellSensitivity);
            }
            else if (c.Steam.BtnVirtualLeftThumbLeft.HoldRepeat(Context.ThumbToWhellFirstRepeat, Context.ThumbToWhellRepeat, Consumed))
            {
                c.Mouse.HorizontalScroll(-Context.ThumbToWhellSensitivity);
            }
            else if (c.Steam.BtnVirtualLeftThumbRight.HoldRepeat(Context.ThumbToWhellFirstRepeat, Context.ThumbToWhellRepeat, Consumed))
            {
                c.Mouse.HorizontalScroll(Context.ThumbToWhellSensitivity);
            }

            if (c.Steam.BtnRStickTouch && (c.Steam.RightThumbX || c.Steam.RightThumbY))
            {
                c.Mouse.MoveBy(
                    c.Steam.RightThumbX.Scaled(Context.JoystickToMouseSensitivity, Devices.SteamController.SteamAxis.ScaledMode.AbsoluteTime),
                    -c.Steam.RightThumbY.Scaled(Context.JoystickToMouseSensitivity, Devices.SteamController.SteamAxis.ScaledMode.AbsoluteTime)
                );
            }

            return Status.Continue;
        }

        private void EmulateLizardButtons(Context c)
        {
            c.Mouse[Devices.MouseController.Button.Right] = c.Steam.BtnL2 || c.Steam.BtnLPadPress;
            c.Mouse[Devices.MouseController.Button.Left] = c.Steam.BtnR2 || c.Steam.BtnRPadPress;

#if true
            if (c.Steam.BtnA.Pressed())
                c.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            if (c.Steam.BtnDpadLeft.HoldRepeat(Consumed))
                c.Keyboard.KeyPress(VirtualKeyCode.LEFT);
            if (c.Steam.BtnDpadRight.HoldRepeat(Consumed))
                c.Keyboard.KeyPress(VirtualKeyCode.RIGHT);
            if (c.Steam.BtnDpadUp.HoldRepeat(Consumed))
                c.Keyboard.KeyPress(VirtualKeyCode.UP);
            if (c.Steam.BtnDpadDown.HoldRepeat(Consumed))
                c.Keyboard.KeyPress(VirtualKeyCode.DOWN);
#else
            c.Keyboard[VirtualKeyCode.RETURN] = c.Steam.BtnA;
            c.Keyboard[VirtualKeyCode.LEFT] = c.Steam.BtnDpadLeft;
            c.Keyboard[VirtualKeyCode.RIGHT] = c.Steam.BtnDpadRight;
            c.Keyboard[VirtualKeyCode.UP] = c.Steam.BtnDpadUp;
            c.Keyboard[VirtualKeyCode.DOWN] = c.Steam.BtnDpadDown;
#endif
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
