using System.Runtime.InteropServices;

namespace PowerControl.Helpers.AMD
{
    internal class ModeTiming
    {
        public struct DetailedTiming
        {
            public ADLDetailedTiming Timing { get; set; }
            public List<int> PixelClocks { get; } = new List<int>();
            public List<int> RefreshRates { get; } = new List<int>();

            public DetailedTiming(
                short hDisplay, short hFront, short hSync, short hBack,
                short vDisplay, short vFront, short vSync, short vBack,
                short timingFlags = ADL.ADL_DL_TIMINGFLAG_H_SYNC_POLARITY | ADL.ADL_DL_TIMINGFLAG_V_SYNC_POLARITY,
                short pixelClock = 0)
            {
                Timing = new ADLDetailedTiming()
                {
                    iSize = 96,
                    sPixelClock = pixelClock,
                    sTimingFlags = timingFlags,
                    sHDisplay = hDisplay,
                    sHSyncStart = (short)(hDisplay + hFront),
                    sHSyncWidth = hSync,
                    sHTotal = (short)(hDisplay + hFront + hSync + hBack),
                    sVDisplay = vDisplay,
                    sVSyncStart = (short)(vDisplay + vFront),
                    sVSyncWidth = vSync,
                    sVTotal = (short)(vDisplay + vFront + vSync + vBack),
                };
            }

            public IEnumerable<ADLDisplayModeInfo> EachModeInfo()
            {
                foreach (var refreshRate in RefreshRates)
                // foreach (var pixelClock in PixelClocks)
                {
                    var pixelClock = (short)Math.Ceiling((double)Timing.sHTotal * Timing.sVTotal * refreshRate / 10000);
                    // var refreshRate = Math.Round((double)pixelClock / (Timing.sHTotal * Timing.sVTotal));

                    ADLDisplayModeInfo modeInfo = new ADLDisplayModeInfo()
                    {
                        iPelsHeight = Timing.sVDisplay,
                        iPelsWidth = Timing.sHDisplay,
                        iPossibleStandard = 0,
                        iRefreshRate = (short)refreshRate,
                        iTimingStandard = ADL.ADL_DL_MODETIMING_STANDARD_CUSTOM,
                        sDetailedTiming = Timing,
                    };

                    modeInfo.sDetailedTiming.sPixelClock = (short)pixelClock;

                    yield return modeInfo;
                }
            }
        };

