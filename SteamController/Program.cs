using CommonHelpers;
using System.Diagnostics;

namespace SteamController
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Instance.WithSentry(() =>
            {
                // Raise the application priority to Above Normal for better responsiveness
                try
                {
                    Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;
                }
                catch { }

                // To customize application configuration such as set high DPI settings or default font,
                // see https://aka.ms/applicationconfiguration.
                ApplicationConfiguration.Initialize();

                using (var controller = new Controller())
                {
                    Application.Run();
                }
            });
        }
    }
}
