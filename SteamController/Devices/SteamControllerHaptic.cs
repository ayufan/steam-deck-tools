using System.Runtime.InteropServices;
using CommonHelpers;
using hidapi;
using PowerControl.External;
using static CommonHelpers.Log;

namespace SteamController.Devices
{
    public partial class SteamController
    {
        private bool[] hapticEnabled = new bool[byte.MaxValue];

        private bool sendHaptic(byte position, ushort amplitude, ushort period, ushort count = 0)
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

            Log.TraceLine("STEAM: Haptic: pos={0}, amplitude={1}, period={2}, count={3}",
                position, amplitude, period, count);

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

        public bool SetHaptic(byte position, ushort amplitude, ushort period, ushort count = 0)
        {
            // do not send repeated haptic queries if was disabled
            bool enabled = amplitude != 0 && period != 0;
            if (!hapticEnabled[position] && !enabled)
                return false;
            hapticEnabled[position] = enabled;

            return sendHaptic(position, amplitude, period, count);
        }
    }
}
