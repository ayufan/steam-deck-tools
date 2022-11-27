using WindowsInput;
using static CommonHelpers.Log;

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
        public const int UpdateValidInterval = 100;

        InputSimulator simulator = new InputSimulator();

        HashSet<Button> mouseButtons = new HashSet<Button>();
        HashSet<Button> lastMouseButtons = new HashSet<Button>();

        Accum movedX, movedY, verticalScroll, horizontalScroll;

        bool? valid = null;
        DateTime lastValid = DateTime.Now;

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
                else
                    mouseButtons.Remove(button);
            }
        }

        public bool Valid
        {
            get { return valid ?? true; }
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

        private void Safe(Func<bool> action)
        {
            try
            {
                if (action())
                {
                    valid = true;
                    lastValid = DateTime.Now;
                }

            }
            catch (InvalidOperationException)
            {
                valid = false;
                lastValid = DateTime.Now;
            }
        }

        private void UpdateValid()
        {
            Safe(() =>
            {
                if (valid is null || lastValid.AddMilliseconds(UpdateValidInterval) < DateTime.Now)
                {
                    simulator.Mouse.MoveMouseBy(0, 0);
                    return true;
                }

                return false;
            });
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
                Safe(() =>
                {
                    switch (button)
                    {
                        case Button.Left:
                            simulator.Mouse.LeftButtonUp();
                            return true;

                        case Button.Right:
                            simulator.Mouse.RightButtonUp();
                            return true;

                        case Button.Middle:
                            simulator.Mouse.MiddleButtonUp();
                            return true;

                        case Button.X:
                            simulator.Mouse.XButtonUp(XButtonID);
                            return true;

                        case Button.Y:
                            simulator.Mouse.XButtonUp(YButtonID);
                            return true;

                        default:
                            return false;
                    }
                });
            }

            // Key Down: new keys being down
            foreach (var button in mouseButtons.Except(lastMouseButtons))
            {
                Safe(() =>
                {
                    switch (button)
                    {
                        case Button.Left:
                            simulator.Mouse.LeftButtonDown();
                            return true;

                        case Button.Right:
                            simulator.Mouse.RightButtonDown();
                            return true;

                        case Button.Middle:
                            simulator.Mouse.MiddleButtonDown();
                            return true;

                        case Button.X:
                            simulator.Mouse.XButtonDown(XButtonID);
                            return true;

                        case Button.Y:
                            simulator.Mouse.XButtonDown(YButtonID);
                            return true;

                        default:
                            return false;
                    }
                });
            }

            // Move cursor
            if (movedX.Used || movedY.Used)
            {
                Safe(() =>
                {
                    simulator.Mouse.MoveMouseBy(movedX.Consume(), movedY.Consume());
                    return true;
                });
            }

            // Scroll
            if (verticalScroll.Used)
            {
                Safe(() =>
                {
                    int value = verticalScroll.Consume();
                    if (value != 0)
                    {
                        simulator.Mouse.VerticalScroll(value);
                        return true;
                    }
                    return false;
                });
            }

            if (horizontalScroll.Used)
            {
                Safe(() =>
                {
                    int value = horizontalScroll.Consume();
                    if (value != 0)
                    {
                        simulator.Mouse.HorizontalScroll(value);
                        return true;
                    }
                    return false;
                });
            }

            UpdateValid();
        }

        public void MouseClick(Button button)
        {
            Safe(() =>
            {
                switch (button)
                {
                    case Button.Left:
                        simulator.Mouse.LeftButtonClick();
                        return true;

                    case Button.Right:
                        simulator.Mouse.RightButtonClick();
                        return true;

                    case Button.Middle:
                        simulator.Mouse.MiddleButtonClick();
                        return true;

                    case Button.X:
                        simulator.Mouse.XButtonClick(XButtonID);
                        return true;

                    case Button.Y:
                        simulator.Mouse.XButtonClick(YButtonID);
                        return true;

                    default:
                        return false;
                }
            });
        }

        public void MouseDoubleClick(Button button)
        {
            Safe(() =>
            {
                switch (button)
                {
                    case Button.Left:
                        simulator.Mouse.LeftButtonDoubleClick();
                        return true;

                    case Button.Right:
                        simulator.Mouse.RightButtonDoubleClick();
                        return true;

                    case Button.Middle:
                        simulator.Mouse.MiddleButtonDoubleClick();
                        return true;

                    case Button.X:
                        simulator.Mouse.XButtonDoubleClick(XButtonID);
                        return true;

                    case Button.Y:
                        simulator.Mouse.XButtonDoubleClick(YButtonID);
                        return true;

                    default:
                        return false;
                }
            });
        }

        public void MoveBy(double pixelDeltaX, double pixelDeltaY)
        {
            movedX.Add(pixelDeltaX);
            movedY.Add(pixelDeltaY);
        }

        public void MoveTo(double absoluteX, double absoluteY)
        {
            Safe(() =>
            {
                simulator.Mouse.MoveMouseTo(absoluteX, absoluteY);
                return true;
            });
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
