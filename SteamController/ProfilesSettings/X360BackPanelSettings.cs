using System.ComponentModel;
using System.Configuration;

namespace SteamController.ProfilesSettings
{
    internal class X360BackPanelSettings : BackPanelSettings
    {
        private const String MappingsDescription = @"Mappings are to be changed in future release.";

        public static X360BackPanelSettings Default { get; } = (X360BackPanelSettings)ApplicationSettingsBase.Synchronized(
          new X360BackPanelSettings("X360BackPanelSettings"));

        public X360BackPanelSettings(String settingsKey) : base(settingsKey)
        {
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("None")]
        [Description(MappingsDescription)]
        public VirtualX360Code L4_X360
        {
            get { return ((VirtualX360Code)(this["L4_X360"])); }
            set { this["L4_X360"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("None")]
        [Description(MappingsDescription)]
        public VirtualX360Code L5_X360
        {
            get { return ((VirtualX360Code)(this["L5_X360"])); }
            set { this["L5_X360"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("None")]
        [Description(MappingsDescription)]
        public VirtualX360Code R4_X360
        {
            get { return ((VirtualX360Code)(this["R4_X360"])); }
            set { this["R4_X360"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("None")]
        [Description(MappingsDescription)]
        public VirtualX360Code R5_X360
        {
            get { return ((VirtualX360Code)(this["R5_X360"])); }
            set { this["R5_X360"] = value; }
        }
    }
}