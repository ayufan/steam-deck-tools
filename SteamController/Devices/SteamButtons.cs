using hidapi;
using PowerControl.External;
using static CommonHelpers.Log;

namespace SteamController.Devices
{
    public partial class SteamController
    {
        public readonly SteamButton BtnL5 = new SteamButton2(0x08, SDCButton0.BTN_L5);
        public readonly SteamButton BtnOptions = new SteamButton2(0x08, SDCButton0.BTN_OPTIONS);
        public readonly SteamButton BtnSteam = new SteamButton2(0x08, SDCButton0.BTN_STEAM);
        public readonly SteamButton BtnMenu = new SteamButton2(0x08, SDCButton0.BTN_MENU);
        public readonly SteamButton BtnDpadDown = new SteamButton2(0x08, SDCButton0.BTN_DPAD_DOWN) { LizardButton = true };
        public readonly SteamButton BtnDpadLeft = new SteamButton2(0x08, SDCButton0.BTN_DPAD_LEFT) { LizardButton = true };
        public readonly SteamButton BtnDpadRight = new SteamButton2(0x08, SDCButton0.BTN_DPAD_RIGHT) { LizardButton = true };
        public readonly SteamButton BtnDpadUp = new SteamButton2(0x08, SDCButton0.BTN_DPAD_UP) { LizardButton = true };
        public readonly SteamButton BtnA = new SteamButton2(0x08, SDCButton0.BTN_A) { LizardButton = true };
        public readonly SteamButton BtnX = new SteamButton2(0x08, SDCButton0.BTN_X);
        public readonly SteamButton BtnB = new SteamButton2(0x08, SDCButton0.BTN_B) { LizardButton = true };
        public readonly SteamButton BtnY = new SteamButton2(0x08, SDCButton0.BTN_Y);
        public readonly SteamButton BtnL1 = new SteamButton2(0x08, SDCButton0.BTN_L1);
        public readonly SteamButton BtnL2 = new SteamButton2(0x08, SDCButton0.BTN_L2) { LizardButton = true };
        public readonly SteamButton BtnR1 = new SteamButton2(0x08, SDCButton0.BTN_R1);
        public readonly SteamButton BtnR2 = new SteamButton2(0x08, SDCButton0.BTN_R2) { LizardButton = true };
        public readonly SteamButton BtnLeftStickPress = new SteamButton2(0x0a, SDCButton1.BTN_LSTICK_PRESS);
        public readonly SteamButton BtnLPadTouch = new SteamButton2(0x0a, SDCButton1.BTN_LPAD_TOUCH);
        public readonly SteamButton BtnLPadPress = new SteamButton2(0x0a, SDCButton1.BTN_LPAD_PRESS);
        public readonly SteamButton BtnRPadPress = new SteamButton2(0x0a, SDCButton1.BTN_RPAD_PRESS);
        public readonly SteamButton BtnRPadTouch = new SteamButton2(0x0a, SDCButton1.BTN_RPAD_TOUCH);
        public readonly SteamButton BtnR5 = new SteamButton2(0x0a, SDCButton1.BTN_R5);
        public readonly SteamButton BtnRightStickPress = new SteamButton2(0x0B, SDCButton2.BTN_RSTICK_PRESS);
        public readonly SteamButton BtnLStickTouch = new SteamButton2(0x0D, SDCButton4.BTN_LSTICK_TOUCH);
        public readonly SteamButton BtnRStickTouch = new SteamButton2(0x0D, SDCButton4.BTN_RSTICK_TOUCH);
        public readonly SteamButton BtnR4 = new SteamButton2(0x0D, SDCButton4.BTN_R4);
        public readonly SteamButton BtnL4 = new SteamButton2(0x0D, SDCButton4.BTN_L4);
        public readonly SteamButton BtnQuickAccess = new SteamButton2(0x0E, SDCButton5.BTN_QUICK_ACCESS);

        public readonly SteamButton BtnVirtualLeftThumbUp = new SteamButton();
        public readonly SteamButton BtnVirtualLeftThumbDown = new SteamButton();
        public readonly SteamButton BtnVirtualLeftThumbLeft = new SteamButton();
        public readonly SteamButton BtnVirtualLeftThumbRight = new SteamButton();

        public readonly SteamAxis LPadX = new SteamAxis(0x10) { DeltaValueMode = Devices.DeltaValueMode.Delta };
        public readonly SteamAxis LPadY = new SteamAxis(0x12) { DeltaValueMode = Devices.DeltaValueMode.Delta };
        public readonly SteamAxis RPadX = new SteamAxis(0x14) { LizardMouse = true, DeltaValueMode = Devices.DeltaValueMode.Delta };
        public readonly SteamAxis RPadY = new SteamAxis(0x16) { LizardMouse = true, DeltaValueMode = Devices.DeltaValueMode.Delta };
        public readonly SteamAxis AccelX = new SteamAxis(0x18);
        public readonly SteamAxis AccelY = new SteamAxis(0x1A);
        public readonly SteamAxis AccelZ = new SteamAxis(0x1C);
        public readonly SteamAxis GyroPitch = new SteamAxis(0x1E);
        public readonly SteamAxis GyroRoll = new SteamAxis(0x20);
        public readonly SteamAxis GyroYaw = new SteamAxis(0x22);
        public readonly SteamAxis LeftTrigger = new SteamAxis(0x2C);
        public readonly SteamAxis RightTrigger = new SteamAxis(0x2E);
        public readonly SteamAxis LeftThumbX = new SteamAxis(0x30) { Deadzone = 5000, MinChange = 10, DeltaValueMode = Devices.DeltaValueMode.AbsoluteTime };
        public readonly SteamAxis LeftThumbY = new SteamAxis(0x32) { Deadzone = 5000, MinChange = 10, DeltaValueMode = Devices.DeltaValueMode.AbsoluteTime };
        public readonly SteamAxis RightThumbX = new SteamAxis(0x34) { Deadzone = 5000, MinChange = 10, DeltaValueMode = Devices.DeltaValueMode.AbsoluteTime };
        public readonly SteamAxis RightThumbY = new SteamAxis(0x36) { Deadzone = 5000, MinChange = 10, DeltaValueMode = Devices.DeltaValueMode.AbsoluteTime };
        public readonly SteamAxis LPadPressure = new SteamAxis(0x38);
        public readonly SteamAxis RPadPressure = new SteamAxis(0x38);

        private void InitializeButtons()
        {
            LPadX.ActiveButton = BtnLPadTouch;
            LPadY.ActiveButton = BtnLPadTouch;
            RPadX.ActiveButton = BtnRPadTouch;
            RPadY.ActiveButton = BtnRPadTouch;

            // map virtual key presses
            LeftThumbX.VirtualLeft = BtnVirtualLeftThumbLeft;
            LeftThumbX.VirtualRight = BtnVirtualLeftThumbRight;
            LeftThumbY.VirtualLeft = BtnVirtualLeftThumbDown;
            LeftThumbY.VirtualRight = BtnVirtualLeftThumbUp;
        }
    }
}
