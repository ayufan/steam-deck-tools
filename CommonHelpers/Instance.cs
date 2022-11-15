
using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Windows.Forms;

namespace CommonHelpers
{
    public static class Instance
    {
        public static LibreHardwareMonitor.Hardware.Computer HardwareComputer = new LibreHardwareMonitor.Hardware.Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsStorageEnabled = true,
            IsBatteryEnabled = true
        };

        private static Mutex? runOnceMutex;

        private const String GLOBAL_MUTEX_NAME = "Global\\SteamDeckToolsCommonHelpers";
        private const int GLOBAL_DEFAULT_TIMEOUT = 5000;

        public static void Open(String title, String? runOnce = null, int runOnceTimeout = 100)
        {
            if (runOnce is not null)
            {
                RunOnce(title, runOnce, runOnceTimeout);
            }

            using (var globalLock = TryCreateOrOpenExistingMutex(GLOBAL_MUTEX_NAME))
            {
                if (!globalLock.WaitOne(GLOBAL_DEFAULT_TIMEOUT))
                {
                    Fatal(title, "Failed to acquire global mutex.");
                }

                if (!Vlv0100.IsSupported())
                {
                    String message = "";
                    message += "Current device is not supported.\n";
                    message += "FirmwareVersion: " + Vlv0100.GetFirmwareVersion().ToString("X") + "\n";
                    message += "BoardID: " + Vlv0100.GetBoardID().ToString("X") + "\n";
                    message += "PDCS: " + Vlv0100.GetPDCS().ToString("X") + "\n";

                    Fatal(title, message);
                }

                HardwareComputer.Open();

                globalLock.ReleaseMutex();
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
