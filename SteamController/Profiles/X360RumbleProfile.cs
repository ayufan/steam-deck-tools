using CommonHelpers;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace SteamController.Profiles
{
    public class X360RumbleProfile : X360Profile
    {
        public const ushort FeedbackMaxAmplitude = 255;
        public const ushort FeedbackPeriod = 10;
        public const ushort FeedbackCount = 1;

        private ProfilesSettings.X360RumbleSettings RumbleSettings
        {
            get { return ProfilesSettings.X360RumbleSettings.Default; }
        }

        public override Status Run(Context context)
        {
            if (base.Run(context).IsDone)
            {
                return Status.Done;
            }

            if (context.X360.FeedbackLargeMotor.HasValue)
            {
                context.Steam.SetHaptic(
                    1, GetHapticAmplitude(context.X360.FeedbackLargeMotor), RumbleSettings.Period, FeedbackCount);
            }

            if (context.X360.FeedbackSmallMotor.HasValue)
            {
                context.Steam.SetHaptic(
                    0, GetHapticAmplitude(context.X360.FeedbackSmallMotor), RumbleSettings.Period, FeedbackCount);
            }

            context.X360.ResetFeedback();

            return Status.Continue;
        }

        private ushort GetHapticAmplitude(byte? value)
        {
            if (RumbleSettings.FixedAmplitude > 0)
                return value is not null ? (ushort)RumbleSettings.FixedAmplitude : (ushort)0;
            else
                return (ushort)(RumbleSettings.MaxAmplitude * (value ?? 0) / byte.MaxValue);
        }
    }
}
