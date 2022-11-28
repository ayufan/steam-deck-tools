using System.ComponentModel;
using System.Configuration;
using WindowsInput;

namespace SteamController.ProfilesSettings
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    internal abstract class BaseSettings : ApplicationSettingsBase
    {
        public BaseSettings(String settingsKey) : base(settingsKey)
        {
            PropertyChanged += delegate
            {
                Save();
            };
        }

        public override string ToString()
        {
            return "";
        }
    }
}