using CommonHelpers;
using SteamController.Devices;
using SteamController.ProfilesSettings;

namespace SteamController.Profiles.Predefined
{
    public class DS4Profile : Default.BackPanelShortcutsProfile
    {
        public override System.Drawing.Icon Icon
        {
            get
            {
                if (CommonHelpers.WindowsDarkMode.IsDarkModeEnabled)
                    return Resources.controller_classic_custom_white;
                else
                    return Resources.controller_classic_custom;
            }
        }

        public override bool Visible
        {
            get => Settings.Default.EnableDS4Support && base.Visible;
            set => base.Visible = value;
        }

        public override bool Selected(Context context)
        {
            return Settings.Default.EnableDS4Support && context.Enabled && context.DS4.Valid && context.KeyboardMouseValid && !context.State.SteamUsesSteamInput;
        }

        internal override ProfilesSettings.BackPanelSettings BackPanelSettings
        {
            get { return ProfilesSettings.DS4BackPanelSettings.Default; }
        }

        private TimedValue<bool> btnSteamPressed;

        public override Status Run(Context context)
        {
            context.Steam.LizardButtons = false;
            context.Steam.LizardMouse = false;
            context.DS4.Connected = true;

            // Lock BtnSteam
            if (context.Steam.BtnSteam.Pressed())
                btnSteamPressed = new TimedValue<bool>(true, 100);

            // Controls
            context.DS4[DS4Controller.PS] = btnSteamPressed.GetValueOrDefault(false);
            context.DS4[DS4Controller.Share] = context.Steam.BtnMenu;
            context.DS4[DS4Controller.Options] = context.Steam.BtnOptions;

            if (base.Run(context).IsDone)
            {
                return Status.Done;
            }

            // DPad
            if (context.Steam.BtnDpadUp && context.Steam.BtnDpadLeft)
                context.DS4[DS4Controller.DPadNorthwest] = true;
            else if (context.Steam.BtnDpadUp && context.Steam.BtnDpadRight)
                context.DS4[DS4Controller.DPadNortheast] = true;
            else if (context.Steam.BtnDpadDown && context.Steam.BtnDpadLeft)
                context.DS4[DS4Controller.DPadSouthwest] = true;
            else if (context.Steam.BtnDpadDown && context.Steam.BtnDpadRight)
                context.DS4[DS4Controller.DPadSoutheast] = true;
            else if (context.Steam.BtnDpadUp)
                context.DS4[DS4Controller.DPadNorth] = true;
            else if (context.Steam.BtnDpadDown)
                context.DS4[DS4Controller.DPadSouth] = true;
            else if (context.Steam.BtnDpadLeft)
                context.DS4[DS4Controller.DPadWest] = true;
            else if (context.Steam.BtnDpadRight)
                context.DS4[DS4Controller.DPadEast] = true;

            // Buttons
            context.DS4[DS4Controller.Cross] = context.Steam.BtnA;
            context.DS4[DS4Controller.Circle] = context.Steam.BtnB;
            context.DS4[DS4Controller.Square] = context.Steam.BtnX;
            context.DS4[DS4Controller.Triangle] = context.Steam.BtnY;

            // Sticks
            context.DS4[DS4Controller.LeftThumbX] = context.Steam.LeftThumbX;
            context.DS4[DS4Controller.LeftThumbY] = context.Steam.LeftThumbY;
            context.DS4[DS4Controller.RightThumbX] = context.Steam.RightThumbX;
            context.DS4[DS4Controller.RightThumbY] = context.Steam.RightThumbY;
            context.DS4[DS4Controller.ThumbLeft] = context.Steam.BtnLeftStickPress;
            context.DS4[DS4Controller.ThumbRight] = context.Steam.BtnRightStickPress;

            // Triggers
            context.DS4[DS4Controller.LeftTrigger] = context.Steam.LeftTrigger;
            context.DS4[DS4Controller.RightTrigger] = context.Steam.RightTrigger;
            context.DS4[DS4Controller.TriggerLeft] = context.Steam.LeftTrigger > short.MaxValue * 3 / 4;
            context.DS4[DS4Controller.TriggerRight] = context.Steam.RightTrigger > short.MaxValue * 3 / 4;
            context.DS4[DS4Controller.ShoulderLeft] = context.Steam.BtnL1;
            context.DS4[DS4Controller.ShoulderRight] = context.Steam.BtnR1;

            // Accel & Gyro
            context.DS4[DS4Controller.GyroX] = context.Steam.GyroPitch;
            context.DS4[DS4Controller.GyroY] = context.Steam.GyroYaw;
            context.DS4[DS4Controller.GyroZ] = context.Steam.GyroRoll;
            context.DS4[DS4Controller.AccelX] = context.Steam.AccelX;
            context.DS4[DS4Controller.AccelY] = context.Steam.AccelY;
            context.DS4[DS4Controller.AccelZ] = context.Steam.AccelZ;

            // Trackpad
            context.DS4[DS4Controller.TPadClick] = context.Steam.BtnLPadPress || context.Steam.BtnRPadPress;
            context.DS4[DS4Controller.LeftFinger] = GetTPadPoint(context.Steam.LPadX, context.Steam.LPadY);
            context.DS4[DS4Controller.RightFinger] = GetTPadPoint(context.Steam.RPadX, context.Steam.RPadY);

            return Status.Continue;
        }

        private void SetVirtualDS4Code(Context c, VirtualDS4Code ds4Code, bool value)
        {
            var button = ds4Code.ToDS4Button();
            if (button is not null)
                c.DS4[button.Value] = value;
        }

        protected override void BackPanelShortcuts(Context c)
        {
            base.BackPanelShortcuts(c);

            var settings = ProfilesSettings.DS4BackPanelSettings.Default;

            SetVirtualDS4Code(c, settings.L4_DS4, c.Steam.BtnL4);
            SetVirtualDS4Code(c, settings.L5_DS4, c.Steam.BtnL5);
            SetVirtualDS4Code(c, settings.R4_DS4, c.Steam.BtnR4);
            SetVirtualDS4Code(c, settings.R5_DS4, c.Steam.BtnR5);
        }

        private Point? GetTPadPoint(SteamAxis x, SteamAxis y)
        {
            if (x || y)
            {
                return new Point(
                    (int)x.GetDeltaValue(0, 1920, DeltaValueMode.Absolute),
                    (int)y.GetDeltaValue(943 + 488, -488, DeltaValueMode.Absolute)
                );
            }
            else
            {
                return null;
            }
        }
    }
}
