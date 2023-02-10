using System.Diagnostics;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Exceptions;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.DualShock4;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using static CommonHelpers.Log;

namespace SteamController.Devices
{
    public partial class DS4Controller : IDisposable
    {
        public readonly TimeSpan FeedbackTimeout = TimeSpan.FromMilliseconds(1000);
        public const ushort VendorID = 0x054C;
        public const ushort ProductID = 0x05C4;

        private const int REPORT_SIZE = 63;
        private const int TIMESTAMP_HZ = 800;
        private const int TIMESTAMP_INCREMENT = 188;

        private ViGEmClient? client;
        private IDualShock4Controller? device;
        private bool isConnected;
        private byte[] reportBytes = new byte[REPORT_SIZE];
        private bool submitReport;
        private int submittedReports = 0;
        private Stopwatch stopwatch = new Stopwatch();

        public DS4Controller()
        {
        }

        public void Dispose()
        {
            using (client) { }
        }

        public void Start()
        {
        }

        public void Stop()
        {
            lock (this) { Fail(); }
        }

        internal bool Tick()
        {
            if (this.device is not null)
                return true;

            try
            {
                var client = new ViGEmClient();
                var device = client.CreateDualShock4Controller();
                device.AutoSubmitReport = false;
                device.FeedbackReceived += DS4_FeedbackReceived;
                this.device = device;
                this.client = client;
                return true;
            }
            catch (VigemBusNotFoundException)
            {
                // ViGem is not installed
                return false;
            }
        }

        private void Fail()
        {
            var client = this.client;

            // unset current device
            this.isConnected = false;
            this.client = null;
            this.device = null;
            this.stopwatch.Stop();

            try { using (client) { } }
            catch (Exception) { }
        }

        private void PrepareReport()
        {
            var oldReportBytes = reportBytes;

            reportBytes = new byte[63];
            submitReport = false;

            Counter.Set(reportBytes, (byte)((submittedReports << 2) & 0xFF));
            BatteryLevel.Set(reportBytes, 255);
            Timestamp.Set(reportBytes, (ushort)(stopwatch.ElapsedMilliseconds * TIMESTAMP_HZ * TIMESTAMP_INCREMENT / 1000));

            DPadReleased.Set(reportBytes);
            LeftThumbX.SetScaled(reportBytes, 0);
            LeftThumbY.SetScaled(reportBytes, 0);
            RightThumbX.SetScaled(reportBytes, 0);
            RightThumbY.SetScaled(reportBytes, 0);
            GyroX.Set(reportBytes, 0);
            GyroY.Set(reportBytes, 0);
            GyroZ.Set(reportBytes, 0);
            AccelX.Set(reportBytes, 0);
            AccelY.Set(reportBytes, 0);
            AccelZ.Set(reportBytes, 0);

            // Copy trackpad packets
            reportBytes[32] = (byte)((reportBytes[32] + 1) & 0x3);
            Array.Copy(oldReportBytes, 33, reportBytes, 33, 9);
            Array.Copy(oldReportBytes, 33, reportBytes, 42, 9 * 2);
        }

        internal void BeforeUpdate()
        {
            device?.ResetReport();

            if (!isConnected)
            {
                FeedbackLargeMotor = null;
                FeedbackSmallMotor = null;
                FeedbackReceived = null;
                LightbarColor = null;
            }

            PrepareReport();
            Connected = false;
        }

        private void SetConnected(bool wantsConnected)
        {
            if (wantsConnected == isConnected)
                return;

            if (wantsConnected)
            {
                try
                {
                    device?.Connect();
                    stopwatch.Restart();
                    TraceLine("Connected DS4 Controller.");
                }
                catch (System.ComponentModel.Win32Exception e)
                {
                    // This is expected exception (as sometimes device will fail to connect)
                    // ERROR_SUCCESS, which likely means COM did not succeed
                    if (e.NativeErrorCode == 0)
                        DebugException("DS4", "ConnectExpected", e);
                    else
                        TraceException("DS4", "ConnectExpected", e);
                    Fail();
                    return;
                }
                catch (Exception e)
                {
                    TraceException("DS4", "Connect", e);
                    Fail();
                    return;
                }
            }
            else
            {
                try
                {
                    device?.Disconnect();
                    stopwatch.Stop();
                    TraceLine("Disconnected DS4 Controller.");
                }
                catch (VigemTargetNotPluggedInException)
                {
                    // everything fine
                }
                catch (Exception e)
                {
                    TraceException("DS4", "Disconnect", e);
                    Fail();
                    return;
                }
            }

            isConnected = wantsConnected;
        }

        internal void Update()
        {
            if (device is not null && Connected != isConnected)
            {
                lock (this)
                {
                    SetConnected(Connected);
                }
            }

            if (isConnected && submitReport)
            {
                try
                {
                    device?.SubmitRawReport(reportBytes);
                    submittedReports++;
                }
                catch (VigemInvalidTargetException)
                {
                    // Device was lost
                    lock (this) { Fail(); }
                }
                catch (Exception e)
                {
                    TraceException("DS4", "SubmitReport", e);
                }
            }

            if (FeedbackReceived is not null && FeedbackReceived.Value.Add(FeedbackTimeout) < DateTime.Now)
            {
                FeedbackLargeMotor = null;
                FeedbackSmallMotor = null;
                FeedbackReceived = null;
            }
        }

        public bool Valid
        {
            get { return device is not null; }
        }

        public bool Connected { get; set; }
        public byte? FeedbackLargeMotor { get; private set; }
        public byte? FeedbackSmallMotor { get; private set; }
        public LightbarColor? LightbarColor { get; private set; }
        public DateTime? FeedbackReceived { get; private set; }

        public bool this[DualShock4Button button]
        {
            set
            {
                if (value)
                    button.Set(reportBytes, value);
                submitReport = true;
            }
        }

        public bool this[DualShock4DPadDirection button]
        {
            set
            {
                if (value)
                    button.Set(reportBytes);
                submitReport = true;
            }
        }

        public short this[DualShock4Axis axis]
        {
            set
            {
                axis.SetScaled(reportBytes, value);
                submitReport = true;
            }
        }

        public short this[DualShock4Slider slider]
        {
            set
            {
                slider.Set(reportBytes, value);
                submitReport = true;
            }
        }

        public short this[DualShock4Sensor sensor]
        {
            set
            {
                sensor.Set(reportBytes, value);
                submitReport = true;
            }
        }

        public Point? this[DualShock4Finger finger]
        {
            set
            {
                finger.Set(reportBytes, value);
                submitReport = true;
            }
        }

        public void Overwrite(DualShock4Button button, bool value, int minPressedTime = 0)
        {
            button.Set(reportBytes, value);
            submitReport = true;
        }

        public void ResetFeedback()
        {
            FeedbackReceived = null;
        }

        private void DS4_FeedbackReceived(object sender, DualShock4FeedbackReceivedEventArgs e)
        {
            FeedbackLargeMotor = e.LargeMotor;
            FeedbackSmallMotor = e.SmallMotor;
            LightbarColor = e.LightbarColor;
            FeedbackReceived = DateTime.Now;
        }
    }
}
