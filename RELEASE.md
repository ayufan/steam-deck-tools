<a href='https://ko-fi.com/ayufan' target='_blank'><img height='35' style='border:0px;height:46px;' src='https://az743702.vo.msecnd.net/cdn/kofi3.png?v=0' alt='Buy Me a Coffee at ko-fi.com' />

**If you found it useful buy me [Ko-fi](https://ko-fi.com/ayufan).**

It does help this project on being supported.

## 0.5.x

- Introduce SteamController that provides 3 main modes of operation Desktop, X360 and Steam
- Try to disable usage of Kernel Drivers (when FAN in Default, and OSD Kernel Drivers are disabled)
  to allow apps to work with Anti-Cheat detections
- Add mapping for `STEAM+DPadUp`
- Configure Steam to switch between Steam Input or X360 Controller mode
- Steam Games detection also works for X360 Controller mode
- Allow to configure `StartupProfile` in `SteamController.dll.config`
- STEAM + 3 dots brings Task Manager (CTRL+SHIFT+ESCAPE)
- Add configurable BackPanel keys (allowed mappings are subject to change)
- Provide currated list of mapping keys in settings (might be extended in the future)
- Improve performance on critical loop (code is lock-less)
- Reduce hold for `Toggle desktop mode` to 2s
- Fix `Process Kill` action (STEAM+B for 3s)
- Go back to `Startup Profile` on `Toggle deskptop mode`
- The `X360.Beep()` cycles currently connected device (fixes Playnite error)
- Fix using Playnite to launch Steam game where on exit Desktop was activated
- Build DEBUG that has all experimental features
