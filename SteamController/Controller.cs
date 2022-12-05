using CommonHelpers;
using ExternalHelpers;
using SteamController.Profiles;
using System.ComponentModel;
using System.Diagnostics;

namespace SteamController
{
    internal class Controller : IDisposable
    {
        public const String Title = "Steam Controller";
        public readonly String TitleWithVersion = Title + " v" + Application.ProductVersion.ToString();

        Container components = new Container();
        NotifyIcon notifyIcon;
        StartupManager startupManager = new StartupManager(Title);

        Context context = new Context()
        {
            Profiles = {
                new Profiles.DesktopProfile() { Name = "Desktop" },
                new Profiles.SteamProfile() { Name = "Steam", Visible = false },
                new Profiles.SteamWithShorcutsProfile() { Name = "Steam with Shortcuts", Visible = false },
                new Profiles.X360HapticProfile() { Name = "X360" }
            },
            Managers = {
                new Managers.ProcessManager(),
                new Managers.SteamManager(),
                new Managers.ProfileSwitcher(),
                new Managers.SteamConfigsManager(),
                new Managers.SharedDataManager()
            }
        };

        public Controller()
        {
            // Set available profiles
            ProfilesSettings.Helpers.ProfileNameConverter.Profiles = context.Profiles.
                Where((profile) => profile.Visible).
                Select((profile) => profile.Name).ToArray();

            Instance.RunOnce(TitleWithVersion, "Global\\SteamController");

            var contextMenu = new ContextMenuStrip(components);

            var enabledItem = new ToolStripMenuItem("&Enabled");
            enabledItem.Click += delegate { context.RequestEnable = !context.RequestEnable; };
            contextMenu.Opening += delegate { enabledItem.Checked = context.RequestEnable; };
            contextMenu.Items.Add(enabledItem);
            contextMenu.Items.Add(new ToolStripSeparator());

            foreach (var profile in context.Profiles)
            {
                if (profile.Name == "" || !profile.Visible)
                    continue;

                var profileItem = new ToolStripMenuItem(profile.Name);
                profileItem.Click += delegate { context.SelectProfile(profile.Name); };
                contextMenu.Opening += delegate { profileItem.Checked = context.CurrentProfile == profile; };
                contextMenu.Items.Add(profileItem);
            }

            contextMenu.Items.Add(new ToolStripSeparator());

#if DEBUG
            var lizardMouseItem = new ToolStripMenuItem("Use Lizard &Mouse");
            lizardMouseItem.Click += delegate { DefaultGuideShortcutsProfile.SteamModeLizardMouse = !DefaultGuideShortcutsProfile.SteamModeLizardMouse; };
            contextMenu.Opening += delegate { lizardMouseItem.Checked = DefaultGuideShortcutsProfile.SteamModeLizardMouse; };
            contextMenu.Items.Add(lizardMouseItem);

            var lizardButtonsItem = new ToolStripMenuItem("Use Lizard &Buttons");
            lizardButtonsItem.Click += delegate { DefaultGuideShortcutsProfile.SteamModeLizardButtons = !DefaultGuideShortcutsProfile.SteamModeLizardButtons; };
            contextMenu.Opening += delegate { lizardButtonsItem.Checked = DefaultGuideShortcutsProfile.SteamModeLizardButtons; };
            contextMenu.Items.Add(lizardButtonsItem);

            contextMenu.Items.Add(new ToolStripSeparator());
#endif

            AddSteamOptions(contextMenu);

            if (startupManager.IsAvailable)
            {
                var startupItem = new ToolStripMenuItem("Run On Startup");
                startupItem.Checked = startupManager.Startup;
                startupItem.Click += delegate { startupItem.Checked = startupManager.Startup = !startupManager.Startup; };
                contextMenu.Items.Add(startupItem);
            }

            var settingsItem = contextMenu.Items.Add("&Settings");
            settingsItem.Click += Settings_Click;

            contextMenu.Items.Add(new ToolStripSeparator());

            var helpItem = contextMenu.Items.Add("&Help");
            helpItem.Click += delegate { Process.Start("explorer.exe", "http://github.com/ayufan/steam-deck-tools"); };

            var mappingItem = contextMenu.Items.Add("&Mappings");
            mappingItem.Click += delegate { Process.Start("explorer.exe", "https://github.com/ayufan/steam-deck-tools#42-mappings"); };

            contextMenu.Items.Add(new ToolStripSeparator());

            var exitItem = contextMenu.Items.Add("&Exit");
            exitItem.Click += delegate { Application.Exit(); };

            notifyIcon = new NotifyIcon(components);
            notifyIcon.Icon = Resources.microsoft_xbox_controller_off;
            notifyIcon.Text = TitleWithVersion;
            notifyIcon.Visible = true;
            notifyIcon.ContextMenuStrip = contextMenu;

            var contextStateUpdate = new System.Windows.Forms.Timer(components);
            contextStateUpdate.Interval = 250;
            contextStateUpdate.Enabled = true;
            contextStateUpdate.Tick += ContextStateUpdate_Tick;

            context.SelectDefault = () =>
            {
                if (!context.SelectProfile(Settings.Default.DefaultProfile))
                    context.SelectProfile(context.Profiles.First().Name);
            };
            context.BackToDefault();

            context.ProfileChanged += (profile) =>
            {
#if false
                notifyIcon.ShowBalloonTip(
                    1000,
                    TitleWithVersion,
                    String.Format("Selected profile: {0}", profile.Name),
                    ToolTipIcon.Info
                );
#endif
            };

            context.Start();
        }

