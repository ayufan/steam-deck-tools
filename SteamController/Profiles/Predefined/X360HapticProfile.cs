using SteamController.ProfilesSettings;
using HapticPad = SteamController.Devices.SteamController.HapticPad;

namespace SteamController.Profiles.Predefined
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
                return Status.Done;

            if (GetHapticIntensity(context.X360.FeedbackLargeMotor, HapticSettings.LeftIntensity, out var leftIntensity))
                context.Steam.SendHaptic(HapticPad.Right, HapticSettings.HapticStyle, leftIntensity);

            if (GetHapticIntensity(context.X360.FeedbackSmallMotor, HapticSettings.RightIntensity, out var rightIntensity))
                context.Steam.SendHaptic(HapticPad.Left, HapticSettings.HapticStyle, rightIntensity);

            context.X360.ResetFeedback();

            return Status.Continue;
        }

        private bool GetHapticIntensity(byte? input, sbyte maxIntensity, out sbyte output)
        {
            output = default;
            if (input is null || input.Value == 0)
                return false;

            int value = X360HapticSettings.MinIntensity + (maxIntensity - X360HapticSettings.MinIntensity) * input.Value / 255;
            output = (sbyte)value;
            return true;
        }
    }
}
