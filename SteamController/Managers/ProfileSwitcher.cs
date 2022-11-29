using System.Diagnostics;
using SteamController.Helpers;

namespace SteamController.Managers
{
    public sealed class ProfileSwitcher : Manager
    {
        private Context.ContextState wasState;

        public override void Tick(Context context)
        {
            if (wasState.Equals(context.State))
                return;

            if (context.State.IsActive)
                context.SelectController();
            else
                context.BackToDefault();

            wasState = context.State;
        }
    }
}
