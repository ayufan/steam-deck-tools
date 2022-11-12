using FanControl.FromLibreHardwareMonitor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FanControl
{
    public partial class FanControlForm : Form
    {
        private FanController fanControl = new FanController();
        private StartupManager startupManager = new StartupManager();

        public FanControlForm()
        {
            InitializeComponent();

            Text += " v" + Application.ProductVersion.ToString();
            notifyIcon.Text = Text;

            toolStripMenuItemStartupOnBoot.Visible = startupManager.IsAvailable;
            toolStripMenuItemStartupOnBoot.Checked = startupManager.Startup;

            propertyGrid1.SelectedObject = fanControl;
            propertyGrid1.ExpandAllGridItems();

            foreach (var item in Enum.GetValues(typeof(FanController.FanMode)))
            {
                fanModeSelectMenu.Items.Add(item);
                fanModeSelectNotifyMenu.Items.Add(item);
            }

            try
            {
                var fanMode = Enum.Parse(typeof(FanController.FanMode), Properties.Settings.Default.FanMode);
                setFanMode((FanController.FanMode)fanMode);
            }
            catch(System.ArgumentException)
            {
                setFanMode(FanController.FanMode.Default);
            }

            notifyIcon.ShowBalloonTip(3000, Text, "Fan Control Started", ToolTipIcon.Info);
        }

        private void setFanMode(FanController.FanMode mode)
        {
            fanControl.SetMode(mode);
            fanModeSelectMenu.SelectedItem = mode;
            fanModeSelectNotifyMenu.SelectedItem = mode;
            Properties.Settings.Default["FanMode"] = mode.ToString();
            Properties.Settings.Default.Save();
        }

        private void fanModeSelect_SelectedValueChanged(object sender, EventArgs e)
        {
            var comboBox = (ToolStripComboBox)sender;
            var selectedMode = (FanController.FanMode)comboBox.SelectedItem;
            setFanMode(selectedMode);
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
            WindowState = FormWindowState.Normal;
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
            fanControl.SetMode(FanController.FanMode.Default);
        }

        private void fanLoopTimer_Tick(object sender, EventArgs e)
        {
            fanControl.Update();
        }

        private void propertyGridUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (!Visible)
                return;

            var item = propertyGrid1.SelectedGridItem;
            propertyGrid1.Refresh();
            propertyGrid1.SelectedGridItem = item;
            sensorWarningLabel.Visible = fanControl.IsAnyInvalid();
            notifyIcon.Text = String.Format("Fan: {0} RPM Mode: {1}", fanControl.CurrentRPM, fanControl.Mode);
        }

        private void toolStripMenuItemStartupOnBoot_Click(object sender, EventArgs e)
        {
            startupManager.Startup = !startupManager.Startup;
            toolStripMenuItemStartupOnBoot.Checked = startupManager.Startup;
        }

        private void help_DoubleClick(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "http://github.com/ayufan-research/SteamDeckTools");
        }
    }
}
