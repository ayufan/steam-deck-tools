using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace CommonHelpers
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public abstract class BaseSettings
    {
        private IDictionary<String, object?> cachedValues = new ConcurrentDictionary<string, object?>();

        public event Action<string> SettingChanging;
        public event Action<string> SettingChanged;

        [Browsable(false)]
        public bool TouchSettings { get; set; }

        [Browsable(false)]
        public bool UseMemoryCache { get; set; } = true;

        [Browsable(false)]
        public bool UseConfigFile { get; set; } = true;

        [Browsable(false)]
        public String SettingsKey { get; protected set; }

        [Browsable(false)]
        public String ConfigFile { get; protected set; }

        protected BaseSettings(string settingsKey)
        {
            this.SettingsKey = settingsKey;
            this.ConfigFile = System.Reflection.Assembly.GetEntryAssembly()?.Location + ".ini";

            this.SettingChanging += delegate { };
            this.SettingChanged += delegate { };
        }

        [Browsable(false)]
        public bool Exists
        {
            get { return !this.UseConfigFile || File.Exists(this.ConfigFile); }
        }

        public override string ToString()
        {
            return "";
        }

        protected bool SetRelativeConfigFile(string relativePath)
        {
            var currentDir = Path.GetDirectoryName(
                System.Reflection.Assembly.GetEntryAssembly()?.Location);
            if (currentDir is null)
                return false;

            this.ConfigFile = Path.Combine(currentDir, relativePath);
            return true;
        }

        protected bool Set<T>(string key, T value)
        {
            var typeConverter = TypeDescriptor.GetConverter(typeof(T));
            var valueString = typeConverter.ConvertToString(value);
            if (valueString is null)
                return false;

            SettingChanging(key);
            if (this.UseConfigFile && !SetProfileString(key, valueString))
                return false;

            cachedValues[key] = value;
            SettingChanged(key);
            return true;
        }

        protected T Get<T>(string key, T defaultValue, bool touchSettings = false)
        {
            if (this.UseMemoryCache)
            {
                if (cachedValues.TryGetValue(key, out var cachedValue))
                    return ((T?)cachedValue) ?? defaultValue;
            }

            var typeConverter = TypeDescriptor.GetConverter(typeof(T));
            var defaultString = typeConverter.ConvertToString(defaultValue);
            if (defaultString is null)
            {
                cachedValues[key] = defaultValue;
                return defaultValue;
            }

            if (!this.UseConfigFile)
            {
                cachedValues[key] = defaultValue;
                return defaultValue;
            }

            try
            {
                var valueString = GetProfileString(key, defaultString);
                var value = (T?)typeConverter.ConvertFromString(valueString);
                if (value is null)
                {
                    cachedValues[key] = defaultValue;
                    return defaultValue;
                }

                if ((TouchSettings || touchSettings) && valueString == defaultString)
                {
                    // Persist current value on a first access
                    SetProfileString(key, valueString);
                }

                cachedValues[key] = value;
                return value;
            }
            catch (Exception e)
            {
                Log.TraceLine("Settings: {0}/{1}: {2}", SettingsKey, key, e);
                cachedValues[key] = defaultValue;
                return defaultValue;
            }
        }

        private string GetProfileString(string key, string defaultValue)
        {
            StringBuilder sb = new StringBuilder(500);
            uint res = GetPrivateProfileString(SettingsKey, key, defaultValue, sb, (uint)sb.Capacity, ConfigFile);
            if (res != 0)
                return sb.ToString();
            return defaultValue;
        }

        private bool SetProfileString(string key, string value)
        {
            lock (this)
            {
                return WritePrivateProfileString(SettingsKey, key, value, ConfigFile);
            }
        }

        public bool DeleteKey(string key)
        {
            lock (this)
            {
                cachedValues.Remove(key);

                if (this.UseConfigFile)
                {
                    return WritePrivateProfileString(SettingsKey, key, null, ConfigFile);
                }
                return true;
            }
        }

        public bool DeleteAll()
        {
            lock (this)
            {
                cachedValues.Clear();

                if (this.UseConfigFile)
                {
                    return WritePrivateProfileString(SettingsKey, null, null, ConfigFile);
                }
                return true;
            }
        }

        public void TouchFile()
        {
            lock (this)
            {
                if (this.UseConfigFile && Exists)
                {
                    using (File.Create(ConfigFile)) { }
                }
            }
        }

        public void DeleteFile()
        {
            lock (this)
            {
                cachedValues.Clear();

                if (this.UseConfigFile)
                {
                    try { File.Delete(ConfigFile); }
                    catch (DirectoryNotFoundException) { }
                }
            }
        }

        [DllImport("kernel32.dll")]
        static extern bool WritePrivateProfileString(string lpAppName, string? lpKeyName, string? lpString, string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);
    }
}
