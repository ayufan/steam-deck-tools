using System.Linq;
using System.Runtime.InteropServices;

namespace PowerControl.Helpers.AMD
{
    internal class EDID
    {
        internal static byte[]? GetEDID(int displayIndex = ADL.ADL_DEFAULT_DISPLAY)
        {
            return ADLContext.WithSafe((context) =>
            {
                byte[] edid = new byte[0];

                for (int block = 0; block < ADL.ADL_MAX_EDID_EXTENSION_BLOCKS; ++block)
                {
                    ADLDisplayEDIDData displayEdidData = new ADLDisplayEDIDData()
                    {
                        iSize = Marshal.SizeOf<ADLDisplayEDIDData>(),
                        iBlockIndex = block,
                    };

                    int res = ADL.ADL2_Display_EdidData_Get(context.Context, ADL.ADL_DEFAULT_ADAPTER, ADL.ADL_DEFAULT_DISPLAY, ref displayEdidData);
                    if (res != 0)
                        break;

                    var blockBytes = displayEdidData.cEDIDData.Take(displayEdidData.iEDIDSize);
                    edid = edid.Concat(blockBytes).ToArray();
                }

                return edid;
            });
        }

        internal static bool? SetEDID(byte[] value, int displayIndex = ADL.ADL_DEFAULT_DISPLAY)
        {
            return ADLContext.WithSafe((context) =>
            {
                var bytes = new byte[ADL.ADL_MAX_EDIDDATA_SIZE * 2];
                value.CopyTo(bytes, 0);

                var blockData = new ADLDisplayEDIDDataX2()
                {
                    // TODO: Hack to send a full EDID at once
                    iSize = Marshal.SizeOf<ADLDisplayEDIDData>(),
                    cEDIDData = bytes,
                    iEDIDSize = value.Length,
                };

                int res = ADL.ADL2_Display_EdidData_Set(context.Context,
                    ADL.ADL_DEFAULT_ADAPTER, ADL.ADL_DEFAULT_DISPLAY,
                    ref blockData);

                return res == 0;
            });
        }
    }
}
