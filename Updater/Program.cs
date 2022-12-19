using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Web;
using AutoUpdaterDotNET;
using CommonHelpers;
using ExternalHelpers;
using Microsoft.Win32;

namespace Updater
{
    internal static class Program
    {
        public const String Title = "Steam Deck Tools";
        public const String RunPrefix = "-run=";
        public const String UpdatedArg = "-updated";

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Instance.WithSentry(() =>
            {
                Run();
            });
        }

        static void Run()
        {
            bool firstRun = Environment.GetCommandLineArgs().Contains("-first");
            bool userCheck = Environment.GetCommandLineArgs().Contains("-user");
            bool updated = Environment.GetCommandLineArgs().Contains(UpdatedArg);
            bool cmdLine = !firstRun && !userCheck;

            Instance.OnUninstall(() =>
            {
                KillApps();
            });

            if (updated)
            {
                foreach (var arg in Environment.GetCommandLineArgs())
                {
                    if (!arg.StartsWith(RunPrefix))
                        continue;

                    var processName = arg.Substring(RunPrefix.Length);
                    CommonHelpers.Log.TraceLine("Running {0}", processName);
                    try { Process.Start(processName); } catch { }
                }
                return;
            }

            Instance.RunOnce(null, "Global\\SteamDeckToolsAutoUpdater");

            if (Instance.HasFile("DisableCheckForUpdates.txt"))
            {
                if (userCheck || cmdLine)
                {
                    MessageBox.Show(
                        "This application has explicitly disabled auto-updates. Remove the 'DisableCheckForUpdates.txt' file and retry again",
                        Title,
                        MessageBoxButtons.OK
                    );
                }
                return;
            }

            var persistence = new RegistryPersistenceProvider(@"Software\SteamDeckTools\AutoUpdater");

            if (userCheck || cmdLine)
            {
                persistence.SetRemindLater(null);
                persistence.SetSkippedVersion(new Version());
            }

            AutoUpdater.AppTitle = Title;
            AutoUpdater.RemindLaterTimeSpan = RemindLaterFormat.Days;
            AutoUpdater.LetUserSelectRemindLater = true;
            AutoUpdater.ShowRemindLaterButton = true;
            AutoUpdater.HttpUserAgent = String.Format("AutoUpdater/{0}/{1}/{2}",
                InstallationTime,
                Instance.ProductVersionWithSha,
                Instance.IsProductionBuild ? "prod" : "dev");
            AutoUpdater.PersistenceProvider = persistence;
            AutoUpdater.ReportErrors = userCheck || cmdLine;
            AutoUpdater.UpdateFormSize = new Size(800, 300);
            AutoUpdater.ShowSkipButton = true;
            AutoUpdater.Synchronous = true;

            if (!IsUsingInstaller)
            {
                // Only when not using installer we have to kill apps
                AutoUpdater.ApplicationExitEvent += KillApps;
            }

            AppendArg(UpdatedArg);

            List<string> usedTools = new List<string>();
            TrackProcess("FanControl", usedTools);
            TrackProcess("PowerControl", usedTools);
            TrackProcess("PerformanceOverlay", usedTools);
            TrackProcess("SteamController", usedTools);

            var todayMatch = DateTimeOffset.UtcNow.DayOfYear;
            var runToday = Settings.Default.GetRunTimes("Today", todayMatch) + 1;
            var thisWeekMatch = DateTimeOffset.UtcNow.DayOfYear / 7;
            var runThisWeek = Settings.Default.GetRunTimes("ThisWeek", thisWeekMatch) + 1;

            AutoUpdater.ParseUpdateInfoEvent += delegate
            {
                Settings.Default.SetRunTimes("Today", todayMatch, runToday);
                Settings.Default.SetRunTimes("ThisWeek", thisWeekMatch, runThisWeek);
            };

            // This method requests an auto-update from remote server. It includes the following information:
            // Type of installation: prod/dev, release/debug, setup/zip
            // Version of application: 0.5.40+12345cdef
            // Installation time: when the application was installed to track the age
            // Used Tools: which application of suite are running, like: FanControl,PerformanceOverlay
            // Updates Today/ThisWeek: amount of times update run today and this week

            var updateURL = String.Format(
                "https://steam-deck-tools.ayufan.dev/updates/{4}_{0}_{1}.xml?version={2}&installTime={3}&env={4}&apps={5}&updatesToday={6}&updatesThisWeek={7}",
                Instance.IsDEBUG ? "debug" : "release",
                IsUsingInstaller ? "setup" : "zip",
                HttpUtility.UrlEncode(Instance.ProductVersionWithSha),
                InstallationTime,
                Instance.IsProductionBuild ? "prod" : "dev",
                HttpUtility.UrlEncode(String.Join(",", usedTools)),
                runToday,
                runThisWeek
            );

            AutoUpdater.Start(updateURL);
        }

