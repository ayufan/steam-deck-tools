using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace SteamController.Helpers
{
    internal static class HidHideCLI
    {
        public const String CLIPath = @"C:\Program Files\Nefarius Software Solutions\HidHide\x64\HidHideCLI.exe";

        static HidHideCLI()
        {
            IsAvailable = File.Exists(CLIPath);
        }

        public static bool IsAvailable
        {
            get; private set;
        }

        public static bool Cloak(bool enable = true)
        {
            if (enable)
                return StartCLI("--cloak-on");
            else
                return StartCLI("--cloak-off");
        }

        public static bool RegisterApplication(String path, bool seeHidden = true)
        {
            if (seeHidden)
                return StartCLI("--app-reg", path);
            else
                return StartCLI("--app-unreg", path);
        }

        public static bool HideDevice(String path, bool hide = true)
        {
            if (hide)
                return StartCLI("--dev-hide", path);
            else
                return StartCLI("--dev-unhide", path);
        }

        public static bool RegisterApplication(bool seeHidden)
        {
            var process = Process.GetCurrentProcess();
            var fullPath = process?.MainModule?.FileName;
            if (fullPath is not null)
                return RegisterApplication(fullPath, seeHidden);

            return true;
        }

        private static bool StartCLI(params string[] args)
        {
            var si = new ProcessStartInfo()
            {
                FileName = CLIPath,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            foreach (var arg in args)
            {
                si.ArgumentList.Add(arg);
            }

            var process = Process.Start(si);
            return process is not null;
        }
    }
}
