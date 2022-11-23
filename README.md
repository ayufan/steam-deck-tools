# Steam Deck Tools

This repository contains my own personal set of tools to help running Windows on Steam Deck.

**This software is provided on best-effort basis and can break your SteamDeck.**
To learn more go to [Risks](#4-risks).

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

- **F11** - enable performance overlay
- **Shift+F11** - cycle to next performance overlay (and enable it)

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

## 4. Risks

**This software is provided on best-effort basis and can break your SteamDeck.** It does a direct manipulation
of kernel memory to control usage the EC (Embedded Controller) and setting desired fan RPM via VLV0100

The Tools are written trying to check if you are truly using SteamDeck and compatible system.
The applications try it's best to start only on **validated configurations** to avoid breaking your system.

It was build based on knowledge gained in Steam Deck kernel patch and DSDT presented by bios.
The memory addresses used are hardcoded and can be changed any moment by the Bios update.

Fortunately quite amount of people are using it with a success and no problems observed.
However, you should be aware of the consequences.

### 4.1. Risk of CPU and GPU frequency change

The APU manipulation of CPU and GPU frequency uses a ported implementation from Linux kernel.
It is more unstable than Fan Control (since the Fan Control is controller managing EC on Windows).
The change of the CPU and GPU frequency switch might sometimes crash system.
The AMD Display Driver periodically requests GPU metrics and in some rare circumstances the change request
might lock the GPU driver. If something like this happens it is advised to
power shutdown the device. Disconnect from charging, and hold power button for a few seconds.
Power device normally afterwards. This is rather unlikely to break the device if done right
after when such event occurs.

## 5. Anti-Cheat and Antivirus software

Since this project uses direct manipulation of kernel memory via `inpoutx64.dll`
it might trigger Antivirus and Anti-Cheat software.

You might consider disabling all tools if this happens. Unfortunatelly there's no worakound for that
unless someone gets access to driver signing certificate for Windows.

## Author

Kamil Trzciński, 2022

## License

[Creative Commons Attribution-NonCommercial (CC-BY-NC)](://creativecommons.org/licenses/by-nc/4.0/).

Free for personal use. Contact me in other cases (`ayufan@ayufan.eu`).
