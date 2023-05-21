using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using CommonHelpers;
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
        public const String CrashReportConfigPath = @"bin/cef/cef.win7x64/crash_reporter.cfg";

        private static readonly Regex ControllerBlacklistRegex = new Regex("^(\\s*\"controller_blacklist\"\\s*\")([^\"]*)(\"\\s*)$");

        public static bool IsRunning
        {
            get
            {
                using (var process = SteamProcess)
                {
                    return process is not null;
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

        public static uint SteamVersion
        {
            get
            {
                var path = GetConfigPath(CrashReportConfigPath);
                if (path is null)
                    return 0;

                return GetPrivateProfileInt("Config", "ProductVersion", 0, path);
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

        private static Process? SteamProcess
        {
            get
            {
                var value = GetValue<int>(ActiveProcessKey, PIDValue);
                if (value is null)
                    return null;
                try
                {
                    var process = Process.GetProcessById(value.Value);
                    if (!process.ProcessName.Equals("Steam", StringComparison.CurrentCultureIgnoreCase))
                        return null;
                    if (process.HasExited)
                        return null;
                    return process;
                }
                catch { return null; }
            }
        }

        public static String? GetConfigPath(String configPath)
        {
            var path = SteamPath;
            if (path is null)
                return null;
            return Path.Join(SteamPath, configPath);
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
                if (!IsRunning)
                    return true;
                Application.DoEvents();
                Thread.Sleep(50);
            }
            return false;
        }

        public static HashSet<String>? GetControllerBlacklist()
        {
            try
            {
                var configPath = GetConfigPath(RelativeConfigPath);
                if (configPath is null)
                    return null;

                foreach (var line in File.ReadLines(configPath).Reverse())
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
            catch (DirectoryNotFoundException)
            {
                // Steam was installed, but got removed
                return null;
            }
            catch (IOException e)
            {
                Log.TraceException("STEAM", "Config", e);
                return null;
            }
        }

        public static bool? IsControllerBlacklisted(ushort vendorId, ushort productId)
        {
            var controllers = GetControllerBlacklist();
            if (controllers is null)
                return null;

            var id = String.Format("{0:x}/{1:x}", vendorId, productId);
            return controllers.Contains(id);
        }

        public static bool BackupSteamConfig(String path)
        {
            var configPath = GetConfigPath(path);
            if (configPath is null)
                return true;

            try
            {
                var suffix = DateTime.Now.ToString("yyyyMMddHHmmss");
                File.Copy(configPath, String.Format("{0}.{1}.bak", configPath, suffix));
                return true;
            }
            catch (DirectoryNotFoundException)
            {
                // Steam was installed, but got removed
                return false;
            }
            catch (IOException e)
            {
                Log.TraceException("STEAM", "Config", e);
                return false;
            }
        }

        public static bool BackupSteamConfig()
        {
            return BackupSteamConfig(RelativeConfigPath);
        }

        public static bool UpdateControllerBlacklist(ushort vendorId, ushort productId, bool add)
        {
            if (IsRunning)
                return false;

            var configPath = GetConfigPath(RelativeConfigPath);
            if (configPath is null)
                return false;

            try
            {
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
            catch (DirectoryNotFoundException)
            {
                // Steam was installed, but got removed
                return false;
            }
            catch (IOException e)
            {
                Log.TraceException("STEAM", "Config", e);
                return false;
            }
        }

        public static bool? IsConfigFileOverwritten(String path, byte[] content)
        {
            try
            {
                var configPath = GetConfigPath(path);
                if (configPath is null)
                    return null;

                byte[] diskContent = File.ReadAllBytes(configPath);
                return content.SequenceEqual(diskContent);
            }
            catch (DirectoryNotFoundException)
            {
                // Steam was installed, but got removed
                return null;
            }
            catch (IOException e)
            {
                Log.TraceException("STEAM", "Config", e);
                return null;
            }
        }

        public static bool? ResetConfigFile(String path)
        {
            try
            {
                var configPath = GetConfigPath(path);
                if (configPath is null)
                    return null;

                File.Copy(configPath + ".orig", configPath, true);
                return true;
            }
            catch (FileNotFoundException e)
            {
                // File was not found (which is valid as it might be before first start of the application)
                Log.DebugException("STEAM", e);
                return false;
            }
            catch (DirectoryNotFoundException)
            {
                // Steam was installed, but got removed
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (System.Security.SecurityException)
            {
                return false;
            }
            catch (IOException e)
            {
                Log.TraceException("STEAM", "Config", e);
                return null;
            }
        }

        public static bool? OverwriteConfigFile(String path, byte[] content, bool backup)
        {
            try
            {
                var configPath = GetConfigPath(path);
                if (configPath is null)
                    return null;

                try
                {
                    byte[] diskContent = File.ReadAllBytes(configPath);
                    if (content.Equals(diskContent))
                        return false;
                }
                catch (IOException) { }

                if (backup)
                    File.Copy(configPath, configPath + ".orig", true);
                File.WriteAllBytes(configPath, content);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (System.Security.SecurityException)
            {
                return false;
            }
            catch (DirectoryNotFoundException)
            {
                // Steam was installed, but got removed
                return false;
            }
            catch (IOException e)
            {
                Log.TraceException("STEAM", "Config", e);
                return null;
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

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern uint GetPrivateProfileInt(string lpAppName, string lpKeyName, int nDefault, string lpFileName);
    }
}