        private void ContextStateUpdate_Tick(object? sender, EventArgs e)
        {
            context.Tick();

            var isDesktop = context.CurrentProfile?.IsDesktop ?? false;

            if (!context.Mouse.Valid)
            {
                notifyIcon.Text = TitleWithVersion + ". Cannot send input.";
                notifyIcon.Icon = Resources.microsoft_xbox_controller_off_red;
            }
            else if (!context.X360.Valid)
            {
                notifyIcon.Text = TitleWithVersion + ". Missing ViGEm?";
                notifyIcon.Icon = Resources.microsoft_xbox_controller_red;
            }
            else if (context.Enabled)
            {
                if (context.State.SteamUsesSteamInput)
                {
                    notifyIcon.Icon = isDesktop ? Resources.monitor_off : Resources.microsoft_xbox_controller_off;
                    notifyIcon.Text = TitleWithVersion + ". Steam uses Steam Input";
                }
                else
                {
                    notifyIcon.Icon = isDesktop ? Resources.monitor : Resources.microsoft_xbox_controller;
                    notifyIcon.Text = TitleWithVersion;
                }

                var profile = context.CurrentProfile;
                if (profile is not null)
                    notifyIcon.Text = TitleWithVersion + ". Profile: " + profile.Name;
            }
            else
            {
                notifyIcon.Icon = isDesktop ? Resources.monitor_off : Resources.microsoft_xbox_controller_off;
                notifyIcon.Text = TitleWithVersion + ". Disabled";
            }

            notifyIcon.Text += String.Format(". Updates: {0}/s", context.UpdatesPerSec);
        }

        public void Dispose()
        {
            notifyIcon.Visible = false;
            context.Stop();
            using (context) { }
        }

