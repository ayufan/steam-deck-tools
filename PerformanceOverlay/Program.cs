using RTSSSharedMemoryNET;
using System.Diagnostics;

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
                    Trace.WriteLine("Entry: {0}", entry.Owner);
                    Trace.WriteLine("\t", entry.Text);

                    using (var newOSD = new OSD("New OSD"))
                    {
                        newOSD.Update(entry.Text);
                    }
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