## 0.5.x

- Introduce SteamController that provides 3 main modes of operation Desktop, X360 and Steam
- Fix `FanControl` broken context menu
- Fix incorrect `CurrentProfile` of `SteamController`
- Fix right stick serving as mouse in `X360` mode
- Improve build scripts in `scripts/`

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
