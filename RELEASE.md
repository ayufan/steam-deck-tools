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

## 0.4.x

- Highly risky: Allow to change CPU and GPU frequency (enable `EnableExperimentalFeatures` in `PowerControl.dll.config`)
- Show CPU/GPU frequency in Full overlay
- Allow to control GPU Scaling and Display Color Correction
- Do not use WinRing0 for GPU detection to control CPU/GPU frequency
- Reset `LibreHardware` on system resume to fix battery bug
- Reset FPS limit if anything related to resolution changes
- Retry Vangogh GPU detection 3 times
- Add `Radeon Image Sharpening` option (Yes, or No)
- Fix FanControl window not hidden on startup
- Performance Overlay defaults are changed to `Shift+F11` (Toggle OSD) and `Alt+Shift+F11` (Switch OSD)

## 0.3.x

- Adds Power Control
- Improve flickering of OSD
- Add Volume Up/Down controls (disable with `EnableVolumeControl` in `PowerControl.dll.config`)
- Add FPSWithBattery overlay
- Add FPS Limit using RTSS
- Press `3 dots + L4 + R4 + L5 + R5` to reset (TDP, Refresh Rate, FPS limit) to default
- Allow to disable SMT (second threads of each physical cores)

If you found it useful buy me [Ko-fi](https://ko-fi.com/ayufan).
