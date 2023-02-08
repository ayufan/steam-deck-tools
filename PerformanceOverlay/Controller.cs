using CommonHelpers;
using ExternalHelpers;
using RTSSSharedMemoryNET;
using System.ComponentModel;

namespace PerformanceOverlay
{
    internal class Controller : IDisposable
    {
        public const String Title = "Performance Overlay";
        public static readonly String TitleWithVersion = Title + " v" + System.Windows.Forms.Application.ProductVersion.ToString();

        Container components = new Container();
        RTSSSharedMemoryNET.OSD? osd;
        System.Windows.Forms.ContextMenuStrip contextMenu;
        ToolStripMenuItem showItem;
        System.Windows.Forms.NotifyIcon notifyIcon;
        System.Windows.Forms.Timer osdTimer;
        Sensors sensors = new Sensors();
        StartupManager startupManager = new StartupManager(
            Title,
            "Starts Performance Overlay on Windows startup."
        );

        SharedData<OverlayModeSetting> sharedData = SharedData<OverlayModeSetting>.CreateNew();

        static Controller()
        {
            Dependencies.ValidateRTSSSharedMemoryNET(TitleWithVersion);
        }

        public Controller()
        {
            Instance.OnUninstall(() =>
            {
                startupManager.Startup = false;
            });

            contextMenu = new System.Windows.Forms.ContextMenuStrip(components);

            SharedData_Update();
            Instance.Open(TitleWithVersion, Settings.Default.EnableKernelDrivers, "Global\\PerformanceOverlay");
            Instance.RunUpdater(TitleWithVersion);

            if (Instance.WantsRunOnStartup)
                startupManager.Startup = true;

            var notRunningRTSSItem = contextMenu.Items.Add("&RTSS is not running");
            notRunningRTSSItem.Enabled = false;
            contextMenu.Opening += delegate { notRunningRTSSItem.Visible = Dependencies.EnsureRTSS(null) && !OSDHelpers.IsLoaded; };

            showItem = new ToolStripMenuItem("&Show OSD");
            showItem.Click += ShowItem_Click;
            showItem.Checked = Settings.Default.ShowOSD;
            contextMenu.Items.Add(showItem);
            contextMenu.Items.Add(new ToolStripSeparator());
            foreach (var mode in Enum.GetValues<OverlayMode>())
            {
                var modeItem = new ToolStripMenuItem(mode.ToString());
                modeItem.Tag = mode;
                modeItem.Click += delegate
                {
                    Settings.Default.OSDMode = mode;
                    updateContextItems(contextMenu);
                };
                contextMenu.Items.Add(modeItem);
            }
            updateContextItems(contextMenu);

            contextMenu.Items.Add(new ToolStripSeparator());

            var kernelDriversItem = new ToolStripMenuItem("Use &Kernel Drivers");
            kernelDriversItem.Click += delegate { setKernelDrivers(!Instance.UseKernelDrivers); };
            contextMenu.Opening += delegate { kernelDriversItem.Checked = Instance.UseKernelDrivers; };
            contextMenu.Items.Add(kernelDriversItem);

            contextMenu.Items.Add(new ToolStripSeparator());

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

            var missingRTSSItem = contextMenu.Items.Add("&Install missing RTSS");
            missingRTSSItem.Click += delegate { Dependencies.OpenLink(Dependencies.RTSSURL); };
            contextMenu.Opening += delegate { missingRTSSItem.Visible = !Dependencies.EnsureRTSS(null); };

            var checkForUpdatesItem = contextMenu.Items.Add("&Check for Updates");
            checkForUpdatesItem.Click += delegate { Instance.RunUpdater(TitleWithVersion, true); };

            var helpItem = contextMenu.Items.Add("&Help");
            helpItem.Click += delegate { Dependencies.OpenLink(Dependencies.SDTURL); };

            contextMenu.Items.Add(new ToolStripSeparator());

            var exitItem = contextMenu.Items.Add("&Exit");
            exitItem.Click += ExitItem_Click;

            notifyIcon = new System.Windows.Forms.NotifyIcon(components);
            notifyIcon.Icon = WindowsDarkMode.IsDarkModeEnabled ? Resources.poll_light : Resources.poll;
            notifyIcon.Text = TitleWithVersion;
            notifyIcon.Visible = true;
            notifyIcon.ContextMenuStrip = contextMenu;

            osdTimer = new System.Windows.Forms.Timer(components);
            osdTimer.Tick += OsdTimer_Tick;
            osdTimer.Interval = 250;
            osdTimer.Enabled = true;

            if (Settings.Default.ShowOSDShortcut != "")
            {
                GlobalHotKey.RegisterHotKey(Settings.Default.ShowOSDShortcut, () =>
                {
                    Settings.Default.ShowOSD = !Settings.Default.ShowOSD;

                    updateContextItems(contextMenu);
                });
            }

            if (Settings.Default.CycleOSDShortcut != "")
            {
                GlobalHotKey.RegisterHotKey(Settings.Default.CycleOSDShortcut, () =>
                {
                    var values = Enum.GetValues<OverlayMode>().ToList();

                    int index = values.IndexOf(Settings.Default.OSDMode);
                    Settings.Default.OSDMode = values[(index + 1) % values.Count];
                    Settings.Default.ShowOSD = true;

                    updateContextItems(contextMenu);
                });
            }

            Microsoft.Win32.SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        }

        private void SystemEvents_PowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
        {
            if (e.Mode == Microsoft.Win32.PowerModes.Resume)
            {
                Instance.HardwareComputer.Reset();
            }
        }

