using System.Diagnostics;
using SteamController.Helpers;

namespace SteamController.Managers
{
    public sealed class SteamManager : Manager
    {
        private bool lastState;

        public override void Tick(Context context)
        {
            if (!Settings.Default.EnableSteamDetection)
            {
                context.SteamUsesSteamInput = false;
                context.SteamUsesX360Controller = false;
                lastState = false;
                return;
            }

            var usesController = UsesController() ?? false;
            if (lastState == usesController)
                return;

            if (usesController)
            {
                context.SteamUsesSteamInput = Helpers.SteamConfiguration.IsControllerBlacklisted(
                    Devices.SteamController.VendorID,
                    Devices.SteamController.ProductID
                ) != true;

                context.SteamUsesX360Controller = Helpers.SteamConfiguration.IsControllerBlacklisted(
                    Devices.Xbox360Controller.VendorID,
                    Devices.Xbox360Controller.ProductID
                ) != true;

                context.ToggleDesktopMode(false);
            }
            else
            {
                context.SteamUsesSteamInput = false;
                context.SteamUsesX360Controller = false;
                context.ToggleDesktopMode(true);
            }

            lastState = usesController;
        }

        private bool? UsesController()
        {
            if (!SteamConfiguration.IsRunning.GetValueOrDefault(false))
                return null;

            return
                SteamConfiguration.IsBigPictureMode.GetValueOrDefault(false) ||
                SteamConfiguration.IsRunningGame.GetValueOrDefault(false) ||
                SteamConfiguration.IsGamePadUI;
        }
    }
}
