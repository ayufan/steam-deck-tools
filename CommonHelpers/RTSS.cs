using System.Runtime.InteropServices;

namespace CommonHelpers
{
    public static class RTSS
    {
        private const uint WM_APP = 0x8000;
        private const uint WM_RTSS_UPDATESETTINGS = WM_APP + 100;
        private const uint WM_RTSS_SHOW_PROPERTIES = WM_APP + 102;

        private const uint RTSSHOOKSFLAG_OSD_VISIBLE = 1;
        private const uint RTSSHOOKSFLAG_LIMITER_DISABLED = 4;
        private const string GLOBAL_PROFILE = "";

        public static bool GetProfileProperty<T>(string propertyName, out T value)
        {
            var bytes = new byte[Marshal.SizeOf<T>()];
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            value = default;
            try
            {
                if (!GetProfileProperty(propertyName, handle.AddrOfPinnedObject(), (uint)bytes.Length))
                    return false;

                value = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                handle.Free();
            }
        }

        public static bool SetProfileProperty<T>(string propertyName, T value)
        {
            var bytes = new byte[Marshal.SizeOf<T>()];
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                Marshal.StructureToPtr(value, handle.AddrOfPinnedObject(), false);
                return SetProfileProperty(propertyName, handle.AddrOfPinnedObject(), (uint)bytes.Length);
            }
            catch
            {
                return false;
            }
            finally
            {
                handle.Free();
            }
        }

        public static uint EnableFlag(uint flag, bool status)
        {
            var current = SetFlags(~flag, status ? flag : 0);
            UpdateSettings();
            return current;
        }

        public static void UpdateSettings()
        {
            PostMessage(WM_RTSS_UPDATESETTINGS, IntPtr.Zero, IntPtr.Zero);
        }

        private static void PostMessage(uint Msg, IntPtr wParam, IntPtr lParam)
        {
            var hWnd = FindWindow(null, "RTSS");
            if (hWnd == IntPtr.Zero)
                hWnd = FindWindow(null, "RivaTuner Statistics Server");

            if (hWnd != IntPtr.Zero)
                PostMessage(hWnd, Msg, wParam, lParam);
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("RTSSHooks64.dll")]
        public static extern uint SetFlags(uint dwAND, uint dwXOR);

        [DllImport("RTSSHooks64.dll", CharSet = CharSet.Ansi)]
        public static extern void LoadProfile(string profile = GLOBAL_PROFILE);

        [DllImport("RTSSHooks64.dll", CharSet = CharSet.Ansi)]
        public static extern void SaveProfile(string profile = GLOBAL_PROFILE);

        [DllImport("RTSSHooks64.dll", CharSet = CharSet.Ansi)]
        public static extern void DeleteProfile(string profile = GLOBAL_PROFILE);

        [DllImport("RTSSHooks64.dll", CharSet = CharSet.Ansi)]
        public static extern bool GetProfileProperty(string propertyName, IntPtr value, uint size);

        [DllImport("RTSSHooks64.dll", CharSet = CharSet.Ansi)]
        public static extern bool SetProfileProperty(string propertyName, IntPtr value, uint size);

        [DllImport("RTSSHooks64.dll", CharSet = CharSet.Ansi)]
        public static extern void ResetProfile(string profile = GLOBAL_PROFILE);

        [DllImport("RTSSHooks64.dll", CharSet = CharSet.Ansi)]
        public static extern void UpdateProfiles();
    }
}
