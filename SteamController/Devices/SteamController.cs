using System.Diagnostics;
using CommonHelpers;
using hidapi;
using PowerControl.External;
using static CommonHelpers.Log;

namespace SteamController.Devices
{
    public partial class SteamController : IDisposable
    {
        public const ushort VendorID = 0x28DE;
        public const ushort ProductID = 0x1205;
        private const int ReadTimeout = 50;

        private hidapi.HidDevice neptuneDevice;
        private Stopwatch stopwatch = new Stopwatch();
        private TimeSpan? lastUpdate;
        public double DeltaTime { get; private set; }

        internal SteamController()
        {
            InitializeButtons();
            InitializeActions();

            neptuneDevice = new hidapi.HidDevice(VendorID, ProductID, 64);
            neptuneDevice.OpenDevice();

            stopwatch.Start();
        }

        public void Dispose()
        {
        }

        public bool Updated { get; private set; }

        internal void Reset()
        {
            foreach (var action in AllActions)
                action.Reset();
        }

        private void BeforeUpdate(byte[] buffer)
        {
            foreach (var action in AllActions)
                action.BeforeUpdate(buffer);
        }

        internal void BeforeUpdate()
        {
            var ts = stopwatch.Elapsed;
            DeltaTime = lastUpdate is not null ? (ts - lastUpdate.Value).TotalSeconds : 0.0;
            DeltaTime = Math.Min(DeltaTime, 0.1); // max update is 100ms
            lastUpdate = ts;

            LizardButtons = true;
            LizardMouse = true;

            try
            {
                byte[] data = neptuneDevice.Read(ReadTimeout);
                if (data == null)
                {
                    Reset();
                    Updated = false;
                    return;
                }

                BeforeUpdate(data);
                Updated = true;
            }
            catch (Exception e)
            {
                Log.TraceLine("STEAM: Exception: {0}", e);
                Reset();
                Updated = false;
            }
        }

        internal void Update()
        {
            foreach (var action in AllActions)
                action.Update();

            try
            {
                UpdateLizardButtons();
                UpdateLizardMouse();
            }
            catch (Exception e)
            {
                Log.TraceLine("STEAM: Exception: {0}", e);
                Reset();
                Updated = false;
            }
        }
    }
}
