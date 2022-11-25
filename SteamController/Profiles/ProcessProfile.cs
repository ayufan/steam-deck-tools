using System.Diagnostics;

namespace SteamController.Profiles
{
    public sealed class ProcessProfile : Profile
    {
        public static readonly String[] ActivationProcessNames = new String[]
        {
            "Playnite.FullscreenApp"
        };

        private bool activated;

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
            // React to state change
            if (FindActivationProcess() is not null)
            {
                if (!activated)
                {
                    activated = true;
                    context.RequestDesktopMode = false;
                }
            }
            else
            {
                if (activated)
                {
                    activated = false;
                    context.RequestDesktopMode = true;
                }
            }
        }

        public override Status Run(Context context)
        {
            return Status.Continue;
        }
    }
}
