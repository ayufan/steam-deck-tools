using System.Diagnostics;
using SteamController.Helpers;

namespace SteamController.Profiles
{
    public sealed class SteamDetectProfile : Profile
    {
        public override void Tick(Context context)
        {
            if (!Settings.Default.EnableSteamDetection)
            {
                context.DisableDueToSteam = false;
                return;
            }

            var usesController = UsesController();

            // if controller is used, disable due to Steam
            context.DisableDueToSteam = usesController ?? true;
        }

        private bool? UsesController()
        {
            if (!SteamManager.IsRunning.GetValueOrDefault(false))
                return null;

            return SteamManager.IsBigPictureMode.GetValueOrDefault(false) || SteamManager.IsRunningGame.GetValueOrDefault(false);
        }

        public override Status Run(Context context)
        {
            return Status.Continue;
        }
    }
}
