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

    [StructLayout(LayoutKind.Sequential)]
    public struct FanModeSetting
    {
        public FanMode Current, Desired;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct OverlayModeSetting
    {
        public OverlayMode Current, Desired;
        public OverlayEnabled CurrentEnabled, DesiredEnabled;
    }
}
