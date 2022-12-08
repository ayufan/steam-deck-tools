using System.ComponentModel;
using System.Globalization;

namespace SteamController.ProfilesSettings.Helpers
{
    internal class ProfileStringConverter : TypeConverter
    {
        public static string[] Profiles = new string[0];

        private volatile StandardValuesCollection? collection;

        public override StandardValuesCollection? GetStandardValues(ITypeDescriptorContext? context)
        {
            return collection ??= new StandardValuesCollection(Profiles.ToArray());
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext? context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context)
        {
            return true;
        }

        public override bool IsValid(ITypeDescriptorContext? context, object? value)
        {
            return Profiles.Contains(value?.ToString());
        }
    }
}
