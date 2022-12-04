using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace SteamController.Profiles
{
    public sealed class SteamProfile : DefaultShortcutsProfile
    {
        public SteamProfile()
        {
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
