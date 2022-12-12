using CommonHelpers;
using RTSSSharedMemoryNET;
using System.Diagnostics;

namespace PerformanceOverlay
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Instance.WithSentry(() =>
            {
                ApplicationConfiguration.Initialize();

                using (var controller = new Controller())
                {
                    Application.Run();
                }
            });
        }
    }
}