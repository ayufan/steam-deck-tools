using System.ComponentModel;
using System.Configuration;

namespace SteamController.ProfilesSettings
{
    internal class DesktopPanelSettings : BackPanelSettings
    {
        public static DesktopPanelSettings Default { get; } = new DesktopPanelSettings();

        public DesktopPanelSettings() : base("DesktopPanelSettings")
        {
        }
    }
}