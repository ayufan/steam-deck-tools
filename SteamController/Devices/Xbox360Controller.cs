using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Exceptions;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using SteamController.ProfilesSettings;
using static CommonHelpers.Log;

namespace SteamController.Devices
{
    public class Xbox360Controller : IDisposable
    {
        public readonly TimeSpan FeedbackTimeout = TimeSpan.FromMilliseconds(1000);
        public const ushort VendorID = 0x045E;
        public const ushort ProductID = 0x028E;

        public static readonly Dictionary<VirtualX360Code, Xbox360Button> codeToButton = new Dictionary<VirtualX360Code, Xbox360Button>()
        {
            { VirtualX360Code.X360_UP, Xbox360Button.Up },
            { VirtualX360Code.X360_DOWN, Xbox360Button.Down },
            { VirtualX360Code.X360_LEFT, Xbox360Button.Left },
            { VirtualX360Code.X360_RIGHT, Xbox360Button.Right },
            { VirtualX360Code.X360_BACK, Xbox360Button.Back },
            { VirtualX360Code.X360_START, Xbox360Button.Start },
            { VirtualX360Code.X360_A, Xbox360Button.A },
            { VirtualX360Code.X360_B, Xbox360Button.B },
            { VirtualX360Code.X360_X, Xbox360Button.X },
            { VirtualX360Code.X360_Y, Xbox360Button.Y },
            { VirtualX360Code.X360_LB, Xbox360Button.LeftShoulder },
            { VirtualX360Code.X360_RB, Xbox360Button.RightShoulder },
            { VirtualX360Code.X360_LS, Xbox360Button.LeftThumb },
            { VirtualX360Code.X360_RS, Xbox360Button.RightThumb }
        };

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
                return false;
            }
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

            submitReport = false;
            Connected = false;
        }

        private void SetConnected(bool wantsConnected)
        {
            if (wantsConnected == isConnected)
                return;

            if (wantsConnected)
            {
                device?.Connect();
                TraceLine("Connected X360 Controller.");
            }
            else
            {
                device?.Disconnect();
                TraceLine("Disconnected X360 Controller.");
            }

            isConnected = wantsConnected;
        }

        internal void Beep()
        {
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

            if (isConnected && submitReport)
            {
                try
                {
                    device?.SubmitReport();
                }
                catch (VigemInvalidTargetException)
                {
                    device?.Disconnect();
                    isConnected = false;
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
        public byte LedNumber { get; private set; }
        public DateTime? FeedbackReceived { get; private set; }

        public bool this[VirtualX360Code code]
        {
            set
            {
                if (codeToButton.TryGetValue(code, out var button))
                {
                    this[button] = value;
                }
            }
        }

        public bool this[Xbox360Button button]
        {
            set
            {
                if (value)
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

