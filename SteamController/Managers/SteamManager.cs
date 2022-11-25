using System.Diagnostics;
using SteamController.Helpers;

namespace SteamController.Managers
{
    public sealed class SteamManager : Manager
    {
        public override void Tick(Context context)
        {
            if (!Settings.Default.EnableSteamDetection)
            {
                context.SteamRunning = true;
                context.SteamUsesController = false;
                return;
            }

            var usesController = UsesController();

            // if controller is used, disable due to Steam unless it is hidden
            context.SteamRunning = usesController is not null;
            context.SteamUsesController = usesController ?? false;
        }

        private bool? UsesController()
        {
            if (!SteamProcess.IsRunning.GetValueOrDefault(false))
                return null;

            return
                SteamProcess.IsBigPictureMode.GetValueOrDefault(false) ||
                SteamProcess.IsRunningGame.GetValueOrDefault(false) ||
                SteamProcess.IsGamePadUI;
        }
    }
}
