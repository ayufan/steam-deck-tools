using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace SteamController.Profiles
{
    public class X360Profile : DefaultGuideShortcutsProfile
    {
        public override bool Selected(Context context)
        {
            return context.Enabled && !context.DesktopMode && !context.SteamUsesSteamInput;
        }

        public override Status Run(Context context)
        {
            context.Steam.LizardButtons = false;
            context.Steam.LizardMouse = SteamModeLizardMouse;
            context.X360.Connected = true;

            // Controls
            context.X360[Xbox360Button.Guide] = context.Steam.BtnSteam.Pressed();
            context.X360[Xbox360Button.Back] = context.Steam.BtnMenu;
            context.X360[Xbox360Button.Start] = context.Steam.BtnOptions;

            if (base.Run(context).IsDone)
            {
                return Status.Done;
            }

            // Default emulation
            EmulateScrollOnLPad(context);
            EmulateMouseOnRPad(context, false);

            // DPad
            context.X360[Xbox360Button.Up] = context.Steam.BtnDpadUp;
            context.X360[Xbox360Button.Down] = context.Steam.BtnDpadDown;
            context.X360[Xbox360Button.Left] = context.Steam.BtnDpadLeft;
            context.X360[Xbox360Button.Right] = context.Steam.BtnDpadRight;

            // Buttons
            context.X360[Xbox360Button.A] = context.Steam.BtnA;
            context.X360[Xbox360Button.B] = context.Steam.BtnB;
            context.X360[Xbox360Button.X] = context.Steam.BtnX;
            context.X360[Xbox360Button.Y] = context.Steam.BtnY;

            // Sticks
            context.X360[Xbox360Axis.LeftThumbX] = context.Steam.LeftThumbX;
            context.X360[Xbox360Axis.LeftThumbY] = context.Steam.LeftThumbY;
            context.X360[Xbox360Axis.RightThumbX] = context.Steam.RightThumbX;
            context.X360[Xbox360Axis.RightThumbY] = context.Steam.RightThumbY;
            context.X360[Xbox360Button.LeftThumb] = context.Steam.BtnLeftStickPress;
            context.X360[Xbox360Button.RightThumb] = context.Steam.BtnRightStickPress;

            // Triggers
            context.X360[Xbox360Slider.LeftTrigger] = context.Steam.LeftTrigger;
            context.X360[Xbox360Slider.RightTrigger] = context.Steam.RightTrigger;
            context.X360[Xbox360Button.LeftShoulder] = context.Steam.BtnL1;
            context.X360[Xbox360Button.RightShoulder] = context.Steam.BtnR1;

            return Status.Continue;
        }
    }
}
