namespace SteamController.Profiles
{
    public abstract class Profile
    {
        public enum Status
        {
            Continue,
            Stop
        }

        public bool RunAlways { get; set; }

        public abstract Status Run(Context context);

        public virtual void Tick(Context context)
        {
        }

        public virtual void Skipped(Context context)
        {
        }
    }
}
