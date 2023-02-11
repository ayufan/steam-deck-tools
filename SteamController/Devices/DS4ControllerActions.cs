using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Exceptions;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.DualShock4;
using static CommonHelpers.Log;

namespace SteamController.Devices
{
    public partial class DS4Controller
    {
        public struct DualShock4Axis
        {
            public int Offset { get; }
            public bool Invert { get; }

            public DualShock4Axis(int offset, bool invert)
            {
                Offset = offset;
                Invert = invert;
            }

            internal void Set(byte[] report, byte value)
            {
                report[Offset] = value;
            }

            internal void SetScaled(byte[] report, short value)
            {
                var valuef = value / 256;
                if (Invert)
                    valuef = -valuef;
                Set(report, (byte)(valuef + sbyte.MinValue));
            }
        }

        public struct DualShock4Button
        {
            public int Offset { get; }
            public int Bit { get; }
            public byte Mask { get => (byte)(1 << Bit); }

            public DualShock4Button(int offset, int bit)
            {
                Offset = offset;
                Bit = bit;
            }

            internal void Set(byte[] report, bool value)
            {
                report[Offset] = (byte)((report[Offset] & ~Mask) | (value ? Mask : 0));
            }
        }

        public struct DualShock4DPadDirection
        {
            public int Offset { get; }
            public int Value { get; }
            public int Mask { get; }

            public DualShock4DPadDirection(int offset, int value, int mask)
            {
                Offset = offset;
                Value = value;
                Mask = mask;
            }

            internal void Set(byte[] report)
            {
                report[Offset] = (byte)((report[Offset] & ~Mask) | Value);
            }
        }

        public struct DualShock4Slider
        {
            public int Offset { get; }

            public DualShock4Slider(int offset)
            {
                Offset = offset;
            }

            internal void Set(byte[] report, byte value)
            {
                report[Offset] = value;
            }

            internal void Set(byte[] report, short value)
            {
                int result = Math.Clamp(value, (short)0, short.MaxValue) * byte.MaxValue / short.MaxValue;
                Set(report, (byte)result);
            }
        }

        public struct DualShock4Sensor
        {
            public int Offset { get; }

            public DualShock4Sensor(int offset)
            {
                Offset = offset;
            }

            internal void Set(byte[] report, short value)
            {
                BitConverter.GetBytes(value).CopyTo(report, Offset);
            }

            internal void Set(byte[] report, ushort value)
            {
                BitConverter.GetBytes(value).CopyTo(report, Offset);
            }
        }

        public struct DualShock4Finger
        {
            public const int MaxX = 1920;
            public const int MaxY = 942;

            public int Index { get; }
            private int Offset { get => 34 + Index * 4; }

            public DualShock4Finger(int index)
            {
                Index = index;
            }

            internal void Set(byte[] report, Point? data)
            {
                uint currentValue = BitConverter.ToUInt32(report, Offset);

                // copy report ID
                uint calculatedValue = (byte)(((currentValue & 0x7F) + 1) & 0x7F);

                if (data.HasValue)
                {
                    // store coordinates into report
                    int x = Math.Clamp(data.Value.X, 0, MaxX);
                    int y = Math.Clamp(data.Value.Y, 0, MaxY);
                    calculatedValue |= (uint)((x & 0x7FF) << 8);
                    calculatedValue |= (uint)((y & 0x7FF) << 20);
                }
                else
                {
                    // copy existing coordinates
                    calculatedValue |= 0x80;
                    calculatedValue |= (uint)(currentValue & 0xFFFFFF00);
                }

                // compare position and key status
                if ((currentValue & 0xFFFFFF80) == (calculatedValue & 0xFFFFFF80))
                    return;

                // increment packet number (if it changed since the last packet)
                if (report[33] == report[42])
                    report[33] = (byte)(report[33] + 1);
                BitConverter.GetBytes(calculatedValue).CopyTo(report, Offset);
            }
        }

        public readonly static DualShock4Axis LeftThumbX = new DualShock4Axis(0, false);
        public readonly static DualShock4Axis LeftThumbY = new DualShock4Axis(1, true);
        public readonly static DualShock4Axis RightThumbX = new DualShock4Axis(2, false);
        public readonly static DualShock4Axis RightThumbY = new DualShock4Axis(3, true);

        public readonly static DualShock4Slider LeftTrigger = new DualShock4Slider(7);
        public readonly static DualShock4Slider RightTrigger = new DualShock4Slider(8);

        public readonly static DualShock4Button ThumbRight = new DualShock4Button(5, 7);
        public readonly static DualShock4Button ThumbLeft = new DualShock4Button(5, 6);
        public readonly static DualShock4Button Options = new DualShock4Button(5, 5);
        public readonly static DualShock4Button Share = new DualShock4Button(5, 4);
        public readonly static DualShock4Button TriggerRight = new DualShock4Button(5, 3);
        public readonly static DualShock4Button TriggerLeft = new DualShock4Button(5, 2);
        public readonly static DualShock4Button ShoulderRight = new DualShock4Button(5, 1);
        public readonly static DualShock4Button ShoulderLeft = new DualShock4Button(5, 0);
        public readonly static DualShock4Button Triangle = new DualShock4Button(4, 7);
        public readonly static DualShock4Button Circle = new DualShock4Button(4, 6);
        public readonly static DualShock4Button Cross = new DualShock4Button(4, 5);
        public readonly static DualShock4Button Square = new DualShock4Button(4, 4);

        public readonly static DualShock4Button TPadClick = new DualShock4Button(6, 1);
        public readonly static DualShock4Button PS = new DualShock4Button(6, 0);

        private readonly static DualShock4Sensor Timestamp = new DualShock4Sensor(9);
        private readonly static DualShock4Slider BatteryLevel = new DualShock4Slider(11);
        private readonly static DualShock4Slider Counter = new DualShock4Slider(6);

        public readonly static DualShock4Sensor GyroX = new DualShock4Sensor(12);
        public readonly static DualShock4Sensor GyroY = new DualShock4Sensor(14);
        public readonly static DualShock4Sensor GyroZ = new DualShock4Sensor(16);
        public readonly static DualShock4Sensor AccelX = new DualShock4Sensor(18);
        public readonly static DualShock4Sensor AccelY = new DualShock4Sensor(20);
        public readonly static DualShock4Sensor AccelZ = new DualShock4Sensor(22);

        public readonly static DualShock4DPadDirection DPadReleased = new DualShock4DPadDirection(4, 8, 15);
        public readonly static DualShock4DPadDirection DPadNorthwest = new DualShock4DPadDirection(4, 7, 15);
        public readonly static DualShock4DPadDirection DPadWest = new DualShock4DPadDirection(4, 6, 15);
        public readonly static DualShock4DPadDirection DPadSouthwest = new DualShock4DPadDirection(4, 5, 15);
        public readonly static DualShock4DPadDirection DPadSouth = new DualShock4DPadDirection(4, 4, 15);
        public readonly static DualShock4DPadDirection DPadSoutheast = new DualShock4DPadDirection(4, 3, 15);
        public readonly static DualShock4DPadDirection DPadEast = new DualShock4DPadDirection(4, 2, 15);
        public readonly static DualShock4DPadDirection DPadNortheast = new DualShock4DPadDirection(4, 1, 15);
        public readonly static DualShock4DPadDirection DPadNorth = new DualShock4DPadDirection(4, 0, 15);

        public readonly static DualShock4Finger LeftFinger = new DualShock4Finger(0);
        public readonly static DualShock4Finger RightFinger = new DualShock4Finger(1);
    }
}
