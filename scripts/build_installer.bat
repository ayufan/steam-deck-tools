cd "%~dp0"
powershell -File "build.ps1" Debug
"C:\Program Files (x86)\nsis\Bin\makensis.exe" /V3 /DUSE_WINGET /DBUILD_DIR=../build-Debug SteamDeckTools_Setup.nsi
