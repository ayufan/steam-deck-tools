using System.ComponentModel;
using System.Configuration;

namespace SteamController
{
    [Category("Settings")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    internal sealed partial class SettingsDebug : CommonHelpers.BaseSettings
    {
        public static readonly SettingsDebug Default = new SettingsDebug();

        public SettingsDebug() : base("SettingsDebug")
        {
        }

        [Description("Keep X360 controller connected always - it is strongly advised to disable this option. Might be required by some games that do not like disonnecting controller. Will disable beep notifications.")]
        public bool KeepX360AlwaysConnected
        {
            get { return Get<bool>("KeepX360AlwaysConnected", false); }
            set { Set("KeepX360AlwaysConnected", value); }
        }

        [Description("Use Lizard Buttons instead of emulated. This option is only for testing purposes.")]
        public bool LizardButtons { get; set; } = false;

        [Description("Use Lizard Mouse instead of emulated. This option is only for testing purposes.")]
        public bool LizardMouse { get; set; } = true;

        public override string ToString()
        {
            return "";
        }
    }
}
