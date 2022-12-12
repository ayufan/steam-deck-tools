using WindowsInput;

namespace SteamController.Devices
{
    public class MouseController : IDisposable
    {
        private struct Accum
        {
            double? last, now;

            public bool Used
            {
                get { return now is not null; }
            }

            public void Tick()
            {
                last = now;
                now = null;
            }

            public void Add(double delta)
            {
                now = (now ?? 0.0) + delta;
            }

            public int Consume()
            {
                double accum = ((now ?? 0.0) + (last ?? 0.0));
                now = accum - (int)accum;
                last = null;
                return (int)accum;
            }
        }

        // TODO: Unsure what it is
        public const int XButtonID = 0;
        public const int YButtonID = 1;

        InputSimulator simulator = new InputSimulator();

        HashSet<Button> mouseButtons = new HashSet<Button>();
        HashSet<Button> lastMouseButtons = new HashSet<Button>();

        Accum movedX, movedY, verticalScroll, horizontalScroll;

        public enum Button
        {
            Left,
            Right,
            Middle,
            X,
            Y
        }

        public bool this[Button button]
        {
            get { return mouseButtons.Contains(button); }
            set
            {
                if (value)
                    mouseButtons.Add(button);
            }
        }

        public Button[] DownButtons
        {
            get { return mouseButtons.ToArray(); }
        }

        internal MouseController()
        {
        }

        public void Dispose()
        {
        }

        private void Safe(Action action)
        {
            try
            {
                action();

                Managers.SASManager.Valid = true;
            }
            catch (InvalidOperationException)
            {
                Managers.SASManager.Valid = false;
            }
        }

        internal void BeforeUpdate()
        {
            lastMouseButtons = mouseButtons;
            mouseButtons = new HashSet<Button>();
            movedX.Tick();
            movedY.Tick();
            verticalScroll.Tick();
            horizontalScroll.Tick();
        }

        internal void Update()
        {
            // Mouse Up: it is missing now
            foreach (var button in lastMouseButtons.Except(mouseButtons))
            {
                switch (button)
                {
                    case Button.Left:
                        Safe(() => simulator.Mouse.LeftButtonUp());
                        break;

                    case Button.Right:
                        Safe(() => simulator.Mouse.RightButtonUp());
                        break;

                    case Button.Middle:
                        Safe(() => simulator.Mouse.MiddleButtonUp());
                        break;

                    case Button.X:
                        Safe(() => simulator.Mouse.XButtonUp(XButtonID));
                        break;

                    case Button.Y:
                        Safe(() => simulator.Mouse.XButtonUp(YButtonID));
                        break;
                }
            }

            // Key Down: new keys being down
            foreach (var button in mouseButtons.Except(lastMouseButtons))
            {
                switch (button)
                {
                    case Button.Left:
                        Safe(() => simulator.Mouse.LeftButtonDown());
                        break;

                    case Button.Right:
                        Safe(() => simulator.Mouse.RightButtonDown());
                        break;

                    case Button.Middle:
                        Safe(() => simulator.Mouse.MiddleButtonDown());
                        break;

                    case Button.X:
                        Safe(() => simulator.Mouse.XButtonDown(XButtonID));
                        break;

                    case Button.Y:
                        Safe(() => simulator.Mouse.XButtonDown(YButtonID));
                        break;
                }
            }

            // Move cursor
            if (movedX.Used || movedY.Used)
            {
                int x = movedX.Consume();
                int y = movedY.Consume();
                if (x != 0 || y != 0)
                {
                    Safe(() => simulator.Mouse.MoveMouseBy(x, y));
                }
            }

            // Scroll
            if (verticalScroll.Used)
            {
                int value = verticalScroll.Consume();
                if (value != 0)
                {
                    Safe(() => simulator.Mouse.VerticalScroll(value));
                }
            }

            if (horizontalScroll.Used)
            {
                int value = horizontalScroll.Consume();
                if (value != 0)
                {
                    Safe(() => simulator.Mouse.HorizontalScroll(value));
                }
            }
        }

        public void MouseClick(Button button)
        {
            switch (button)
            {
                case Button.Left:
                    Safe(() => simulator.Mouse.LeftButtonClick());
                    break;

                case Button.Right:
                    Safe(() => simulator.Mouse.RightButtonClick());
                    break;

                case Button.Middle:
                    Safe(() => simulator.Mouse.MiddleButtonClick());
                    break;

                case Button.X:
                    Safe(() => simulator.Mouse.XButtonClick(XButtonID));
                    break;

                case Button.Y:
                    Safe(() => simulator.Mouse.XButtonClick(YButtonID));
                    break;
            }
        }

        public void MouseDoubleClick(Button button)
        {
            switch (button)
            {
                case Button.Left:
                    Safe(() => simulator.Mouse.LeftButtonDoubleClick());
                    break;

                case Button.Right:
                    Safe(() => simulator.Mouse.RightButtonDoubleClick());
                    break;

                case Button.Middle:
                    Safe(() => simulator.Mouse.MiddleButtonDoubleClick());
                    break;

                case Button.X:
                    Safe(() => simulator.Mouse.XButtonDoubleClick(XButtonID));
                    break;

                case Button.Y:
                    Safe(() => simulator.Mouse.XButtonDoubleClick(YButtonID));
                    break;
            }
        }

        public void Overwrite(Button button, bool value)
        {
            if (value)
                mouseButtons.Add(button);
            else
                mouseButtons.Remove(button);
        }

        public void MoveBy(double pixelDeltaX, double pixelDeltaY)
        {
            movedX.Add(pixelDeltaX);
            movedY.Add(pixelDeltaY);
        }

        public void MoveTo(double absoluteX, double absoluteY)
        {
            Safe(() => simulator.Mouse.MoveMouseTo(absoluteX, absoluteY));
        }

        public void VerticalScroll(double scrollAmountInClicks)
        {
            verticalScroll.Add(scrollAmountInClicks);
        }

        public void HorizontalScroll(double scrollAmountInClicks)
        {
            horizontalScroll.Add(scrollAmountInClicks);
        }
    }
}
