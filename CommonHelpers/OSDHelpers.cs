using RTSSSharedMemoryNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelpers
{
    public static class OSDHelpers
    {
        public static uint OSDIndex(this OSD? osd)
        {
            if (osd is null)
                return uint.MaxValue;

            var osdSlot = typeof(OSD).GetField("m_osdSlot",
                   System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var value = osdSlot.GetValue(osd);
            if (value is null)
                return uint.MaxValue;

            return (uint)value;
        }

        public static uint OSDIndex(String name)
        {
            var entries = OSD.GetOSDEntries().ToList();
            for (int i = 0; i < entries.Count(); i++)
            {
                if (entries[i].Owner == name)
                    return (uint)i;
            }
            return 0;
        }
    }
}
