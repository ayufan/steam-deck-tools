using CommonHelpers;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace SteamController.Profiles
{
    public class X360HapticProfile : X360Profile
    {
        private ProfilesSettings.X360HapticSettings HapticSettings
        {
            get { return ProfilesSettings.X360HapticSettings.Default; }
        }

        public override Status Run(Context context)
        {
            if (base.Run(context).IsDone)
            {
                return Status.Done;
            }

            if (context.X360.FeedbackLargeMotor.GetValueOrDefault() > 0)
            {
                context.Steam.SendHaptic(1, 0);
            }

            if (context.X360.FeedbackSmallMotor.GetValueOrDefault() > 0)
            {
                context.Steam.SendHaptic(0, 0);
            }

            context.X360.ResetFeedback();

            return Status.Continue;
        }
    }
}
