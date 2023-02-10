using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Exceptions;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using static CommonHelpers.Log;

namespace SteamController.Devices
{
    public class Xbox360Controller : IDisposable
    {
        public readonly TimeSpan FeedbackTimeout = TimeSpan.FromMilliseconds(1000);
        public const ushort VendorID = 0x045E;
        public const ushort ProductID = 0x028E;

        private ViGEmClient? client;
        private IXbox360Controller? device;
        private bool isConnected;
        private bool submitReport;
        private Dictionary<Xbox360Button, DateTime> lastPressed = new Dictionary<Xbox360Button, DateTime>();
        private Dictionary<Xbox360Button, DateTime> pressed = new Dictionary<Xbox360Button, DateTime>();

        public Xbox360Controller()
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
                var device = client.CreateXbox360Controller();
                device.AutoSubmitReport = false;
                device.FeedbackReceived += X360Device_FeedbackReceived;
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

            try { using (client) { } }
            catch (Exception) { }
        }

        internal void BeforeUpdate()
        {
            device?.ResetReport();

            if (!isConnected)
            {
                FeedbackLargeMotor = null;
                FeedbackSmallMotor = null;
                FeedbackReceived = null;
                LedNumber = 0;
            }

            lastPressed = pressed;
            pressed = new Dictionary<Xbox360Button, DateTime>();
            submitReport = false;
            Connected = Settings.Default.KeepX360AlwaysConnected;
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
                    TraceLine("Connected X360 Controller.");
                }
                catch (System.ComponentModel.Win32Exception e)
                {
                    // This is expected exception (as sometimes device will fail to connect)
                    // ERROR_SUCCESS, which likely means COM did not succeed
                    if (e.NativeErrorCode == 0)
                        DebugException("X360", "ConnectExpected", e);
                    else
                        TraceException("X360", "ConnectExpected", e);
                    Fail();
                    return;
                }
                catch (Exception e)
                {
                    TraceException("X360", "Connect", e);
                    Fail();
                    return;
                }
            }
            else
            {
                try
                {
                    device?.Disconnect();
                    TraceLine("Disconnected X360 Controller.");
                }
                catch (VigemTargetNotPluggedInException)
                {
                    // everything fine
                }
                catch (Exception e)
                {
                    TraceException("X360", "Disconnect", e);
                    Fail();
                    return;
                }
            }

            isConnected = wantsConnected;
        }

        internal void Beep()
        {
            if (Settings.Default.KeepX360AlwaysConnected)
                return;
            if (device is null)
                return;

            lock (this)
            {
                // cycle currently connected device
                SetConnected(!isConnected);
                Thread.Sleep(100);
            }
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

            UpdateMinimumPressedTime();

            if (isConnected && submitReport)
            {
                try
                {
                    device?.SubmitReport();
                }
                catch (VigemInvalidTargetException)
                {
                    // Device was lost
                    lock (this) { Fail(); }
                }
                catch (Exception e)
                {
                    TraceException("X360", "SubmitReport", e);
                }
            }

            if (FeedbackReceived is not null && FeedbackReceived.Value.Add(FeedbackTimeout) < DateTime.Now)
            {
                FeedbackLargeMotor = null;
                FeedbackSmallMotor = null;
                FeedbackReceived = null;
            }
        }

        private void UpdateMinimumPressedTime()
        {
            var now = DateTime.Now;

            foreach (var key in lastPressed)
            {
                if (pressed.ContainsKey(key.Key))
                    continue;

                // until time elapsed, keep setting button state
                if (key.Value < DateTime.Now)
                    continue;

                device?.SetButtonState(key.Key, true);
                pressed.Add(key.Key, key.Value);
                submitReport = true;
            }
        }

        public bool Valid
        {
            get { return device is not null; }
        }

        public bool Connected { get; set; }
        public byte? FeedbackLargeMotor { get; private set; }
        public byte? FeedbackSmallMotor { get; private set; }
        public byte LedNumber { get; private set; }
        public DateTime? FeedbackReceived { get; private set; }

        public bool this[Xbox360Button? button]
        {
            set
            {
                if (value && button is not null)
                    device?.SetButtonState(button, value);
                submitReport = true;
            }
        }

        public short this[Xbox360Axis axis]
        {
            set
            {
                device?.SetAxisValue(axis, value);
                submitReport = true;
            }
        }

        public short this[Xbox360Slider slider]
        {
            set
            {
                // rescale from 0..32767 to 0..255
                int result = Math.Clamp(value, (short)0, short.MaxValue) * byte.MaxValue / short.MaxValue;
                device?.SetSliderValue(slider, (byte)result);
                submitReport = true;
            }
        }

        public void Overwrite(Xbox360Button button, bool value, int minPresTimeMs = 0)
        {
            device?.SetButtonState(button, value);
            submitReport = true;

            if (value && minPresTimeMs > 0)
            {
                if (!lastPressed.TryGetValue(button, out var firstPressed))
                    firstPressed = DateTime.Now.AddMilliseconds(minPresTimeMs);
                pressed.Add(button, firstPressed);
            }
        }

        public void ResetFeedback()
        {
            FeedbackReceived = null;
        }

        private void X360Device_FeedbackReceived(object sender, Xbox360FeedbackReceivedEventArgs e)
        {
            FeedbackLargeMotor = e.LargeMotor;
            FeedbackSmallMotor = e.SmallMotor;
            LedNumber = e.LedNumber;
            FeedbackReceived = DateTime.Now;
        }
    }
}

