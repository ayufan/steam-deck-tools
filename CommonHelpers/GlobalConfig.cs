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
        Minimal,
        Detail,
        Full
    }
}
