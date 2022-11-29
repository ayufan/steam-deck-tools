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
                context.State.SteamUsesSteamInput = false;
                context.State.SteamUsesX360Controller = false;
                lastState = false;
                return;
            }

            var usesController = UsesController() ?? false;
            if (lastState == usesController)
                return;

            if (usesController)
            {
                context.State.SteamUsesSteamInput = Helpers.SteamConfiguration.IsControllerBlacklisted(
                    Devices.SteamController.VendorID,
                    Devices.SteamController.ProductID
                ) != true;

                context.State.SteamUsesX360Controller = Helpers.SteamConfiguration.IsControllerBlacklisted(
                    Devices.Xbox360Controller.VendorID,
                    Devices.Xbox360Controller.ProductID
                ) != true;
            }
            else
            {
                context.State.SteamUsesSteamInput = false;
                context.State.SteamUsesX360Controller = false;
            }

            lastState = usesController;
        }

        private bool? UsesController()
        {
            if (!SteamConfiguration.IsRunning)
                return null;

            return
                SteamConfiguration.IsBigPictureMode.GetValueOrDefault(false) ||
                SteamConfiguration.IsRunningGame.GetValueOrDefault(false) ||
                SteamConfiguration.IsGamePadUI;
        }
    }
}
