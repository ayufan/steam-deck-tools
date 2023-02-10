using static CommonHelpers.Log;

namespace SteamController
{
    public partial class Context : IDisposable
    {
        public const double JoystickToMouseSensitivity = 1200;
        public const double PadToMouseSensitivity = 150;
        public const double PadToWhellSensitivity = 4;
        public const double ThumbToWhellSensitivity = 20;

        public Devices.SteamController Steam { get; private set; }
        public Devices.Xbox360Controller X360 { get; private set; }
        public Devices.DS4Controller DS4 { get; private set; }
        public Devices.KeyboardController Keyboard { get; private set; }
        public Devices.MouseController Mouse { get; private set; }

        public List<Profiles.Profile> Profiles { get; } = new List<Profiles.Profile>();
        public List<Managers.Manager> Managers { get; } = new List<Managers.Manager>();

        private int selectedProfile;
        private int controllerProfile;

        public struct ContextState
        {
            public bool GameProcessRunning { get; set; }
            public bool RTSSInForeground { get; set; }
            public bool SteamUsesX360Controller { get; set; }
            public bool SteamUsesDS4Controller { get; set; }
            public bool SteamUsesSteamInput { get; set; }

            public bool IsActive
            {
                get { return RTSSInForeground || GameProcessRunning || SteamUsesX360Controller || SteamUsesDS4Controller || SteamUsesSteamInput; }
            }

            public override string ToString()
            {
                string reason = "state";
                if (GameProcessRunning) reason += " game";
                if (SteamUsesX360Controller) reason += " steamX360";
                if (SteamUsesDS4Controller) reason += " steamDS4";
                if (SteamUsesSteamInput) reason += " steamInput";
                if (RTSSInForeground) reason += " rtss";
                return reason;
            }
        }

        public bool RequestEnable { get; set; } = true;
        public ContextState State;

        public event Action<Profiles.Profile> ProfileChanged;
        public Action? SelectDefault;

        public bool Enabled
        {
            get { return RequestEnable; }
        }

        public bool KeyboardMouseValid
        {
            get { return SteamController.Managers.SASManager.Valid; }
        }

        public Profiles.Profile? CurrentProfile
        {
            get
            {
                for (int i = 0; i < Profiles.Count; i++)
                {
                    var profile = Profiles[(selectedProfile + i) % Profiles.Count];
                    if (profile.Selected(this))
                        return profile;
                }

                return null;
            }
        }

        public Context()
        {
            Steam = new Devices.SteamController();
            X360 = new Devices.Xbox360Controller();
            DS4 = new Devices.DS4Controller();
            Keyboard = new Devices.KeyboardController();
            Mouse = new Devices.MouseController();

            ProfileChanged += (_) => X360.Beep();
            ProfileChanged += (profile) => TraceLine("Context: Selected Profile: {0}", profile.Name);
        }

        public void Dispose()
        {
            foreach (var manager in Managers)
                manager.Dispose();

            using (Steam) { }
            using (X360) { }
            using (DS4) { }
            using (Keyboard) { }
            using (Mouse) { }
        }

        public void Tick()
        {
            X360.Tick();
            DS4.Tick();

            foreach (var manager in Managers)
            {
                try { manager.Tick(this); }
                catch (Exception e) { TraceException("Controller", manager, e); }
            }
        }

        public bool Update()
        {
            Steam.BeforeUpdate();
            X360.BeforeUpdate();
            DS4.BeforeUpdate();
            Keyboard.BeforeUpdate();
            Mouse.BeforeUpdate();

            try
            {
                var profile = CurrentProfile;
                if (profile is not null)
                    profile.Run(this);

                return true;
            }
            catch (Exception e)
            {
                TraceException("Context", "Update", e);
                return false;
            }
            finally
            {
                Steam.Update();
                X360.Update();
                DS4.Update();
                Keyboard.Update();
                Mouse.Update();
            }
        }

        public bool SelectProfile(String name, bool userDefault = false)
        {
            lock (this)
            {
                for (int i = 0; i < Profiles.Count; i++)
                {
                    var profile = Profiles[i];
                    if (profile.Name != name)
                        continue;
                    if (!profile.Selected(this) && !userDefault)
                        continue;

                    if (i != selectedProfile)
                    {
                        selectedProfile = i;
                        if (!profile.IsDesktop && !userDefault)
                            controllerProfile = i;
                        OnProfileChanged(profile);
                    }
                    return true;
                }
            }

            return false;
        }

        public void SelectController()
        {
            lock (this)
            {
                var current = CurrentProfile;
                if (current is null)
                    return;
                if (!current.IsDesktop)
                    return;

                // Use last selected controller profile
                selectedProfile = controllerProfile;
                var currentController = CurrentProfile;
                if (current != currentController && currentController?.IsDesktop != true)
                    return;

                // Otherwise use next one
                TraceLine("Context: SelectController. State={0}", State);
                SelectNext();
            }
        }

        public bool SelectNext()
        {
            lock (this)
            {
                // Update selectedProfile index
                var current = CurrentProfile;
                if (current is null)
                    return false;
                selectedProfile = Profiles.IndexOf(current);

                for (int i = 1; i < Profiles.Count; i++)
                {
                    var idx = (selectedProfile + i) % Profiles.Count;
                    var profile = Profiles[idx];
                    if (profile.IsDesktop)
                        continue;
                    if (!profile.Selected(this))
                        continue;

                    selectedProfile = idx;
                    controllerProfile = idx;
                    OnProfileChanged(profile);
                    return true;
                }
            }

            return false;
        }

        public void BackToDefault()
        {
            TraceLine("Context: Back To Default.");
            if (SelectDefault is not null)
                SelectDefault();
        }

        private void OnProfileChanged(Profiles.Profile profile)
        {
            System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(
                new Action(() => ProfileChanged(profile)));
        }
    }
}
