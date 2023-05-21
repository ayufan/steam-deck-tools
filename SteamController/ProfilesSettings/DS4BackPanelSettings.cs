using System.ComponentModel;
using System.Configuration;

namespace SteamController.ProfilesSettings
{
    internal class DS4BackPanelSettings : BackPanelSettings
    {
        private const String MappingsDescription = @"Shortcuts are to be changed in future release.";

        public static DS4BackPanelSettings Default { get; } = new DS4BackPanelSettings();

        public DS4BackPanelSettings() : base("DS4BackPanelSettings")
        {
        }

        [Description(MappingsDescription)]
        public VirtualDS4Code L4_DS4
        {
            get { return Get<VirtualDS4Code>("L4_DS4", VirtualDS4Code.None); }
            set { Set("L4_DS4", value); }
        }

        [Description(MappingsDescription)]
        public VirtualDS4Code L5_DS4
        {
            get { return Get<VirtualDS4Code>("L5_DS4", VirtualDS4Code.None); }
            set { Set("L5_DS4", value); }
        }

        [Description(MappingsDescription)]
        public VirtualDS4Code R4_DS4
        {
            get { return Get<VirtualDS4Code>("R4_DS4", VirtualDS4Code.None); }
            set { Set("R4_DS4", value); }
        }

        [Description(MappingsDescription)]
        public VirtualDS4Code R5_DS4
        {
            get { return Get<VirtualDS4Code>("R5_DS4", VirtualDS4Code.None); }
            set { Set("R5_DS4", value); }
        }
    }
}