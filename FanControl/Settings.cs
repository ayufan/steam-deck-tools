using CommonHelpers;

namespace FanControl
{
    internal sealed class Settings : BaseSettings
    {
        public static readonly Settings Default = new Settings();

        public Settings() : base("Settings")
        {
            TouchSettings = true;
        }

        public FanMode FanMode
        {
            get { return Get<FanMode>("FanMode", CommonHelpers.FanMode.Default); }
            set { Set("FanMode", value); }
        }

        public bool AlwaysOnTop
        {
            get { return Get<bool>("AlwaysOnTop", true); }
            set { Set("AlwaysOnTop", value); }
        }

        public bool EnableExperimentalFeatures
        {
            get { return Instance.IsDEBUG; }
        }
    }
}
