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
    }
}