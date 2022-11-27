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

        public bool this[VirtualKeyCode button]
        {
            get { return keyCodes.ContainsKey(button); }
            set
            {
                if (value)
                {
                    if (keyCodes.ContainsKey(button))
                        return;
                    var keyRepeat = lastKeyCodes.GetValueOrDefault(button) ?? new KeyRepeats(DateTime.Now.Add(FirstRepeat), 0);
                    keyCodes.Add(button, keyRepeat);
                }
                else
                {
                    keyCodes.Remove(button);
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
