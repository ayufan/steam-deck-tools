using CommonHelpers;
using ExternalHelpers;
using SteamController.Helpers;
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
                new Profiles.DesktopProfile(),
                new Profiles.SteamProfile(),
                new Profiles.X360Profile(),
            },
            Managers = {
                new Managers.ProcessManager(),
                new Managers.SteamManager()
            }
        };

        Thread? contextThread;
        bool running = true;
        Stopwatch stopwatch = new Stopwatch();
        int updatesReceived = 0;
        int lastUpdatesReceived = 0;
        TimeSpan lastUpdatesReset;
        readonly TimeSpan updateResetInterval = TimeSpan.FromSeconds(1);

        public Controller()
        {
            Instance.RunOnce(TitleWithVersion, "Global\\SteamController");

            var contextMenu = new ContextMenuStrip(components);

            var enabledItem = new ToolStripMenuItem("&Enabled");
            enabledItem.Checked = context.RequestEnable;
            enabledItem.Click += delegate { context.RequestEnable = !context.RequestEnable; };
            contextMenu.Opening += delegate { enabledItem.Checked = context.RequestEnable; };
            contextMenu.Items.Add(enabledItem);

            var desktopModeItem = new ToolStripMenuItem("&Desktop Mode");
            desktopModeItem.Checked = context.RequestDesktopMode;
            desktopModeItem.Click += delegate { context.RequestDesktopMode = !context.RequestDesktopMode; };
            contextMenu.Opening += delegate { desktopModeItem.Checked = context.RequestDesktopMode; };
            contextMenu.Items.Add(desktopModeItem);

            var steamDetectionItem = new ToolStripMenuItem("Auto-disable on &Steam");
            steamDetectionItem.Checked = Settings.Default.EnableSteamDetection;
            steamDetectionItem.Click += delegate
            {
                Settings.Default.EnableSteamDetection = !Settings.Default.EnableSteamDetection;
                Settings.Default.Save();
            };
            contextMenu.Opening += delegate { steamDetectionItem.Checked = Settings.Default.EnableSteamDetection; };
            contextMenu.Items.Add(steamDetectionItem);
            contextMenu.Items.Add(new ToolStripSeparator());

            var lizardMouseItem = new ToolStripMenuItem("Use Lizard &Mouse");
            lizardMouseItem.Checked = SteamShortcutsProfile.SteamModeLizardMouse;
            lizardMouseItem.Click += delegate { SteamShortcutsProfile.SteamModeLizardMouse = !SteamShortcutsProfile.SteamModeLizardMouse; };
            contextMenu.Opening += delegate { lizardMouseItem.Checked = SteamShortcutsProfile.SteamModeLizardMouse; };
            contextMenu.Items.Add(lizardMouseItem);

            var lizardButtonsItem = new ToolStripMenuItem("Use Lizard &Buttons");
            lizardButtonsItem.Checked = SteamShortcutsProfile.SteamModeLizardButtons;
            lizardButtonsItem.Click += delegate { SteamShortcutsProfile.SteamModeLizardButtons = !SteamShortcutsProfile.SteamModeLizardButtons; };
            contextMenu.Opening += delegate { lizardButtonsItem.Checked = SteamShortcutsProfile.SteamModeLizardButtons; };
            contextMenu.Items.Add(lizardButtonsItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            if (startupManager.IsAvailable)
            {
                var startupItem = new ToolStripMenuItem("Run On Startup");
                startupItem.Checked = startupManager.Startup;
                startupItem.Click += delegate { startupItem.Checked = startupManager.Startup = !startupManager.Startup; };
                contextMenu.Items.Add(startupItem);
            }

            var helpItem = contextMenu.Items.Add("&Help");
            helpItem.Click += delegate { Process.Start("explorer.exe", "http://github.com/ayufan-research/steam-deck-tools"); };

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

            stopwatch.Start();

            contextThread = new Thread(ContextState_Update);
            contextThread.Start();
        }

        private void ContextState_Update(object? obj)
        {
            while (running)
            {
                if (lastUpdatesReset + updateResetInterval < stopwatch.Elapsed)
                {
                    lastUpdatesReset = stopwatch.Elapsed;
                    lastUpdatesReceived = updatesReceived;
                    updatesReceived = 0;
                }

                updatesReceived++;

                lock (context)
                {
                    context.Update();
                    context.Debug();
                }

                if (!context.Enabled)
                {
                    Thread.Sleep(100);
                }
            }
        }

        private void ContextStateUpdate_Tick(object? sender, EventArgs e)
        {
            lock (context)
            {
                context.Tick();
            }

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
            else if (context.Enabled && context.SteamUsesController)
            {
                notifyIcon.Icon = context.DesktopMode ? Resources.monitor_off : Resources.microsoft_xbox_controller_off;
                notifyIcon.Text = TitleWithVersion + ". Steam Detected";
            }
            else if (context.Enabled)
            {
                notifyIcon.Icon = context.DesktopMode ? Resources.monitor : Resources.microsoft_xbox_controller;
                notifyIcon.Text = TitleWithVersion;
            }
            else
            {
                notifyIcon.Icon = context.DesktopMode ? Resources.monitor_off : Resources.microsoft_xbox_controller_off;
                notifyIcon.Text = TitleWithVersion + ". Disabled";
            }

            notifyIcon.Text += String.Format(". Updates: {0}/s", lastUpdatesReceived);
        }

        public void Dispose()
        {
            notifyIcon.Visible = false;
            running = false;

            if (contextThread != null)
            {
                contextThread.Interrupt();
                contextThread.Join();
            }

            using (context) { }
        }
    }
}
