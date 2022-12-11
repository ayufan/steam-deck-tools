using System.Security.Principal;
using System.Security.AccessControl;
using AutoUpdaterDotNET;
using Microsoft.Win32;
using System.Diagnostics;

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
        private const int GLOBAL_DEFAULT_TIMEOUT = 5000;

        public static bool WantsRunOnStartup
        {
            get { return Environment.GetCommandLineArgs().Contains("-run-on-startup"); }
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

            if (!runOnceMutex.WaitOne(runOnceTimeout))
            {
                Fatal(title, "Run many times");
            }
        }

        public static String MachineID
        {
            get
            {
                try
                {
                    using (var registryKey = Registry.CurrentUser.OpenSubKey(@"Software\SteamDeckTools"))
                    {
                        var machineID = registryKey?.GetValue("MachineID") as string;
                        if (machineID is not null)
                            return machineID;

                        machineID = Guid.NewGuid().ToString();
                        registryKey?.SetValue("MachineID", machineID);
                        return machineID;
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
