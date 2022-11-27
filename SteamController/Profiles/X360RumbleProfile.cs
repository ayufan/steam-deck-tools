using CommonHelpers;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace SteamController.Profiles
{
    public class X360RumbleProfile : X360Profile
    {
        public const ushort FeedbackMaxAmplitude = 255;
        public const ushort FeedbackPeriod = 10;
        public const ushort FeedbackCount = 1;

        public override Status Run(Context context)
        {
            if (base.Run(context).IsDone)
            {
                return Status.Done;
            }

            if (context.X360.FeedbackLargeMotor.HasValue)
            {
                Log.TraceLine("X360: Feedback Large: {0}", context.X360.FeedbackLargeMotor.Value);
                context.Steam.SetHaptic(
                    1, GetHapticAmplitude(context.X360.FeedbackLargeMotor), FeedbackPeriod, FeedbackCount);
            }

            if (context.X360.FeedbackSmallMotor.HasValue)
            {
                Log.TraceLine("X360: Feedback Small: {0}", context.X360.FeedbackSmallMotor.Value);
                context.Steam.SetHaptic(
                    0, GetHapticAmplitude(context.X360.FeedbackSmallMotor), FeedbackPeriod, FeedbackCount);
            }

            context.X360.ResetFeedback();

            return Status.Continue;
        }

        private ushort GetHapticAmplitude(byte? value)
        {
            return (ushort)(FeedbackMaxAmplitude * (value ?? 0) / byte.MaxValue);
        }
    }
}
