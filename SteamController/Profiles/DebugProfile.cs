using static CommonHelpers.Log;

namespace SteamController.Profiles
{
    public sealed class DebugProfile : Profile
    {
        public DebugProfile()
        {
            RunAlways = true;
        }

        List<string> lastItems = new List<string>();

        public override Status Run(Context c)
        {
            var items = new List<string>();

            if (c.DesktopMode)
                items.Add("[DESKTOP]");
            else
                items.Add("[CONTROLLER]");

            if (c.Steam.LizardButtons)
                items.Add("[LB]");
            if (c.Steam.LizardMouse)
                items.Add("[LM]");
            if (c.X360.Connected)
                items.Add("[X360]");
            else if (c.X360.Valid)
                items.Add("[no-X360]");

            foreach (var button in c.Steam.AllButtons)
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

            foreach (var key in c.Keyboard.DownKeys)
            {
                items.Add(String.Format("Key{0}", key));
            }

            foreach (var mouse in c.Mouse.DownButtons)
            {
                items.Add(String.Format("Mouse{0}", mouse));
            }

            if (!items.SequenceEqual(lastItems))
            {
                TraceLine("DEBUG: {0}", String.Join(" ", items));
                lastItems = items;
            }
            return Status.Continue;
        }
    }
}
