using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace PowerControl.External
{
    /// <summary>
    /// Taken from: https://github.com/mKenfenheuer/neptune-hidapi.net/blob/main/Hid/HidEnums.cs
    /// </summary>
    public enum SDCPacketType
    {
        PT_INPUT = 0x01,
        PT_HOTPLUG = 0x03,
        PT_IDLE = 0x04,
        PT_OFF = 0x9f,
        PT_AUDIO = 0xb6,
        PT_CLEAR_MAPPINGS = 0x81,
        PT_CONFIGURE = 0x87,
        PT_LED = 0x87,
        PT_CALIBRATE_JOYSTICK = 0xbf,
        PT_CALIBRATE_TRACKPAD = 0xa7,
        PT_SET_AUDIO_INDICES = 0xc1,
        PT_LIZARD_BUTTONS = 0x85,
        PT_LIZARD_MOUSE = 0x8e,
        PT_FEEDBACK = 0x8f,
        PT_RESET = 0x95,
        PT_GET_SERIAL = 0xAE,
    }
    public enum SDCPacketLength
    {
        PL_LED = 0x03,
        PL_OFF = 0x04,
        PL_FEEDBACK = 0x07,
        PL_CONFIGURE = 0x15,
        PL_CONFIGURE_BT = 0x0f,
        PL_GET_SERIAL = 0x15,
    }
    public enum SDCConfigType
    {
        CT_LED = 0x2d,
        CT_CONFIGURE = 0x32,
        CONFIGURE_BT = 0x18,
    }

    [Flags]
    public enum SDCButton0 : ushort
    {
        BTN_L5 = 0b1000000000000000,
        BTN_OPTIONS = 0b0100000000000000,
        BTN_STEAM = 0b0010000000000000,
        BTN_MENU = 0b0001000000000000,
        BTN_DPAD_DOWN = 0b0000100000000000,
        BTN_DPAD_LEFT = 0b0000010000000000,
        BTN_DPAD_RIGHT = 0b0000001000000000,
        BTN_DPAD_UP = 0b0000000100000000,
        BTN_A = 0b0000000010000000,
        BTN_X = 0b0000000001000000,
        BTN_B = 0b0000000000100000,
        BTN_Y = 0b0000000000010000,
        BTN_L1 = 0b0000000000001000,
        BTN_R1 = 0b0000000000000100,
        BTN_L2 = 0b0000000000000010,
        BTN_R2 = 0b0000000000000001,
    }

    [Flags]
    public enum SDCButton1 : byte
    {
        BTN_LSTICK_PRESS = 0b01000000,
        BTN_RPAD_TOUCH = 0b00010000,
        BTN_LPAD_TOUCH = 0b00001000,
        BTN_RPAD_PRESS = 0b00000100,
        BTN_LPAD_PRESS = 0b00000010,
        BTN_R5 = 0b00000001,
    }

    [Flags]
    public enum SDCButton2 : byte
    {
        BTN_RSTICK_PRESS = 0b00000100,
    }

    [Flags]
    public enum SDCButton4 : byte
    {
        BTN_LSTICK_TOUCH = 0b01000000,
        BTN_RSTICK_TOUCH = 0b10000000,
        BTN_R4 = 0b00000100,
        BTN_L4 = 0b00000010,
    }

    [Flags]
    public enum SDCButton5 : byte
    {
        BTN_QUICK_ACCESS = 0b00000100,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDCInput
    {
        public byte ptype;          //0x00
        public byte _a1;            //0x01
        public byte _a2;            //0x02
        public byte _a3;            //0x03
        public uint seq;            //0x04
        public SDCButton0 buttons0; //0x08
        public SDCButton1 buttons1; //0x0A
        public SDCButton2 buttons2; //0x0B
        public byte buttons3;       //0x0C
        public SDCButton4 buttons4; //0x0D
        public SDCButton5 buttons5; //0x0E
        public byte buttons6;       //0x0F
        public short lpad_x;        //0x10
        public short lpad_y;        //0x12
        public short rpad_x;        //0x14
        public short rpad_y;        //0x16
        public short accel_x;       //0x18
        public short accel_y;       //0x1A
        public short accel_z;       //0x1C
        public short gpitch;        //0x1E
        public short gyaw;          //0x20
        public short groll;         //0x22
        public short q1;            //0x24
        public short q2;            //0x26
        public short q3;            //0x28
        public short q4;            //0x2A
        public short ltrig;         //0x2C
        public short rtrig;         //0x2E
        public short lthumb_x;      //0x30
        public short lthumb_y;      //0x32
        public short rthumb_x;      //0x34
        public short rthumb_y;      //0x36
        public short lpad_pressure; //0x38
        public short rpad_pressure; //0x3A

        public static SDCInput FromBuffer(byte[] bytes)
        {
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                return Marshal.PtrToStructure<SDCInput>(handle.AddrOfPinnedObject());
            }
            catch
            {
                return new SDCInput();
            }
            finally
            {
                handle.Free();
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDCHapticPacket
    {
        public byte packet_type; // = 0x8f;
        public byte len; //  = 0x07;
        public byte position; //  = 1;
        public ushort amplitude;
        public ushort period;
        public ushort count;
    }
}
