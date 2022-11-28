using System.ComponentModel;
using System.Configuration;
using WindowsInput;

namespace SteamController.ProfilesSettings
{
    [Category("Settings")]
    internal sealed class X360RumbleSettings : BaseSettings
    {
        public static X360RumbleSettings Default { get; } = (X360RumbleSettings)ApplicationSettingsBase.Synchronized(
            new X360RumbleSettings("X360RumbleSettings"));

        public X360RumbleSettings(String settingsKey) : base(settingsKey)
        {
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("0")]
        [Description("Use Fixed Amplitude when configured. Set to 0 to disable")]
        public byte FixedAmplitude
        {
            get { return ((byte)(this["FixedAmplitude"])); }
            set { this["FixedAmplitude"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("255")]
        [Description("Scale rumble intensity up to Maximum Amplitude based on feedback request received")]
        public byte MaxAmplitude
        {
            get { return ((byte)(this["MaxAmplitude"])); }
            set { this["MaxAmplitude"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("10")]
        [Description("Rumble feedback period")]
        public byte Period
        {
            get { return ((byte)(this["Period"])); }
            set { this["Period"] = value; }
        }
    }
}
