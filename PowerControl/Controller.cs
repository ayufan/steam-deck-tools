using CommonHelpers;
using ExternalHelpers;
using Microsoft.VisualBasic.Logging;
using Microsoft.Win32;
using PowerControl.Helpers;
using PowerControl.External;
using RTSSSharedMemoryNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace PowerControl
{
    internal class Controller : IDisposable
    {
        public const String Title = "Power Control";
        public static readonly String TitleWithVersion = Title + " v" + Application.ProductVersion.ToString();
        public const int KeyPressRepeatTime = 400;
        public const int KeyPressNextRepeatTime = 90;

        Container components = new Container();
        System.Windows.Forms.NotifyIcon notifyIcon;
        StartupManager startupManager = new StartupManager(Title);

        Menu.MenuRoot rootMenu = MenuStack.Root;
        OSD? osd;
        System.Windows.Forms.Timer osdDismissTimer;

        hidapi.HidDevice neptuneDevice = new hidapi.HidDevice(0x28de, 0x1205, 64);
        SDCInput neptuneDeviceState = new SDCInput();
        DateTime? neptuneDeviceNextKey;
        System.Windows.Forms.Timer? neptuneTimer;

        System.Windows.Forms.Timer gameProfileTimer;

        SharedData<PowerControlSetting> sharedData = SharedData<PowerControlSetting>.CreateNew();

        static Controller()
        {
            Dependencies.ValidateHidapi(TitleWithVersion);
            Dependencies.ValidateRTSSSharedMemoryNET(TitleWithVersion);
        }

        public Controller()
        {
            Instance.OnUninstall(() =>
            {
                startupManager.Startup = false;
            });

            Instance.RunOnce(TitleWithVersion, "Global\\PowerControl");
            Instance.RunUpdater(TitleWithVersion);

            if (Instance.WantsRunOnStartup)
                startupManager.Startup = true;

            var contextMenu = new System.Windows.Forms.ContextMenuStrip(components);

            contextMenu.Opening += delegate (object? sender, CancelEventArgs e)
            {
                rootMenu.Update();
            };

            rootMenu.Visible = false;
            rootMenu.Update();
            rootMenu.CreateMenu(contextMenu.Items);
            rootMenu.VisibleChanged = delegate ()
            {
                updateOSD();
            };
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

            var checkForUpdatesItem = contextMenu.Items.Add("&Check for Updates");
            checkForUpdatesItem.Click += delegate { Instance.RunUpdater(TitleWithVersion, true); };

            var helpItem = contextMenu.Items.Add("&Help");
            helpItem.Click += delegate { System.Diagnostics.Process.Start("explorer.exe", "https://steam-deck-tools.ayufan.dev"); };
            contextMenu.Items.Add(new ToolStripSeparator());

            var exitItem = contextMenu.Items.Add("&Exit");
            exitItem.Click += ExitItem_Click;

            notifyIcon = new System.Windows.Forms.NotifyIcon(components);
            notifyIcon.Icon = WindowsDarkMode.IsDarkModeEnabled ? Resources.traffic_light_outline_light : Resources.traffic_light_outline;
            notifyIcon.Text = TitleWithVersion;
            notifyIcon.Visible = true;
            notifyIcon.ContextMenuStrip = contextMenu;

            // Fix for context menu location
            contextMenu.Show();
            contextMenu.Close();

            osdDismissTimer = new System.Windows.Forms.Timer(components);
            osdDismissTimer.Interval = 3000;
            osdDismissTimer.Tick += delegate (object? sender, EventArgs e)
            {
                hideOSD();
            };

            setProfile(GameProfilesController.CurrentProfile);

            gameProfileTimer = new System.Windows.Forms.Timer(components);
            gameProfileTimer.Interval = 1500;
            gameProfileTimer.Tick += delegate (object? sender, EventArgs e)
            {
                gameProfileTimer.Stop();

                if (GameProfilesController.UpdateGameProfile())
                {
                    setProfile(GameProfilesController.CurrentProfile);
                }

                gameProfileTimer.Start();
            };
            gameProfileTimer.Start();

            var osdTimer = new System.Windows.Forms.Timer(components);
            osdTimer.Tick += OsdTimer_Tick;
            osdTimer.Interval = 250;
            osdTimer.Enabled = true;

            GlobalHotKey.RegisterHotKey(Settings.Default.MenuUpKey, () =>
            {
                if (!RTSS.IsOSDForeground())
                    return;
                rootMenu.Prev();
                setDismissTimer();
                dismissNeptuneInput();
            }, true);

            GlobalHotKey.RegisterHotKey(Settings.Default.MenuDownKey, () =>
            {
                if (!RTSS.IsOSDForeground())
                    return;
                rootMenu.Next();
                setDismissTimer();
                dismissNeptuneInput();
            }, true);

            GlobalHotKey.RegisterHotKey(Settings.Default.MenuLeftKey, () =>
            {
                if (!RTSS.IsOSDForeground())
                    return;
                rootMenu.SelectPrev();
                setDismissTimer();
                dismissNeptuneInput();
            });

            GlobalHotKey.RegisterHotKey(Settings.Default.MenuRightKey, () =>
            {
                if (!RTSS.IsOSDForeground())
                    return;
                rootMenu.SelectNext();
                setDismissTimer();
                dismissNeptuneInput();
            });

            GlobalHotKey.RegisterHotKey(Settings.Default.MenuToggle, () =>
            {
                if (!RTSS.IsOSDForeground())
                    return;
                
                if (rootMenu.Visible)
                {
                    hideOSD();
                    osdDismissTimer.Interval = 3000;
                } else
                {
                    showOSD(false);
                    osdDismissTimer.Interval = 15000;
                }
            }, true);

            if (Settings.Default.EnableNeptuneController)
            {
                neptuneTimer = new System.Windows.Forms.Timer(components);
                neptuneTimer.Interval = 1000 / 60;
                neptuneTimer.Tick += NeptuneTimer_Tick;
                neptuneTimer.Enabled = true;

                neptuneDevice.OnInputReceived += NeptuneDevice_OnInputReceived;
                neptuneDevice.OpenDevice();
                neptuneDevice.BeginRead();
            }

            if (Settings.Default.EnableVolumeControls)
            {
                GlobalHotKey.RegisterHotKey("VolumeUp", () =>
                {
                    if (neptuneDeviceState.buttons5.HasFlag(SDCButton5.BTN_QUICK_ACCESS))
                        rootMenu.SelectNext("Brightness");
                    else
                        rootMenu.SelectNext("Volume");
                    setDismissTimer();
                    dismissNeptuneInput();
                });

                GlobalHotKey.RegisterHotKey("VolumeDown", () =>
                {
                    if (neptuneDeviceState.buttons5.HasFlag(SDCButton5.BTN_QUICK_ACCESS))
                        rootMenu.SelectPrev("Brightness");
                    else
                        rootMenu.SelectPrev("Volume");
                    setDismissTimer();
                    dismissNeptuneInput();
                });
            }
        }

        private void OsdTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                notifyIcon.Text = TitleWithVersion + ". RTSS Version: " + OSD.Version;
                notifyIcon.Icon = WindowsDarkMode.IsDarkModeEnabled ? Resources.traffic_light_outline_light : Resources.traffic_light_outline;
            }
            catch
            {
                notifyIcon.Text = TitleWithVersion + ". RTSS Not Available.";
                notifyIcon.Icon = Resources.traffic_light_outline_red;
            }

            updateOSD();
        }

        private void NeptuneDevice_OnInputReceived(object? sender, hidapi.HidDeviceInputReceivedEventArgs e)
        {
            var input = SDCInput.FromBuffer(e.Buffer);

            var filteredInput = new SDCInput()
            {
                buttons0 = input.buttons0,
                buttons1 = input.buttons1,
                buttons2 = input.buttons2,
                buttons3 = input.buttons3,
                buttons4 = input.buttons4,
                buttons5 = input.buttons5
            };

            if (!neptuneDeviceState.Equals(filteredInput))
            {
                neptuneDeviceState = filteredInput;
                neptuneDeviceNextKey = null;
            }

            // Consume only some events to avoid under-running SWICD
            if (!neptuneDeviceState.buttons5.HasFlag(SDCButton5.BTN_QUICK_ACCESS))
                Thread.Sleep(50);
        }

        private void dismissNeptuneInput()
        {
            neptuneDeviceNextKey = DateTime.UtcNow.AddDays(1);
        }

        private void NeptuneTimer_Tick(object? sender, EventArgs e)
        {
            var input = neptuneDeviceState;

            if (neptuneDeviceNextKey == null)
                neptuneDeviceNextKey = DateTime.UtcNow.AddMilliseconds(KeyPressRepeatTime);
            else if (neptuneDeviceNextKey < DateTime.UtcNow)
                neptuneDeviceNextKey = DateTime.UtcNow.AddMilliseconds(KeyPressNextRepeatTime);
            else
                return; // otherwise it did not yet trigger

            // Reset sequence: 3 dots + L4|R4|L5|R5
            if (input.buttons0 == SDCButton0.BTN_L5 &&
                input.buttons1 == SDCButton1.BTN_R5 &&
                input.buttons2 == 0 &&
                input.buttons3 == 0 &&
                input.buttons4 == (SDCButton4.BTN_L4 | SDCButton4.BTN_R4) &&
                input.buttons5 == SDCButton5.BTN_QUICK_ACCESS)
            {
                rootMenu.Show();
                rootMenu.Reset();
                notifyIcon.ShowBalloonTip(3000, TitleWithVersion, "Settings were reset to default.", ToolTipIcon.Info);
                return;
            }

            if (!neptuneDeviceState.buttons5.HasFlag(SDCButton5.BTN_QUICK_ACCESS) || !RTSS.IsOSDForeground())
            {
                // schedule next repeat far in the future
                dismissNeptuneInput();
                hideOSD();
                return;
            }

            rootMenu.Show();
            setDismissTimer(false);

            if (input.buttons1 != 0 || input.buttons2 != 0 || input.buttons3 != 0 || input.buttons4 != 0)
            {
                return;
            }
            else if (input.buttons0 == SDCButton0.BTN_DPAD_LEFT)
            {
                rootMenu.SelectPrev();
            }
            else if (input.buttons0 == SDCButton0.BTN_DPAD_RIGHT)
            {
                rootMenu.SelectNext();
            }
            else if (input.buttons0 == SDCButton0.BTN_DPAD_UP)
            {
                rootMenu.Prev();
            }
            else if (input.buttons0 == SDCButton0.BTN_DPAD_DOWN)
            {
                rootMenu.Next();
            }
        }

        private void setDismissTimer(bool enabled = true)
        {
            osdDismissTimer.Stop();
            if (enabled)
                osdDismissTimer.Start();
        }

        private void hideOSD()
        {
            if (!rootMenu.Visible)
                return;

            Trace.WriteLine("Hide OSD");
            rootMenu.Visible = false;
            osdDismissTimer.Stop();
            updateOSD();
        }

        private void showOSD(bool enableTimer = true)
        {
            if (rootMenu.Visible)
                return;

            Trace.WriteLine("Show OSD");
            rootMenu.Visible = true;
            setDismissTimer(enableTimer);
            updateOSD();
        }

        public void updateOSD()
        {
            sharedData.SetValue(new PowerControlSetting()
            {
                Current = rootMenu.Visible ? PowerControlVisible.Yes : PowerControlVisible.No
            });

            if (!rootMenu.Visible)
            {
                osdClose();
                return;
            }

            try
            {
                // recreate OSD if index 0
                if (OSDHelpers.OSDIndex("Power Control") == 0 && OSD.GetOSDCount() > 1)
                    osdClose();
                if (osd == null)
                {
                    osd = new OSD("Power Control");
                    Trace.WriteLine("Show OSD");
                }
                osd.Update(rootMenu.Render(null));
            }
            catch (SystemException)
            {
            }
        }

        private void ExitItem_Click(object? sender, EventArgs e)
        {
            Application.Exit();
        }

        public void Dispose()
        {
            components.Dispose();
            osdClose();
        }

        private void osdClose()
        {
            try
            {
                if (osd != null)
                {
                    osd.Dispose();
                    Trace.WriteLine("Close OSD");
                }
                osd = null;
            }
            catch (SystemException)
            {
            }
        }

        private void setProfile(GameProfile profile)
        {
            if (GameProfilesController.CurrentGame != GameProfile.DefaultName)
            {
                // Fixes refresh rate reset for Dragon Age Inquisition
                // Probably should have a list of games with strange behaviour
                Thread.Sleep(7200);
            }

            var profileCopy = GameProfile.Copy(profile);

            rootMenu.SelectValueByKey(GameOptions.RefreshRate, profileCopy.refreshRate);
            Thread.Sleep(1000);
            rootMenu.SelectValueByKey(GameOptions.Fps, profileCopy.fps);
        }
    }
}
