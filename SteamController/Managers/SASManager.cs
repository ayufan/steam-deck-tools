using System.Diagnostics;
using SteamController.Helpers;

namespace SteamController.Managers
{
    public sealed class SASManager : Manager
    {
        public override void Tick(Context context)
        {
            context.KeyboardMouseValid = ForegroundProcess.Find() is not null;
        }
    }
}
