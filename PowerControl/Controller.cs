using CommonHelpers;
using ExternalHelpers;
using hidapi;
using Microsoft.Win32;
using PowerControl.External;
using PowerControl.Helpers;
using RTSSSharedMemoryNET;
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
        OSD osd;
        System.Windows.Forms.Timer osdDismissTimer;
        bool isOSDToggled = false;

        bool wasInternalDisplayConnected;

        hidapi.HidDevice neptuneDevice = new hidapi.HidDevice(0x28de, 0x1205, 64);
        SDCInput neptuneDeviceState = new SDCInput();
        DateTime? neptuneDeviceNextKey;
        System.Windows.Forms.Timer neptuneTimer;

        ProfilesController? profilesController;

        SharedData<PowerControlSetting> sharedData = SharedData<PowerControlSetting>.CreateNew();

        static Controller()
        {
            //Dependencies.ValidateHidapi(TitleWithVersion);
            //Dependencies.ValidateRTSSSharedMemoryNET(TitleWithVersion);
        }

        public Controller()
        {
            Instance.OnUninstall(() =>
            {
                startupManager.Startup = false;
            });

            Log.CleanupLogFiles(DateTime.UtcNow.AddDays(-7));
            Log.LogToFile = true;
            Log.LogToFileDebug = true;

            Instance.RunOnce(TitleWithVersion, "Global\\PowerControl");
            Instance.RunUpdater(TitleWithVersion);

            if (Instance.WantsRunOnStartup)
                startupManager.Startup = true;

            var contextMenu = new System.Windows.Forms.ContextMenuStrip(components);

            var notRunningRTSSItem = contextMenu.Items.Add("&RTSS is not running");
            notRunningRTSSItem.Enabled = false;
            contextMenu.Opening += delegate { notRunningRTSSItem.Visible = Dependencies.EnsureRTSS(null) && !OSDHelpers.IsLoaded; };

            rootMenu.Init();
            rootMenu.Visible = false;
            rootMenu.Update();
            rootMenu.CreateMenu(contextMenu);
            rootMenu.VisibleChanged += delegate { updateOSD(); };
            contextMenu.Items.Add(new ToolStripSeparator());

            if (Settings.Default.EnableExperimentalFeatures)
            {
                var installEDIDItem = contextMenu.Items.Add("Install &Resolutions");
                installEDIDItem.Click += delegate { Helpers.AMD.EDID.SetEDID(Resources.CRU_SteamDeck); };
                var replaceEDIDItem = contextMenu.Items.Add("Replace &Resolutions");
                replaceEDIDItem.Click += delegate { Helpers.AMD.EDID.SetEDID(new byte[0]); Helpers.AMD.EDID.SetEDID(Resources.CRU_SteamDeck); };
                var uninstallEDIDItem = contextMenu.Items.Add("Revert &Resolutions");
                uninstallEDIDItem.Click += delegate { Helpers.AMD.EDID.SetEDID(new byte[0]); };
                contextMenu.Opening += delegate
                {
                    if (ExternalHelpers.DisplayConfig.IsInternalConnected == true)
                    {
                        var edid = Helpers.AMD.EDID.GetEDID() ?? new byte[0];
                        var edidInstalled = Resources.CRU_SteamDeck.SequenceEqual(edid);
                        installEDIDItem.Visible = edid.Length <= 128;
                        replaceEDIDItem.Visible = !edidInstalled && edid.Length > 128;
                        uninstallEDIDItem.Visible = edid.Length > 128;
                    }
                    else
                    {
                        installEDIDItem.Visible = false;
                        replaceEDIDItem.Visible = false;
                        uninstallEDIDItem.Visible = false;
                    }
                };
                contextMenu.Items.Add(new ToolStripSeparator());
            }

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

            var showGameProfilesItem = contextMenu.Items.Add("Show Game &Profiles");
            showGameProfilesItem.Click += delegate { Dependencies.OpenLink(Helper.ProfileSettings.UserProfilesPath); };
            contextMenu.Items.Add(new ToolStripSeparator());

            var checkForUpdatesItem = contextMenu.Items.Add("&Check for Updates");
            checkForUpdatesItem.Click += delegate { Instance.RunUpdater(TitleWithVersion, true); };

            var helpItem = contextMenu.Items.Add("&Help");
            helpItem.Click += delegate { Dependencies.OpenLink(Dependencies.SDTURL); };
            contextMenu.Items.Add(new ToolStripSeparator());

            var exitItem = contextMenu.Items.Add("&Exit");
            exitItem.Click += ExitItem_Click;

            notifyIcon = new System.Windows.Forms.NotifyIcon(components);
            notifyIcon.Icon = WindowsDarkMode.IsDarkModeEnabled ? Resources.traffic_light_outline_light : Resources.traffic_light_outline;
            notifyIcon.Text = TitleWithVersion;
            notifyIcon.Visible = true;
            notifyIcon.ContextMenuStrip = contextMenu;

            // Fix first time context menu position
            contextMenu.Show();
            contextMenu.Close();

            osdDismissTimer = new System.Windows.Forms.Timer(components);
            osdDismissTimer.Interval = 3000;
            osdDismissTimer.Tick += delegate (object? sender, EventArgs e)
            {
                if (!isOSDToggled)
                {
                    hideOSD();
                }
            };

            var osdTimer = new System.Windows.Forms.Timer(components);
            osdTimer.Tick += OsdTimer_Tick;
            osdTimer.Interval = 250;
            osdTimer.Enabled = true;

            profilesController = new ProfilesController();

            GlobalHotKey.RegisterHotKey(Settings.Default.MenuUpKey, () =>
            {
                if (!OSDHelpers.IsOSDForeground())
                    return;
                rootMenu.Next(-1);
                setDismissTimer();
                dismissNeptuneInput();
            }, true);

            GlobalHotKey.RegisterHotKey(Settings.Default.MenuDownKey, () =>
            {
                if (!OSDHelpers.IsOSDForeground())
                    return;
                rootMenu.Next(1);
                setDismissTimer();
                dismissNeptuneInput();
            }, true);

            GlobalHotKey.RegisterHotKey(Settings.Default.MenuLeftKey, () =>
            {
                if (!OSDHelpers.IsOSDForeground())
                    return;
                rootMenu.SelectNext(-1);
                setDismissTimer();
                dismissNeptuneInput();
            });

            GlobalHotKey.RegisterHotKey(Settings.Default.MenuRightKey, () =>
            {
                if (!OSDHelpers.IsOSDForeground())
                    return;
                rootMenu.SelectNext(1);
                setDismissTimer();
                dismissNeptuneInput();
            });

            GlobalHotKey.RegisterHotKey(Settings.Default.MenuToggle, () =>
            {
                isOSDToggled = !rootMenu.Visible;

                if (!OSDHelpers.IsOSDForeground())
                    return;

                if (isOSDToggled)
                {
                    showOSD();
                }
                else
                {
                    hideOSD();
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
                        rootMenu.Select("Brightness");
                    else
                        rootMenu.Select("Volume");
                    rootMenu.SelectNext(1);
                    setDismissTimer();
                    dismissNeptuneInput();
                });

                GlobalHotKey.RegisterHotKey("VolumeDown", () =>
                {
                    if (neptuneDeviceState.buttons5.HasFlag(SDCButton5.BTN_QUICK_ACCESS))
                        rootMenu.Select("Brightness");
                    else
                        rootMenu.Select("Volume");
                    rootMenu.SelectNext(-1);
                    setDismissTimer();
                    dismissNeptuneInput();
                });
            }

            wasInternalDisplayConnected = ExternalHelpers.DisplayConfig.IsInternalConnected.GetValueOrDefault(false);
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
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

            var watchedProfiles = profilesController?.WatchedProfiles ?? new string[0];
            if (watchedProfiles.Any())
                notifyIcon.Text += ". Profile: " + string.Join(", ", watchedProfiles);

            updateOSD();
        }

        private Task NeptuneDevice_OnInputReceived(hidapi.HidDeviceInputReceivedEventArgs e)
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

            return new Task(() => { });
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

            if (!neptuneDeviceState.buttons5.HasFlag(SDCButton5.BTN_QUICK_ACCESS) || !OSDHelpers.IsOSDForeground())
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
                rootMenu.SelectNext(-1);
            }
            else if (input.buttons0 == SDCButton0.BTN_DPAD_RIGHT)
            {
                rootMenu.SelectNext(1);
            }
            else if (input.buttons0 == SDCButton0.BTN_DPAD_UP)
            {
                rootMenu.Next(-1);
            }
            else if (input.buttons0 == SDCButton0.BTN_DPAD_DOWN)
            {
                rootMenu.Next(1);
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

        private void showOSD()
        {
            if (rootMenu.Visible)
                return;

            Trace.WriteLine("Show OSD");
            rootMenu.Update();
            rootMenu.Visible = true;
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
            using (profilesController) { }
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

        private void SystemEvents_DisplaySettingsChanged(object? sender, EventArgs e)
        {
            var isInternalDisplayConnected = ExternalHelpers.DisplayConfig.IsInternalConnected.GetValueOrDefault(false);
            if (wasInternalDisplayConnected == isInternalDisplayConnected)
                return;

            Log.TraceLine("SystemEvents_DisplaySettingsChanged: wasConnected={0}, isConnected={1}",
                wasInternalDisplayConnected, isInternalDisplayConnected);

            wasInternalDisplayConnected = isInternalDisplayConnected;
            System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(
                new Action(() =>
                {
                    Options.RefreshRate.Instance?.Reset();
                    Options.FPSLimit.Instance?.Reset();

                    rootMenu.Update();
                })
            );
        }
    }
}
