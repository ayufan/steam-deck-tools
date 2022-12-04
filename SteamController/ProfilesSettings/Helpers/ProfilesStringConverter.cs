using System.ComponentModel;
using System.Globalization;

namespace SteamController.ProfilesSettings.Helpers
{
    [TypeConverter(typeof(ProfileNameConverter))]
    public class ProfileName
    {
        public String Name { get; set; } = "";

        public ProfileName(string name)
        {
            this.Name = name;
        }

        public static implicit operator string(ProfileName name)
        {
            return name.Name;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }

    internal class ProfileNameConverter : TypeConverter
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

        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object value)
        {
            if (value is string)
                return new ProfileName(value?.ToString() ?? "");
            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        {
            return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (destinationType == typeof(string))
                return value?.ToString();
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool IsValid(ITypeDescriptorContext? context, object? value)
        {
            return Profiles.Contains(value?.ToString()) || base.IsValid(context, value);
        }
    }
}