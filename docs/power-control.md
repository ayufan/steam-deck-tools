# Power Control

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

## 1. Use it

It will only work in OSD mode when rendering graphics.
The notification setting is always available.

- SteamDeck Controller: press and hold Quick Access (3 dots), and then DPad Left, Rigth, Up, Down.
- Keyboard: `Ctrl+Win+Numpad2` (Down), `Ctrl+Win+Numpad4` (Left), `Ctrl+Win+Numpad6` (Right), `Ctrl+Win+Numpad8` (Up)

Additional shortcuts:

- Control Volume: use Volume Up and Down
- Control Brightness: press and hold Quick Access (3 dots), and then Volume Up and Down
- Press `3 dots + L4 + R4 + L5 + R5` to reset (TDP, Refresh Rate, FPS limit) to default
- Press `3 dots + L1 + R1` to reset current resolution (allows to fix black screen)

## 2. SWICD configuration

> This work seemlessly with [Steam Controller](steam-controller.md).

- Set 3 dots to **NONE** in button mapping

  <img src="images/power_control_swicd_1.png" height="150"/>
