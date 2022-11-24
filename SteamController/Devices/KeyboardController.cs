using WindowsInput;

namespace SteamController.Devices
{
    public class KeyboardController : IDisposable
    {
        InputSimulator simulator = new InputSimulator();

        HashSet<VirtualKeyCode> keyCodes = new HashSet<VirtualKeyCode>();
        HashSet<VirtualKeyCode> lastKeyCodes = new HashSet<VirtualKeyCode>();

        public KeyboardController()
        {
        }

        public void Dispose()
        {
        }

        public bool this[VirtualKeyCode button]
        {
            get { return keyCodes.Contains(button); }
            set
            {
                if (value)
                    keyCodes.Add(button);
                else
                    keyCodes.Remove(button);
            }
        }

        public VirtualKeyCode[] DownKeys
        {
            get { return keyCodes.ToArray(); }
        }

        internal void BeforeUpdate()
        {
            lastKeyCodes = keyCodes;
            keyCodes = new HashSet<VirtualKeyCode>();
        }

        internal void Update()
        {
            try
            {
                // Key Up: it is missing now
                var keyUp = lastKeyCodes.Except(keyCodes).ToArray();
                if (keyUp.Any())
                    simulator.Keyboard.KeyUp(keyUp);

                // Key Down: new keys being down
                var keyDown = keyCodes.Except(lastKeyCodes).ToArray();
                if (keyDown.Any())
                    simulator.Keyboard.KeyUp(keyDown);
            }
            catch (InvalidOperationException)
            {
            }
        }

        public void KeyPress(params VirtualKeyCode[] keyCodes)
        {
            try
            {
                simulator.Keyboard.KeyPress(keyCodes);
            }
            catch (InvalidOperationException)
            {
            }
        }

        public void KeyPress(VirtualKeyCode modifierKey, params VirtualKeyCode[] keyCodes)
        {
            try
            {
                simulator.Keyboard.ModifiedKeyStroke(modifierKey, keyCodes);
            }
            catch (InvalidOperationException)
            {
            }
        }

        public void KeyPress(IEnumerable<VirtualKeyCode> modifierKeys, params VirtualKeyCode[] keyCodes)
        {
            try
            {
                simulator.Keyboard.ModifiedKeyStroke(modifierKeys, keyCodes);
            }
            catch (InvalidOperationException)
            {
            }
        }
    }
}
