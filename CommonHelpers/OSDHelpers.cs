using RTSSSharedMemoryNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelpers
{
    public static class OSDHelpers
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

        public static bool IsLoaded
        {
            get
            {
                Applications.Instance.Refresh();
                return Applications.Instance.IsLoaded;
            }
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
            public bool IsLoaded { get; private set; }

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

                try
                {
                    appEntries = OSD.GetAppEntries(AppFlags.MASK);
                    IsLoaded = true;
                }
                catch
                {
                    IsLoaded = false;
                    return;
                }

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

        private static int? GetTopLevelProcessId()
        {
            var hWnd = GetForegroundWindow();
            var result = GetWindowThreadProcessId(hWnd, out uint processId);
            if (result != 0)
                return (int)processId;
            return null;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        public static uint OSDIndex(this OSD? osd)
        {
            if (osd is null)
                return uint.MaxValue;

            var osdSlot = typeof(OSD).GetField("m_osdSlot",
                   System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var value = osdSlot.GetValue(osd);
            if (value is null)
                return uint.MaxValue;

            return (uint)value;
        }

        public static uint OSDIndex(String name)
        {
            var entries = OSD.GetOSDEntries().ToList();
            for (int i = 0; i < entries.Count(); i++)
            {
                if (entries[i].Owner == name)
                    return (uint)i;
            }
            return 0;
        }
    }
}
