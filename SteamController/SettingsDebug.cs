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
