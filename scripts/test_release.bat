if not "%1"=="am_admin" (powershell start -verb runas '%0' am_admin & exit /b)

:retry
cd "%~dp0"

set configuration=Release

taskkill /F /IM FanControl.exe
taskkill /F /IM PerformanceOverlay.exe
taskkill /F /IM PowerControl.exe
taskkill /F /IM SteamController.exe

powershell -ExecutionPolicy UnRestricted -File "%~dp0\build.ps1" "%configuration%"

start ..\build-%configuration%\FanControl.exe
start ..\build-%configuration%\PerformanceOverlay.exe
start ..\build-%configuration%\PowerControl.exe
start ..\build-%configuration%\SteamController.exe

timeout /t 5
