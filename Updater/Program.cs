using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
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
            AutoUpdater.ParseUpdateInfoEvent += ParseUpdateInfoEvent;

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

            var last1Match = DateTimeOffset.UtcNow.DayOfYear;
            var last1 = Settings.Default.GetRunTimes("Last1", last1Match) + 1;
            var last3Match = DateTimeOffset.UtcNow.DayOfYear / 3;
            var last3 = Settings.Default.GetRunTimes("Last3", last3Match) + 1;
            var last7Match = DateTimeOffset.UtcNow.DayOfYear / 7;
            var last7 = Settings.Default.GetRunTimes("Last7", last7Match) + 1;

            AutoUpdater.ParseUpdateInfoEvent += delegate
            {
                Settings.Default.SetRunTimes("Last1", last1Match, last1);
                Settings.Default.SetRunTimes("Last3", last3Match, last3);
                Settings.Default.SetRunTimes("Last7", last7Match, last7);
            };

            // This method requests an auto-update from remote server.
            // It includes the following information:
            // - Type of installation: prod/dev, release/debug, setup/zip
            // - Version of application: 0.5.40+12345cdef
            // - Installation time: when the application was installed
            // - Used Tools: which application of suite are running, like: FanControl,PerformanceOverlay
            // - Updates 1, 3 and 7 days: amount of times update run in 1 day, 3 and 7 days

            var updateURL = String.Format(
                "https://steam-deck-tools.ayufan.dev/updates/{4}_{0}_{1}.xml?version={2}&installTime={3}&env={4}&apps={5}&updatesLast1={6}&updatesLast3={7}&updatesLast7={8}",
                Instance.IsDEBUG ? "debug" : "release",
                IsUsingInstaller ? "setup" : "zip",
                HttpUtility.UrlEncode(Instance.ProductVersionWithSha),
                InstallationTime,
                Instance.IsProductionBuild ? "prod" : "dev",
                HttpUtility.UrlEncode(String.Join(",", usedTools)),
                last1,
                last3,
                last7
            );

            AutoUpdater.Start(updateURL);
        }

        private static UpdateInfoEventArgs? UpdateInfo { get; set; }

        private static void ParseUpdateInfoEvent(ParseUpdateInfoEventArgs args)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(UpdateInfoEventArgs));
            XmlTextReader xmlTextReader = new XmlTextReader(new StringReader(args.RemoteData)) { XmlResolver = null };
            UpdateInfo = xmlSerializer.Deserialize(xmlTextReader) as UpdateInfoEventArgs;
            if (UpdateInfo is not null)
            {
                args.UpdateInfo = UpdateInfo;
            }
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
            if (UpdateInfo?.InstallerArgs?.StartsWith("/nokill") == true)
                return;

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
                return "";
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
