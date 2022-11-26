
using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Windows.Forms;
using System.Runtime.CompilerServices;

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

        public static void RunOnce(String title, String mutexName, int runOnceTimeout = 1000)
        {
            runOnceMutex = TryCreateOrOpenExistingMutex(mutexName);

            if (!runOnceMutex.WaitOne(runOnceTimeout))
            {
                Fatal(title, "Run many times");
            }
        }

        public static void Fatal(String title, String message)
        {
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
