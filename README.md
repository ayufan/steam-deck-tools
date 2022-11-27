# Steam Deck Tools

This repository contains my own personal set of tools to help running Windows on Steam Deck.

**This software is provided on best-effort basis and can break your SteamDeck.**
To learn more go to [Risks](#4-risks).

### If you found it useful buy me [Ko-fi](https://ko-fi.com/ayufan).

<a href='https://ko-fi.com/ayufan' target='_blank'><img height='35' style='border:0px;height:46px;' src='https://az743702.vo.msecnd.net/cdn/kofi3.png?v=0' alt='Buy Me a Coffee at ko-fi.com' />

## 1. Steam Deck Fan Control

This is a very early and highly experimental fan controller for Windows build based
on `linux/jupiter-fan-control.py` available on SteamOS.

<img src="images/fan_control.png" height="400"/>

### 1.1. Usage

You can download latest precompiled build from Releases tab. Currently the application requires
administrative privileges in order to read various temperature sensors.

It provides 3 modes of operation:

1. **Default** - use EC to control fan (as done today)
1. **SteamOS** - use reimplemented `jupiter-fan-control.py` (as in SteamOS when Fan Control is enabled)
1. **Max** - blow at full speed.

### 1.2. How it works?

See [Risks](#4-risks).

### 1.3. Limitations

As of Today (Nov, 2022) the GPU temperature is missing on Windows. Which makes it a little incomplete.
However, it should be expected that GPU temperature is "kind of similar" to CPU due to GPU being in the same
silicon. But! The device might overheat and break due to this missing temperature. **So, use at your own risk.**

### 1.4. Supported devices

The application tries it best to not harm device (just in case). So, it validates bios version.
Those are currently supported:

- F7A0107 (PD ver: 0xB030)
- F7A0110 (PD ver: 0xB030)

## 2. Performance Overlay

This is a very simple application that requires [Rivatuner Statistics Server Download](https://www.guru3d.com/files-details/rtss-rivatuner-statistics-server-download.html))
and provides asthetics of SteamOS Performance Overlay.

Uninstall MSI Afterburner and any other OSD software.

It currently registers two global hotkeys:

- **Shift+F11** - enable performance overlay
- **Alt+Shift+F11** - cycle to next performance overlay (and enable it)

There are 5 modes of presentation:

### 2.1. FPS

<img src="images/perf_overlay_fps.png" width="600"/>

### 2.2. FPS with Battery

<img src="images/perf_overlay_fpsbat.png" width="600"/>

### 2.3. Minimal

<img src="images/perf_overlay_min.png" width="600"/>

### 2.4. Detail

<img src="images/perf_overlay_detail.png" width="600"/>

### 2.5. Full

<img src="images/perf_overlay_full.png" height="100"/>

## 3. Power Control

This is a very simple application that requires [Rivatuner Statistics Server Download](https://www.guru3d.com/files-details/rtss-rivatuner-statistics-server-download.html)
and provides an easily accessible controls.

Uninstall MSI Afterburner and any other OSD software.

There are currently configurable settings:

- Volume
- Brightness
- Refresh Rate
- Resolution (requires enabling `EnableExperimentalFeatures` in `appconfig`)
- FPS Limit (requires: RTSS > Setup > Enable Framelimit)
- TDP
- SMT (Each core of AMD has 2 threads, this allows to enable/disable second threads)
- OSD / OSDMode (requires PerformanceOverlay running)
- Fan (requires FanControl running)

<img src="images/power_control.png" height="250"/>

### 3.1. Use it

It will only work in OSD mode when rendering graphics.
The notification setting is always available.

- SteamDeck Controller: press and hold Quick Access (3 dots), and then DPad Left, Rigth, Up, Down.
- Keyboard: `Ctrl+Win+Numpad2` (Down), `Ctrl+Win+Numpad4` (Left), `Ctrl+Win+Numpad6` (Right), `Ctrl+Win+Numpad8` (Up)

Additional shortcuts:

- Control Volume: use Volume Up and Down
- Control Brightness: press and hold Quick Access (3 dots), and then Volume Up and Down
- Press `3 dots + L4 + R4 + L5 + R5` to reset (TDP, Refresh Rate, FPS limit) to default

### 3.2. SWICD configuration

- Set 3 dots to **NONE** in button mapping

    <img src="images/power_control_swicd_1.png" height="150"/>

## 4. Steam Controller

This is highly experimental "opinionated" implementation of Steam Controller that is meant
to replace [SWICD](https://github.com/mKenfenheuer/steam-deck-windows-usermode-driver/)
and [Glossi](https://github.com/Alia5/GlosSI). This is hard piece and it is rather
unsupported (by me), so if it works it works.

It offers 3 main modes of operation:

- **Desktop**
- **X360 emulation** (optionally with Rumble) - activated automatically when entering Playnite Fullscreen
- **Steam** - active automatically when running Steam Gamepad UI, Steam Big Screen UI or running Steam Game

You can easily switch between Desktop and X360 by holding Options button (3 horizontal lines,
on top of the right joystick). When profile is switched you will hear a beep.

Requirements:

1. Ensure that you have SWICD and Glossi disabled, stopped or uninstalled!
1. Ensure that HidHide is disabled, or ensure that `Steam Controller.exe` can see `Neptune Valve Controller`.
1. Install latest version of [https://github.com/ViGEm/ViGEmBus/releases](https://github.com/ViGEm/ViGEmBus/releases).

### 4.1. Possible setups

The are various ways how `Steam Controller` can work with Steam.
If you do not use Steam this is simple, just run it and enjoy.
Since when there's no Steam, there's no problem at all.

#### 4.1.1. The ideal setup - run Steam when running game via Playnite

It is **preferred** to configure Steam as described in [Configure Steam](#43-configure-steam).

The perfect way to use it:

1. Keep Steam closed at all times
1. Use Playnite Fullscreen to start game of Steam

This makes the `Steam Controller` to automatically detect Steam running game
and disable itself for the duration of game play.

This allows to use `Steam Input` for Steam games.

Configure Playnite to close Steam after game session.

#### 4.1.2. Another great option - Always use X360 controller and completly disable Steam Input

It is **not needed** to configure Steam as described in [Configure Steam](#43-configure-steam).

In this mode you would use either Desktop or X360 controls, and all Steam games would use X360.
When running in this mode you would have to switch between Desktop and X360 mode with `Options` button
or with `PowerControl`.

Of course you will have access to all described shortcuts.

#### 4.1.3. Less ideal (quirky) - Run Steam in background

It is **required** to configure Steam as described in [Configure Steam](#43-configure-steam).

In general it is possible to run `Steam Controller` alongside running Steam in background,
but additional configuration of Steam needs to be done (disabling all Desktop configuration shortcuts in Steam).
This is sometimes fincky due to Steam always processing Steam Deck controller, but in general
should be fairly stable.

This makes the `Steam Controller` to automatically detect Steam running game
and disable itself for the duration of game play. This allows to use `Steam Input` for Steam games.

> Getting controllers support is hard especially with Steam not making it easy to disable it.
> I'm doing this to solve my usage pattern. So, I might have limited will to fix all quirks
> of handling Steam running in background alongside `Steam Controller`. This will never
> be supported mode of operation.

### 4.2. Mappings

| Button                     | Desktop                | X360 (with Rumble)     | Steam                  | Steam With Shortcuts   |
|----------------------------|------------------------|------------------------|------------------------|------------------------|
| X360 Controller            | Not connected          | Connected              | Not connected          | Not connected          |
| Options (hold for 1s)      | Switch to next profile | Switch to next profile | Switch to next profile | Switch to next profile |
| Options (hold for 3s)      | Toggle desktop mode    | Toggle desktop mode    |                        |                        |
| STEAM + 3 dots             | CTRL + SHIFT + ESCAPE  | CTRL + SHIFT + ESCAPE  | CTRL + SHIFT + ESCAPE  | CTRL + SHIFT + ESCAPE  |
| STEAM + Menu               | WIN + TAB              | WIN + TAB              | WIN + TAB              | WIN + TAB              |
| STEAM + Options            | F11                    | F11                    | F11                    | F11                    |
| STEAM + A                  | RETURN                 | RETURN                 |                        | RETURN                 |
| STEAM + B (hold for 1s)    | ALT + F4               | ALT + F4               |                        | ALT + F4               |
| STEAM + B (hold for 3s)    | Kill active process    | Kill active process    |                        | Kill active process    |
| STEAM + X                  | Toggle Keyboard        | Toggle Keyboard        |                        | Toggle Keyboard        |
| STEAM + L1                 | Toggle Magnify         | Toggle Magnify         |                        | Toggle Magnify         |
| STEAM + R1                 | Screenshot             | Screenshot             |                        | Screenshot             |
| STEAM + Left Joystick Up   | Increase Brightness    | Increase Brightness    |                        | Increase Brightness    |
| STEAM + Left Joystick Down | Decrease Brightness    | Decrease Brightness    |                        | Decrease Brightness    |
| STEAM + DPad Right         | RETURN                 | RETURN                 |                        | RETURN                 |
| STEAM + DPad Down          | TAB                    | TAB                    |                        | TAB                    |
| STEAM + DPad Left          | ESCAPE                 | ESCAPE                 |                        | ESCAPE                 |
| STEAM + DPad Up            | CTRL + ALT + U         | CTRL + ALT + U         |                        | CTRL + ALT + U         |
| STEAM + Left Pad           | Mouse Scroll           | Mouse Scroll           |                        | Mouse Scroll           |
| STEAM + Left Joystick      | Mouse Scroll           | Mouse Scroll           |                        | Mouse Scroll           |
| STEAM + Right Joystick     | Mouse Trackpad         | Mouse Trackpad         |                        | Mouse Trackpad         |
| STEAM + Right Pad          | Mouse Move             | Mouse Move             |                        | Mouse Move             |
| STEAM + L2                 | Mouse Right Click      | Mouse Right Click      |                        | Mouse Right Click      |
| STEAM + R2                 | Mouse Left Click       | Mouse Left Click       |                        | Mouse Left Click       |
| STEAM + Left Pad Press     | Mouse Right Click      | Mouse Right Click      |                        | Mouse Right Click      |
| STEAM + Right Pad Press    | Mouse Left Click       | Mouse Left Click       |                        | Mouse Left Click       |
| Left Pad                   | Mouse Scroll           |                        |                        |                        |
| Left Joystick              | Mouse Scroll           |                        |                        |                        |
| Right Joystick             | Mouse Trackpad         |                        |                        |                        |
| Right Pad                  | Mouse Move             |                        |                        |                        |
| DPad Arrows                | Keyboard Arrows        |                        |                        |                        |
| A                          | RETURN                 |                        |                        |                        |
| B                          | BACKSPACE              |                        |                        |                        |

### 4.3. Configure Steam

If Steam is running in background it is essential to remove Desktop mode configuration
to make `SteamController.exe` the one mapping those.

#### 4.3.1. Disable Desktop mode in Steam Gamepad UI (preferred)

1. Run `steam.exe` with `-gamepadui`.
1. Click `Steam` button, go to `Settings` > `Controller`.
1. Scroll down to find `Desktop Layout`, click `Edit` and `Edit Layout`.
1. Go to `Action Sets` > `Add Action Set`.
1. In `Add Action Set` type `Empty` and `Continue`.
1. Now click on `Default`, `Remove Set` and `Confirm`.
1. You can now exit with `B` (Back).

#### 4.3.2. Disable Desktop mode in Steam Desktop (slightly buggy on Steam Deck)

1. Run Steam.
1. Go to `Steam` > `Settings` > `Controller` > `Desktop Configuration`.
1. Click `Add Action Set`.
1. In new window type `Empty` and click `OK`.
1. At top select `Default`.
1. Now click `Menu` button (the button on top of left joystick with two squares)
   on Steam Deck (or `Manage Action Set` button).
1. In a new window click `DELETE`.
1. Now click `B` or `DONE`.

### 4.4. Fullscreen Scaling Hotkey (Steam + Up Pad Press)

Many games in Windows will crash when attempting to start in fullscreen.  To have a fullscreen experience on the Steam Deck, you can use this keybind in any fullscreen scaling application to provide a shortcut capable of toggling the scaling.

Example of Scaling software: https://store.steampowered.com/app/993090/Lossless_Scaling/

To configure the hotkey in Lossless Scaling:
1. Launch Lossless Scaling
1. Open "Settings"
1. Select the "Scale hotkey"
1. Press "STEAM + DPad Up"
1. Verify that the hotkey was set to "CTRL + ALT + U"
1. Close the settings dialog

## 5. Risks

**This software is provided on best-effort basis and can break your SteamDeck.** It does a direct manipulation
of kernel memory to control usage the EC (Embedded Controller) and setting desired fan RPM via VLV0100

The Tools are written trying to check if you are truly using SteamDeck and compatible system.
The applications try it's best to start only on **validated configurations** to avoid breaking your system.

It was build based on knowledge gained in Steam Deck kernel patch and DSDT presented by bios.
The memory addresses used are hardcoded and can be changed any moment by the Bios update.

Fortunately quite amount of people are using it with a success and no problems observed.
However, you should be aware of the consequences.

### 5.1. Risk of CPU and GPU frequency change

The APU manipulation of CPU and GPU frequency uses a ported implementation from Linux kernel.
It is more unstable than Fan Control (since the Fan Control is controller managing EC on Windows).
The change of the CPU and GPU frequency switch might sometimes crash system.
The AMD Display Driver periodically requests GPU metrics and in some rare circumstances the change request
might lock the GPU driver. If something like this happens it is advised to
power shutdown the device. Disconnect from charging, and hold power button for a few seconds.
Power device normally afterwards. This is rather unlikely to break the device if done right
after when such event occurs.

## 6. Anti-Cheat and Antivirus software

Since this project uses direct manipulation of kernel memory via `inpoutx64.dll`
it might trigger Antivirus and Anti-Cheat software.

If you use at least version `0.5.x` you can disable kernel drivers usage that should
avoid trippping Anti-Cheat detection. Set FAN to `Default` and disable `OSD Kernel Drivers`.
Of course you will miss some metrics due to that.

## 7. Development

It should be enough to run `scripts/build.bat` to see binaries in `build-release/`.

You might also run `scripts/test_debug.bat` which will build and run built version.

## Author

Kamil Trzciński, 2022

## License

[Creative Commons Attribution-NonCommercial (CC-BY-NC)](://creativecommons.org/licenses/by-nc/4.0/).

Free for personal use. Contact me in other cases (`ayufan@ayufan.eu`).
