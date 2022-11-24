using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Exceptions;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using static CommonHelpers.Log;

namespace SteamController.Devices
{
    public class Xbox360Controller : IDisposable
    {
        private ViGEmClient? client;
        private IXbox360Controller? device;
        private bool isConnected;
        private bool submitReport;

        public Xbox360Controller()
        {
        }

        public void Dispose()
        {
            using (client) { }
        }

        internal bool CreateClient()
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
                return false;
            }
        }

        internal void BeforeUpdate()
        {
            device?.ResetReport();

            if (!isConnected)
            {
                FeedbackLargeMotor = 0;
                FeedbackSmallMotor = 0;
                LedNumber = 0;
            }

            submitReport = false;
        }

        private void UpdateConnected()
        {
            if (Connected == isConnected)
                return;

            if (Connected)
            {
                device?.Connect();
                TraceLine("Connected X360 Controller.");
            }
            else
            {
                device?.Disconnect();
                TraceLine("Disconnected X360 Controller.");
            }

            isConnected = Connected;
        }

        internal void Update()
        {
            UpdateConnected();

            if (isConnected && submitReport)
            {
                device?.SubmitReport();
            }
        }

        public bool Valid
        {
            get { return device is not null; }
        }

        public bool Connected { get; set; }
        public byte FeedbackLargeMotor { get; internal set; }
        public byte FeedbackSmallMotor { get; internal set; }
        public byte LedNumber { get; internal set; }

        public bool this[Xbox360Button button]
        {
            set
            {
                SetButtonState(button, value);
            }
        }

        public short this[Xbox360Axis axis]
        {
            set
            {
                SetAxisValue(axis, value);
            }
        }

        public short this[Xbox360Slider slider]
        {
            set
            {
                SetSliderValue(slider, value);
            }
        }

        public void SetAxisValue(Xbox360Axis axis, short value)
        {
            device?.SetAxisValue(axis, value);
            submitReport = true;
        }

        public void SetButtonState(Xbox360Button button, bool pressed)
        {
            device?.SetButtonState(button, pressed);
            submitReport = true;
        }

        public void SetSliderValue(Xbox360Slider slider, byte value)
        {
            device?.SetSliderValue(slider, value);
            submitReport = true;
        }

        public void SetSliderValue(Xbox360Slider slider, short value)
        {
            // rescale from -32767..32768 to 0..255
            int result = value;
            result -= short.MinValue;
            result *= byte.MaxValue;
            result /= ushort.MaxValue;
            device?.SetSliderValue(slider, (byte)result);
            submitReport = true;
        }

        private void X360Device_FeedbackReceived(object sender, Xbox360FeedbackReceivedEventArgs e)
        {
            FeedbackLargeMotor = e.LargeMotor;
            FeedbackSmallMotor = e.SmallMotor;
            LedNumber = e.LedNumber;
        }
    }
}

