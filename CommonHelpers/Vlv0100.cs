namespace CommonHelpers
{
    public class Vlv0100
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
            6,
            0xA
        };

        public static readonly byte[] SupportedPDCS = {
            0x2B // 43
        };

        private static InpOut? inpOut;

        public static bool IsOpen
        {
            get { return inpOut is not null; }
        }

        public static bool IsSupported
        {
            get
            {
                return SupportedFirmwares.Contains(FirmwareVersion) &&
                    SupportedBoardID.Contains(BoardID);
            }
        }

        public static ushort FirmwareVersion { get; private set; }
        public static byte BoardID { get; private set; }
        public static byte PDCS { get; private set; }

        public static bool Open()
        {
            if (inpOut != null)
                return true;

            try
            {
                inpOut = new InpOut();

                var data = inpOut?.ReadMemory(PDFV, 2);
                if (data is not null)
                    FirmwareVersion = BitConverter.ToUInt16(data);
                else
                    FirmwareVersion = 0xFFFF;

                data = inpOut?.ReadMemory(XBID, 1);
                if (data is not null)
                    BoardID = data[0];
                else
                    BoardID = 0xFF;

                data = inpOut?.ReadMemory(PDCT, 1);
                if (data is not null)
                    PDCS = data[0];
                else
                    PDCS = 0xFF;

                return true;
            }
            catch (Exception e)
            {
                Log.TraceException("VLV0100", "InpOut", e);
                Close();
                return false;
            }
        }

        public static void Close()
        {
            SetFanControl(false);
            using (inpOut) { }
            inpOut = null;
        }

        public static ushort GetFanDesiredRPM()
        {
            var data = inpOut?.ReadMemory(FSLO_FSHI, 2);
            if (data is null)
                return 0;
            return BitConverter.ToUInt16(data);
        }

        public static ushort? GetFanRPM()
        {
            var data = inpOut?.ReadMemory(FNRL_FNRH, 2);
            if (data is null)
                return null;
            return BitConverter.ToUInt16(data);
        }

        public static void SetFanControl(Boolean userControlled)
        {
            SetGain(10);
            SetRampRate(userControlled ? (byte)10 : (byte)20);

            inpOut?.DlPortWritePortUchar(IO6C, userControlled ? (byte)0xCC : (byte)0xCD);
        }

        public static void SetFanDesiredRPM(ushort rpm)
        {
            if (rpm > MAX_FAN_RPM)
                rpm = MAX_FAN_RPM;

            byte[] data = BitConverter.GetBytes(rpm);
            inpOut?.WriteMemory(FSLO_FSHI, data);
        }

        public static bool GetFanCheck()
        {
            var data = inpOut?.ReadMemory(FNCK, 1);
            if (data is null)
                return false;
            return (data[0] & 0x1) != 0;
        }

        public static float GetBattTemperature()
        {
            var data = inpOut?.ReadMemory(BATH_BATL, 2);
            if (data is null)
                return 0;
            int value = (data[0] << 8) + data[1];
            return (float)(value - 0x0AAC) / 10.0f;
        }

        private static void SetGain(ushort gain)
        {
            byte[] data = BitConverter.GetBytes(gain);
            inpOut?.WriteMemory(GNLO_GNHI, data);
        }
        private static void SetRampRate(byte rampRate)
        {
            byte[] data = BitConverter.GetBytes(rampRate);
            inpOut?.WriteMemory(FRPR, data);
        }
    }
}
