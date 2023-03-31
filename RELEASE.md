**This project is provided free of charge, but development of it is not free - it takes a lot of effort**:

- Consider donating to keep this project alive.
- Donating also helps to fund new features.

<a href='https://ko-fi.com/ayufan' target='_blank'><img height='35' style='border:0px;height:50px;' src='https://az743702.vo.msecnd.net/cdn/kofi3.png?v=0' alt='Buy Me a Coffee at ko-fi.com' /></a> <a href="https://www.paypal.com/donate/?hosted_button_id=DHNBE2YR9D5Y2" target='_blank'><img height='35' src="https://raw.githubusercontent.com/stefan-niedermann/paypal-donate-button/master/paypal-donate-button.png" alt="Donate with PayPal" style='border:0px;height:55px;'/></a>

[**READ IF PLAYING ONLINE GAMES AND/OR GAMES THAT HAVE ANTI-CHEAT ENABLED**](https://steam-deck-tools.ayufan.dev/#anti-cheat-and-antivirus-software)

## 0.6.x

- PowerControl: Support SMU of Vangogh GPU shipped with BIOS 115
- SteamController: Add `DS4` support (with Gyro, Accel, Trackpads and Haptics)
- SteamController: Move `KeepX360AlwaysConnected` to `Settings`
- PowerControl: Install custom resolutions (EDID) (experimental feature)
- All: Show `Missing RTSS` button to install RTSS
- PowerControl: Retain FPS Limit (proportion) on refresh rate change
- PowerControl: Support RTSS in custom folder
- SteamController: Fix Steam Big Picture detection for non-english
- PowerControl: Allow user to configure selectable TDP, CPU and GPU from `PowerControl.dll.ini`
- SteamController: Promote RTSS detection to Release - enable by default
- SteamController: Improve detection of Steam processes (especially latest controller UI changes)
- SteamController: Add configuration wizard for the first time or when configuration was lost
- PowerControl: Show current time
- PowerControl: Consider the foreground process to be holding profile configuration as long as it is running
- SteamController: Require administrator privileges
- PowerControl: Apply profile changes with a delay in bulk
- SteamController: Fix detection of the Steam client released around 2023-01-20, version: 1674182294
- All: Improve Anti-Cheat protection to allow to dismiss it
- SteamController: Fix `STEAM+DPadUp` not working
- PowerControl/SteamController: Improve RTSS detection to ignore processes not generating frames for over 5s
- PowerControl: Expose all in `GameProfiles`, and fix `GPU Scaling`, `Refresh Rate` and `FPS Limit` interwork
- PowerControl: Add `GameProfiles` allowing to persist per-game setttings for most of settings (Colors, Refresh Rate, FPS limit, etc.) [Thank you @maniman303 for https://github.com/ayufan/steam-deck-tools/pull/38]
- SteamController: Add experimental `ControllerProfiles` to allow creating user controllers
- SteamController: Add experimental RTSS-based detection (disables the need to use Steam, or Playnite workflow)
- SteamController: Hold Press Left and Right Pad to toggle touchpads in X360 mode
- PowerControl: Make all PowerControl options to accept Strings
