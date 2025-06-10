﻿using System.Runtime.InteropServices;

namespace CommonHelpers
{
    public enum FanMode : uint
    {
        Default = 17374,
        Silent,
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
        Battery,
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
