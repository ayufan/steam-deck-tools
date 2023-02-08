using CommonHelpers;

namespace SteamController.Managers
{
    public sealed class RTSSManager : Manager
    {
        public override void Tick(Context context)
        {
            context.State.RTSSInForeground = Settings.Default.DetectRTSSForeground && OSDHelpers.IsOSDForeground();
        }
    }
}
