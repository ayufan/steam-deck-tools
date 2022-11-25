using static CommonHelpers.Log;

namespace SteamController
{
    public partial class Context : IDisposable
    {
        public const double JoystickToMouseSensitivity = 1200;
        public const double PadToMouseSensitivity = 150;
        public const double PadToWhellSensitivity = 4;
        public const double ThumbToWhellSensitivity = 4;
        public static readonly TimeSpan ThumbToWhellFirstRepeat = TimeSpan.FromMilliseconds(30 * ThumbToWhellSensitivity);
        public static readonly TimeSpan ThumbToWhellRepeat = TimeSpan.FromMilliseconds(30 * ThumbToWhellSensitivity);

        public Devices.SteamController Steam { get; private set; }
        public Devices.Xbox360Controller X360 { get; private set; }
        public Devices.KeyboardController Keyboard { get; private set; }
        public Devices.MouseController Mouse { get; private set; }

        public List<Profiles.Profile> Profiles { get; } = new List<Profiles.Profile>();
        public List<Managers.Manager> Managers { get; } = new List<Managers.Manager>();

        public List<Profiles.Profile>? orderedProfiles;

        public bool RequestEnable { get; set; } = true;
        public bool RequestDesktopMode { get; set; } = true;
        public bool SteamRunning { get; set; } = false;
        public bool SteamUsesController { get; set; } = false;
        public bool ControllerHidden { get; set; } = false;

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
                    TraceLine("Manager: {0}. Exception: {1}", e);
                }
            }
        }

        public bool Update()
        {
            Steam.BeforeUpdate();
            X360.BeforeUpdate();
            Keyboard.BeforeUpdate();
            Mouse.BeforeUpdate();

            try
            {
                foreach (var profile in OrderedProfiles)
                {
                    if (profile.Selected(this))
                    {
                        profile.Run(this);
                        break;
                    }
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
                return true;
            }

            return false;
        }
    }
}
