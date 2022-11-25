using System.Diagnostics;
using SteamController.Helpers;

namespace SteamController.Managers
{
    public sealed class HidHideManager : Manager
    {
        public const String NeptuneDevicePath = @"HID\VID_28DE&PID_1205&MI_02\8&a5f3a41&0&0000";

        private bool? applicationRegistered;
        private bool? deviceHidden;
        private bool? clockDevices;

        public override void Tick(Context context)
        {
            if (applicationRegistered != true)
            {
                HidHideCLI.RegisterApplication(true);
                applicationRegistered = true;
            }

            if (clockDevices != true)
            {
                HidHideCLI.Cloak(true);
                clockDevices = true;
            }

            if (!Settings.Default.EnableHidHide)
            {
                HideDevice(false);
                return;
            }

            if (context.SteamUsesController)
            {
                HideDevice(false);
                context.ControllerHidden = false;
            }
            else
            {
                HideDevice(true);
                context.ControllerHidden = true;
            }
        }

        private void HideDevice(bool hidden)
        {
            if (deviceHidden == hidden)
                return;

            HidHideCLI.HideDevice(NeptuneDevicePath, hidden);
            deviceHidden = hidden;
        }
    }
}
