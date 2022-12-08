using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace CommonHelpers
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public abstract class BaseSettings
    {
        private String settingsKey;
        private String configFile;
        private IDictionary<String, object?> cachedValues = new ConcurrentDictionary<string, object?>();

        public event Action<string> SettingChanging;
        public event Action<string> SettingChanged;

        [Browsable(false)]
        public bool TouchSettings { get; set; }

        protected BaseSettings(string settingsKey)
        {
            this.settingsKey = settingsKey;
            this.configFile = System.Reflection.Assembly.GetEntryAssembly()?.Location + ".ini";

            this.SettingChanging += delegate { };
            this.SettingChanged += delegate { };
        }

        public override string ToString()
        {
            return "";
        }

        protected bool Set<T>(string key, T value)
        {
            var typeConverter = TypeDescriptor.GetConverter(typeof(T));
            var valueString = typeConverter.ConvertToString(value);
            if (valueString is null)
                return false;

            SettingChanging(key);
            if (!SetString(key, valueString))
                return false;

            cachedValues[key] = value;
            SettingChanged(key);
            return true;
        }

        protected T Get<T>(string key, T defaultValue, bool touchSettings = false)
        {
            if (cachedValues.TryGetValue(key, out var cachedValue))
                return ((T?)cachedValue) ?? defaultValue;

            var typeConverter = TypeDescriptor.GetConverter(typeof(T));
            var defaultString = typeConverter.ConvertToString(defaultValue);
            if (defaultString is null)
            {
                cachedValues[key] = defaultValue;
                return defaultValue;
            }

            try
            {
                var valueString = GetString(key, defaultString);
                var value = (T?)typeConverter.ConvertFromString(valueString);
                if (value is null)
                {
                    cachedValues[key] = defaultValue;
                    return defaultValue;
                }

                if ((TouchSettings || touchSettings) && valueString == defaultString)
                {
                    // Persist current value on a first access
                    SetString(key, valueString);
                }

                cachedValues[key] = value;
                return value;
            }
            catch (Exception e)
            {
                Log.TraceLine("Settings: {0}/{1}: {2}", settingsKey, key, e);
                cachedValues[key] = defaultValue;
                return defaultValue;
            }
        }

        protected string GetString(string key, string defaultValue)
        {
            StringBuilder sb = new StringBuilder(500);
            uint res = GetPrivateProfileString(settingsKey, key, defaultValue, sb, (uint)sb.Capacity, configFile);
            if (res != 0)
                return sb.ToString();
            return defaultValue;
        }

        protected bool SetString(string key, string value)
        {
            lock (this)
            {
                return WritePrivateProfileString(settingsKey, key, value, configFile);
            }
        }

        [DllImport("kernel32.dll")]
        static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);
    }
}
