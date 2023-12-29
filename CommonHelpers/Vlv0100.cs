namespace CommonHelpers
{
    public class Vlv0100 : IDisposable
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
        static IntPtr MCBL = new IntPtr(0xFE700B00 + 0x9F);
        static ushort IO6C = 0x6C;

        public const ushort MAX_FAN_RPM = 0x1C84;

        public struct DeviceVersion
        {
            public ushort Firmware { get; set; }
            public byte BoardID { get; set; }
            public byte PDCS { get; set; }

            public bool BatteryTempLE { get; set; }
            public bool MaxBatteryCharge { get; set; }

            public bool IsSupported(ushort deviceFirmware, byte deviceBoardID, byte devicePDCS)
            {
                if (Firmware != 0 && Firmware != deviceFirmware)
                    return false;
                if (BoardID != 0 && BoardID != deviceBoardID)
                    return false;
                if (PDCS != 0 && PDCS != devicePDCS)
                    return false;
                return true;
            }
        };

        private static readonly DeviceVersion[] deviceVersions = {
            // Steam Deck - LCD version
            new DeviceVersion() { Firmware = 0xB030, BoardID = 0x6, PDCS = 0 /* 0x2B */, BatteryTempLE = false, MaxBatteryCharge = true },
            new DeviceVersion() { Firmware = 0xB030, BoardID = 0xA, PDCS = 0 /* 0x2B */, BatteryTempLE = false, MaxBatteryCharge = true },

            // Steam Deck - OLED version
            // new DeviceVersion() { Firmware = 0x1030, BoardID = 0x5, PDCS = 0 /* 0x2F */, BatteryTempLE = true },
            new DeviceVersion() { Firmware = 0x1050, BoardID = 0x5, PDCS = 0 /* 0x2F */, BatteryTempLE = true, MaxBatteryCharge = true }
        };

        public static Vlv0100 Instance = new Vlv0100();

        ~Vlv0100()
        {
            Close();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Close();
        }

        private InpOut? inpOut;

        public bool IsOpen
        {
            get { return inpOut is not null; }
        }

        public DeviceVersion? SupportedDevice
        {
            get { return deviceVersions.FirstOrDefault((v) => v.IsSupported(FirmwareVersion, BoardID, PDCS)); }
        }

        public bool IsSupported
        {
            get { return SupportedDevice is not null; }
        }

        public ushort FirmwareVersion { get; private set; }
        public byte BoardID { get; private set; }
        public byte PDCS { get; private set; }

        public bool Open()
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

        public void Close()
        {
            SetFanControl(false);
            using (inpOut) { }
            inpOut = null;
        }

        public ushort GetFanDesiredRPM()
        {
            var data = inpOut?.ReadMemory(FSLO_FSHI, 2);
            if (data is null)
                return 0;
            return BitConverter.ToUInt16(data);
        }

        public ushort? GetFanRPM()
        {
            var data = inpOut?.ReadMemory(FNRL_FNRH, 2);
            if (data is null)
                return null;
            return BitConverter.ToUInt16(data);
        }

        public void SetFanControl(Boolean userControlled)
        {
            SetGain(10);
            SetRampRate(userControlled ? (byte)10 : (byte)20);

            inpOut?.DlPortWritePortUchar(IO6C, userControlled ? (byte)0xCC : (byte)0xCD);
        }

        public void SetFanDesiredRPM(ushort rpm)
        {
            if (rpm > MAX_FAN_RPM)
                rpm = MAX_FAN_RPM;

            byte[] data = BitConverter.GetBytes(rpm);
            inpOut?.WriteMemory(FSLO_FSHI, data);
        }

        public bool GetFanCheck()
        {
            var data = inpOut?.ReadMemory(FNCK, 1);
            if (data is null)
                return false;
            return (data[0] & 0x1) != 0;
        }

        public float GetBattTemperature()
        {
            var data = inpOut?.ReadMemory(BATH_BATL, 2);
            if (data is null)
                return 0;
            int value = SupportedDevice?.BatteryTempLE == true ?
                ((data[1] << 8) + data[0]) :
                ((data[0] << 8) + data[1]);
            return (float)(value - 0x0AAC) / 10.0f;
        }

        public int? GetMaxBatteryCharge()
        {
            if (SupportedDevice?.MaxBatteryCharge != true)
                return null;
            var data = inpOut?.ReadMemory(MCBL, 1);
            if (data is null)
                return null;
            if (data[0] > 100)
                return null;
            return data[0];
        }
        public void SetMaxBatteryCharge(int chargeLimit)
        {
            if (SupportedDevice?.MaxBatteryCharge != true)
                return;
            if (chargeLimit < 0 || chargeLimit > 100)
                return;
            byte[] data = BitConverter.GetBytes(chargeLimit);
            inpOut?.WriteMemory(MCBL, data);
        }

        private void SetGain(ushort gain)
        {
            byte[] data = BitConverter.GetBytes(gain);
            inpOut?.WriteMemory(GNLO_GNHI, data);
        }
        private void SetRampRate(byte rampRate)
        {
            byte[] data = BitConverter.GetBytes(rampRate);
            inpOut?.WriteMemory(FRPR, data);
        }
    }
}
