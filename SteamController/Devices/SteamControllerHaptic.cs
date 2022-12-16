using System.Runtime.InteropServices;
using CommonHelpers;
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
                TraceException("STEAM", "Feedback", e);
                return false;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SDCHapticPacket2
        {
            public byte packet_type = 0xea;
            public byte len = 0xd;
            public HapticPad position = HapticPad.Left;
            public HapticStyle style = HapticStyle.Strong; //
            public byte unsure2 = 0x0;
            public sbyte intensity = 0x00; // -7..5 => -2dB..10dB
            public byte unsure3 = 0x4;
            public int tsA = 0; // timestamp?
            public int tsB = 0;

            public SDCHapticPacket2()
            {
                var ts = Random.Shared.Next();
                this.tsA = ts;
                this.tsB = ts;
            }

            public SDCHapticPacket2(HapticPad position, HapticStyle style, sbyte intensityDB) : this()
            {
                this.position = position;
                this.style = style;
                this.intensity = (sbyte)(intensityDB - 5); // convert from dB to values
            }
        }

        private Dictionary<HapticPad, Task?> hapticTasks = new Dictionary<HapticPad, Task?>();

        public enum HapticPad : byte
        {
            Left,
            Right
        };

        public enum HapticStyle : byte
        {
            Disabled = 0,
            Weak = 1,
            Strong = 2
        };

        public bool SendHaptic(HapticPad position, HapticStyle style, sbyte intensityDB)
        {
            if (hapticTasks.GetValueOrDefault(position)?.IsCompleted == false)
                return false;
            if (style == HapticStyle.Disabled)
                return true;

            var haptic = new SDCHapticPacket2(position, style, intensityDB);

            Log.TraceLine("STEAM: Haptic: position={0}, style={1}, intensity={2}",
                position, style, intensityDB);

            var bytes = new byte[Marshal.SizeOf<SDCHapticPacket2>()];
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                Marshal.StructureToPtr(haptic, handle.AddrOfPinnedObject(), false);
                hapticTasks[position] = neptuneDevice.RequestFeatureReportAsync(bytes);
                return true;
            }
            catch (hidapi.HidDeviceInvalidException)
            {
                // Steam might disconnect device
                Fail();
                return false;
            }
            catch (Exception e)
            {
                TraceException("STEAM", "Haptic", e);
                return false;
            }
        }
    }
}
