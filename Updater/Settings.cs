using CommonHelpers;

namespace Updater
{
    internal sealed class Settings : BaseSettings
    {
        public static readonly Settings Default = new Settings();

        public Settings() : base("Settings")
        {
        }

        public int GetRunTimes(string key, Int64 match)
        {
            if (Get<Int64>(key + "Match", 0) != match)
                return 0;
            return Get<int>(key + "Counter", 0);
        }

        public void SetRunTimes(string key, Int64 match, int value)
        {
            Set(key + "Match", match);
            Set(key + "Counter", value);
        }
    }
}
