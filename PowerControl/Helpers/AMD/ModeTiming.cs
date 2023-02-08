using System.Runtime.InteropServices;

namespace PowerControl.Helpers.AMD
{
    internal class ModeTiming
    {
        internal static bool AddAndSetTiming(ADLDisplayModeX2 displayMode)
        {
            return AddTiming(displayMode) && SetTiming(displayMode);
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
