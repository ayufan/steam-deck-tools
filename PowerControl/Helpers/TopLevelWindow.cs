using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PowerControl.Helpers
{
    public class TopLevelWindow
    {
        [DllImport("user32.dll", SetLastError= true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        public static uint? GetTopLevelProcessId()
        {
            var hWnd = GetForegroundWindow();
            var result = GetWindowThreadProcessId(hWnd, out uint processId);
            if (result != 0)
                return processId;
            return null;
        }
    }
}
