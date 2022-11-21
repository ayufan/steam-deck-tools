using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerControl.Helpers.AMD
{
    internal class DCE
    {
        internal enum Mode
        {
            Normal = 1,
            Vivid
        }

        internal static Mode? Current
        {
            get
            {
                return ADLContext.WithSafe((context) =>
                {
                    int res = ADL.ADL2_Display_SCE_State_Get(
                        context.Context,
                        ADL.ADL_DEFAULT_ADAPTER,
                        ADL.ADL_DEFAULT_DISPLAY,
                        out var current, out var support, out _);

                    if (res == 0 && support == 1)
                        return current == 2 ? Mode.Vivid : Mode.Normal;

                    return (DCE.Mode?)null;
                });
            }
            set
            {
                ADLContext.WithSafe((context) =>
                {
                    if (value is null)
                        return false;

                    ADL.ADL2_Display_SCE_State_Set(
                        context.Context,
                        ADL.ADL_DEFAULT_ADAPTER,
                        ADL.ADL_DEFAULT_DISPLAY,
                        (int)value
                   );

                    return true;
                });
            }
        }
    }
}
