using WindowsInput;

namespace SteamController.Profiles
{
    public sealed class DesktopProfile : DefaultGuideShortcutsProfile
    {
        private const String Consumed = "DesktopProfileOwner";

        public DesktopProfile()
        {
            IsDesktop = true;
        }

        public override bool Selected(Context context)
        {
            return context.Enabled && context.DesktopMode && !context.SteamUsesController;
        }

        public override Status Run(Context c)
        {
            if (base.Run(c).IsDone)
            {
                return Status.Done;
            }

            if (!c.Mouse.Valid)
            {
                // Failed to acquire secure context
                // Enable emergency Lizard
                c.Steam.LizardButtons = true;
                c.Steam.LizardMouse = true;
                return Status.Done;
            }

            c.Steam.LizardButtons = SteamModeLizardButtons;
            c.Steam.LizardMouse = SteamModeLizardMouse;

            EmulateScrollOnLPad(c);
            EmulateScrollOnLStick(c);
            EmulateMouseOnRPad(c);
            EmulateMouseOnRStick(c);
            EmulateDPadArrows(c);

            if (c.Steam.BtnA.Pressed())
            {
                c.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            }
            if (c.Steam.BtnB.Pressed())
            {
                c.Keyboard.KeyPress(VirtualKeyCode.BACK);
            }

            return Status.Continue;
        }

        private void EmulateScrollOnLStick(Context c)
        {
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
        }

        private void EmulateDPadArrows(Context c)
        {
            c.Keyboard[VirtualKeyCode.RETURN] = c.Steam.BtnA;
            c.Keyboard[VirtualKeyCode.LEFT] = c.Steam.BtnDpadLeft;
            c.Keyboard[VirtualKeyCode.RIGHT] = c.Steam.BtnDpadRight;
            c.Keyboard[VirtualKeyCode.UP] = c.Steam.BtnDpadUp;
            c.Keyboard[VirtualKeyCode.DOWN] = c.Steam.BtnDpadDown;
        }
    }
}
