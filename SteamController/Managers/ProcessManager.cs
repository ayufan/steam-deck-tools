using System.Diagnostics;

namespace SteamController.Managers
{
    public sealed class ProcessManager : Manager
    {
        public static readonly String[] ActivationProcessNames = new String[]
        {
            "Playnite.FullscreenApp"
        };

        private Process? FindActivationProcess()
        {
            foreach (var processName in ActivationProcessNames)
            {
                var process = Process.GetProcessesByName(processName).FirstOrDefault();
                if (process is not null)
                    return process;
            }

            return null;
        }

        public override void Tick(Context context)
        {
            context.State.GameProcessRunning = FindActivationProcess() is not null;
        }
    }
}
