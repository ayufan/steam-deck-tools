using CommonHelpers;

namespace PowerControl
{
    internal sealed class Settings : BaseSettings
    {
        public static readonly Settings Default = new Settings();

        public Settings() : base("Settings")
        {
            TouchSettings = true;
        }

        public string MenuUpKey
        {
            get { return Get("MenuUpKey", "Ctrl+Win+Numpad8"); }
            set { Set("MenuUpKey", value); }
        }

        public string MenuDownKey
        {
            get { return Get("MenuDownKey", "Ctrl+Win+Numpad2"); }
            set { Set("MenuDownKey", value); }
        }

        public string MenuLeftKey
        {
            get { return Get("MenuLeftKey", "Ctrl+Win+Numpad4"); }
            set { Set("MenuLeftKey", value); }
        }

        public string MenuRightKey
        {
            get { return Get("MenuRightKey", "Ctrl+Win+Numpad6"); }
            set { Set("MenuRightKey", value); }
        }

        public string MenuToggle
        {
            get { return Get("MenuToggle", "Alt+F11"); }
            set { Set("MenuToggle", value); }
        }

        public bool EnableNeptuneController
        {
            get { return Get<bool>("EnableNeptuneController", true); }
            set { Set("EnableNeptuneController", value); }
        }

        public bool EnableVolumeControls
        {
            get { return Get<bool>("EnableVolumeControls", true); }
            set { Set("EnableVolumeControls", value); }
        }

        public bool EnableExperimentalFeatures
        {
            get { return Instance.IsDEBUG; }
        }

    }
}
