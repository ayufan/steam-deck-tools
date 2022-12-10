using System.ComponentModel;
using System.Configuration;

namespace SteamController.ProfilesSettings
{
    [Category("Shortcuts")]
    internal abstract class BackPanelSettings : CommonHelpers.BaseSettings
    {
        private const String MappingsDescription = @"Only some of those keys do work. Allowed shortcuts are to be changed in future release.";

        public BackPanelSettings(String settingsKey) : base(settingsKey)
        {
        }

        [Description(MappingsDescription)]
        public VirtualKeyCode L4_KEY
        {
            get { return Get<VirtualKeyCode>("L4_KEY", VirtualKeyCode.None); }
            set { Set("L4_KEY", value); }
        }

        [Description(MappingsDescription)]
        public VirtualKeyCode L5_KEY
        {
            get { return Get<VirtualKeyCode>("L5_KEY", VirtualKeyCode.None); }
            set { Set("L5_KEY", value); }
        }

        [Description(MappingsDescription)]
        public VirtualKeyCode R4_KEY
        {
            get { return Get<VirtualKeyCode>("R4_KEY", VirtualKeyCode.None); }
            set { Set("R4_KEY", value); }
        }

        [Description(MappingsDescription)]
        public VirtualKeyCode R5_KEY
        {
            get { return Get<VirtualKeyCode>("R5_KEY", VirtualKeyCode.None); }
            set { Set("R5_KEY", value); }
        }
    }
}
