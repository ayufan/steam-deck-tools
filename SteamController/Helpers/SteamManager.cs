using System.Diagnostics;
using Microsoft.Win32;

namespace SteamController.Helpers
{
    internal static class SteamManager
    {
        public const String SteamKey = @"Software\Valve\Steam";
        public const String RunningAppIDValue = @"RunningAppID";
        public const String BigPictureInForegroundValue = @"BigPictureInForeground";

        public const String ActiveProcessKey = @"Software\Valve\Steam\ActiveProcess";
        public const String PIDValue = @"pid";

        public static bool? IsRunning
        {
            get
            {
                var value = GetValue<int>(ActiveProcessKey, PIDValue);
                if (value is null)
                    return null;
                if (Process.GetProcessById(value.Value) is null)
                    return false;
                return true;
            }
        }

        public static bool? IsBigPictureMode
        {
            get
            {
                var value = GetValue<int>(SteamKey, BigPictureInForegroundValue);
                return value.HasValue ? value != 0 : null;
            }
        }

        public static bool? IsRunningGame
        {
            get
            {
                var value = GetValue<int>(SteamKey, RunningAppIDValue);
                return value.HasValue ? value != 0 : null;
            }
        }

        private static T? GetValue<T>(string key, string value) where T : struct
        {
            try
            {
                using (var registryKey = Registry.CurrentUser.OpenSubKey(key))
                {
                    return registryKey?.GetValue(value) as T?;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}