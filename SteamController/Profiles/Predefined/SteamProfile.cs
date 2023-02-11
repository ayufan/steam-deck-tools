namespace SteamController.Profiles.Predefined
{
    public sealed class SteamProfile : Default.ShortcutsProfile
    {
        public SteamProfile()
        {
        }

        public override System.Drawing.Icon Icon
        {
            get
            {
                if (CommonHelpers.WindowsDarkMode.IsDarkModeEnabled)
                    return Resources.microsoft_xbox_controller_off_white;
                else
                    return Resources.microsoft_xbox_controller_off;
            }
        }

        public override String FullName
        {
            get { return Name + " uses Steam Input"; }
        }

        public override bool Selected(Context context)
        {
            return context.Enabled && context.State.SteamUsesSteamInput && Settings.Default.SteamControllerConfigs != Settings.SteamControllerConfigsMode.Overwrite;
        }

        public override Status Run(Context context)
        {
            // Steam does not use Lizard
            context.Steam.LizardButtons = false;
            context.Steam.LizardMouse = false;

            if (base.Run(context).IsDone)
            {
                return Status.Done;
            }

            return Status.Continue;
        }
    }
}
