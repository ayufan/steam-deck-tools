using System.Runtime.InteropServices;
using CommonHelpers;
using hidapi;
using PowerControl.External;
using static CommonHelpers.Log;

namespace SteamController.Devices
{
    public partial class SteamController
    {
        public bool SetHaptic(byte position, ushort amplitude, ushort period, ushort count = 0)
        {
            var haptic = new SDCHapticPacket()
            {
                packet_type = (byte)SDCPacketType.PT_FEEDBACK,
                len = (byte)SDCPacketLength.PL_FEEDBACK,
                position = position,
                amplitude = amplitude,
                period = period,
                count = count
            };

            var bytes = new byte[Marshal.SizeOf<SDCHapticPacket>()];
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                Marshal.StructureToPtr(haptic, handle.AddrOfPinnedObject(), false);
                neptuneDevice.RequestFeatureReport(bytes);
                return true;
            }
            catch (Exception e)
            {
                Log.TraceLine("STEAM: Haptic: Exception: {0}", e);
                return false;
            }
        }
    }
}
