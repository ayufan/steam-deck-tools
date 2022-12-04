using System.Diagnostics;
using SteamController.Helpers;

namespace SteamController.Managers
{
    public sealed class SteamManager : Manager
    {
        public const int DebounceStates = 1;

        private string? lastState;
        private int stateChanged;

        public override void Tick(Context context)
        {
            if (!Settings.Default.EnableSteamDetection)
            {
                context.State.SteamUsesSteamInput = false;
                context.State.SteamUsesX360Controller = false;
                lastState = null;
                stateChanged = 0;
                return;
            }

            var usesController = UsesController();
            if (lastState == usesController)
            {
                stateChanged = 0;
                return;
            }
            else if (stateChanged < DebounceStates)
            {
                stateChanged++;
                return;
            }

            if (usesController is not null)
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
            stateChanged = 0;

#if DEBUG
            CommonHelpers.Log.TraceLine(
                "SteamManager: uses={0}, isRunning={1}, usesSteamInput={2}, usesX360={3}",
                usesController,
                SteamConfiguration.IsRunning,
                context.State.SteamUsesSteamInput,
                context.State.SteamUsesX360Controller
            );
#endif
        }

        private string? UsesController()
        {
            if (!SteamConfiguration.IsRunning)
                return null;
            if (SteamConfiguration.IsBigPictureMode.GetValueOrDefault(false))
                return "bigpicture";
            if (SteamConfiguration.IsRunningGame.GetValueOrDefault(false))
                return "game";
            if (SteamConfiguration.IsGamePadUI)
                return "gamepadui";
            return null;
        }
    }
}
