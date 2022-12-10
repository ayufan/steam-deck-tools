using CommonHelpers;
using ExternalHelpers;

namespace FanControl
{
    public partial class FanControlForm : Form
    {
        private FanController fanControl;
        private StartupManager startupManager = new StartupManager(
            "Steam Deck Fan Control",
            "Starts Steam Deck Fan Control on Windows startup."
        );

        private SharedData<FanModeSetting> sharedData = SharedData<FanModeSetting>.CreateNew();

        public FanControlForm()
        {
            InitializeComponent();

            Text += " v" + Application.ProductVersion.ToString();
            Instance.Open(Text, true, "Global\\FanControlOnce");

            fanControl = new FanController();
            SharedData_Update();

            notifyIcon.Text = Text;
            notifyIcon.Visible = true;

            TopMost = Settings.Default.AlwaysOnTop;
            toolStripMenuItemAlwaysOnTop.Checked = TopMost;
            toolStripMenuItemAlwaysOnTopContext.Checked = TopMost;

            toolStripMenuItemStartupOnBoot.Visible = startupManager.IsAvailable;
            toolStripMenuItemStartupOnBoot.Checked = startupManager.Startup;
            toolStripMenuItemStartupOnBootContext.Visible = startupManager.IsAvailable;
            toolStripMenuItemStartupOnBootContext.Checked = startupManager.Startup;

            foreach (var item in Enum.GetValues(typeof(FanMode)))
            {
                var menuItem = new ToolStripMenuItem(item.ToString()) { Tag = item };
                menuItem.Click += FanMode_Click;
                int insertIndex = contextMenu.Items.IndexOf(toolStripSeparatorEndOfModes);
                contextMenu.Items.Insert(insertIndex, menuItem);

                fanModeSelectMenu.Items.Add(item);
            }

            setFanMode(Settings.Default.FanMode);

            propertyGrid1.SelectedObject = fanControl;
            propertyGrid1.ExpandAllGridItems();

            notifyIcon.ShowBalloonTip(3000, Text, "Fan Control Started", ToolTipIcon.Info);

            Microsoft.Win32.SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;

            Opacity = 0;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Visible = false;
            Opacity = 100;
        }

        private void SystemEvents_PowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
        {
            // Restore fan mode on resume
            if (e.Mode == Microsoft.Win32.PowerModes.Resume)
            {
                Instance.HardwareComputer.Reset();
                fanControl.SetMode(fanControl.Mode);
            }
        }

        private void setFanMode(FanMode mode)
        {
            fanControl.SetMode(mode);
            Settings.Default.FanMode = mode;

            foreach (ToolStripItem menuItem in contextMenu.Items)
            {
                if (menuItem is ToolStripMenuItem && menuItem.Tag is FanMode)
                    ((ToolStripMenuItem)menuItem).Checked = ((FanMode)menuItem.Tag == mode);
            }

            fanModeSelectMenu.SelectedItem = mode;
        }

        private void FanMode_Click(object? sender, EventArgs e)
        {
            var menuItem = (ToolStripMenuItem)sender;
            setFanMode((FanMode)menuItem.Tag);
        }

        private void fanModeSelectMenu_SelectedIndexChanged(object sender, EventArgs e)
        {
            var menuItem = (ToolStripComboBox)sender;
            setFanMode((FanMode)menuItem.SelectedItem);
        }

        private void FanControlForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && Visible)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void formShow_Event(object sender, EventArgs e)
        {
            Show();
            Activate();
            propertyGrid1.Refresh();
        }

        private void formClose_Event(object sender, EventArgs e)
        {
            Hide();
            Close();
        }

        private void FanControlForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Always revert to default on closing
            fanControl.SetMode(FanMode.Default);
        }

        private void SharedData_Update()
        {
            if (sharedData.GetValue(out var value) && Enum.IsDefined<FanMode>(value.Desired))
            {
                setFanMode((FanMode)value.Desired);
            }

            sharedData.SetValue(new FanModeSetting()
            {
                Current = fanControl.Mode,
                KernelDriversLoaded = Instance.UseKernelDrivers ? KernelDriversLoaded.Yes : KernelDriversLoaded.No
            });
        }

        private void fanLoopTimer_Tick(object sender, EventArgs e)
        {
            SharedData_Update();
            fanControl.Update(Visible);
        }

        private void propertyGridUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (!Visible)
                return;

            propertyGrid1.Refresh();
            sensorWarningLabel.Visible = fanControl.IsAnyInvalid();

            if (fanControl.IsActive)
                notifyIcon.Text = String.Format("Fan: {0} RPM Mode: {1}", fanControl.CurrentRPM, fanControl.Mode);
            else
                notifyIcon.Text = String.Format("Mode: {0}", fanControl.Mode);
        }

        private void toolStripMenuItemStartupOnBoot_Click(object sender, EventArgs e)
        {
            startupManager.Startup = !startupManager.Startup;
            toolStripMenuItemStartupOnBoot.Checked = startupManager.Startup;
            toolStripMenuItemStartupOnBootContext.Checked = startupManager.Startup;
        }

        private void help_DoubleClick(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://steam-deck-tools.ayufan.dev");
        }

        private void toolStripMenuItemAlwaysOnTop_Click(object sender, EventArgs e)
        {
            TopMost = !TopMost;
            toolStripMenuItemAlwaysOnTop.Checked = TopMost;
            toolStripMenuItemAlwaysOnTopContext.Checked = TopMost;
            Settings.Default.AlwaysOnTop = toolStripMenuItemAlwaysOnTop.Checked;
        }
    }
}
