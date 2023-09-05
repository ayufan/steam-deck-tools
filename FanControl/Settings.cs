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

        public int Silent4000RPMTemp
        {
            get { return ClampSilent4000RPMTemp(Get("Silent4000RPMTemp", 85)); }
            set { Set("Silent4000RPMTemp", ClampSilent4000RPMTemp(value)); }
        }

        public bool EnableExperimentalFeatures
        {
            get { return Instance.IsDEBUG; }
        }

        private int ClampSilent4000RPMTemp(int value)
        {
            return Math.Clamp(value, 70, 90);
        }
    }
}
