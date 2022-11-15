using CommonHelpers;
using PowerControl.Helpers.GPU;
using System.Diagnostics;

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
                Trace.WriteLine("WinRing0 initialized=" + WinRing0.InitializeOls().ToString());

                VangoghGPU.Detect();
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