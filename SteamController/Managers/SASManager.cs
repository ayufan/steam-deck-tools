using System.Diagnostics;
using System.Runtime.InteropServices;
using SteamController.Helpers;

namespace SteamController.Managers
{
    public sealed class SASManager : Manager
    {
        internal static bool Valid { get; set; }

        public override void Tick(Context context)
        {
            Valid = GetCursorPos(out var _);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X, Y;
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);
    }
}
