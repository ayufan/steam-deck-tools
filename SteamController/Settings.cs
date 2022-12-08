using System.ComponentModel;
using System.Configuration;

namespace SteamController
{
    [Category("Settings")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    internal sealed partial class Settings : CommonHelpers.BaseSettings
    {
        public static readonly Settings Default = new Settings();
        private static readonly ProfilesSettings.Helpers.ProfileName DefaultProfileDefault = new ProfilesSettings.Helpers.ProfileName("Default");

        public Settings() : base("Settings")
        {
        }

        [BrowsableAttribute(false)]
        public bool EnableSteamDetection
        {
            get { return Get<bool>("EnableSteamDetection", false); }
            set { Set("EnableSteamDetection", value); }
        }

        [Description("Default profile used when going back to Desktop mode")]
        [BrowsableAttribute(true)]
        public ProfilesSettings.Helpers.ProfileName DefaultProfile
        {
            get { return Get<ProfilesSettings.Helpers.ProfileName>("DefaultProfile", DefaultProfileDefault); }
            set { Set("DefaultProfile", value); }
        }

        public enum ScrollMode : int
        {
            DownScrollUp = -1,
            DownScrollDown = 1
        }

        [Description("Scroll direction for right pad and joystick.")]
        [BrowsableAttribute(true)]
        public ScrollMode ScrollDirection
        {
            get { return Get<ScrollMode>("ScrollDirection", ScrollMode.DownScrollDown); }
            set { Set("ScrollDirection", value); }
        }

        public enum SteamControllerConfigsMode
        {
            DoNotTouch,
            Overwrite
        }

        [BrowsableAttribute(true)]
        [Description("This does replace Steam configuration for controllers to prevent double inputs. " +
            "Might require going to Steam > Settings > Controller > Desktop to apply " +
            "'SteamController provided empty configuration'.")]
        public SteamControllerConfigsMode SteamControllerConfigs
        {
            get { return Get<SteamControllerConfigsMode>("SteamControllerConfigs", SteamControllerConfigsMode.Overwrite); }
            set { Set("SteamControllerConfigs", value); }
        }

        [UserScopedSetting]
        [BrowsableAttribute(true)]
        [DefaultSettingValue("True")]
        [Description("Show Touch Keyboard or CTRL+WIN+O")]
        public bool ShowTouchKeyboard
        {
            get { return Get<bool>("ShowTouchKeyboard", true); }
            set { Set("ShowTouchKeyboard", value); }
        }

        public override string ToString()
        {
            return "";
        }
    }
}
