using System.ComponentModel;
using System.Configuration;

namespace SteamController
{
    [Category("Settings")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    internal sealed partial class Settings : ApplicationSettingsBase
    {
        public static readonly Settings Default = (Settings)Synchronized(new Settings());

        public Settings()
        {
            PropertyChanged += delegate
            {
                Save();
            };
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        [BrowsableAttribute(false)]
        public bool EnableSteamDetection
        {
            get { return ((bool)(this["EnableSteamDetection"])); }
            set { this["EnableSteamDetection"] = value; }
        }

        [ApplicationScopedSetting]
        [DefaultSettingValue("Desktop")]
        [BrowsableAttribute(false)]
        public string StartupProfile
        {
            get { return ((string)(this["StartupProfile"])); }
            set { this["StartupProfile"] = value; }
        }

#if DEBUG
        [UserScopedSetting]
        [BrowsableAttribute(true)]
#else
        [ApplicationScopedSetting]
        [BrowsableAttribute(false)]
#endif
        [DefaultSettingValue("False")]
        [DisplayName("Manage Steam Controller Configs")]
        [Description("This does replace Steam configuration for controllers to prevent double inputs")]
        public bool ManageSteamControllerConfigs
        {
            get { return ((bool)(this["ManageSteamControllerConfigs"])); }
            set { this["ManageSteamControllerConfigs"] = value; }
        }

        public override string ToString()
        {
            return "";
        }
    }
}
