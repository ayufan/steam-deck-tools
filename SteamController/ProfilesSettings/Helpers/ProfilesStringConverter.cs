using System.ComponentModel;
using System.Globalization;

namespace SteamController.ProfilesSettings.Helpers
{
    internal class ProfileStringConverter : TypeConverter
    {
        public static List<Profiles.Profile> Profiles = new List<Profiles.Profile>();

        private volatile StandardValuesCollection? collection;

        public override StandardValuesCollection? GetStandardValues(ITypeDescriptorContext? context)
        {
            return collection ??= new StandardValuesCollection(Profiles.
                Where((profile) => profile.Visible).
                Select((profile) => profile.Name).
                ToArray());
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
            return Profiles.Find((profile) => profile.Name == value?.ToString()) is not null;
        }
    }
}
