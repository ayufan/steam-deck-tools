using hidapi;
using PowerControl.External;
using static CommonHelpers.Log;

namespace SteamController.Devices
{
    public partial class SteamController
    {
        // Based on: https://github.com/torvalds/linux/blob/master/drivers/hid/hid-steam.c
        public const byte ID_SET_DIGITAL_MAPPINGS = 0x80;
        public const byte ID_CLEAR_DIGITAL_MAPPINGS = 0x81;
        public const byte ID_SET_DEFAULT_DIGITAL_MAPPINGS = 0x85;
        public const byte ID_FACTORY_RESET = 0x86;
        public const byte ID_SET_SETTINGS_VALUES = 0x87;
        public const byte ID_DEFAULT_MAPPINGS = 0x8e;

        public const byte SETTING_LEFT_TRACKPAD_MODE = 7;
        public const byte SETTING_RIGHT_TRACKPAD_MODE = 8;
        public const byte SETTING_LEFT_TRACKPAD_CLICK_PRESSURE = 52;
        public const byte SETTING_RIGHT_TRACKPAD_CLICK_PRESSURE = 53;
        public const byte SETTING_STEAM_WATCHDOG_ENABLE = 71;

        public const ushort TRACKPAD_NONE = 7;

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

            // Enable or disable mouse emulation
            if (LizardMouse)
            {
                SendFeatureByte(ID_DEFAULT_MAPPINGS);
            }
            else
            {
                SendSettings(
                    (SETTING_LEFT_TRACKPAD_MODE, TRACKPAD_NONE), // disable mouse
                    (SETTING_RIGHT_TRACKPAD_MODE, TRACKPAD_NONE), // disable mouse
                    (SETTING_LEFT_TRACKPAD_CLICK_PRESSURE, 0xFFFF), // disable haptic click
                    (SETTING_RIGHT_TRACKPAD_CLICK_PRESSURE, 0xFFFF), // disable haptic click
                    (SETTING_STEAM_WATCHDOG_ENABLE, 0) // disable watchdog that tests if Steam is active
                );
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

            // Enable / Disable esc, enter, cursors
            if (LizardButtons)
                SendFeatureByte(ID_SET_DEFAULT_DIGITAL_MAPPINGS);
            else
                SendFeatureByte(ID_CLEAR_DIGITAL_MAPPINGS);
        }

        private void SendFeatureByte(byte b)
        {
            neptuneDevice.RequestFeatureReport(new byte[] { b, 0 });
        }

        private void SendSettings(params (byte setting, ushort val)[] settings)
        {
            // Format: 0x87 len (reg valLo valHi)*
            byte[] cmd = new byte[2 + settings.Length * 3];
            cmd[0] = ID_SET_SETTINGS_VALUES;
            cmd[1] = (byte)(settings.Length * 3); // length

            int length = 2;
            foreach (var (setting, val) in settings)
            {
                cmd[length++] = (byte)setting;
                cmd[length++] = (byte)(val & 0xFF);
                cmd[length++] = (byte)(val >> 8);
            }

            neptuneDevice.RequestFeatureReport(cmd);
        }
    }
}
