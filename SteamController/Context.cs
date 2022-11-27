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
        public Devices.KeyboardController Keyboard { get; private set; }
        public Devices.MouseController Mouse { get; private set; }

        public List<Profiles.Profile> Profiles { get; } = new List<Profiles.Profile>();
        public List<Managers.Manager> Managers { get; } = new List<Managers.Manager>();

        public List<Profiles.Profile>? orderedProfiles;

        public bool RequestEnable { get; set; } = true;
        public bool RequestDesktopMode { get; set; } = true;
        public bool SteamUsesX360Controller { get; set; } = false;
        public bool SteamUsesSteamInput { get; set; } = false;

        public event Action<Profiles.Profile> ProfileChanged;

        public bool Enabled
        {
            get { return RequestEnable; }
        }

        public bool DesktopMode
        {
            get
            {
                return RequestDesktopMode || !X360.Valid || !Mouse.Valid;
            }
        }

        public List<Profiles.Profile> OrderedProfiles
        {
            get
            {
                if (orderedProfiles == null)
                    orderedProfiles = Profiles.ToList();
                return orderedProfiles;
            }
        }

        public Context()
        {
            Steam = new Devices.SteamController();
            X360 = new Devices.Xbox360Controller();
            Keyboard = new Devices.KeyboardController();
            Mouse = new Devices.MouseController();

            ProfileChanged += (_) => X360.Beep();
        }

        public void Dispose()
        {
            using (Steam) { }
            using (X360) { }
            using (Keyboard) { }
            using (Mouse) { }
        }

        public void Tick()
        {
            X360.Tick();

            foreach (var manager in Managers)
            {
                try
                {
                    manager.Tick(this);
                }
                catch (Exception e)
                {
                    TraceLine("Manager: {0}. Exception: {1}", manager, e);
                }
            }
        }

        public Profiles.Profile? GetCurrentProfile()
        {
            foreach (var profile in OrderedProfiles)
            {
                if (profile.Selected(this))
                {
                    return profile;
                }
            }

            return null;
        }

        public bool Update()
        {
            Steam.BeforeUpdate();
            X360.BeforeUpdate();
            Keyboard.BeforeUpdate();
            Mouse.BeforeUpdate();

            try
            {
                var profile = GetCurrentProfile();
                if (profile is not null)
                {
                    profile.Run(this);
                }

                return true;
            }
            catch (Exception e)
            {
                TraceLine("Controller: Exception: {0}", e);
                return false;
            }
            finally
            {
                Steam.Update();
                X360.Update();
                Keyboard.Update();
                Mouse.Update();
            }
        }

        public Profiles.Profile? FindProfile(String name)
        {
            return Profiles.Find((profile) => profile.Name == name);
        }

        public bool SelectProfile(String name, bool enable = true)
        {
            var profile = FindProfile(name);
            if (profile is null)
                return false;

            var list = OrderedProfiles;
            list.Remove(profile);
            list.Insert(0, profile);
            RequestDesktopMode = profile.IsDesktop;

            if (profile.Selected(this))
                ProfileChanged(profile);
            return true;
        }

        public bool SelectNext()
        {
            bool firstSelected = false;

            var list = OrderedProfiles;
            foreach (var profile in list)
            {
                if (!profile.Selected(this))
                    continue;

                if (!firstSelected)
                {
                    firstSelected = true;
                    continue;
                }

                list.Remove(profile);
                list.Insert(0, profile);
                ProfileChanged(profile);
                return true;
            }

            return false;
        }

        public void ToggleDesktopMode(bool? forceState = null)
        {
            var oldProfile = GetCurrentProfile();
            if (forceState is null)
                RequestDesktopMode = !RequestDesktopMode;
            else
                RequestDesktopMode = forceState.Value;

            var newProfile = GetCurrentProfile();
            if (oldProfile != newProfile && newProfile is not null)
                ProfileChanged(newProfile);
        }
    }
}
