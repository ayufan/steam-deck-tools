using System.ComponentModel;

namespace CommonHelpers
{
    public class Notifier
    {
        public static void Notify(
            string message,
            string title,
            Icon minorIcon,
            ToolTipIcon majorIcon = ToolTipIcon.Info,
            int timeout = 3000)
        {
            using (var container = new Container())
            using (var notifyIcon = new NotifyIcon(container))
            {
                notifyIcon.Icon = minorIcon;
                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(timeout, title, message, majorIcon);
            }
        }
    }
}
