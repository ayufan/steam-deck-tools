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

        internal SteamController()
        {
            InitializeButtons();
            InitializeActions();

            neptuneDevice = new hidapi.HidDevice(VendorID, ProductID, 64);
            neptuneDevice.OpenDevice();
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
