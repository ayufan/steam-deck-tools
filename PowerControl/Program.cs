using CommonHelpers;
using PowerControl.Helpers;
using PowerControl.Helpers.AMD;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PowerControl
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
#if DEBUG
            Settings.Default.EnableExperimentalFeatures = true;
#endif

            if (Settings.Default.EnableExperimentalFeatures)
            {
                Instance.WithGlobalMutex(1000, () => VangoghGPU.Detect());
            }

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            using (var controller = new Controller())
            {
                Application.Run();
            }
        }
    }
}