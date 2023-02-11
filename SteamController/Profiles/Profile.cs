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

        public event Action<string[]> ErrorsChanged;

        public virtual String Name { get; set; } = "";
        public virtual String FullName { get; } = "";
        public virtual bool Visible { get; set; } = true;
        public virtual bool IsDesktop { get; set; }
        public virtual string[]? Errors { get; set; }
        public abstract System.Drawing.Icon Icon { get; }

        public abstract bool Selected(Context context);

        public abstract Status Run(Context context);

        public Profile()
        {
            ErrorsChanged += delegate { };
        }

        protected void OnErrorsChanged()
        {
            ErrorsChanged(this.Errors ?? new string[0]);
        }
    }
}
