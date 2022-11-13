using RTSSSharedMemoryNET;

namespace PerformanceOverlay
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            try
            {
                foreach (var entry in RTSSSharedMemoryNET.OSD.GetOSDEntries())
                {
                    Console.WriteLine("Entry: {0}", entry.Owner);
                    Console.WriteLine("\t", entry.Text);
                }
            }
            catch(SystemException)
            { }

            using (var controller = new Controller())
            {
                Application.Run();
            }
        }
    }
}