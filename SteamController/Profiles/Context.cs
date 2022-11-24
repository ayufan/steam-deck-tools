using static CommonHelpers.Log;

namespace SteamController.Profiles
{
    public class Context : IDisposable
    {
        public const double JoystickToMouseSensitivity = 1200;
        public const double PadToMouseSensitivity = 200;
        public const double ThumbToWhellSensitivity = 2;

        public Devices.SteamController Steam { get; private set; }
        public Devices.Xbox360Controller X360 { get; private set; }
        public Devices.KeyboardController Keyboard { get; private set; }
        public Devices.MouseController Mouse { get; private set; }

        public List<Profile> Profiles { get; } = new List<Profile>();

        public bool RequestDesktopMode { get; set; } = true;

        public bool DesktopMode
        {
            get
            {
                return RequestDesktopMode || !X360.Valid || !Mouse.Valid;
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

        public bool Update()
        {
            Steam.BeforeUpdate();
            X360.BeforeUpdate();
            Keyboard.BeforeUpdate();
            Mouse.BeforeUpdate();

            try
            {
                bool skip = false;

                foreach (Profile profile in Profiles)
                {
                    if (!profile.RunAlways && skip)
                    {
                        profile.Skipped(this);
                        continue;
                    }

                    try
                    {
                        var status = profile.Run(this);
                        if (status == Profile.Status.Stop)
                            skip = true;
                    }
                    catch (Exception e)
                    {
                        TraceLine("Profile: Exception: {0}", e.Message);
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
    }
}
