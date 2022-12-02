using SteamController.Profiles;

namespace SteamController.Managers
{
    public abstract class Manager : IDisposable
    {
        public abstract void Tick(Context context);

        public virtual void Dispose()
        {
        }
    }
}
