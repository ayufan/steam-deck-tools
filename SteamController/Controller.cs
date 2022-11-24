using CommonHelpers;
using ExternalHelpers;
using SteamController.Profiles;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SteamController
{
    internal class Controller : IDisposable
    {
        public const String Title = "Steam Controller";
        public readonly String TitleWithVersion = Title + " v" + Application.ProductVersion.ToString();

        Container components = new Container();
        NotifyIcon notifyIcon;
        StartupManager startupManager = new StartupManager(Title);

        Context context;
        Thread? contextThread;
        bool running = true;

        [DllImport("sas.dll")]
        static extern void SendSAS(bool asUser);

        public Controller()
        {
            Instance.RunOnce(TitleWithVersion, "Global\\SteamController");

            SendSAS(true);

            context = new Context()
            {
                Profiles = {
                    new Profiles.SteamShortcutsProfile(),
                    new Profiles.DesktopProfile(),
                    new Profiles.X360Profile(),
                    new Profiles.DebugProfile()
                }
            };

            var contextMenu = new ContextMenuStrip(components);

            if (startupManager.IsAvailable)
            {
                var startupItem = new ToolStripMenuItem("Run On Startup");
                startupItem.Checked = startupManager.Startup;
                startupItem.Click += delegate
                {
                    startupManager.Startup = !startupManager.Startup;
                    startupItem.Checked = startupManager.Startup;
                };
                contextMenu.Items.Add(startupItem);
            }

            var helpItem = contextMenu.Items.Add("&Help");
            helpItem.Click += delegate
            {
                System.Diagnostics.Process.Start("explorer.exe", "http://github.com/ayufan-research/steam-deck-tools");
            };

            contextMenu.Items.Add(new ToolStripSeparator());

            var exitItem = contextMenu.Items.Add("&Exit");
            exitItem.Click += delegate
            {
                Application.Exit();
            };

            notifyIcon = new NotifyIcon(components);
            notifyIcon.Icon = Resources.microsoft_xbox_controller_off;
            notifyIcon.Text = TitleWithVersion;
            notifyIcon.Visible = true;
            notifyIcon.ContextMenuStrip = contextMenu;

            var contextStateUpdate = new System.Windows.Forms.Timer(components);
            contextStateUpdate.Interval = 250;
            contextStateUpdate.Enabled = true;
            contextStateUpdate.Tick += ContextStateUpdate_Tick;

            contextThread = new Thread(ContextState_Update);
            contextThread.Start();
        }

        private void ContextState_Update(object? obj)
        {
            while (running)
            {
                context.Update();
            }
        }

        private void ContextStateUpdate_Tick(object? sender, EventArgs e)
        {
            context.X360.CreateClient();

            if (!context.Mouse.Valid)
            {
                notifyIcon.Text = TitleWithVersion + ". Cannot send input";
                notifyIcon.Icon = Resources.microsoft_xbox_controller_off_red;
            }
            else if (!context.X360.Valid)
            {
                notifyIcon.Text = TitleWithVersion + ". Missing ViGEm?";
                notifyIcon.Icon = Resources.microsoft_xbox_controller_red;
            }
            else if (context.DesktopMode)
            {
                notifyIcon.Icon = Resources.microsoft_xbox_controller_off;
                notifyIcon.Text = TitleWithVersion + ". Desktop mode";
            }
            else
            {
                notifyIcon.Icon = Resources.microsoft_xbox_controller;
                notifyIcon.Text = TitleWithVersion;
            }
        }

        public void Dispose()
        {
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
