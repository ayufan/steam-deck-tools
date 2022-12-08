using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace SteamController.Profiles.Predefined
{
    public sealed class SteamWithShorcutsProfile : Default.GuideShortcutsProfile
    {
        public SteamWithShorcutsProfile()
        {
        }

        public override bool Selected(Context context)
        {
            return context.Enabled && context.State.SteamUsesSteamInput;
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
