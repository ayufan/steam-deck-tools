using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace SteamController.Helpers
{
    internal static class SteamProcess
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
                try
                {
                    Process.GetProcessById(value.Value);
                    return true;
                }
                catch (ArgumentException)
                {
                    return false;
                }
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

        public static bool IsGamePadUI
        {
            get
            {
                var steamWindow = FindWindow("SDL_app", "SP");
                if (steamWindow == null)
                    return false;

                return GetForegroundWindow() == steamWindow;
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

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    }
}