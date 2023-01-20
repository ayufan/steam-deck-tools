**This project is provided free of charge, but development of it is not free - it takes a lot of effort**:

- Consider donating to keep this project alive.
- Donating also helps to fund new features.

<a href='https://ko-fi.com/ayufan' target='_blank'><img height='35' style='border:0px;height:50px;' src='https://az743702.vo.msecnd.net/cdn/kofi3.png?v=0' alt='Buy Me a Coffee at ko-fi.com' /></a> <a href="https://www.paypal.com/donate/?hosted_button_id=DHNBE2YR9D5Y2" target='_blank'><img height='35' src="https://raw.githubusercontent.com/stefan-niedermann/paypal-donate-button/master/paypal-donate-button.png" alt="Donate with PayPal" style='border:0px;height:55px;'/></a>

[**READ IF PLAYING ONLINE GAMES AND/OR GAMES THAT HAVE ANTI-CHEAT ENABLED**](https://steam-deck-tools.ayufan.dev/#anti-cheat-and-antivirus-software)

## 0.6.x

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

## 0.5.x

- Try to disable usage of Kernel Drivers (when FAN in Default, and OSD Kernel Drivers are disabled)
  to allow apps to work with Anti-Cheat detections
- The X360 has Haptics enabled by default
- Allow to assign BackPanel keys to X360 controller (breaks all current configs to set mappings)
- Use `white` icons when using `Dark Theme` (thanks @maniman303 https://github.com/ayufan/steam-deck-tools/pull/23)
- Validate that all dependencies are installed
- Allow `Updater.exe` to disable automatic updates - this is selectable option via `setup.exe`
- Use `Sentry.io` for error tracking for all builds
- Make `Updater.exe` to be able to update from `.zip` to `setup.exe`
- Bug fixing to handle all known exceptions
- Require to acknowledge when using function that might trigger `Anti-Cheat` protection via top-most window
- GPU detection will log errors to `Sentry.io`
- Support SMU of Vangogh GPU shipped with BIOS 113
- Fix Steam Game detection when in X360 controller mode
- Force to reconnect X360 controller on Resume (deadlock)
- Hold-press Guide button for 100ms in X360 mode
