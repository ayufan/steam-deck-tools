using System.Diagnostics;
using SteamController.Helpers;

namespace SteamController.Managers
{
    public sealed class ProfileSwitcher : Manager
    {
        [Flags]
        private enum ActiveMode
        {
            None,
            SteamInput = 1,
            SteamX360 = 2,
            OtherGame = 4
        }

        private ActiveMode wasActive;

        public override void Tick(Context context)
        {
            ActiveMode active = GetActiveMode(context);
            if (wasActive == active)
                return;

            if (active != ActiveMode.None)
                context.SelectController();
            else
                context.BackToDefault();

            wasActive = active;
        }

        private ActiveMode GetActiveMode(Context context)
        {
            ActiveMode mode = ActiveMode.None;
            if (context.SteamUsesSteamInput)
                mode |= ActiveMode.SteamInput;
            if (context.SteamUsesX360Controller)
                mode |= ActiveMode.SteamX360;
            if (context.GameProcessRunning)
                mode |= ActiveMode.OtherGame;
            return mode;
        }
    }
}
