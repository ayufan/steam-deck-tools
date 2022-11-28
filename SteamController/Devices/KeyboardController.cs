using WindowsInput;
using KeyRepeats = System.Tuple<System.DateTime, int>;

namespace SteamController.Devices
{
    public class KeyboardController : IDisposable
    {
        public static readonly TimeSpan FirstRepeat = TimeSpan.FromMilliseconds(400);
        public static readonly TimeSpan NextRepeats = TimeSpan.FromMilliseconds(45);

        InputSimulator simulator = new InputSimulator();

        Dictionary<VirtualKeyCode, KeyRepeats> keyCodes = new Dictionary<VirtualKeyCode, KeyRepeats>();
        Dictionary<VirtualKeyCode, KeyRepeats> lastKeyCodes = new Dictionary<VirtualKeyCode, KeyRepeats>();

        public KeyboardController()
        {
        }

        public void Dispose()
        {
        }

        public bool this[ProfilesSettings.VirtualKeyCode key]
        {
            get { return this[(VirtualKeyCode)key]; }
            set { this[(VirtualKeyCode)key] = value; }
        }

        public bool this[System.Windows.Input.Key key]
        {
            get { return this[(VirtualKeyCode)System.Windows.Input.KeyInterop.VirtualKeyFromKey(key)]; }
            set { this[(VirtualKeyCode)System.Windows.Input.KeyInterop.VirtualKeyFromKey(key)] = value; }
        }

        public bool this[System.Windows.Forms.Keys key]
        {
            get
            {
                if (key.HasFlag(System.Windows.Forms.Keys.Shift) && !this[VirtualKeyCode.SHIFT])
                    return false;
                if (key.HasFlag(System.Windows.Forms.Keys.Alt) && !this[VirtualKeyCode.MENU])
                    return false;
                if (key.HasFlag(System.Windows.Forms.Keys.Control) && !this[VirtualKeyCode.CONTROL])
                    return false;
                return this[(VirtualKeyCode)(key & System.Windows.Forms.Keys.KeyCode)];
            }
            set
            {
                if (value)
                {
                    this[VirtualKeyCode.SHIFT] = key.HasFlag(System.Windows.Forms.Keys.Shift);
                    this[VirtualKeyCode.MENU] = key.HasFlag(System.Windows.Forms.Keys.Alt);
                    this[VirtualKeyCode.CONTROL] = key.HasFlag(System.Windows.Forms.Keys.Control);
                    this[(VirtualKeyCode)(key & System.Windows.Forms.Keys.KeyCode)] = true;
                }
            }
        }

        public bool this[VirtualKeyCode key]
        {
            get { return keyCodes.ContainsKey(key); }
            set
            {
                if (key == VirtualKeyCode.None)
                    return;

                if (value)
                {
                    if (keyCodes.ContainsKey(key))
                        return;
                    var keyRepeat = lastKeyCodes.GetValueOrDefault(key) ?? new KeyRepeats(DateTime.Now.Add(FirstRepeat), 0);
                    keyCodes.Add(key, keyRepeat);
                }
            }
        }

        public VirtualKeyCode[] DownKeys
        {
            get { return keyCodes.Keys.ToArray(); }
        }

        internal void BeforeUpdate()
        {
            lastKeyCodes = keyCodes;
            keyCodes = new Dictionary<VirtualKeyCode, KeyRepeats>();
        }

        internal void Update()
        {
            // Key Up: it is missing now
            foreach (var keyUp in lastKeyCodes.Except(keyCodes))
            {
                try
                {
                    // if key was not yet pressed, send it
                    if (keyUp.Value.Item2 < 0)
                        simulator.Keyboard.KeyPress(keyUp.Key);
                    simulator.Keyboard.KeyUp(keyUp.Key);
                }
                catch (InvalidOperationException) { }
            }

            // Key Down: new keys being down
            foreach (var keyUp in keyCodes.Except(lastKeyCodes))
            {
                try { simulator.Keyboard.KeyDown(keyUp.Key); }
                catch (InvalidOperationException) { }
            }

            // Key Repeats
            var now = DateTime.Now;
            foreach (var keyPress in keyCodes)
            {
                if (keyPress.Value.Item1 > now)
                    continue;

                try { simulator.Keyboard.KeyPress(keyPress.Key); }
                catch (InvalidOperationException) { }

                keyCodes[keyPress.Key] = new KeyRepeats(
                    DateTime.Now.Add(NextRepeats),
                    keyPress.Value.Item2 + 1
                );
            }
        }

        public void KeyPress(params VirtualKeyCode[] keyCodes)
        {
            try { simulator.Keyboard.KeyPress(keyCodes); }
            catch (InvalidOperationException) { }
        }

        public void KeyPress(VirtualKeyCode modifierKey, params VirtualKeyCode[] keyCodes)
        {
            try { simulator.Keyboard.ModifiedKeyStroke(modifierKey, keyCodes); }
            catch (InvalidOperationException) { }
        }

        public void KeyPress(IEnumerable<VirtualKeyCode> modifierKeys, params VirtualKeyCode[] keyCodes)
        {
            try { simulator.Keyboard.ModifiedKeyStroke(modifierKeys, keyCodes); }
            catch (InvalidOperationException) { }
        }
    }
}
