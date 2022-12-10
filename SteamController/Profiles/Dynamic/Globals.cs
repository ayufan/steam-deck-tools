using Nefarius.ViGEm.Client.Targets.Xbox360;
using SteamController.ProfilesSettings;

namespace SteamController.Profiles.Dynamic
{
    [Flags]
    public enum KeyModifiers
    {
        MOD_None = 0,
        MOD_SHIFT = 1,
        MOD_ALT = 2,
        MOD_CONTROL = 4,
        MOD_WIN = 8
    }

    public class Globals
    {
        private const string Consumed = "RoslynGlobals";

        private RoslynDynamicProfile _profile;
        private Context _context;

        public class SteamAPI
        {
            internal Devices.SteamController Target;
            internal SteamAPI(Devices.SteamController target) { Target = target; }

            public struct Button
            {
                internal Devices.SteamButton Target;
                public static implicit operator bool(Button button) => button.Target;
                public Button(Devices.SteamButton target) { Target = target; }
            }

            public struct Axis
            {
                internal Devices.SteamAxis Target;
                public static implicit operator short(Axis button) => button.Target;
                public Axis(Devices.SteamAxis target) { Target = target; }
            }

            public Button BtnL5 { get => new Button(Target.BtnL5); }
            public Button BtnOptions { get => new Button(Target.BtnOptions); }
            public Button BtnSteam { get => new Button(Target.BtnSteam); }
            public Button BtnMenu { get => new Button(Target.BtnMenu); }
            public Button BtnDpadDown { get => new Button(Target.BtnDpadDown); }
            public Button BtnDpadLeft { get => new Button(Target.BtnDpadLeft); }
            public Button BtnDpadRight { get => new Button(Target.BtnDpadRight); }
            public Button BtnDpadUp { get => new Button(Target.BtnDpadUp); }
            public Button BtnA { get => new Button(Target.BtnA); }
            public Button BtnX { get => new Button(Target.BtnX); }
            public Button BtnB { get => new Button(Target.BtnB); }
            public Button BtnY { get => new Button(Target.BtnY); }
            public Button BtnL1 { get => new Button(Target.BtnL1); }
            public Button BtnL2 { get => new Button(Target.BtnL2); }
            public Button BtnR1 { get => new Button(Target.BtnR1); }
            public Button BtnR2 { get => new Button(Target.BtnR2); }
            public Button BtnLeftStickPress { get => new Button(Target.BtnLeftStickPress); }
            public Button BtnLPadTouch { get => new Button(Target.BtnLPadTouch); }
            public Button BtnLPadPress { get => new Button(Target.BtnLPadPress); }
            public Button BtnRPadPress { get => new Button(Target.BtnRPadPress); }
            public Button BtnRPadTouch { get => new Button(Target.BtnRPadTouch); }
            public Button BtnR5 { get => new Button(Target.BtnR5); }
            public Button BtnRightStickPress { get => new Button(Target.BtnRightStickPress); }
            public Button BtnLStickTouch { get => new Button(Target.BtnLStickTouch); }
            public Button BtnRStickTouch { get => new Button(Target.BtnRStickTouch); }
            public Button BtnR4 { get => new Button(Target.BtnR4); }
            public Button BtnL4 { get => new Button(Target.BtnL4); }
            public Button BtnQuickAccess { get => new Button(Target.BtnQuickAccess); }

            public Button BtnVirtualLeftThumbUp { get => new Button(Target.BtnVirtualLeftThumbUp); }
            public Button BtnVirtualLeftThumbDown { get => new Button(Target.BtnVirtualLeftThumbDown); }
            public Button BtnVirtualLeftThumbLeft { get => new Button(Target.BtnVirtualLeftThumbLeft); }
            public Button BtnVirtualLeftThumbRight { get => new Button(Target.BtnVirtualLeftThumbRight); }

