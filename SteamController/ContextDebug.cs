using CommonHelpers;

namespace SteamController
{
    public partial class Context
    {
        private List<string> debugLastItems = new List<string>();
        private Profiles.Profile? debugLastProfile = null;

        public void Debug()
        {
            var items = new List<string>();

            var profile = CurrentProfile;
            if (profile?.IsDesktop ?? false)
                items.Add("[DESKTOP]");
            else
                items.Add("[CONTROLLER]");

            if (profile != debugLastProfile)
            {
                Log.TraceLine("ProfileChanged: {0} to {1}", debugLastProfile?.Name ?? "null", profile?.Name ?? "null");
                debugLastProfile = profile;
            }

            if (Steam.LizardButtons)
                items.Add("[LB]");
            if (Steam.LizardMouse)
                items.Add("[LM]");

            items.Add(X360.Connected ? "[X360]" : X360.Valid ? "[no-X360]" : "[inv-X360]");
            items.Add(DS4.Connected ? "[DS4]" : DS4.Valid ? "[no-DS4]" : "[inv-DS4]");
            items.Add(KeyboardMouseValid ? "[KM]" : "[inv-KM]");

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
                Log.TraceDebug("DEBUG: {0}", String.Join(" ", items));
                debugLastItems = items;
            }
        }
    }
}
