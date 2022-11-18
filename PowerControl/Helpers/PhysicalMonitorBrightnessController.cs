using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;

namespace PowerControl.Helpers
{
    // Taken from: https://stackoverflow.com/questions/4013622/adjust-screen-brightness-using-c-sharp
    public class PhysicalMonitorBrightnessController : IDisposable
    {
        #region DllImport
        [DllImport("dxva2.dll", EntryPoint = "GetNumberOfPhysicalMonitorsFromHMONITOR")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, ref uint pdwNumberOfPhysicalMonitors);

        [DllImport("dxva2.dll", EntryPoint = "GetPhysicalMonitorsFromHMONITOR")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        [DllImport("dxva2.dll", EntryPoint = "GetMonitorBrightness")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetMonitorBrightness(IntPtr handle, ref uint minimumBrightness, ref uint currentBrightness, ref uint maxBrightness);

        [DllImport("dxva2.dll", EntryPoint = "SetMonitorBrightness")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetMonitorBrightness(IntPtr handle, uint newBrightness);

        [DllImport("dxva2.dll", EntryPoint = "DestroyPhysicalMonitor")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyPhysicalMonitor(IntPtr hMonitor);

        [DllImport("dxva2.dll", EntryPoint = "DestroyPhysicalMonitors")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyPhysicalMonitors(uint dwPhysicalMonitorArraySize, [In] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        [DllImport("user32.dll")]
        static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, EnumMonitorsDelegate lpfnEnum, IntPtr dwData);
        delegate bool EnumMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);

        enum DISP_CHANGE : int
        {
            Successful = 0,
            Restart = 1,
            Failed = -1,
            BadMode = -2,
            NotUpdated = -3,
            BadFlags = -4,
            BadParam = -5,
            BadDualView = -6
        }

        [Flags()]
        enum DM : int
        {
            Orientation = 0x1,
            PaperSize = 0x2,
            PaperLength = 0x4,
            PaperWidth = 0x8,
            Scale = 0x10,
            Position = 0x20,
            NUP = 0x40,
            DisplayOrientation = 0x80,
            Copies = 0x100,
            DefaultSource = 0x200,
            PrintQuality = 0x400,
            Color = 0x800,
            Duplex = 0x1000,
            YResolution = 0x2000,
            TTOption = 0x4000,
            Collate = 0x8000,
            FormName = 0x10000,
            LogPixels = 0x20000,
            BitsPerPixel = 0x40000,
            PelsWidth = 0x80000,
            PelsHeight = 0x100000,
            DisplayFlags = 0x200000,
            DisplayFrequency = 0x400000,
            ICMMethod = 0x800000,
            ICMIntent = 0x1000000,
            MeduaType = 0x2000000,
            DitherType = 0x4000000,
            PanningWidth = 0x8000000,
            PanningHeight = 0x10000000,
            DisplayFixedOutput = 0x20000000
        }

        struct POINTL
        {
            public Int32 x;
            public Int32 y;
        };

        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
        struct DEVMODE
        {
            public const int CCHDEVICENAME = 32;
            public const int CCHFORMNAME = 32;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            [System.Runtime.InteropServices.FieldOffset(0)]
            public string dmDeviceName;
            [System.Runtime.InteropServices.FieldOffset(32)]
            public Int16 dmSpecVersion;
            [System.Runtime.InteropServices.FieldOffset(34)]
            public Int16 dmDriverVersion;
            [System.Runtime.InteropServices.FieldOffset(36)]
            public Int16 dmSize;
            [System.Runtime.InteropServices.FieldOffset(38)]
            public Int16 dmDriverExtra;
            [System.Runtime.InteropServices.FieldOffset(40)]
            public DM dmFields;

            [System.Runtime.InteropServices.FieldOffset(44)]
            Int16 dmOrientation;
            [System.Runtime.InteropServices.FieldOffset(46)]
            Int16 dmPaperSize;
            [System.Runtime.InteropServices.FieldOffset(48)]
            Int16 dmPaperLength;
            [System.Runtime.InteropServices.FieldOffset(50)]
            Int16 dmPaperWidth;
            [System.Runtime.InteropServices.FieldOffset(52)]
            Int16 dmScale;
            [System.Runtime.InteropServices.FieldOffset(54)]
            Int16 dmCopies;
            [System.Runtime.InteropServices.FieldOffset(56)]
            Int16 dmDefaultSource;
            [System.Runtime.InteropServices.FieldOffset(58)]
            Int16 dmPrintQuality;

            [System.Runtime.InteropServices.FieldOffset(44)]
            public POINTL dmPosition;
            [System.Runtime.InteropServices.FieldOffset(52)]
            public Int32 dmDisplayOrientation;
            [System.Runtime.InteropServices.FieldOffset(56)]
            public Int32 dmDisplayFixedOutput;

            [System.Runtime.InteropServices.FieldOffset(60)]
            public short dmColor; // See note below!
            [System.Runtime.InteropServices.FieldOffset(62)]
            public short dmDuplex; // See note below!
            [System.Runtime.InteropServices.FieldOffset(64)]
            public short dmYResolution;
            [System.Runtime.InteropServices.FieldOffset(66)]
            public short dmTTOption;
            [System.Runtime.InteropServices.FieldOffset(68)]
            public short dmCollate; // See note below!
            //[System.Runtime.InteropServices.FieldOffset(70)]
            //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
            //public string dmFormName;
            [System.Runtime.InteropServices.FieldOffset(102)]
            public Int16 dmLogPixels;
            [System.Runtime.InteropServices.FieldOffset(104)]
            public Int32 dmBitsPerPel;
            [System.Runtime.InteropServices.FieldOffset(108)]
            public Int32 dmPelsWidth;
            [System.Runtime.InteropServices.FieldOffset(112)]
            public Int32 dmPelsHeight;
            [System.Runtime.InteropServices.FieldOffset(116)]
            public Int32 dmDisplayFlags;
            [System.Runtime.InteropServices.FieldOffset(116)]
            public Int32 dmNup;
            [System.Runtime.InteropServices.FieldOffset(120)]
            public Int32 dmDisplayFrequency;
        }

        [Flags()]
        public enum ChangeDisplaySettingsFlags : uint
        {
            CDS_NONE = 0,
            CDS_UPDATEREGISTRY = 0x00000001,
            CDS_TEST = 0x00000002,
            CDS_FULLSCREEN = 0x00000004,
            CDS_GLOBAL = 0x00000008,
            CDS_SET_PRIMARY = 0x00000010,
            CDS_VIDEOPARAMETERS = 0x00000020,
            CDS_ENABLE_UNSAFE_MODES = 0x00000100,
            CDS_DISABLE_UNSAFE_MODES = 0x00000200,
            CDS_RESET = 0x40000000,
            CDS_RESET_EX = 0x20000000,
            CDS_NORESET = 0x10000000
        }

        const int ENUM_CURRENT_SETTINGS = -1;
        const int ENUM_REGISTRY_SETTINGS = -2;

        [DllImport("user32.dll")]
        static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll")]
        static extern DISP_CHANGE ChangeDisplaySettingsEx(string lpszDeviceName, ref DEVMODE lpDevMode, IntPtr hwnd, ChangeDisplaySettingsFlags dwflags, IntPtr lParam);
        #endregion

        private IReadOnlyCollection<MonitorInfo> Monitors { get; set; }

        public PhysicalMonitorBrightnessController()
        {
            UpdateMonitors();
        }

        public struct DisplayResolution
        {
            public int Width { get; set; }
            public int Height { get; set; }

            public DisplayResolution() { Width = 0; Height = 0; }

            public DisplayResolution(int width, int height) { Width = width; Height = height; }

            public static bool operator ==(DisplayResolution sz1, DisplayResolution sz2) => sz1.Width == sz2.Width && sz1.Height == sz2.Height;

            public static bool operator !=(DisplayResolution sz1, DisplayResolution sz2) => !(sz1 == sz2);

            public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is DisplayResolution && Equals((DisplayResolution)obj);
            public readonly bool Equals(DisplayResolution other) => this == other;

            public override readonly int GetHashCode() => HashCode.Combine(Width, Height);

            public override string ToString()
            {
                return String.Format("{0}x{1}", Width, Height);
            }
        }

        #region Get & Set
        public void Set(uint brightness)
        {
            Set(brightness, true);
        }

        public static DisplayResolution[] GetAllResolutions()
        {
            HashSet<DisplayResolution> resolutions = new HashSet<DisplayResolution>();
            DEVMODE current = new DEVMODE();

            if (!EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref current))
                return new DisplayResolution[0];

            DEVMODE dm = new DEVMODE();
            for (int i = 0; EnumDisplaySettings(null, i, ref dm); i++)
            {
                if (!dm.dmFields.HasFlag(DM.PelsWidth) || !dm.dmFields.HasFlag(DM.PelsHeight))
                    continue;
                resolutions.Add(new DisplayResolution(dm.dmPelsWidth, dm.dmPelsHeight));
            }

            return resolutions.ToArray();
        }

        public static DisplayResolution? GetResolution()
        {
            DEVMODE dm = new DEVMODE();

            if (!EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm))
                return null;

            if (!dm.dmFields.HasFlag(DM.PelsWidth) || !dm.dmFields.HasFlag(DM.PelsHeight))
                return null;

            return new DisplayResolution(dm.dmPelsWidth, dm.dmPelsHeight);
        }

        public static bool SetResolution(DisplayResolution size)
        {
            DEVMODE dm = new DEVMODE();

            if (!EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm))
                return false;

            var refreshRates = GetRefreshRates(size);
            if (refreshRates.Count() == 0)
                return false;

            dm.dmFields |= DM.PelsWidth | DM.PelsHeight | DM.DisplayFrequency;
            dm.dmPelsWidth = size.Width;
            dm.dmPelsHeight = size.Height;
            dm.dmDisplayFrequency = refreshRates.Last();

            var dispChange = ChangeDisplaySettingsEx(null, ref dm, IntPtr.Zero, ChangeDisplaySettingsFlags.CDS_NONE, IntPtr.Zero);

            if (!EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm))
                return false;

            Trace.WriteLine("DispChange: " + dispChange.ToString() + " Size:" + size.ToString() + 
                " SetSize:" + new DisplayResolution(dm.dmPelsWidth, dm.dmPelsHeight).ToString());

            if (dispChange == DISP_CHANGE.Successful)
                return true;

            return true;
        }

