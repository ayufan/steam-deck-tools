using System.ComponentModel;
using System.Configuration;
using WindowsInput;

namespace SteamController.ProfilesSettings
{
    [Category("Settings")]
    internal sealed class X360HapticSettings : BaseSettings
    {
        public const sbyte MinIntensity = -2;
        public const sbyte MaxIntensity = 10;

        public static X360HapticSettings Default { get; } = (X360HapticSettings)ApplicationSettingsBase.Synchronized(
            new X360HapticSettings("X360HapticSettings"));

        public X360HapticSettings(String settingsKey) : base(settingsKey)
        {
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("Weak")]
        public Devices.SteamController.HapticStyle HapticStyle
        {
            get { return ((Devices.SteamController.HapticStyle)(this["HapticStyle"])); }
            set { this["HapticStyle"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("2")]
        [Description("Haptic intensity between -2dB and 10dB")]
        public sbyte LeftIntensity
        {
            get { return ((sbyte)(this["LeftIntensity"])); }
            set { this["LeftIntensity"] = Math.Clamp(value, MinIntensity, MaxIntensity); }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("2")]
        [Description("Haptic intensity between -2dB and 10dB")]
        public sbyte RightIntensity
        {
            get { return ((sbyte)(this["RightIntensity"])); }
            set { this["RightIntensity"] = Math.Clamp(value, MinIntensity, MaxIntensity); }
        }
    }
}
