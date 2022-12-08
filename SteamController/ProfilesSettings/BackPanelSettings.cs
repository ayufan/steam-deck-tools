using System.ComponentModel;
using System.Configuration;

namespace SteamController.ProfilesSettings
{
    [Category("Mappings")]
    internal abstract class BackPanelSettings : BaseSettings
    {
        private const String MappingsDescription = @"Only some of those keys do work. Allowed mappings are to be changed in future release.";

        public BackPanelSettings(String settingsKey) : base(settingsKey)
        {
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("None")]
        [Description(MappingsDescription)]
        public VirtualKeyCode L4_KEY
        {
            get { return ((VirtualKeyCode)(this["L4_KEY"])); }
            set { this["L4_KEY"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("None")]
        [Description(MappingsDescription)]
        public VirtualKeyCode L5_KEY
        {
            get { return ((VirtualKeyCode)(this["L5_KEY"])); }
            set { this["L5_KEY"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("None")]
        [Description(MappingsDescription)]
        public VirtualKeyCode R4_KEY
        {
            get { return ((VirtualKeyCode)(this["R4_KEY"])); }
            set { this["R4_KEY"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("None")]
        [Description(MappingsDescription)]
        public VirtualKeyCode R5_KEY
        {
            get { return ((VirtualKeyCode)(this["R5_KEY"])); }
            set { this["R5_KEY"] = value; }
        }
    }
}
