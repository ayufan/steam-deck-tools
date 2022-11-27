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
            return context.Enabled && context.DesktopMode;
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

            c.Keyboard[VirtualKeyCode.RETURN] = c.Steam.BtnA;
            c.Keyboard[VirtualKeyCode.BACK] = c.Steam.BtnB;

            return Status.Continue;
        }

        private void EmulateScrollOnLStick(Context c)
        {
            if (c.Steam.LeftThumbX)
            {
                c.Mouse.HorizontalScroll(c.Steam.LeftThumbX.DeltaValue * Context.ThumbToWhellSensitivity);
            }
            if (c.Steam.LeftThumbY)
            {
                c.Mouse.VerticalScroll(c.Steam.LeftThumbY.DeltaValue * Context.ThumbToWhellSensitivity);
            }
        }

        private void EmulateDPadArrows(Context c)
        {
            c.Keyboard[VirtualKeyCode.LEFT] = c.Steam.BtnDpadLeft;
            c.Keyboard[VirtualKeyCode.RIGHT] = c.Steam.BtnDpadRight;
            c.Keyboard[VirtualKeyCode.UP] = c.Steam.BtnDpadUp;
            c.Keyboard[VirtualKeyCode.DOWN] = c.Steam.BtnDpadDown;
        }
    }
}
