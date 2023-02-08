using System.Runtime.InteropServices;

namespace PowerControl.Helpers.AMD
{
    internal class ModeTiming
    {
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
            displayMode.TimingStandard = Helpers.AMD.ADL.ADL_DL_MODETIMING_STANDARD_DRIVER_DEFAULT;
            return AddTiming(displayMode);
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
