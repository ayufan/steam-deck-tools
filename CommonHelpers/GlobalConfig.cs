using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelpers
{
    public enum FanMode : uint
    {
        Default = 17374,
        SteamOS,
        Max
    }

    public enum KernelDriversLoaded : uint
    {
        Yes = 4363232,
        No
    }

    public enum OverlayMode : uint
    {
        FPS = 10032,
        FPSWithBattery,
        Minimal,
        Detail,
        Full
    }

    public enum OverlayEnabled : uint
    {
        Yes = 378313,
        No
    }

    public enum PowerControlVisible : uint
    {
        Yes = 371313,
        No
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FanModeSetting
    {
        public FanMode Current, Desired;
        public KernelDriversLoaded KernelDriversLoaded;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct OverlayModeSetting
    {
        public OverlayMode Current, Desired;
        public OverlayEnabled CurrentEnabled, DesiredEnabled;
        public KernelDriversLoaded KernelDriversLoaded;
        public KernelDriversLoaded DesiredKernelDriversLoaded;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PowerControlSetting
    {
        public PowerControlVisible Current;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct SteamControllerSetting
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public String CurrentProfile;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 2048)]
        public String SelectableProfiles;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public String DesiredProfile;
    }
}
