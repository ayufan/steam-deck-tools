using PerformanceOverlay.External;
using RTSSSharedMemoryNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceOverlay
{
    internal class Controller : IDisposable
    {
        Container components = new Container();
        RTSSSharedMemoryNET.OSD? osd;
        ToolStripMenuItem showItem;
        Sensors sensors = new Sensors();

        LibreHardwareMonitor.Hardware.Computer libreHardwareComputer = new LibreHardwareMonitor.Hardware.Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsStorageEnabled = true,
            IsBatteryEnabled = true
        };

        public Controller()
        {
            var contextMenu = new System.Windows.Forms.ContextMenuStrip(components);

            showItem = new ToolStripMenuItem("&Show OSD");
            showItem.Click += ShowItem_Click;
            showItem.Checked = Settings.Default.ShowOSD;
            contextMenu.Items.Add(showItem);
            contextMenu.Items.Add(new ToolStripSeparator());
            foreach (var mode in Enum.GetValues<Overlays.Mode>())
            {
                var modeItem = new ToolStripMenuItem(mode.ToString());
                modeItem.Tag = mode;
                modeItem.Click += delegate
                {
                    Settings.Default.OSDModeParsed = mode;
                    updateContextItems(contextMenu);
                };
                contextMenu.Items.Add(modeItem);
            }
            updateContextItems(contextMenu);

            contextMenu.Items.Add(new ToolStripSeparator());

            var exitItem = contextMenu.Items.Add("&Exit");
            exitItem.Click += ExitItem_Click;

            var notifyIcon = new System.Windows.Forms.NotifyIcon(components);
            notifyIcon.Icon = Resources.traffic_light_outline1;
            notifyIcon.Text = "Steam Deck Fan Control";
            notifyIcon.Visible = true;
            notifyIcon.ContextMenuStrip = contextMenu;
            notifyIcon.Click += NotifyIcon_Click;

            var osdTimer = new System.Windows.Forms.Timer(components);
            osdTimer.Tick += OsdTimer_Tick;
            osdTimer.Interval = 250;
            osdTimer.Enabled = true;

            GlobalHotKey.RegisterHotKey("F11", () =>
            {
                showItem.Checked = !showItem.Checked;
            });

            // Select next overlay
            GlobalHotKey.RegisterHotKey("Shift+F11", () =>
            {
                var values = Enum.GetValues<Overlays.Mode>().ToList();

                int index = values.IndexOf(Settings.Default.OSDModeParsed);
                Settings.Default.OSDModeParsed = values[(index + 1) % values.Count];

                showItem.Checked = true;
                updateContextItems(contextMenu);
            });
        }

        private void updateContextItems(ContextMenuStrip contextMenu)
        {
            foreach (ToolStripItem item in contextMenu.Items)
            {
                if (item.Tag is Overlays.Mode)
                   ((ToolStripMenuItem)item).Checked = ((Overlays.Mode)item.Tag == Settings.Default.OSDModeParsed);
            }
        }

        private void NotifyIcon_Click(object? sender, EventArgs e)
        {
            ((NotifyIcon)sender).ContextMenuStrip.Show(Control.MousePosition);
        }

        private void ShowItem_Click(object? sender, EventArgs e)
        {
            showItem.Checked = !showItem.Checked;
        }

        private void OsdTimer_Tick(object? sender, EventArgs e)
        {
            if (!showItem.Checked)
            {
                using (osd) { }
                osd = null;
                return;
            }

            sensors.Update();

            var osdOverlay = Overlays.GetOSD(Settings.Default.OSDModeParsed, sensors);

            try
            {
                if (osd == null)
                    osd = new OSD("PerformanceOverlay");
                osd.Update(osdOverlay);
            }
            catch(SystemException)
            {
                osd = null;
            }
        }

        private void ExitItem_Click(object? sender, EventArgs e)
        {
            Application.Exit();
        }

        public void Dispose()
        {
            components.Dispose();
            using (osd) { }
            using (sensors) { }
        }
    }
}
