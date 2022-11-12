using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace FanControl
{
    internal class Vlv0100
    {
        // Those addresses are taken from DSDT for VLV0100
        // and might change at any time with a BIOS update
        // Purpose: https://lore.kernel.org/lkml/20220206022023.376142-1-andrew.smirnov@gmail.com/
        // Addresses: DSDT.txt
        static IntPtr FSLO_FSHI = new IntPtr(0xFE700B00 + 0x92);
        static IntPtr GNLO_GNHI = new IntPtr(0xFE700B00 + 0x95);
        static IntPtr FRPR = new IntPtr(0xFE700B00 + 0x97);
        static IntPtr FNRL_FNRH = new IntPtr(0xFE700300 + 0xB0);
        static IntPtr FNCK = new IntPtr(0xFE700300 + 0x9F);
        static IntPtr BATH_BATL = new IntPtr(0xFE700400 + 0x6E);
        static IntPtr PDFV = new IntPtr(0xFE700C00 + 0x4C);
        static IntPtr XBID = new IntPtr(0xFE700300 + 0xBD);
        static IntPtr PDCT = new IntPtr(0xFE700C00 + 0x01);
        static ushort IO6C = 0x6C;

        public const ushort MAX_FAN_RPM = 0x1C84;
        
        public static readonly ushort[] SupportedFirmwares = {
            0xB030 // 45104
        };

        public static readonly byte[] SupportedBoardID = {
            6
        };

        public static readonly byte[] SupportedPDCS = {
            0x2B // 43
        };

        public static bool IsSupported()
        {
            var firmwareVersion = GetFirmwareVersion();
            var boardID = GetBoardID();
            var pdcs = GetPDCS();

            return SupportedFirmwares.Contains(firmwareVersion) &&
                SupportedBoardID.Contains(boardID) &&
                SupportedPDCS.Contains(pdcs);
        }

        public static ushort GetFirmwareVersion()
        {
            byte[] data = InpOut.ReadMemory(PDFV, 2);
            return BitConverter.ToUInt16(data);
        }

        public static byte GetBoardID()
        {
            byte[] data = InpOut.ReadMemory(XBID, 1);
            return data[0];
        }

        public static byte GetPDCS()
        {
            byte[] data = InpOut.ReadMemory(PDCT, 1);
            return data[0];
        }

        public static ushort GetFanDesiredRPM()
        {
            byte[] data = InpOut.ReadMemory(FSLO_FSHI, 2);
            return BitConverter.ToUInt16(data);
        }

        public static ushort GetFanRPM()
        {
            byte[] data = InpOut.ReadMemory(FNRL_FNRH, 2);
            return BitConverter.ToUInt16(data);
        }
        public static void SetFanControl(Boolean userControlled)
        {
            SetGain(10);
            SetRampRate(userControlled ? (byte)10 : (byte)20);

            InpOut.DlPortWritePortUchar(IO6C, userControlled ? (byte)0xCC : (byte)0xCD);
        }

        public static void SetFanDesiredRPM(ushort rpm)
        {
            if (rpm > MAX_FAN_RPM)
                rpm = MAX_FAN_RPM;

            byte[] data = BitConverter.GetBytes(rpm);
            InpOut.WriteMemory(FSLO_FSHI, data);
        }

        public static bool GetFanCheck()
        {
            byte[] data = InpOut.ReadMemory(FNCK, 1);
            return (data[0] & 0x1) != 0;
        }

        public static float GetBattTemperature()
        {
            byte[] data = InpOut.ReadMemory(BATH_BATL, 2);
            int value = (data[0] << 8) + data[1];
            return (float)(value - 0x0AAC) / 10.0f;
        }

        private static void SetGain(ushort gain)
        {
            byte[] data = BitConverter.GetBytes(gain);
            InpOut.WriteMemory(GNLO_GNHI, data);
        }
        private static void SetRampRate(byte rampRate)
        {
            byte[] data = BitConverter.GetBytes(rampRate);
            InpOut.WriteMemory(FRPR, data);
        }
    }
}
