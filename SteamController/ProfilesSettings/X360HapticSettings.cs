using System.ComponentModel;
using System.Configuration;

namespace SteamController.ProfilesSettings
{
    [Category("Settings")]
    internal sealed class HapticSettings : CommonHelpers.BaseSettings
    {
        public const sbyte MinIntensity = -2;
        public const sbyte MaxIntensity = 10;

        public static HapticSettings X360 = new HapticSettings("X360HapticSettings");
        public static HapticSettings DS4 = new HapticSettings("DS4HapticSettings");

        public HapticSettings(string name) : base(name)
        {
        }

        public static bool GetHapticIntensity(byte? input, sbyte maxIntensity, out sbyte output)
        {
            output = default;
            if (input is null || input.Value == 0)
                return false;

            int value = MinIntensity + (maxIntensity - MinIntensity) * input.Value / 255;
            output = (sbyte)value;
            return true;
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
