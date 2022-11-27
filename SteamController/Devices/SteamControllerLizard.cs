using hidapi;
using PowerControl.External;
using static CommonHelpers.Log;

namespace SteamController.Devices
{
    public partial class SteamController
    {
        private const int LizardModeUpdateInterval = 250;

        public bool LizardMouse { get; set; } = true;
        public bool LizardButtons { get; set; } = true;

        private bool? savedLizardMouse;
        private bool? savedLizardButtons;
        private DateTime lizardMouseUpdated = DateTime.Now;
        private DateTime lizardButtonUpdated = DateTime.Now;

        private void UpdateLizardMouse()
        {
            if (savedLizardMouse == LizardMouse)
            {
                // We need to explicitly disable lizard every some time
                // but don't fight enabling it, as someone else might be taking control (Steam?)
                if (lizardMouseUpdated.AddMilliseconds(LizardModeUpdateInterval) > DateTime.Now)
                    return;
            }

            savedLizardMouse = LizardMouse;
            lizardMouseUpdated = DateTime.Now;

            if (LizardMouse)
            {
                //Enable mouse emulation
                byte[] data = new byte[] { 0x8e, 0x00 };
                neptuneDevice.RequestFeatureReport(data);
            }
            else
            {
                //Disable mouse emulation
                byte[] data = new byte[] { 0x87, 0x03, 0x08, 0x07 };
                neptuneDevice.RequestFeatureReport(data);
            }
        }

        private void UpdateLizardButtons()
        {
            if (savedLizardButtons == LizardButtons)
            {
                // We need to explicitly disable lizard every some time
                // but don't fight enabling it, as someone else might be taking control (Steam?)
                if (lizardButtonUpdated.AddMilliseconds(LizardModeUpdateInterval) > DateTime.Now)
                    return;
            }

            savedLizardButtons = LizardButtons;
            lizardButtonUpdated = DateTime.Now;

            if (LizardButtons)
            {
                //Enable keyboard/mouse button emulation
                byte[] data = new byte[] { 0x85, 0x00 };
                neptuneDevice.RequestFeatureReport(data);
            }
            else
            {
                //Disable keyboard/mouse button emulation
                byte[] data = new byte[] { 0x81, 0x00 };
                neptuneDevice.RequestFeatureReport(data);
            }
        }
    }
}
