using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace SteamController.Helpers
{
    internal static class SteamConfiguration
    {
        public const String SteamKey = @"Software\Valve\Steam";
        public const String RunningAppIDValue = @"RunningAppID";
        public const String SteamExeValue = @"SteamExe";
        public const String SteamPathValue = @"SteamPath";
        public const String BigPictureInForegroundValue = @"BigPictureInForeground";

        public const String ActiveProcessKey = @"Software\Valve\Steam\ActiveProcess";
        public const String PIDValue = @"pid";

        public const String RelativeConfigPath = @"config/config.vdf";

        private static readonly Regex ControllerBlacklistRegex = new Regex("^(\\s*\"controller_blacklist\"\\s*\")([^\"]*)(\"\\s*)$");

        public static bool? IsRunning
        {
            get
            {
                var value = GetValue<int>(ActiveProcessKey, PIDValue);
                if (value is null)
                    return null;
                try
                {
                    using (var process = Process.GetProcessById(value.Value))
                    {
                        return !process.HasExited;
                    }
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

        public static String? SteamExe
        {
            get { return GetValue2<string>(SteamKey, SteamExeValue); }
        }

        public static String? SteamPath
        {
            get { return GetValue2<string>(SteamKey, SteamPathValue); }
        }

        public static String? SteamConfigPath
        {
            get
            {
                var path = SteamPath;
                if (path is null)
                    return null;
                return Path.Join(SteamPath, RelativeConfigPath);
            }
        }

        private static Process? SteamProcess
        {
            get
            {

                var value = GetValue<int>(ActiveProcessKey, PIDValue);
                if (value is null)
                    return null;
                try
                {
                    return Process.GetProcessById(value.Value);
                }
                catch (ArgumentException)
                {
                    return null;
                }
            }
        }

        public static bool ShutdownSteam()
        {
            var steamExe = SteamExe;
            if (steamExe is null)
                return false;

            var process = Process.Start(new ProcessStartInfo()
            {
                FileName = steamExe,
                ArgumentList = { "-shutdown" },
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            return process is not null;
        }

        public static bool KillSteam()
        {
            var process = SteamProcess;
            if (process is null)
                return true;

            try
            {
                using (process) { process.Kill(); }
                return true;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                return false;
            }
        }

        public static bool WaitForSteamClose(int timeout)
        {
            var waitTill = DateTimeOffset.Now.AddMilliseconds(timeout);

            while (DateTimeOffset.Now < waitTill)
            {
                if (IsRunning != true)
                    return true;
                Application.DoEvents();
                Thread.Sleep(50);
            }
            return false;
        }

        public static HashSet<String>? GetControllerBlacklist()
        {
            var configPath = SteamConfigPath;
            if (configPath is null)
                return null;

            foreach (var line in File.ReadLines(configPath))
            {
                var match = ControllerBlacklistRegex.Match(line);
                if (!match.Success)
                    continue;

                // matches `"controller_blacklist" "<value>"`
                var value = match.Groups[2].Captures[0].Value;
                return value.Split(',', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
            }

            return new HashSet<String>();
        }

        public static bool? IsControllerBlacklisted(ushort vendorId, ushort productId)
        {
            var controllers = GetControllerBlacklist();
            if (controllers is null)
                return null;

            var id = String.Format("{0:x}/{1:x}", vendorId, productId);
            return controllers.Contains(id);
        }

        public static void BackupSteamConfig()
        {
            var configPath = SteamConfigPath;
            if (configPath is null)
                return;

            var suffix = DateTime.Now.ToString("yyyyMMddHHmmss");
            File.Copy(configPath, String.Format("{0}.{1}.bak", configPath, suffix));
        }

        public static bool UpdateControllerBlacklist(ushort vendorId, ushort productId, bool add)
        {
            if (IsRunning == true)
                return false;

            var configPath = SteamConfigPath;
            if (configPath is null)
                return false;

            var lines = File.ReadLines(configPath).ToList();
            var id = String.Format("{0:x}/{1:x}", vendorId, productId);

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i] == "}")
                {
                    if (add)
                    {
                        // append controller_blacklist
                        lines.Insert(i, String.Format("\t\"controller_blacklist\"\t\t\"{0}\"", id));
                        break;
                    }
                }

                var match = ControllerBlacklistRegex.Match(lines[i]);
                if (!match.Success)
                    continue;

                var value = match.Groups[2].Captures[0].Value;
                var controllers = value.Split(',', StringSplitOptions.RemoveEmptyEntries).ToHashSet();

                if (add)
                    controllers.Add(id);
                else
                    controllers.Remove(id);

                lines[i] = String.Format("{0}{1}{2}",
                    match.Groups[1].Captures[0].Value,
                    String.Join(',', controllers),
                    match.Groups[3].Captures[0].Value
                );
                break;
            }

            File.WriteAllLines(configPath, lines);

            return true;
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

        private static T? GetValue2<T>(string key, string value) where T : class
        {
            try
            {
                using (var registryKey = Registry.CurrentUser.OpenSubKey(key))
                {
                    return registryKey?.GetValue(value) as T;
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