        private void AddSteamOptions(ContextMenuStrip contextMenu)
        {
            var ignoreSteamItem = new ToolStripMenuItem("&Ignore Steam");
            ignoreSteamItem.ToolTipText = "Disable Steam detection. Ensures that neither Steam Controller or X360 Controller are not blacklisted.";
            ignoreSteamItem.Click += delegate
            {
                ConfigureSteam(
                    "This will enable Steam Controller and X360 Controller in Steam.",
                    false, false, false
                );
            };
            contextMenu.Items.Add(ignoreSteamItem);

            var useX360WithSteamItem = new ToolStripMenuItem("Use &X360 Controller with Steam");
            useX360WithSteamItem.ToolTipText = "Hide Steam Deck Controller from Steam, and uses X360 controller instead.";
            useX360WithSteamItem.Click += delegate
            {
                ConfigureSteam(
                    "This will hide Steam Controller from Steam and use X360 Controller for all games.",
                    true, true, false
                );
            };
            contextMenu.Items.Add(useX360WithSteamItem);

            var useSteamInputItem = new ToolStripMenuItem("Use &Steam Input with Steam");
            useSteamInputItem.ToolTipText = "Uses Steam Input and hides X360 Controller from Steam. Requires disabling ALL Steam Desktop Mode shortcuts.";
            useSteamInputItem.Click += delegate
            {
                ConfigureSteam(
                    "This will hide X360 Controller from Steam, and will try to detect Steam presence " +
                    "to disable usage of this application when running Steam Games.\n\n" +
                    "This does REQUIRE disabling DESKTOP MODE shortcuts in Steam.\n" +
                    "Follow guide found at https://github.com/ayufan/steam-deck-tools.",
                    true, false, true
                );
            };
            contextMenu.Items.Add(useSteamInputItem);

            var steamSeparatorItem = new ToolStripSeparator();
            contextMenu.Items.Add(steamSeparatorItem);

            contextMenu.Opening += delegate
            {
                var blacklistedSteamController = Helpers.SteamConfiguration.IsControllerBlacklisted(
                    Devices.SteamController.VendorID,
                    Devices.SteamController.ProductID
                );

                ignoreSteamItem.Visible = blacklistedSteamController is not null;
                useX360WithSteamItem.Visible = blacklistedSteamController is not null;
                useSteamInputItem.Visible = blacklistedSteamController is not null;
                steamSeparatorItem.Visible = blacklistedSteamController is not null;

                ignoreSteamItem.Checked = !Settings.Default.EnableSteamDetection || blacklistedSteamController == null;
                useX360WithSteamItem.Checked = Settings.Default.EnableSteamDetection && blacklistedSteamController == true;
                useSteamInputItem.Checked = Settings.Default.EnableSteamDetection && blacklistedSteamController == false;
            };
        }

        private void ConfigureSteam(String message, bool steamDetection, bool blacklistSteamController, bool blacklistX360Controller)
        {
            String text;

            text = "This will change Steam configuration.\n\n";
            text += "Close Steam before confirming as otherwise Steam will be forcefully closed.\n\n";
            text += message;

            var result = MessageBox.Show(
                text,
                TitleWithVersion,
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Exclamation
            );
            if (result != DialogResult.OK)
                return;

            Helpers.SteamConfiguration.KillSteam();
            Helpers.SteamConfiguration.WaitForSteamClose(5000);
            Helpers.SteamConfiguration.BackupSteamConfig();

            var steamControllerUpdate = Helpers.SteamConfiguration.UpdateControllerBlacklist(
                Devices.SteamController.VendorID,
                Devices.SteamController.ProductID,
                blacklistSteamController
            );
            var x360ControllerUpdate = Helpers.SteamConfiguration.UpdateControllerBlacklist(
                Devices.Xbox360Controller.VendorID,
                Devices.Xbox360Controller.ProductID,
                blacklistX360Controller
            );
            Settings.Default.EnableSteamDetection = steamDetection;

            if (steamControllerUpdate && x360ControllerUpdate)
            {
                notifyIcon.ShowBalloonTip(
                    3000, TitleWithVersion,
                    "Steam Configuration changed. You can start Steam now.",
                    ToolTipIcon.Info
                );
            }
            else
            {
                notifyIcon.ShowBalloonTip(
                    3000, TitleWithVersion,
                    "Steam Configuration was not updated. Maybe Steam is open?",
                    ToolTipIcon.Warning
                );
            }
        }

        private void Settings_Click(object? sender, EventArgs e)
        {
            var form = new Form()
            {
                Text = TitleWithVersion + " Settings",
                StartPosition = FormStartPosition.CenterScreen,
                Size = new Size(400, 600)
            };

            var propertyGrid = new PropertyGrid()
            {
                Dock = DockStyle.Fill,
                SelectedObject = new
                {
                    Desktop = ProfilesSettings.BackPanelSettings.Desktop,
                    X360 = ProfilesSettings.BackPanelSettings.X360,
                    X360Haptic = ProfilesSettings.X360HapticSettings.Default,
                    Application = Settings.Default
                }
            };

            propertyGrid.ExpandAllGridItems();
            form.Controls.Add(propertyGrid);
            form.ShowDialog();
        }
    }
}
