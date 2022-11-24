using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerControl.Helpers.AMD
{
    internal class ImageSharpening
    {
        internal static bool? Enabled
        {
            get
            {
                return GetSettings(out var settings) ? settings.GlobalEnable != 0 : null;
            }
            set
            {
                if (!GetSettings(out var settings))
                    return;

                var enabled = value.GetValueOrDefault(false) ? 1 : 0;
                if (settings.GlobalEnable == enabled)
                    return;

                settings.GlobalEnable = enabled;
                SetSettings(settings, new ADL_RIS_NOTFICATION_REASON() { GlobalEnableChanged = 1 });
            }
        }

        private static bool GetSettings(out ADL_RIS_SETTINGS settings)
        {
            ADL_RIS_SETTINGS settings2 = default;

            var result = ADLContext.WithSafe((context) =>
            {
                int res = ADL.ADL2_RIS_Settings_Get(
                        context.Context,
                        ADL.ADL_DEFAULT_ADAPTER,
                        out settings2);
                return res == 0;
            });

            settings = settings2;
            return result;
        }

        private static bool SetSettings(ADL_RIS_SETTINGS settings, ADL_RIS_NOTFICATION_REASON reason)
        {
            return ADLContext.WithSafe((context) =>
            {
                int res = ADL.ADL2_RIS_Settings_Set(
                        context.Context,
                        ADL.ADL_DEFAULT_ADAPTER,
                        settings, reason);
                return res == 0;
            });
        }
    }
}
