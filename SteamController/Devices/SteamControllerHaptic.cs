using System.Runtime.InteropServices;
using CommonHelpers;
using hidapi;
using PowerControl.External;
using static CommonHelpers.Log;

namespace SteamController.Devices
{
    public partial class SteamController
    {
        private bool[] feedbackEnabled = new bool[byte.MaxValue];

        public bool SetFeedback(byte position, ushort amplitude, ushort period, ushort count = 0)
        {
            // do not send repeated haptic queries if was disabled
            bool enabled = amplitude != 0 && period != 0;
            if (!feedbackEnabled[position] && !enabled)
                return false;
            feedbackEnabled[position] = enabled;

            var haptic = new SDCHapticPacket()
            {
                packet_type = (byte)SDCPacketType.PT_FEEDBACK,
                len = (byte)SDCPacketLength.PL_FEEDBACK,
                position = position,
                amplitude = amplitude,
                period = period,
                count = count
            };

            Log.TraceLine("STEAM: Feedback: pos={0}, amplitude={1}, period={2}, count={3}",
                position, amplitude, period, count);

            var bytes = new byte[Marshal.SizeOf<SDCHapticPacket>()];
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                Marshal.StructureToPtr(haptic, handle.AddrOfPinnedObject(), false);
                neptuneDevice.RequestFeatureReportAsync(bytes);
                return true;
            }
            catch (Exception e)
            {
                Log.TraceLine("STEAM: Feedback: Exception: {0}", e);
                return false;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SDCHapticPacket2
        {
            public byte packet_type = 0xea;
            public byte len = 0xd;
            public byte position = 0x00;
            public byte amplitude = 0x2; // 
            public byte unsure2 = 0x0;
            public sbyte intensity = 0x00; // -7..5 => -2dB..10dB
            public byte unsure3 = 0x4;
            public int tsA = 0; // timestamp?
            public int tsB = 0;

            public SDCHapticPacket2() { }
        }

        public bool SendHaptic(byte position, sbyte intensity)
        {
            var ts = Random.Shared.Next();

            var haptic = new SDCHapticPacket2()
            {
                position = position,
                intensity = (sbyte)(intensity - 5), // convert from dB to values
                tsA = ts,
                tsB = ts
            };

            Log.TraceLine("STEAM: Haptic: pos={0}, intensity={1}",
                position, intensity);

            var bytes = new byte[Marshal.SizeOf<SDCHapticPacket2>()];
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                Marshal.StructureToPtr(haptic, handle.AddrOfPinnedObject(), false);
                neptuneDevice.RequestFeatureReportAsync(bytes);
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
