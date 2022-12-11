# Windows Deck Tools for Steam Deck

This repository contains my own personal set of tools to help running Windows on Steam Deck.

**This software is provided on best-effort basis and can break your SteamDeck.**

<img src="docs/images/power_control.png" height="250"/>

## Help this project

**This project is provided free of charge, but development of it is not free - it takes a lot of effort**:

- Consider donating to keep this project alive.
- Donating also helps to fund new features.

<a href='https://ko-fi.com/ayufan' target='_blank'><img height='35' style='border:0px;height:50px;' src='https://az743702.vo.msecnd.net/cdn/kofi3.png?v=0' alt='Buy Me a Coffee at ko-fi.com' /></a> <a href="https://www.paypal.com/donate/?hosted_button_id=DHNBE2YR9D5Y2" target='_blank'><img height='35' src="https://raw.githubusercontent.com/stefan-niedermann/paypal-donate-button/master/paypal-donate-button.png" alt="Donate with PayPal" style='border:0px;height:55px;'/></a>

## Applications

This project provides the following applications:

- [Fan Control](docs/fan-control.md) - control Fan on Windows
- [Performance Overlay](docs/performance-overlay.md) - see FPS and other stats
- [Power Control](docs/power-control.md) - change TDP or refresh rate
- [Steam Controller](docs/steam-controller.md) - use Steam Deck with Game Pass

## Install

- Download latest `SteamDeckTools-<version>.zip` from [Latest GitHub Releases](https://github.com/ayufan/steam-deck-tools/releases/latest).
- Unpack to `C:\SteamDeckTools` folder.
- Install bellow dependencies.
- Run each application, and mark `Run On Startup`.

This project requires those dependencies to be installed in order to function properly:

- [Microsoft Visual C++ Redistributable](https://aka.ms/vs/17/release/vc_redist.x64.exe)
- [Rivatuner Statistics Server](https://www.guru3d.com/files-details/rtss-rivatuner-statistics-server-download.html)
- [ViGEmBus](https://github.com/ViGEm/ViGEmBus/releases)

It is strongly advised that following software is uninstalled or disabled:

- [SWICD](https://github.com/mKenfenheuer/steam-deck-windows-usermode-driver)
- [GlosSI](https://github.com/Alia5/GlosSI)
- [HidHide](https://github.com/ViGEm/HidHide)

## Additional informations

- [Controller Shortcuts](docs/shortcuts.md) - default shortcuts when using [Steam Controller](docs/steam-controller.md).
- [Development](docs/development.md) - how to compile this project.
- [Risks](docs/risks.md) - this project uses kernel manipulation and might result in unstable system.
- The latest development version can be found in [GitHub Releases](https://github.com/ayufan/steam-deck-tools/releases).
- The variant with `SteamDeckTools-<version>-debug.zip` enables all experimental features enabled, but might be unstable and buggy.

## Anti-Cheat and Antivirus software

Since this project uses direct manipulation of kernel memory via `inpoutx64.dll`
it might trigger Antivirus and Anti-Cheat software.

If you use at least version `0.5.x` you can disable kernel drivers usage that should
avoid trippping Anti-Cheat detection. Set FAN to `Default` and disable `OSD Kernel Drivers`.
Of course you will miss some metrics due to that.

## Author

Kamil Trzciński, 2022

Steam Deck Tools is not affiliated with Valve, Steam, or any of their partners.

## License

[Creative Commons Attribution-NonCommercial (CC-BY-NC)](https://creativecommons.org/licenses/by-nc/4.0/).

Free for personal use. Contact me in other cases (`ayufan@ayufan.eu`).
