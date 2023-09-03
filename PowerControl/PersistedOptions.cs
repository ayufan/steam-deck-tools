using PowerControl.Options;

namespace PowerControl
{
    public class PersistedOptions : CommonHelpers.BaseSettings
    {
        public const string OptionsKey = "Options";

        public PersistedOptions(string name) : base(name + OptionsKey)
        {
        }

        public struct Option
        {
            public PersistedOptions Options { get; set; }
            public string Key { get; set; }

            public string FullKey(string setting)
            {
                return String.Format("{0}_{1}", Key, setting);
            }

            public bool Exist
            {
                get => Options.GetOptions().Contains(Key);
            }

            public Option Set<T>(string setting, T value)
            {
                // Get and persist value on first access
                Options.Get(FullKey(setting), value, true);
                return this;
            }

            public T Get<T>(string setting, T defaultValue)
            {
                // Get and do not persist value
                return Options.Get(FullKey(setting), defaultValue, false);
            }

            public static bool operator <(Option a, Option b)
            {
                if (a.Options.SettingsKey == TDP.Name + OptionsKey && b.Options.SettingsKey == TDP.Name + OptionsKey)
                {
                    return a.Get(TDP.SlowTDP, TDP.DefaultSlowTDP) < b.Get(TDP.SlowTDP, TDP.DefaultSlowTDP)
                        && a.Get(TDP.FastTDP, TDP.DefaultFastTDP) < b.Get(TDP.FastTDP, TDP.DefaultFastTDP);
                }
                else if (a.Options.SettingsKey == CPUFrequency.Name + OptionsKey && b.Options.SettingsKey == CPUFrequency.Name + OptionsKey)
                {
                    return a.Get(CPUFrequency.SoftMin, CPUFrequency.DefaultMin) < b.Get(CPUFrequency.SoftMin, CPUFrequency.DefaultMin)
                        && a.Get(CPUFrequency.SoftMax, CPUFrequency.DefaultMax) < b.Get(CPUFrequency.SoftMax, CPUFrequency.DefaultMax);
                }
                else
                    throw new NotSupportedException($"Operation \"<\" is not supported for operands of types \"{a.Options.SettingsKey}\" and \"{b.Options.SettingsKey}\".");
            }

            public static bool operator >(Option a, Option b)
            {
                if (a.Options.SettingsKey == TDP.Name + OptionsKey && b.Options.SettingsKey == TDP.Name + OptionsKey)
                {
                    return a.Get(TDP.SlowTDP, TDP.DefaultSlowTDP) > b.Get(TDP.SlowTDP, TDP.DefaultSlowTDP)
                        && a.Get(TDP.FastTDP, TDP.DefaultFastTDP) > b.Get(TDP.FastTDP, TDP.DefaultFastTDP);
                }
                else if (a.Options.SettingsKey == CPUFrequency.Name + OptionsKey && b.Options.SettingsKey == CPUFrequency.Name + OptionsKey)
                {
                    return a.Get(CPUFrequency.SoftMin, CPUFrequency.DefaultMin) > b.Get(CPUFrequency.SoftMin, CPUFrequency.DefaultMin)
                        && a.Get(CPUFrequency.SoftMax, CPUFrequency.DefaultMax) > b.Get(CPUFrequency.SoftMax, CPUFrequency.DefaultMax);
                }
                else
                    throw new NotSupportedException($"Operation \">\" is not supported for operands of types \"{a.Options.SettingsKey}\" and \"{b.Options.SettingsKey}\".");
            }
        }

        public Option ForOption(string option)
        {
            return new Option() { Options = this, Key = option };
        }

        public void SetOptions(IEnumerable<Option> options)
        {
            Set<string>(OptionsKey, String.Join(",", options.Select((option) => option.Key)));
        }

        public string[] GetOptions()
        {
            var options = Get<string>(OptionsKey, "");
            return options.Split(',', StringSplitOptions.RemoveEmptyEntries);
        }
    }
}