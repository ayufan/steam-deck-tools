using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SteamController.Helpers
{
    internal static class ForegroundProcess
    {
        private static int? lastForegroundProcess;

        public static void Store()
        {
            lastForegroundProcess = GetTopLevelProcessId();
        }

        public static bool Kill(bool onlyTheSame = false)
        {
            var process = Find();
            if (process is null)
                return true;
            if (onlyTheSame && process.Id != lastForegroundProcess)
                return false;

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
        public static Process? Find()
        {
            return Find(GetForegroundWindow());
        }

        public static Process? Find(IntPtr hWnd)
        {
            try
            {
                var id = GetProcessId(hWnd);
                if (id is null)
                    return null;

                var process = Process.GetProcessById(id.Value);
                if (!process.HasExited)
                    return process;
            }
            catch { }

            return null;
        }

        private static int? GetTopLevelProcessId()
        {
            return GetProcessId(GetForegroundWindow());
        }

        private static int? GetProcessId(IntPtr hWnd)
        {
            var result = GetWindowThreadProcessId(hWnd, out var processId);
            if (result != 0)
                return processId;
            return null;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);
    }
}