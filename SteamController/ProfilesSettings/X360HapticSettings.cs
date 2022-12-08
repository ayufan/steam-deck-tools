using System.ComponentModel;
using System.Configuration;

namespace SteamController.ProfilesSettings
{
    [Category("Settings")]
    internal sealed class X360HapticSettings : CommonHelpers.BaseSettings
    {
        public const sbyte MinIntensity = -2;
        public const sbyte MaxIntensity = 10;

        public static X360HapticSettings Default = new X360HapticSettings();

        public X360HapticSettings() : base("X360HapticSettings")
        {
        }

        public Devices.SteamController.HapticStyle HapticStyle
        {
            get { return Get<Devices.SteamController.HapticStyle>("HapticStyle", Devices.SteamController.HapticStyle.Weak); }
            set { Set("HapticStyle", value); }
        }

        [Description("Haptic intensity between -2dB and 10dB")]
        public sbyte LeftIntensity
        {
            get { return Get<sbyte>("LeftIntensity", 2); }
            set { Set("LeftIntensity", Math.Clamp(value, MinIntensity, MaxIntensity)); }
        }

        [Description("Haptic intensity between -2dB and 10dB")]
        public sbyte RightIntensity
        {
            get { return Get<sbyte>("RightIntensity", 2); }
            set { Set("RightIntensity", Math.Clamp(value, MinIntensity, MaxIntensity)); }
        }
    }
}