            public Axis LPadX { get => new Axis(Target.LPadX); }
            public Axis LPadY { get => new Axis(Target.LPadY); }
            public Axis RPadX { get => new Axis(Target.RPadX); }
            public Axis RPadY { get => new Axis(Target.RPadY); }
            public Axis AccelX { get => new Axis(Target.AccelX); }
            public Axis AccelY { get => new Axis(Target.AccelY); }
            public Axis AccelZ { get => new Axis(Target.AccelZ); }
            public Axis GyroPitch { get => new Axis(Target.GyroPitch); }
            public Axis GyroYaw { get => new Axis(Target.GyroYaw); }
            public Axis GyroRoll { get => new Axis(Target.GyroRoll); }
            public Axis LeftTrigger { get => new Axis(Target.LeftTrigger); }
            public Axis RightTrigger { get => new Axis(Target.RightTrigger); }
            public Axis LeftThumbX { get => new Axis(Target.LeftThumbX); }
            public Axis LeftThumbY { get => new Axis(Target.LeftThumbY); }
            public Axis RightThumbX { get => new Axis(Target.RightThumbX); }
            public Axis RightThumbY { get => new Axis(Target.RightThumbY); }
            public Axis LPadPressure { get => new Axis(Target.LPadPressure); }
            public Axis RPadPressure { get => new Axis(Target.RPadPressure); }
        }

        public class KeyboardAPI
        {
            internal Devices.KeyboardController Target;
            internal KeyboardAPI(Devices.KeyboardController target) { Target = target; }

            public bool this[VirtualKeyCode key]
            {
                get { return Target[key.ToWindowsInput()]; }
                set { Target.Overwrite(key.ToWindowsInput(), value); }
            }

            public void KeyPress(params VirtualKeyCode[] keyCodes)
            {
                KeyPress(KeyModifiers.MOD_None, keyCodes);
            }

            public void KeyPress(KeyModifiers modifiers, params VirtualKeyCode[] keyCodes)
            {
                var virtualCodes = keyCodes.Select((code) => (WindowsInput.VirtualKeyCode)code).ToArray();

                if (modifiers != KeyModifiers.MOD_None)
                {
                    List<WindowsInput.VirtualKeyCode> modifierCodes = new List<WindowsInput.VirtualKeyCode>();
                    if (modifiers.HasFlag(KeyModifiers.MOD_SHIFT))
                        modifierCodes.Add(WindowsInput.VirtualKeyCode.SHIFT);
                    if (modifiers.HasFlag(KeyModifiers.MOD_CONTROL))
                        modifierCodes.Add(WindowsInput.VirtualKeyCode.CONTROL);
                    if (modifiers.HasFlag(KeyModifiers.MOD_ALT))
                        modifierCodes.Add(WindowsInput.VirtualKeyCode.MENU);
                    if (modifiers.HasFlag(KeyModifiers.MOD_WIN))
                        modifierCodes.Add(WindowsInput.VirtualKeyCode.LWIN);
                    Target.KeyPress(modifierCodes, virtualCodes);
                }
                else
                {
                    Target.KeyPress(virtualCodes);
                }
            }
        }

        public class MouseAPI
        {
            internal Devices.MouseController Target;
            internal MouseAPI(Devices.MouseController target) { Target = target; }

            public struct Button
            {
                internal Devices.MouseController? Controller = null;
                internal Devices.MouseController.Button Target = Devices.MouseController.Button.Left;
                internal bool Value;

                internal Button(Devices.MouseController controller, Devices.MouseController.Button target) { Controller = controller; Target = target; Value = Controller[Target]; }
                internal Button(bool value) { this.Value = value; }

                public void Click() { Controller?.MouseClick(Target); }
                public void DoubleClick() { Controller?.MouseClick(Target); }

                internal void Set(Button value) { Controller?.Overwrite(Target, value.Value); }

                public static implicit operator Button(bool value) { return new Button(value); }
            }

            public Button BtnLeft { get => new Button(Target, Devices.MouseController.Button.Left); set => this.BtnLeft.Set(value); }
            public Button BtnRight { get => new Button(Target, Devices.MouseController.Button.Right); set => this.BtnRight.Set(value); }
            public Button BtnMiddle { get => new Button(Target, Devices.MouseController.Button.Middle); set => this.BtnMiddle.Set(value); }
            public Button BtnX { get => new Button(Target, Devices.MouseController.Button.X); set => this.BtnX.Set(value); }
            public Button BtnY { get => new Button(Target, Devices.MouseController.Button.Y); set => this.BtnY.Set(value); }

            public void MoveBy(double pixelDeltaX, double pixelDeltaY) { Target.MoveBy(pixelDeltaX, pixelDeltaY); }
            public void MoveTo(double absoluteX, double absoluteY) { Target.MoveTo(absoluteX, absoluteY); }
            public void VerticalScroll(double scrollAmountInClicks) { Target.VerticalScroll(scrollAmountInClicks); }
            public void HorizontalScroll(double scrollAmountInClicks) { Target.HorizontalScroll(scrollAmountInClicks); }
        }

