using CommonHelpers;
using CommonHelpers.FromLibreHardwareMonitor;
using Microsoft.VisualBasic.Logging;
using PerformanceOverlay.External;
using RTSSSharedMemoryNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace PerformanceOverlay
{
    internal class Controller : IDisposable
    {
        public const String Title = "Performance Overlay";
        public readonly String TitleWithVersion = Title + " v" + System.Windows.Forms.Application.ProductVersion.ToString();

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

        public Controller()
        {
            contextMenu = new System.Windows.Forms.ContextMenuStrip(components);

            Instance.Open(TitleWithVersion, "Global\\PerformanceOverlay");

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
                    Settings.Default.OSDModeParsed = mode;
                    updateContextItems(contextMenu);
                };
                contextMenu.Items.Add(modeItem);
            }
            updateContextItems(contextMenu);

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

            var helpItem = contextMenu.Items.Add("&Help");
            helpItem.Click += delegate
            {
                System.Diagnostics.Process.Start("explorer.exe", "http://github.com/ayufan-research/steam-deck-tools");
            };

            contextMenu.Items.Add(new ToolStripSeparator());

            var exitItem = contextMenu.Items.Add("&Exit");
            exitItem.Click += ExitItem_Click;

            notifyIcon = new System.Windows.Forms.NotifyIcon(components);
            notifyIcon.Icon = Resources.traffic_light_outline;
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
                    Settings.Default.Save();

                    updateContextItems(contextMenu);
                });
            }

            if (Settings.Default.CycleOSDShortcut != "")
            {
                GlobalHotKey.RegisterHotKey(Settings.Default.CycleOSDShortcut, () =>
                {
                    var values = Enum.GetValues<OverlayMode>().ToList();

                    int index = values.IndexOf(Settings.Default.OSDModeParsed);
                    Settings.Default.OSDModeParsed = values[(index + 1) % values.Count];
                    Settings.Default.ShowOSD = true;
                    Settings.Default.Save();

                    updateContextItems(contextMenu);
                });
            }
        }

        private void updateContextItems(ContextMenuStrip contextMenu)
        {
            foreach (ToolStripItem item in contextMenu.Items)
            {
                if (item.Tag is OverlayMode)
                   ((ToolStripMenuItem)item).Checked = ((OverlayMode)item.Tag == Settings.Default.OSDModeParsed);
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
            Settings.Default.Save();
            updateContextItems(contextMenu);
        }

        private void SharedData_Update()
        {
            if (sharedData.GetValue(out var value))
            {
                if (Enum.IsDefined<OverlayMode>(value.Desired))
                {
                    Settings.Default.OSDModeParsed = (OverlayMode)value.Desired;
                    Settings.Default.ShowOSD = true;
                    Settings.Default.Save();
                    updateContextItems(contextMenu);
                }

                if (Enum.IsDefined<OverlayEnabled>(value.DesiredEnabled))
                {
                    Settings.Default.ShowOSD = (OverlayEnabled)value.DesiredEnabled == OverlayEnabled.Yes;
                    Settings.Default.Save();
                    updateContextItems(contextMenu);
                }
            }

            sharedData.SetValue(new OverlayModeSetting()
            {
                Current = Settings.Default.OSDModeParsed,
                CurrentEnabled = Settings.Default.ShowOSD ? OverlayEnabled.Yes : OverlayEnabled.No
            });
        }

        private void OsdTimer_Tick(object? sender, EventArgs e)
        {
            SharedData_Update();

            try
            {
                notifyIcon.Text = TitleWithVersion + ". RTSS Version: " + OSD.Version;
                notifyIcon.Icon = Resources.traffic_light_outline;
            }
            catch
            {
                notifyIcon.Text = TitleWithVersion + ". RTSS Not Available.";
                notifyIcon.Icon = Resources.traffic_light_outline_red;
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

            var osdOverlay = Overlays.GetOSD(Settings.Default.OSDModeParsed, sensors);

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
            catch(SystemException)
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
