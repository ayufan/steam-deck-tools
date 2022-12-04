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

        [UserScopedSetting]
        [DefaultSettingValue("Desktop")]
        [Description("Default profile used when going back to Desktop mode")]
        [BrowsableAttribute(true)]
        public ProfilesSettings.Helpers.ProfileName DefaultProfile
        {
            get { return ((ProfilesSettings.Helpers.ProfileName)(this["DefaultProfile"])); }
            set { this["DefaultProfile"] = value; }
        }

        public enum ScrollMode : int
        {
            DownScrollUp = -1,
            DownScrollDown = 1
        }

        [UserScopedSetting]
        [DefaultSettingValue("DownScrollDown")]
        [Description("Scroll direction for right pad and joystick.")]
        [BrowsableAttribute(true)]
        public ScrollMode ScrollDirection
        {
            get { return ((ScrollMode)(this["ScrollDirection"])); }
            set { this["ScrollDirection"] = value; }
        }

        public enum SteamControllerConfigsMode
        {
            DoNotTouch,
            Overwrite
        }

#if DEBUG
        [UserScopedSetting]
        [BrowsableAttribute(true)]
#else
        [ApplicationScopedSetting]
        [BrowsableAttribute(false)]
#endif
        [DefaultSettingValue("DoNotTouch")]
        [Description("This does replace Steam configuration for controllers to prevent double inputs")]
        public SteamControllerConfigsMode SteamControllerConfigs
        {
            get { return ((SteamControllerConfigsMode)(this["SteamControllerConfigs"])); }
            set { this["SteamControllerConfigs"] = value; }
        }

        public override string ToString()
        {
            return "";
        }
    }
}
