using System.ComponentModel;
using System.Configuration;

namespace SteamController
{
    [Category("Settings")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    internal sealed partial class Settings : CommonHelpers.BaseSettings
    {
        public static readonly Settings Default = new Settings();

        public Settings() : base("Settings")
        {
        }

        [Browsable(false)]
        public bool EnableSteamDetection
        {
            get { return Get<bool>("EnableSteamDetection", false); }
            set { Set("EnableSteamDetection", value); }
        }

        [Description("Default profile used when going back to Desktop mode")]
        [Browsable(true)]
        [TypeConverter(typeof(ProfilesSettings.Helpers.ProfileStringConverter))]
        public string DefaultProfile
        {
            get { return Get<string>("DefaultProfile", "Desktop"); }
            set { Set("DefaultProfile", value); }
        }

        public enum ScrollMode : int
        {
            DownScrollUp = -1,
            DownScrollDown = 1
        }

        [Description("Scroll direction for right pad and joystick.")]
        [Browsable(true)]
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

        [Browsable(true)]
        [Description("This does replace Steam configuration for controllers to prevent double inputs. " +
            "Might require going to Steam > Settings > Controller > Desktop to apply " +
            "'SteamController provided empty configuration'.")]
        public SteamControllerConfigsMode SteamControllerConfigs
        {
            get { return Get<SteamControllerConfigsMode>("SteamControllerConfigs", SteamControllerConfigsMode.Overwrite); }
            set { Set("SteamControllerConfigs", value); }
        }

        public enum KeyboardStyles
        {
            DoNotShow,
            WindowsTouch,
            CTRL_WIN_O
        }

        [Browsable(true)]
        [Description("Show Touch Keyboard or CTRL+WIN+O")]
        public KeyboardStyles KeyboardStyle
        {
            get { return Get<KeyboardStyles>("KeyboardStyle", KeyboardStyles.WindowsTouch); }
            set { Set("KeyboardStyle", value); }
        }

        public override string ToString()
        {
            return "";
        }
    }
}