        public class X360API
        {
            internal const int MinimumPresTimeMilliseconds = 30;
            internal Devices.Xbox360Controller Target;
            internal X360API(Devices.Xbox360Controller target) { Target = target; }

            public bool Connected { set => Target.Connected = value; }

            public bool BtnUp { set => Target.Overwrite(Xbox360Button.Up, value, MinimumPresTimeMilliseconds); }
            public bool BtnDown { set => Target.Overwrite(Xbox360Button.Down, value, MinimumPresTimeMilliseconds); }
            public bool BtnLeft { set => Target.Overwrite(Xbox360Button.Left, value, MinimumPresTimeMilliseconds); }
            public bool BtnRight { set => Target.Overwrite(Xbox360Button.Right, value, MinimumPresTimeMilliseconds); }
            public bool BtnA { set => Target.Overwrite(Xbox360Button.A, value, MinimumPresTimeMilliseconds); }
            public bool BtnB { set => Target.Overwrite(Xbox360Button.B, value, MinimumPresTimeMilliseconds); }
            public bool BtnX { set => Target.Overwrite(Xbox360Button.X, value, MinimumPresTimeMilliseconds); }
            public bool BtnY { set => Target.Overwrite(Xbox360Button.Y, value, MinimumPresTimeMilliseconds); }
            public bool BtnGuide { set => Target.Overwrite(Xbox360Button.Guide, value, MinimumPresTimeMilliseconds); }
            public bool BtnBack { set => Target.Overwrite(Xbox360Button.Back, value, MinimumPresTimeMilliseconds); }
            public bool BtnStart { set => Target.Overwrite(Xbox360Button.Start, value, MinimumPresTimeMilliseconds); }
            public bool BtnLeftShoulder { set => Target.Overwrite(Xbox360Button.LeftShoulder, value, MinimumPresTimeMilliseconds); }
            public bool BtnRightShoulder { set => Target.Overwrite(Xbox360Button.RightShoulder, value, MinimumPresTimeMilliseconds); }
            public bool BtnLeftThumb { set => Target.Overwrite(Xbox360Button.LeftThumb, value, MinimumPresTimeMilliseconds); }
            public bool BtnRightThumb { set => Target.Overwrite(Xbox360Button.RightThumb, value, MinimumPresTimeMilliseconds); }
            public short AxisLeftThumbX { set => Target[Xbox360Axis.LeftThumbX] = value; }
            public short AxisLeftThumbY { set => Target[Xbox360Axis.LeftThumbY] = value; }
            public short AxisRightThumbX { set => Target[Xbox360Axis.RightThumbX] = value; }
            public short AxisRightThumbY { set => Target[Xbox360Axis.RightThumbY] = value; }
            public short SliderLeftTrigger { set => Target[Xbox360Slider.LeftTrigger] = value; }
            public short SliderRightTrigger { set => Target[Xbox360Slider.RightTrigger] = value; }
        }

        private SteamAPI? _steamAPI;
        private X360API? _x360API;
        private KeyboardAPI? _keyboardAPI;
        private MouseAPI? _mouseAPI;

        public SteamAPI Steam { get => _steamAPI ??= new SteamAPI(_context.Steam); }
        public X360API X360 { get => _x360API ??= new X360API(_context.X360); }
        public KeyboardAPI Keyboard { get => _keyboardAPI ??= new KeyboardAPI(_context.Keyboard); }
        public MouseAPI Mouse { get => _mouseAPI ??= new MouseAPI(_context.Mouse); }

        public Globals(RoslynDynamicProfile profile, Context context)
        {
            this._profile = profile;
            this._context = context;
        }

        public bool Pressed(SteamAPI.Button button)
        {
            return button.Target.Pressed();
        }

        public bool JustPressed(SteamAPI.Button button)
        {
            return button.Target.JustPressed();
        }

        public bool HoldFor(SteamAPI.Button button, int minTimeMs)
        {
            return button.Target.Hold(TimeSpan.FromMilliseconds(minTimeMs), null);
        }

        public bool Turbo(SteamAPI.Button button, int timesPerSec)
        {
            var interval = TimeSpan.FromMilliseconds(1000 / timesPerSec);
            return button.Target.JustPressed() || button.Target.HoldRepeat(interval, interval, null);
        }

        public void Log(string format, params object?[] arg)
        {
            var output = String.Format(format, arg);
            CommonHelpers.Log.TraceLine("{0}: {1}: {2}", _profile.Name, _context.Steam.ElapsedMilliseconds, output);
        }
    }
}
