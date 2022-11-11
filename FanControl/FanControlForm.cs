using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
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
        private FanControl fanControl = new FanControl();

        public FanControlForm()
        {
            InitializeComponent();

            propertyGrid1.SelectedObject = fanControl;

            foreach(var item in Enum.GetValues(typeof(FanControl.FanMode)))
            {
                fanModeSelectMenu.Items.Add(item);
                fanModeSelectNotifyMenu.Items.Add(item);
            }

            fanModeSelectMenu.SelectedIndex = 0;
            fanModeSelectNotifyMenu.SelectedIndex = 0;

            notifyIcon.ShowBalloonTip(3000, "Steam Deck Fan Control", "Fan Control Started", ToolTipIcon.Info);
        }

        private void fanModeSelect_SelectedValueChanged(object sender, EventArgs e)
        {
            var comboBox = (ToolStripComboBox)sender;
            var selectedMode = (FanControl.FanMode)comboBox.SelectedItem;
            fanControl.SetMode(selectedMode);
            fanModeSelectMenu.SelectedItem = selectedMode;
            fanModeSelectNotifyMenu.SelectedItem = selectedMode;
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
        }

        private void formClose_Event(object sender, EventArgs e)
        {
            Hide();
            Close();
        }

        private void FanControlForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Always revert to default on closing
            fanControl.SetMode(FanControl.FanMode.Default);
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            fanControl.Update();

            if (Visible)
            {
                propertyGrid1.Refresh();
            }

            notifyIcon.Text = String.Format("Fan: {0} RPM Mode: {1}", fanControl.CurrentRPM, fanControl.Mode);
        }
    }
}
