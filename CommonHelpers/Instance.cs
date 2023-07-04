using System.Security.Principal;
using System.Security.AccessControl;
using Microsoft.Win32;
using System.Diagnostics;
using System.Reflection;

namespace CommonHelpers
{
    public static class Instance
    {
        public static LibreHardwareMonitor.Hardware.Computer HardwareComputer = new LibreHardwareMonitor.Hardware.Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsStorageEnabled = true,
            IsBatteryEnabled = true
        };

        private static Mutex? runOnceMutex;
        private static Mutex? globalLockMutex;
        private static bool useKernelDrivers;

        private const String GLOBAL_MUTEX_NAME = "Global\\SteamDeckToolsCommonHelpers";
        private const int GLOBAL_DEFAULT_TIMEOUT = 10000;

        public static bool WantsRunOnStartup
        {
            get { return Environment.GetCommandLineArgs().Contains("-run-on-startup"); }
        }

        public static bool Uninstall
        {
            get { return Environment.GetCommandLineArgs().Contains("-uninstall"); }
        }

        public static bool IsDEBUG
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        public static bool IsProductionBuild
        {
            get
            {
#if PRODUCTION_BUILD
                return true;
#else
                return false;
#endif
            }
        }

        public static void OnUninstall(Action action)
        {
            if (Uninstall)
            {
                action();
                Environment.Exit(0);
            }
        }

        public static bool UseKernelDrivers
        {
            get { return useKernelDrivers; }
            set
            {
                if (useKernelDrivers == value)
                    return;

                useKernelDrivers = value;

                if (value)
                    Vlv0100.Open();
                else
                    Vlv0100.Close();

                // CPU requires reading RyzenSMU
                HardwareComputer.IsCpuEnabled = value;
                HardwareComputer.Reset();
            }
        }

        public static Mutex? WaitGlobalMutex(int timeoutMs)
        {
            if (globalLockMutex == null)
                globalLockMutex = TryCreateOrOpenExistingMutex(GLOBAL_MUTEX_NAME);

            try
            {
                if (globalLockMutex.WaitOne(timeoutMs))
                    return globalLockMutex;
                return null;
            }
            catch (AbandonedMutexException)
            {
                return globalLockMutex;
            }
        }

        public static T? WithGlobalMutex<T>(int timeoutMs, Func<T?> func)
        {
            var mutex = WaitGlobalMutex(timeoutMs);
            if (mutex is null)
                return default(T);

            try
            {
                return func();
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        public static void Open(String title, bool useKernelDrivers, String? runOnce = null, int runOnceTimeout = 100)
        {
            if (runOnce is not null)
            {
                RunOnce(title, runOnce, runOnceTimeout);
            }

            var mutex = WaitGlobalMutex(GLOBAL_DEFAULT_TIMEOUT);

            if (mutex is null)
            {
                Fatal(title, "Failed to acquire global mutex.");
                return;
            }

            try
            {
                UseKernelDrivers = useKernelDrivers;

                if (Vlv0100.IsOpen && !Vlv0100.IsSupported)
                {
                    String message = "";
                    message += "Current device is not supported.\n";
                    message += "FirmwareVersion: " + Vlv0100.FirmwareVersion.ToString("X") + "\n";
                    message += "BoardID: " + Vlv0100.BoardID.ToString("X") + "\n";
                    message += "PDCS: " + Vlv0100.PDCS.ToString("X") + "\n";

                    Fatal(title, message);
                }

                HardwareComputer.Open();
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        public static void RunOnce(String? title, String mutexName, int runOnceTimeout = 1000)
        {
            runOnceMutex = TryCreateOrOpenExistingMutex(mutexName);

            try
            {
                if (!runOnceMutex.WaitOne(runOnceTimeout))
                {
                    Fatal(title, "Run many times", false);
                }
            }
            catch (AbandonedMutexException)
            {
                // it is still OK
            }
        }

        public static void WithSentry(Action action, string? dsn = null)
        {
            // Overwrite DSN
            if (dsn != null)
            {
                Log.SENTRY_DSN = dsn;
            }

            using (Sentry.SentrySdk.Init(Log.SentryOptions))
            {
                action();
            }
        }

        public static String ApplicationName
        {
            get { return Assembly.GetEntryAssembly()?.GetName().Name ?? "unknown"; }
        }

        public static String ProductVersion
        {
            get => Application.ProductVersion;
        }

        public static String ProductVersionWithSha
        {
            get
            {
                var releaseVersion = typeof(Instance).Assembly.GetCustomAttributes<AssemblyInformationalVersionAttribute>().FirstOrDefault();
                return releaseVersion?.InformationalVersion ?? ProductVersion;
            }
        }

        public static bool HasFile(String name)
        {
            var currentProcess = Process.GetCurrentProcess();
            var currentDir = Path.GetDirectoryName(currentProcess.MainModule?.FileName);
            if (currentDir is null)
                return false;

            var uninstallExe = Path.Combine(currentDir, name);
            return File.Exists(uninstallExe);
        }

        private static System.Timers.Timer? updateTimer;

        public static void RunUpdater(string Title, bool user = false, int recheckIntervalHours = 24)
        {
            // Schedule updater in 24h
            if (updateTimer == null && !user && recheckIntervalHours > 0)
            {
                updateTimer = new System.Timers.Timer
                {
                    Interval = recheckIntervalHours * 60 * 60 * 1000 // 24h
                };
                updateTimer.Elapsed += delegate { RunUpdater(Title, false); };
                updateTimer.Start();
            }

            try
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = "Updater.exe",
                    ArgumentList = {
                        user ? "-user" : "-first",
                        "-app", ApplicationName,
                        "-version", ProductVersion
                    },
                    UseShellExecute = false
                });
            }
            catch { }
        }

        public static void Fatal(String? title, String message, bool capture = true)
        {
            if (capture)
                Log.TraceError("FATAL: {0}", message);
            if (title is not null)
                MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(1);
        }

        private static Mutex TryCreateOrOpenExistingMutex(string name)
        {
            MutexSecurity mutexSecurity = new();
            SecurityIdentifier identity = new(WellKnownSidType.WorldSid, null);
            mutexSecurity.AddAccessRule(new MutexAccessRule(identity, MutexRights.Synchronize | MutexRights.Modify, AccessControlType.Allow));

            var mutex = new Mutex(false, name, out _);
            mutex.SetAccessControl(mutexSecurity);
            return mutex;
        }
    }
}
