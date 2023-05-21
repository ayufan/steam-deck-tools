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
        public bool? EnableSteamDetection
        {
            get { return Get<bool?>("EnableSteamDetection", null); }
            set { Set("EnableSteamDetection", value); }
        }

        [Description("Keep X360 controller connected always - it is strongly advised to disable this option. Might be required by some games that do not like disonnecting controller. Will disable beep notifications.")]
        public bool KeepX360AlwaysConnected
        {
            get { return Get<bool>("KeepX360AlwaysConnected", false); }
            set { Set("KeepX360AlwaysConnected", value); }
        }

        [Description("Enable DS4 support. If disabled the DS4 will be hidden.")]
        public bool EnableDS4Support
        {
            get { return Get<bool>("EnableDS4Support", true); }
            set { Set("EnableDS4Support", value); }
        }

        [Description("If current foreground process uses overlay, treat it as a game.")]
        public bool DetectRTSSForeground
        {
            get { return Get<bool>("DetectRTSSForeground", false); }
            set { Set("DetectRTSSForeground", value); }
        }

        [Description("Create a debug log in Documents/SteamDeckTools/Logs.")]
        public bool EnableDebugLogging
        {
            get { return Get<bool>("EnableDebugLogging", false); }
            set { Set("EnableDebugLogging", value); CommonHelpers.Log.LogToFileDebug = value; }
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
