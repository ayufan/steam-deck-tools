**This project is provided free of charge, but development of it is not free - it takes a lot of effort**:

- Consider donating to keep this project alive.
- Donating also helps to fund new features.

<a href='https://ko-fi.com/ayufan' target='_blank'><img height='35' style='border:0px;height:50px;' src='https://az743702.vo.msecnd.net/cdn/kofi3.png?v=0' alt='Buy Me a Coffee at ko-fi.com' /></a> <a href="https://www.paypal.com/donate/?hosted_button_id=DHNBE2YR9D5Y2" target='_blank'><img height='35' src="https://raw.githubusercontent.com/stefan-niedermann/paypal-donate-button/master/paypal-donate-button.png" alt="Donate with PayPal" style='border:0px;height:55px;'/></a>

## 0.5.x

- Introduce SteamController that provides 3 main modes of operation Desktop, X360 and Steam
- Try to disable usage of Kernel Drivers (when FAN in Default, and OSD Kernel Drivers are disabled)
  to allow apps to work with Anti-Cheat detections
- STEAM + 3 dots brings Task Manager (CTRL+SHIFT+ESCAPE)
- Add configurable BackPanel keys (allowed mappings are subject to change)
- Build DEBUG that has all experimental features
- The X360 has Haptics enabled by default
- Detect GamePad UI open temporarily for controller layout
- Automatically manage steam controller configs when using Steam Input
- Allow to assign BackPanel keys to X360 controller (breaks all current configs to set mappings)
- All SteamDeckTools settings are stored in `.ini` file in root folder
- Detect SAS (Secure Attention Sequence) in a way that does not prevent screen sleep
- Recreate X360 device on fatal failure (might happen after resume)
- DEBUG allows to keep X360 controller always connected
- Swap `STEAM+Menu` and `STEAM+Options`. It makes more sense to switch windows with STEAM+3 horizontal lines
- If application is run with `-run-on-startup` it will self-set to run on system start
- Depend on `GetCursorPos` to detect `SAS`
- Add `Updater.exe` that can update to latest release and debug
- Add `Setup.exe` installer to install all except RTSS