        private static bool TrackProcess(String processFilterName, List<string>? usedTools = null)
        {
            if (FindProcesses(processFilterName).Any())
            {
                AppendArg(RunPrefix + processFilterName);
                usedTools?.Add(processFilterName);
                return true;
            }
            return false;
        }

        private static void KillApps()
        {
            ExitProcess("FanControl");
            ExitProcess("PowerControl");
            ExitProcess("PerformanceOverlay");
            ExitProcess("SteamController");
            ExitProcess("Updater");
        }

        private static void AppendArg(string arg)
        {
            var setCommandLineArgs = typeof(Environment).GetMethod(
                "SetCommandLineArgs", BindingFlags.Static | BindingFlags.NonPublic,
                new Type[] { typeof(string[]) });
            if (setCommandLineArgs is null)
                return;

            // append `-run:<process>` to command line args
            setCommandLineArgs.Invoke(null, new object[] {
                Environment.GetCommandLineArgs().Append(arg).ToArray()
            });
        }

        private static bool ExitProcess(String processFilerName)
        {
            bool found = false;

            foreach (var process in FindProcesses(processFilerName))
            {
                if (process.CloseMainWindow())
                {
                    process.WaitForExit((int)TimeSpan.FromSeconds(10)
                        .TotalMilliseconds); //give some time to process message
                }

                if (!process.HasExited)
                {
                    process.Kill(); //TODO show UI message asking user to close program himself instead of silently killing it
                }

                found = true;
            }

            return found;
        }

        private static bool IsUsingInstaller
        {
            get
            {
                var currentProcess = Process.GetCurrentProcess();
                var currentDir = Path.GetDirectoryName(currentProcess.MainModule?.FileName);
                if (currentDir is null)
                    return false;

                var uninstallExe = Path.Combine(currentDir, "Uninstall.exe");
                return File.Exists(uninstallExe);
            }
        }

        public static string InstallationTime
        {
            get
            {
                try
                {
                    using (var registryKey = Registry.CurrentUser.CreateSubKey(@"Software\SteamDeckTools", true))
                    {
                        var installationTime = registryKey?.GetValue("InstallationTime") as string;
                        if (installationTime is null)
                        {
                            var previousTime = RegistryUtils.GetDateModified(
                                RegistryHive.CurrentUser, @"Software\SteamDeckTools");
                            Log.TraceLine("PreviousTime: {0}", previousTime);
                            previousTime ??= DateTimeOffset.UtcNow;

                            registryKey?.SetValue("InstallationTime", previousTime.Value.ToUnixTimeMilliseconds());
                            installationTime = registryKey?.GetValue("InstallationTime") as string;
                        }

                        if (!Instance.AcceptedTerms)
                        {
                            return "";
                        }

                        return installationTime ?? "";
                    }
                }
                catch (Exception e)
                {
                    return "";
                }
            }
        }

        private static IEnumerable<Process> FindProcesses(String processFilerName)
        {
            var currentProcess = Process.GetCurrentProcess();
            var currentDir = Path.GetDirectoryName(currentProcess.MainModule?.FileName);

            foreach (var process in Process.GetProcessesByName(processFilerName))
            {
                string? processFileName, processDir;
                try
                {
                    processFileName = process.MainModule?.FileName;
                    if (processFileName is null)
                        continue;

                    processDir = Path.GetDirectoryName(processFileName);
                }
                catch (Win32Exception)
                {
                    // Current process should be same as processes created by other instances of the application so it should be able to access modules of other instances. 
                    // This means this is not the process we are looking for so we can safely skip this.
                    continue;
                }

                //get all instances of assembly except current
                if (process.Id != currentProcess.Id && currentDir == processDir)
                {
                    yield return process;
                }
            }
        }
    }
}
