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
                {
                    var pixelClock = (short)Math.Ceiling((double)Timing.sHTotal * Timing.sVTotal * refreshRate / 10000);

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

        internal static IEnumerable<ADLDisplayModeInfo> AllDetailedTimings()
        {
            foreach (var timing in AllTimings)
            {
                foreach (var modeInfo in timing.EachModeInfo())
                    yield return modeInfo;
            }
        }

        internal static IEnumerable<ADLDisplayModeInfo>? AllTimingOverrideList()
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

        internal static void InstallAll()
        {
            var modeInfos = AllDetailedTimings()?.ToArray();
            if (modeInfos is null)
                return;

            ADLDisplayModeInfo lastModeInfo = modeInfos.LastOrDefault();
            int count = 0;

            foreach (var modeInfo in modeInfos)
            {
                SetTimingOverride(modeInfo, modeInfo.Equals(lastModeInfo) || (count++ % 10 == 9));
            }
        }

        internal static void UninstallAll()
        {
            var modes = AllTimingOverrideList()?.ToArray();
            if (modes is null)
                return;

            ADLDisplayModeInfo lastMode = modes.LastOrDefault();

            foreach (var mode in modes)
            {
                var newMode = mode;
                newMode.iTimingStandard = Helpers.AMD.ADL.ADL_DL_MODETIMING_STANDARD_DRIVER_DEFAULT;
                RemoveTimingOverride(newMode, mode.Equals(lastMode));
            }
        }

        internal static ADLDisplayModeInfo? FindTiming(ADLDisplayModeX2 displayMode)
        {
            foreach (var modeInfo in AllDetailedTimings())
            {
                if (modeInfo.iPelsWidth == displayMode.PelsWidth &&
                    modeInfo.iPelsHeight == displayMode.PelsHeight &&
                    modeInfo.iRefreshRate == displayMode.RefreshRate)
                    return modeInfo;
            }

            return null;
        }

        internal static bool AddTiming(ADLDisplayModeX2 displayMode, bool removeOld = false)
        {
            if (removeOld)
            {
                var modes = AllTimingOverrideList()?.ToArray() ?? new ADLDisplayModeInfo[0];
                foreach (var oldMode in modes)
                    RemoveTimingOverride(oldMode, false);
            }

            var timing = FindTiming(displayMode);
            if (timing is not null)
                return SetTimingOverride(timing.Value, true);
            return false;
        }

        internal static bool RemoveTimingOverride(ADLDisplayModeInfo displayMode, bool forceUpdate = true)
        {
            displayMode.iTimingStandard = ADL.ADL_DL_MODETIMING_STANDARD_DRIVER_DEFAULT;
            return SetTimingOverride(displayMode, forceUpdate);
        }

        internal static bool SetTimingOverride(ADLDisplayModeInfo displayMode, bool forceUpdate = true)
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
    }
}
