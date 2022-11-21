using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerControl.Helpers.AMD
{
    internal class GPUScaling
    {
        public enum ScalingMode
        {
            AspectRatio,
            FullPanel,
            Centered
        }

        internal static bool SafeResolutionChange
        {
            get { return Enabled; }
        }

        internal static bool IsSupported
        {
            get
            {
                return ADLContext.WithSafe((context) =>
                {
                    int res = ADL.ADL2_DFP_GPUScalingEnable_Get(
                        context.Context,
                        ADL.ADL_DEFAULT_ADAPTER,
                        ADL.ADL_DEFAULT_DISPLAY,
                        out var support, out _, out _);

                    if (res == 0 && support == 1)
                        return true;

                    return false;
                });
            }
        }

        internal static bool Enabled
        {
            get
            {
                return ADLContext.WithSafe((context) =>
                {
                    int res = ADL.ADL2_DFP_GPUScalingEnable_Get(
                        context.Context,
                        ADL.ADL_DEFAULT_ADAPTER,
                        ADL.ADL_DEFAULT_DISPLAY,
                        out var support, out var current, out _);
                    if (res == 0 && support == 1 && current == 1)
                        return true;

                    return false;
                });
            }
            set
            {
                ADLContext.WithSafe((context) =>
                {
                    ADL.ADL2_DFP_GPUScalingEnable_Get(
                        context.Context,
                        ADL.ADL_DEFAULT_ADAPTER,
                        ADL.ADL_DEFAULT_DISPLAY,
                        out var _, out var current, out _);
                    if (current == (value ? 1 : 0))
                        return true;

                    ADL.ADL2_DFP_GPUScalingEnable_Set(
                        context.Context,
                        ADL.ADL_DEFAULT_ADAPTER,
                        ADL.ADL_DEFAULT_DISPLAY,
                        value ? 1 : 0
                    );

                    return true;
                });
            }
        }

        internal static ScalingMode? Mode
        {
            get
            {
                return ADLContext.WithSafe((context) =>
                {
                    int resAR = ADL.ADL2_Display_PreservedAspectRatio_Get(
                        context.Context,
                        ADL.ADL_DEFAULT_ADAPTER,
                        ADL.ADL_DEFAULT_DISPLAY,
                        out var supportAR, out var ar, out _);
                    int resIE = ADL.ADL2_Display_ImageExpansion_Get(
                        context.Context,
                        ADL.ADL_DEFAULT_ADAPTER,
                        ADL.ADL_DEFAULT_DISPLAY,
                        out var supportIE, out var ie, out _);

                    if (resAR != 0 || resIE != 0)
                        return default(ScalingMode?);

                    TraceLine("GPUScaling: ar={0}, ie={1}",
                        supportAR > 0 ? ar : -1, supportIE > 0 ? ie : -1);

                    if (ar == 1)
                        return ScalingMode.AspectRatio;
                    else if (ie == 1)
                        return ScalingMode.FullPanel;
                    else if (ie == 0 && supportIE == 1)
                        return ScalingMode.Centered;

                    return default(ScalingMode?);
                });
            }
            set
            {
                ADLContext.WithSafe((context) =>
                {
                    int resGS = ADL.ADL2_DFP_GPUScalingEnable_Get(
                        context.Context,
                        ADL.ADL_DEFAULT_ADAPTER,
                        ADL.ADL_DEFAULT_DISPLAY,
                        out var _, out var current, out _);
                    if (current == 0)
                    {
                        resGS = ADL.ADL2_DFP_GPUScalingEnable_Set(
                            context.Context,
                            ADL.ADL_DEFAULT_ADAPTER,
                            ADL.ADL_DEFAULT_DISPLAY,
                            1
                        );
                    }

                    int resAR = -1;
                    int resIE = -1;

                    switch (value)
                    {
                        case ScalingMode.FullPanel:
                            resIE = ADL.ADL2_Display_ImageExpansion_Set(
                                context.Context,
                                ADL.ADL_DEFAULT_ADAPTER,
                                ADL.ADL_DEFAULT_DISPLAY,
                                1
                            );
                            break;

                        case ScalingMode.AspectRatio:
                            resAR = ADL.ADL2_Display_PreservedAspectRatio_Set(
                                context.Context,
                                ADL.ADL_DEFAULT_ADAPTER,
                                ADL.ADL_DEFAULT_DISPLAY,
                                1
                            );
                            break;

                        case ScalingMode.Centered:
                            resIE = ADL.ADL2_Display_ImageExpansion_Set(
                                context.Context,
                                ADL.ADL_DEFAULT_ADAPTER,
                                ADL.ADL_DEFAULT_DISPLAY,
                                0
                            );
                            break;
                    }

                    TraceLine("GPUScaling: mode={0} => resAR={1}, resIE={2}, resGS={3}",
                        value, resAR, resIE, resGS);

                    return true;
                });
            }
        }

        private static void TraceLine(string format, params object?[]? arg)
        {
            Trace.WriteLine(string.Format(format, arg));
        }
    }
}
