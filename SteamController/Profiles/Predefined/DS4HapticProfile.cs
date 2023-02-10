using SteamController.ProfilesSettings;
using HapticPad = SteamController.Devices.SteamController.HapticPad;

namespace SteamController.Profiles.Predefined
{
    public class DS4HapticProfile : DS4Profile
    {
        private ProfilesSettings.HapticSettings HapticSettings
        {
            get { return ProfilesSettings.HapticSettings.DS4; }
        }

        public override Status Run(Context context)
        {
            if (base.Run(context).IsDone)
                return Status.Done;

            if (HapticSettings.GetHapticIntensity(context.DS4.FeedbackLargeMotor, HapticSettings.LeftIntensity, out var leftIntensity))
                context.Steam.SendHaptic(HapticPad.Right, HapticSettings.HapticStyle, leftIntensity);

            if (HapticSettings.GetHapticIntensity(context.DS4.FeedbackSmallMotor, HapticSettings.RightIntensity, out var rightIntensity))
                context.Steam.SendHaptic(HapticPad.Left, HapticSettings.HapticStyle, rightIntensity);

            context.DS4.ResetFeedback();

            return Status.Continue;
        }
    }
}
