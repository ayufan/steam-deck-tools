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
        private static bool? _isDarkMode;

        private const String GLOBAL_MUTEX_NAME = "Global\\SteamDeckToolsCommonHelpers";
        private const int GLOBAL_DEFAULT_TIMEOUT = 5000;

        private static RegistryMonitor regMonitor = new RegistryMonitor(RegistryHive.CurrentUser, "Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize");

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

        public static T WithGlobalMutex<T>(int timeoutMs, Func<T> func)
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

        public static void Initialize()
        {
            regMonitor.RegChanged += new EventHandler((o, e) => { updateDarkMode(); });
            regMonitor.Start();
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
                    Fatal(title, "Run many times");
                }
            }
            catch (AbandonedMutexException)
            {
                // it is still OK
            }
        }

        public static void WithSentry(Action action)
        {
            using (Sentry.SentrySdk.Init(Log.SentryOptions))
            {
                action();
            }
        }

        public static String ApplicationName
        {
            get { return Assembly.GetEntryAssembly()?.GetName().Name ?? "unknown"; }
        }

        public static String MachineID
        {
            get
            {
                try
                {
                    using (var registryKey = Registry.CurrentUser.CreateSubKey(@"Software\SteamDeckTools", true))
                    {
                        var machineID = registryKey?.GetValue("MachineID") as string;
                        if (machineID is null)
                        {
                            registryKey?.SetValue("MachineID", Guid.NewGuid().ToString());
                            machineID = registryKey?.GetValue("MachineID") as string;
                        }

                        return machineID ?? "undefined";
                    }
                }
                catch (Exception)
                {
                    return "exception";
                }
            }
        }

        public static Version? ApplicationVersion
        {
            get => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        }

        public static String ProductVersion
        {
            get => Application.ProductVersion;
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

            Sentry.SentrySdk.CaptureMessage("Updater: " + ApplicationName, scope =>
            {
                scope.SetTag("type", user ? "user" : "background");
            });

            try
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = "Updater.exe",
                    ArgumentList = { user ? "-user" : "-first" },
                    UseShellExecute = false
                });
            }
            catch { }
        }

        public static void Fatal(String? title, String message)
        {
            if (title is not null)
                MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(1);
        }

        public static bool IsDarkMode()
        {
            if (_isDarkMode == null)
            {
                updateDarkMode();
            }

            return _isDarkMode ?? false;
        }

        private static void updateDarkMode()
        {
            string RegistryKey = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
            int theme = (int?)Registry.GetValue(RegistryKey, "SystemUsesLightTheme", 1) ?? 1;
            _isDarkMode = theme == 0;
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
