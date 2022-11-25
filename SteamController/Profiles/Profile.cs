namespace SteamController.Profiles
{
    public abstract class Profile
    {
        public struct Status
        {
            public static readonly Status Continue = new Status() { IsDone = false };
            public static readonly Status Done = new Status() { IsDone = true };

            public bool IsDone { get; set; }
        }

        public String Name { get; set; } = "";
        public bool Visible { get; set; } = true;
        public bool IsDesktop { get; set; }

        public abstract bool Selected(Context context);

        public abstract Status Run(Context context);
    }
}