        public static readonly DetailedTiming[] AllTimings = new DetailedTiming[] {
            new DetailedTiming(400, 32, 20, 20, 640, 16, 2, 26, ADL.ADL_DL_TIMINGFLAG_H_SYNC_POLARITY | ADL.ADL_DL_TIMINGFLAG_V_SYNC_POLARITY) {
                RefreshRates = { 30, 35, 40, 45, 48, 50, 55, 60 }
            },
            new DetailedTiming(640, 32, 20, 20, 1024, 16, 2, 26, ADL.ADL_DL_TIMINGFLAG_H_SYNC_POLARITY | ADL.ADL_DL_TIMINGFLAG_V_SYNC_POLARITY) {
                RefreshRates = { 30, 35, 40, 45, 48, 50, 55, 60 }
            },
            new DetailedTiming(540, 32, 20, 20, 960, 16, 2, 26, ADL.ADL_DL_TIMINGFLAG_H_SYNC_POLARITY | ADL.ADL_DL_TIMINGFLAG_V_SYNC_POLARITY) {
                RefreshRates = { 30, 35, 40, 45, 48, 50, 55, 60 }
            },
            new DetailedTiming(544, 32, 20, 20, 960, 16, 2, 26, ADL.ADL_DL_TIMINGFLAG_H_SYNC_POLARITY | ADL.ADL_DL_TIMINGFLAG_V_SYNC_POLARITY) {
                RefreshRates = { 30, 35, 40, 45, 48, 50, 55, 60 }
            },
            new DetailedTiming(600, 32, 20, 20, 800, 16, 2, 26, ADL.ADL_DL_TIMINGFLAG_H_SYNC_POLARITY | ADL.ADL_DL_TIMINGFLAG_V_SYNC_POLARITY) {
                RefreshRates = { 30, 35, 40, 45, 48, 50, 55, 60 }
            },
            new DetailedTiming(600, 32, 20, 20, 960, 16, 2, 26, ADL.ADL_DL_TIMINGFLAG_H_SYNC_POLARITY | ADL.ADL_DL_TIMINGFLAG_V_SYNC_POLARITY) {
                RefreshRates = { 30, 35, 40, 45, 48, 50, 55, 60 }
            },
            new DetailedTiming(680, 32, 20, 20, 1088, 16, 2, 26, ADL.ADL_DL_TIMINGFLAG_H_SYNC_POLARITY | ADL.ADL_DL_TIMINGFLAG_V_SYNC_POLARITY) {
                RefreshRates = { 30, 35, 40, 45, 48, 50, 55, 60 }
            },
            new DetailedTiming(576, 32, 20, 20, 1024, 16, 2, 26, ADL.ADL_DL_TIMINGFLAG_H_SYNC_POLARITY | ADL.ADL_DL_TIMINGFLAG_V_SYNC_POLARITY) {
                RefreshRates = { 30, 35, 40, 45, 48, 50, 55, 60 }
            },
            new DetailedTiming(768, 32, 20, 20, 1024, 16, 2, 26, ADL.ADL_DL_TIMINGFLAG_H_SYNC_POLARITY | ADL.ADL_DL_TIMINGFLAG_V_SYNC_POLARITY) {
                RefreshRates = { 30, 35, 40, 45, 48, 50, 55, 60 }
            },
            new DetailedTiming(720, 32, 20, 20, 1152, 16, 2, 26, ADL.ADL_DL_TIMINGFLAG_H_SYNC_POLARITY | ADL.ADL_DL_TIMINGFLAG_V_SYNC_POLARITY) {
                RefreshRates = { 30, 35, 40, 45, 48, 50, 55, 60 }
            },
            new DetailedTiming(720, 112, 20, 20, 1280, 16, 2, 26, ADL.ADL_DL_TIMINGFLAG_H_SYNC_POLARITY | ADL.ADL_DL_TIMINGFLAG_V_SYNC_POLARITY) {
                RefreshRates = { 30, 35, 40, 45, 48, 50, 55, 60 }
            },
            new DetailedTiming(800, 32, 20, 20, 1280, 16, 2, 26, ADL.ADL_DL_TIMINGFLAG_H_SYNC_POLARITY | ADL.ADL_DL_TIMINGFLAG_V_SYNC_POLARITY) {
                RefreshRates = { 30, 35, 40, 45, 48 }
            },
            new DetailedTiming(800, 56, 80, 136, 1280, 3, 10, 27, ADL.ADL_DL_TIMINGFLAG_V_SYNC_POLARITY) {
                RefreshRates = { 50, 55 }
            }
        };

        internal static IEnumerable<ADLDisplayModeInfo> AllModeInfos()
        {
            foreach (var timing in AllTimings)
            {
                foreach (var modeInfo in timing.EachModeInfo())
                    yield return modeInfo;
            }
        }

        internal static void InstallAll()
        {
            var modeInfos = AllModeInfos()?.ToArray();
            if (modeInfos is null)
                return;

            ADLDisplayModeInfo lastModeInfo = modeInfos.LastOrDefault();

            foreach (var modeInfo in modeInfos)
            {
                AddTiming(modeInfo, modeInfo.Equals(lastModeInfo));
            }
        }

        internal static void UninstallAll()
        {
            var modes = GetAllModes()?.ToArray();
            if (modes is null)
                return;

            ADLDisplayModeInfo lastMode = modes.LastOrDefault();

            foreach (var mode in modes)
            {
                var newMode = mode;
                newMode.iTimingStandard = Helpers.AMD.ADL.ADL_DL_MODETIMING_STANDARD_DRIVER_DEFAULT;
                AddTiming(newMode, mode.Equals(lastMode));
            }
        }

        internal static bool AddAndSetTiming(ADLDisplayModeX2 displayMode)
        {
            RemoveTiming(displayMode);
            return AddTiming(displayMode);
        }

