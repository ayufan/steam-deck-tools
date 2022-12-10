using System.ComponentModel;
using System.Configuration;

namespace SteamController.ProfilesSettings
{
    internal class X360BackPanelSettings : BackPanelSettings
    {
        private const String MappingsDescription = @"Shortcuts are to be changed in future release.";

        public static X360BackPanelSettings Default { get; } = new X360BackPanelSettings();

        public X360BackPanelSettings() : base("X360BackPanelSettings")
        {
        }

        [Description(MappingsDescription)]
        public VirtualX360Code L4_X360
        {
            get { return Get<VirtualX360Code>("L4_X360", VirtualX360Code.None); }
            set { Set("L4_X360", value); }
        }

        [Description(MappingsDescription)]
        public VirtualX360Code L5_X360
        {
            get { return Get<VirtualX360Code>("L5_X360", VirtualX360Code.None); }
            set { Set("L5_X360", value); }
        }

        [Description(MappingsDescription)]
        public VirtualX360Code R4_X360
        {
            get { return Get<VirtualX360Code>("R4_X360", VirtualX360Code.None); }
            set { Set("R4_X360", value); }
        }

        [Description(MappingsDescription)]
        public VirtualX360Code R5_X360
        {
            get { return Get<VirtualX360Code>("R5_X360", VirtualX360Code.None); }
            set { Set("R5_X360", value); }
        }
    }
}