        public static int[] GetRefreshRates(DisplayResolution? size = null)
        {
            List<int> refreshRates = new List<int>();
            DEVMODE current = new DEVMODE();

            if (size is null)
                size = GetResolution();
            if (size is null)
                return new int[0];

            if (!EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref current))
                return new int[0];

            DEVMODE dm = new DEVMODE();
            for (int i = 0; EnumDisplaySettings(null, i, ref dm); i++)
            {
                if (dm.dmPelsWidth != size?.Width || dm.dmPelsHeight != size?.Height || dm.dmBitsPerPel != current.dmBitsPerPel)
                    continue;
                refreshRates.Add(dm.dmDisplayFrequency);
            }

            refreshRates.Sort();

            return refreshRates.ToArray();
        }

        public static int GetRefreshRate()
        {
            DEVMODE dm = new DEVMODE();

            if (!EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm))
                return -1;

            return dm.dmDisplayFrequency;
        }

        public static bool SetRefreshRate(int hz)
        {
            DEVMODE dm = new DEVMODE();

            var allowedRefreshRates = GetRefreshRates();
            if (!allowedRefreshRates.Contains(hz))
                return false;

            if (!EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm))
                return false;

            dm.dmFields |= DM.DisplayFrequency;
            dm.dmDisplayFrequency = hz;

            var dispChange = ChangeDisplaySettingsEx(null, ref dm, IntPtr.Zero, ChangeDisplaySettingsFlags.CDS_NONE, IntPtr.Zero);

            if (!EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm))
                return false;

            Trace.WriteLine("DispChange: " + dispChange.ToString() + " HZ:" + hz.ToString() + " SetHZ:" + dm.dmDisplayFrequency.ToString());

            if (dispChange == DISP_CHANGE.Successful)
                return true;

            return true;
        }

        private void Set(uint brightness, bool refreshMonitorsIfNeeded)
        {
            bool isSomeFail = false;
            foreach (var monitor in Monitors)
            {
                uint realNewValue = (monitor.MaxValue - monitor.MinValue) * brightness / 100 + monitor.MinValue;
                if (SetMonitorBrightness(monitor.Handle, realNewValue))
                {
                    monitor.CurrentValue = realNewValue;
                }
                else if (refreshMonitorsIfNeeded)
                {
                    isSomeFail = true;
                    break;
                }
            }

            if (refreshMonitorsIfNeeded && (isSomeFail || !Monitors.Any()))
            {
                UpdateMonitors();
                Set(brightness, false);
                return;
            }
        }

        public int Get()
        {
            if (!Monitors.Any())
            {
                return -1;
            }
            return (int)Monitors.Average(d => d.CurrentValue);
        }
        #endregion

        private void UpdateMonitors()
        {
            DisposeMonitors(this.Monitors);

            var monitors = new List<MonitorInfo>();
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData) =>
            {
                uint physicalMonitorsCount = 0;
                if (!GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, ref physicalMonitorsCount))
                {
                    // Cannot get monitor count
                    return true;
                }

                var physicalMonitors = new PHYSICAL_MONITOR[physicalMonitorsCount];
                if (!GetPhysicalMonitorsFromHMONITOR(hMonitor, physicalMonitorsCount, physicalMonitors))
                {
                    // Cannot get physical monitor handle
                    return true;
                }

                foreach (PHYSICAL_MONITOR physicalMonitor in physicalMonitors)
                {
                    uint minValue = 0, currentValue = 0, maxValue = 0;
                    var info = new MonitorInfo
                    {
                        Handle = physicalMonitor.hPhysicalMonitor,
                        MinValue = minValue,
                        CurrentValue = currentValue,
                        MaxValue = maxValue,
                    };
                    monitors.Add(info);
                }

                return true;
            }, IntPtr.Zero);

            this.Monitors = monitors;
        }

        public void Dispose()
        {
            DisposeMonitors(Monitors);
            GC.SuppressFinalize(this);
        }

        private static void DisposeMonitors(IEnumerable<MonitorInfo> monitors)
        {
            if (monitors?.Any() == true)
            {
                PHYSICAL_MONITOR[] monitorArray = monitors.Select(m => new PHYSICAL_MONITOR { hPhysicalMonitor = m.Handle }).ToArray();
                DestroyPhysicalMonitors((uint)monitorArray.Length, monitorArray);
            }
        }

        #region Classes
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct PHYSICAL_MONITOR
        {
            public IntPtr hPhysicalMonitor;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szPhysicalMonitorDescription;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        public class MonitorInfo
        {
            public uint MinValue { get; set; }
            public uint MaxValue { get; set; }
            public IntPtr Handle { get; set; }
            public uint CurrentValue { get; set; }
        }
        #endregion
    }

}
