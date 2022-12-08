using System.ComponentModel;
using System.Configuration;

namespace SteamController.ProfilesSettings
{
    internal class DesktopPanelSettings : BackPanelSettings
    {
        public static DesktopPanelSettings Default { get; } = (DesktopPanelSettings)ApplicationSettingsBase.Synchronized(
          new DesktopPanelSettings("DesktopPanelSettings"));

        public DesktopPanelSettings(String settingsKey) : base(settingsKey)
        {
        }
    }
}