        internal static IEnumerable<ADLDisplayModeInfo>? GetAllModes()
        {
            return Helpers.AMD.ADLContext.WithSafe((context) =>
            {
                int res = ADL.ADL2_Display_ModeTimingOverrideList_Get(context.Context,
                    Helpers.AMD.ADL.ADL_DEFAULT_ADAPTER, 0,
                    ADL.ADL_MAX_OVERRIDES, out var modes, out var modesCount);
                if (res != 0)
                    return null;

                return modes.ADLDisplayModeInfo.Take(modesCount);
            });
        }

        internal static bool ReplaceTiming(ADLDisplayModeX2 displayMode)
        {
            RemoveTiming(displayMode);
            return AddTiming(displayMode);
        }

        internal static bool RemoveTiming(ADLDisplayModeX2 displayMode)
        {
            displayMode.TimingStandard = ADL.ADL_DL_MODETIMING_STANDARD_DRIVER_DEFAULT;
            return AddTiming(displayMode);
        }

        internal static bool AddTiming(ADLDisplayModeInfo displayMode, bool forceUpdate = true)
        {
            return Helpers.AMD.ADLContext.WithSafe((context) =>
            {
                int res = ADL.ADL2_Display_ModeTimingOverride_Set(
                    context.Context,
                    Helpers.AMD.ADL.ADL_DEFAULT_ADAPTER,
                    0,
                    ref displayMode,
                    forceUpdate ? 1 : 0
                 );

                CommonHelpers.Log.TraceLine("{0}x{1}p{2} => PCLK: {3} => {4} (forceUpdate={5}, remove={6})",
                    displayMode.sDetailedTiming.sHDisplay, displayMode.sDetailedTiming.sVDisplay,
                    displayMode.iRefreshRate, displayMode.sDetailedTiming.sPixelClock,
                    res == 0 ? "OK" : "FAIL",
                    forceUpdate, displayMode.iTimingStandard == ADL.ADL_DL_MODETIMING_STANDARD_DRIVER_DEFAULT);

                return res == 0;
            });
        }

        internal static bool AddTiming(ADLDisplayModeX2 displayMode)
        {
            return Helpers.AMD.ADLContext.WithSafe((context) =>
            {
                var displays = context.DisplayInfos.ToArray();
                if (displays.Count() < 0)
                    return false;

                int res = ADL.ADL2_Display_ModeTimingOverrideX2_Get(
                    context.Context,
                    Helpers.AMD.ADL.ADL_DEFAULT_ADAPTER,
                    displays[0].DisplayID,
                    ref displayMode, out var modeInfOut);

                if (res == 0)
                {
                    res = ADL.ADL2_Display_ModeTimingOverride_Set(
                        context.Context,
                        Helpers.AMD.ADL.ADL_DEFAULT_ADAPTER,
                        displays[0].DisplayID.DisplayLogicalIndex,
                        ref modeInfOut,
                        1
                    );
                }

                return res == 0;
            });
        }

        internal static bool SetTiming(ADLDisplayModeX2 displayMode)
        {
            return Helpers.AMD.ADLContext.WithSafe((context) =>
            {
                int res = ADL.ADL2_Display_Modes_Get(context.Context,
                        Helpers.AMD.ADL.ADL_DEFAULT_ADAPTER,
                        0, out var modeCount, out var modesArray);

                try
                {
                    if (res != 0 || modeCount < 1)
                        return false;

                    var mode = Marshal.PtrToStructure<ADLMode>(modesArray);
                    mode.iXRes = displayMode.PelsWidth;
                    mode.iYRes = displayMode.PelsHeight;
                    mode.fRefreshRate = (float)displayMode.RefreshRate;

                    res = ADL.ADL2_Display_Modes_Set(context.Context,
                        Helpers.AMD.ADL.ADL_DEFAULT_ADAPTER,
                        0, 1, ref mode);
                    return res == 0;
                }
                finally
                {
                    ADL.ADL_Main_Memory_Free(modesArray);
                }
            });
        }
    }
}
