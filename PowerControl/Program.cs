using CommonHelpers;
using PowerControl.Helpers;
using PowerControl.Helpers.AMD;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PowerControl
{
    internal static class Program
    {
        const int MAX_GPU_RETRIES = 3;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Instance.WithSentry(() =>
            {
                // To customize application configuration such as set high DPI settings or default font,
                // see https://aka.ms/applicationconfiguration.
                ApplicationConfiguration.Initialize();

                if (Settings.Default.EnableExperimentalFeatures)
                {
                    if (!AntiCheatSettings.Default.AckAntiCheat(
                        Controller.TitleWithVersion,
                        "You are running EXPERIMENTAL build.", null, false))
                        return;
                }

                for (int i = 0; !VangoghGPU.IsSupported && i < MAX_GPU_RETRIES; i++)
                {
                    var status = Instance.WithGlobalMutex(1000, () => VangoghGPU.Detect());
                    if (status != VangoghGPU.DetectionStatus.Retryable)
                        break;

                    Thread.Sleep(300);
                }

                using (var controller = new Controller())
                {
                    Application.Run();
                }
            });
        }
    }
}