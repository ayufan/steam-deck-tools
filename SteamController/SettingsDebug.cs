using System.ComponentModel;
using System.Configuration;

namespace SteamController
{
    [Category("1. Settings")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    internal sealed partial class SettingsDebug : CommonHelpers.BaseSettings
    {
        public static readonly SettingsDebug Default = new SettingsDebug();

        public SettingsDebug() : base("SettingsDebug")
        {
        }

        [Description("Use Lizard Buttons instead of emulated. This option is only for testing purposes.")]
        public bool LizardButtons
        {
            get { return Get<bool>("LizardButtons", false); }
            set { Set("LizardButtons", value); }
        }

        [Description("Use Lizard Mouse instead of emulated. This option is only for testing purposes.")]
        public bool LizardMouse
        {
            get { return Get<bool>("LizardMouse", false); }
            set { Set("LizardMouse", value); }
        }

        public override string ToString()
        {
            return "";
        }
    }
}
