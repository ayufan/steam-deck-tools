using System.ComponentModel;
using System.Configuration;

namespace SteamController.ProfilesSettings
{
    [Category("Mappings")]
    internal sealed class BackPanelSettings : BaseSettings
    {
        private const String MappingsDescription = @"Only some of those keys do work. Allowed mappings are to be changed in future release.";

        public static BackPanelSettings X360 { get; } = (BackPanelSettings)ApplicationSettingsBase.Synchronized(
          new BackPanelSettings("X360BackPanelSettings"));
        public static BackPanelSettings Desktop { get; } = (BackPanelSettings)ApplicationSettingsBase.Synchronized(
          new BackPanelSettings("DesktopBackPanelSettings"));

        public BackPanelSettings(String settingsKey) : base(settingsKey)
        {
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("None")]
        [Description(MappingsDescription)]
        public VirtualKeyCode L4
        {
            get { return ((VirtualKeyCode)(this["L4"])); }
            set { this["L4"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("None")]
        [Description(MappingsDescription)]
        public VirtualKeyCode L5
        {
            get { return ((VirtualKeyCode)(this["L5"])); }
            set { this["L5"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("None")]
        [Description(MappingsDescription)]
        public VirtualKeyCode R4
        {
            get { return ((VirtualKeyCode)(this["R4"])); }
            set { this["R4"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("None")]
        [Description(MappingsDescription)]
        public VirtualKeyCode R5
        {
            get { return ((VirtualKeyCode)(this["R5"])); }
            set { this["R5"] = value; }
        }
    }
}