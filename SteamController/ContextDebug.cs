using CommonHelpers;

namespace SteamController
{
    public partial class Context
    {
        List<string> debugLastItems = new List<string>();

        public void Debug()
        {
            var items = new List<string>();

            if (DesktopMode)
                items.Add("[DESKTOP]");
            else
                items.Add("[CONTROLLER]");

            if (Steam.LizardButtons)
                items.Add("[LB]");
            if (Steam.LizardMouse)
                items.Add("[LM]");
            if (X360.Connected)
                items.Add("[X360]");
            else if (X360.Valid)
                items.Add("[no-X360]");

            foreach (var button in Steam.AllButtons)
            {
                if (button is null || !button.LastValue)
                    continue;

                String text = button.Name;
                if (button.Consumed is not null)
                    text += String.Format("[{0}]", button.Consumed);
                if (button.Value)
                    text += "[P]";

                items.Add(text);
            }

            foreach (var key in Keyboard.DownKeys)
            {
                items.Add(String.Format("Key{0}", key));
            }

            foreach (var mouse in Mouse.DownButtons)
            {
                items.Add(String.Format("Mouse{0}", mouse));
            }

            if (!items.SequenceEqual(debugLastItems))
            {
                Log.TraceLine("DEBUG: {0}", String.Join(" ", items));
                debugLastItems = items;
            }
        }
    }
}