        private void updateContextItems(ContextMenuStrip contextMenu)
        {
            foreach (ToolStripItem item in contextMenu.Items)
            {
                if (item.Tag is OverlayMode)
                    ((ToolStripMenuItem)item).Checked = ((OverlayMode)item.Tag == Settings.Default.OSDMode);
            }

            showItem.Checked = Settings.Default.ShowOSD;
        }

        private void NotifyIcon_Click(object? sender, EventArgs e)
        {
            ((NotifyIcon)sender).ContextMenuStrip.Show(Control.MousePosition);
        }

        private void ShowItem_Click(object? sender, EventArgs e)
        {
            Settings.Default.ShowOSD = !Settings.Default.ShowOSD;
            updateContextItems(contextMenu);
        }

        private bool AckAntiCheat()
        {
            return AntiCheatSettings.Default.AckAntiCheat(
                TitleWithVersion,
                "Usage of OSD Kernel Drivers might trigger anti-cheat protection in some games.",
                "Ensure that you set it to DISABLED when playing games with ANTI-CHEAT PROTECTION."
            );
        }

        private void setKernelDrivers(bool value)
        {
            if (value && AckAntiCheat())
            {
                Instance.UseKernelDrivers = true;
                Settings.Default.EnableKernelDrivers = true;
            }
            else
            {
                Instance.UseKernelDrivers = false;
                Settings.Default.EnableKernelDrivers = false;
            }
        }

        private void SharedData_Update()
        {
            if (sharedData.GetValue(out var value))
            {
                if (Enum.IsDefined<OverlayMode>(value.Desired))
                {
                    Settings.Default.OSDMode = (OverlayMode)value.Desired;
                    Settings.Default.ShowOSD = true;
                    updateContextItems(contextMenu);
                }

                if (Enum.IsDefined<OverlayEnabled>(value.DesiredEnabled))
                {
                    Settings.Default.ShowOSD = (OverlayEnabled)value.DesiredEnabled == OverlayEnabled.Yes;
                    updateContextItems(contextMenu);
                }

                if (Enum.IsDefined<KernelDriversLoaded>(value.DesiredKernelDriversLoaded))
                {
                    setKernelDrivers((KernelDriversLoaded)value.DesiredKernelDriversLoaded == KernelDriversLoaded.Yes);
                    updateContextItems(contextMenu);
                }
            }

            sharedData.SetValue(new OverlayModeSetting()
            {
                Current = Settings.Default.OSDMode,
                CurrentEnabled = Settings.Default.ShowOSD ? OverlayEnabled.Yes : OverlayEnabled.No,
                KernelDriversLoaded = Instance.UseKernelDrivers ? KernelDriversLoaded.Yes : KernelDriversLoaded.No
            });
        }

        private void OsdTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                osdTimer.Enabled = false;
                SharedData_Update();
            }
            finally
            {
                osdTimer.Enabled = true;
            }

            try
            {
                notifyIcon.Text = TitleWithVersion + ". RTSS Version: " + OSD.Version;
                notifyIcon.Icon = WindowsDarkMode.IsDarkModeEnabled ? Resources.poll_light : Resources.poll;
            }
            catch
            {
                notifyIcon.Text = TitleWithVersion + ". RTSS Not Available.";
                notifyIcon.Icon = Resources.poll_red;
                osdReset();
                return;
            }

            if (!Settings.Default.ShowOSD)
            {
                osdTimer.Interval = 1000;
                osdReset();
                return;
            }

            osdTimer.Interval = 250;

            sensors.Update();

            var osdMode = Settings.Default.OSDMode;

            // If Power Control is visible use temporarily full OSD
            if (Settings.Default.EnableFullOnPowerControl)
            {
                if (SharedData<PowerControlSetting>.GetExistingValue(out var value) && value.Current == PowerControlVisible.Yes)
                    osdMode = OverlayMode.Full;
            }

            var osdOverlay = Overlays.GetOSD(osdMode, sensors);

            try
            {
                // recreate OSD if not index 0
                if (OSDHelpers.OSDIndex("PerformanceOverlay") != 0)
                    osdClose();
                if (osd == null)
                    osd = new OSD("PerformanceOverlay");

                uint offset = 0;
                osdEmbedGraph(ref offset, ref osdOverlay, "[OBJ_FT_SMALL]", -8, -1, 1, 0, 50000.0f, EMBEDDED_OBJECT_GRAPH.FLAG_FRAMETIME);
                osdEmbedGraph(ref offset, ref osdOverlay, "[OBJ_FT_LARGE]", -32, -2, 1, 0, 50000.0f, EMBEDDED_OBJECT_GRAPH.FLAG_FRAMETIME);

                osd.Update(osdOverlay);
            }
            catch (SystemException)
            {
            }
        }

        private void osdReset()
        {
            try
            {
                if (osd != null)
                    osd.Update("");
            }
            catch (SystemException)
            {
            }
        }

        private void osdClose()
        {
            try
            {
                if (osd != null)
                    osd.Dispose();
                osd = null;
            }
            catch (SystemException)
            {
            }
        }

        private uint osdEmbedGraph(ref uint offset, ref String osdOverlay, String name, int dwWidth, int dwHeight, int dwMargin, float fltMin, float fltMax, EMBEDDED_OBJECT_GRAPH dwFlags)
        {
            uint size = osd.EmbedGraph(offset, new float[0], 0, dwWidth, dwHeight, dwMargin, fltMin, fltMax, dwFlags);
            if (size > 0)
                osdOverlay = osdOverlay.Replace(name, "<OBJ=" + offset.ToString("X") + ">");
            offset += size;
            return size;
        }

        private void ExitItem_Click(object? sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        public void Dispose()
        {
            components.Dispose();
            osdClose();
            using (sensors) { }
        }
    }
}
