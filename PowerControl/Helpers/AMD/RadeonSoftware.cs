using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerControl.Helpers.AMD
{
    public class RadeonSoftware
    {
        public static void Kill()
        {
            foreach (var process in Process.GetProcessesByName("RadeonSoftware.exe"))
            {
                try { process.Kill(); }
                catch { }
            }
        }
    }
}
