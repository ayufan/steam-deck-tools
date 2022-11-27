<a href='https://ko-fi.com/ayufan' target='_blank'><img height='35' style='border:0px;height:46px;' src='https://az743702.vo.msecnd.net/cdn/kofi3.png?v=0' alt='Buy Me a Coffee at ko-fi.com' />

**If you found it useful buy me [Ko-fi](https://ko-fi.com/ayufan).**

It does help this project on being supported.

## 0.5.x

- Introduce SteamController that provides 3 main modes of operation Desktop, X360 and Steam
- Fix `FanControl` broken context menu
- Fix incorrect `CurrentProfile` of `SteamController`
- Fix right stick serving as mouse in `X360` mode
- Improve build scripts in `scripts/`
- Show notification on controller changed
- Try to disable usage of Kernel Drivers (when FAN in Default, and OSD Kernel Drivers are disabled)
  to allow apps to work with Anti-Cheat detections
- Hide `Use Lizard Mouse/Buttons` as it does something different than people are used to
- Fix `LT/RT` to trigger up to 50%, instead of 100%
- Add mapping for `STEAM+DPadUp`
- Usage of `KeyboardController` will now generate key repeats
- Configure Steam to switch between Steam Input or X360 Controller mode
- Steam Games detection also works for X360 Controller mode
- STEAM+B will kill foreground if hold longer than 3s
- Allow to configure `StartupProfile` in `SteamController.dll.config`
- Increase joystick speed and key repeats in Desktop Mode
- Fix double presses of A(RETURN)/B(BACKSPACE) in Desktop mode
- Fix detection of SAS to switch into full lizard
- STEAM + 3 dots brings Task Manager (CTRL+SHIFT+ESCAPE)
- Append `controller_blacklist` to `config.vdf` if missing
