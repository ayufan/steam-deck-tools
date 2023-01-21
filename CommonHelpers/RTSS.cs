using RTSSSharedMemoryNET;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CommonHelpers
{
    public static class RTSS
    {
        public static bool IsOSDForeground()
        {
            return IsOSDForeground(out _, out _);
        }

        public static bool IsOSDForeground(out int processId)
        {
            return IsOSDForeground(out processId, out _);
        }

        public static bool IsOSDForeground(out int processId, out string processName)
        {
            Applications.Instance.Refresh();

            return Applications.Instance.FindForeground(out processId, out processName);
        }

        public class Applications
        {
            public readonly static Applications Instance = new Applications();

            public struct Entry
            {
                public String ProcessName { get; set; }
                public uint LastFrame { get; set; }
                public DateTimeOffset LastFrameTime { get; set; }
                public bool IsOSDForeground { get; set; }

                public bool IsRecent
                {
                    get { return LastFrameTime.AddMilliseconds(FrameTimeoutMs) >= DateTimeOffset.UtcNow; }
                }
            }

            public IDictionary<int, Entry> IDs { get; private set; } = new Dictionary<int, Entry>();

            private const int FrameTimeoutMs = 5000;

            public Applications()
            {
                Refresh();
            }

            public void Refresh()
            {
                RTSSSharedMemoryNET.AppEntry[] appEntries;

                var oldIDs = IDs;
                var newIDs = new Dictionary<int, Entry>();

                try { appEntries = OSD.GetAppEntries(AppFlags.MASK); }
                catch { return; }

                var now = DateTimeOffset.UtcNow;

                var topLevelProcessId = GetTopLevelProcessId();

                foreach (var app in appEntries)
                {
                    if (!oldIDs.TryGetValue(app.ProcessId, out var entry))
                    {
                        entry.ProcessName = Path.GetFileNameWithoutExtension(app.Name);
                    }

                    entry.IsOSDForeground = (topLevelProcessId == app.ProcessId);

                    if (entry.LastFrame != app.OSDFrameId || entry.IsOSDForeground)
                    {
                        entry.LastFrame = app.OSDFrameId;
                        entry.LastFrameTime = now;
                    }

                    newIDs.TryAdd(app.ProcessId, entry);
                }

                IDs = newIDs;
            }

            public bool FindForeground(out int processId, out string processName)
            {
                processId = 0;
                processName = "";

                var id = GetTopLevelProcessId();
                if (id is null)
                    return false;

                if (!IDs.TryGetValue(id.Value, out var entry))
                    return false;
                if (!entry.IsRecent)
                    return false;

                processId = id.Value;
                processName = entry.ProcessName;
                return true;
            }

            public bool IsRunning(int processId)
            {
                if (!IDs.TryGetValue(processId, out var entry))
                    return false;
                return entry.IsRecent;
            }
        }

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

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("C:\\Program Files (x86)\\RivaTuner Statistics Server\\RTSSHooks64.dll")]
        public static extern uint SetFlags(uint dwAND, uint dwXOR);

        [DllImport("C:\\Program Files (x86)\\RivaTuner Statistics Server\\RTSSHooks64.dll", CharSet = CharSet.Ansi)]
        public static extern void LoadProfile(string profile = GLOBAL_PROFILE);

        [DllImport("C:\\Program Files (x86)\\RivaTuner Statistics Server\\RTSSHooks64.dll", CharSet = CharSet.Ansi)]
        public static extern void SaveProfile(string profile = GLOBAL_PROFILE);

        [DllImport("C:\\Program Files (x86)\\RivaTuner Statistics Server\\RTSSHooks64.dll", CharSet = CharSet.Ansi)]
        public static extern void DeleteProfile(string profile = GLOBAL_PROFILE);

        [DllImport("C:\\Program Files (x86)\\RivaTuner Statistics Server\\RTSSHooks64.dll", CharSet = CharSet.Ansi)]
        public static extern bool GetProfileProperty(string propertyName, IntPtr value, uint size);

        [DllImport("C:\\Program Files (x86)\\RivaTuner Statistics Server\\RTSSHooks64.dll", CharSet = CharSet.Ansi)]
        public static extern bool SetProfileProperty(string propertyName, IntPtr value, uint size);

        [DllImport("C:\\Program Files (x86)\\RivaTuner Statistics Server\\RTSSHooks64.dll", CharSet = CharSet.Ansi)]
        public static extern void ResetProfile(string profile = GLOBAL_PROFILE);

        [DllImport("C:\\Program Files (x86)\\RivaTuner Statistics Server\\RTSSHooks64.dll", CharSet = CharSet.Ansi)]
        public static extern void UpdateProfiles();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        private static int? GetTopLevelProcessId()
        {
            var hWnd = GetForegroundWindow();
            var result = GetWindowThreadProcessId(hWnd, out uint processId);
            if (result != 0)
                return (int)processId;
            return null;
        }

        private static void PostMessage(uint Msg, IntPtr wParam, IntPtr lParam)
        {
            var hWnd = FindWindow(null, "RTSS");
            if (hWnd == IntPtr.Zero)
                hWnd = FindWindow(null, "RivaTuner Statistics Server");

            if (hWnd != IntPtr.Zero)
                PostMessage(hWnd, Msg, wParam, lParam);
        }

        public const uint WM_APP = 0x8000;
        public const uint WM_RTSS_UPDATESETTINGS = WM_APP + 100;
        public const uint WM_RTSS_SHOW_PROPERTIES = WM_APP + 102;

        public const uint RTSSHOOKSFLAG_OSD_VISIBLE = 1;
        public const uint RTSSHOOKSFLAG_LIMITER_DISABLED = 4;
        public const string GLOBAL_PROFILE = "";
    }
}
