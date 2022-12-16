!pragma warning error all

!define /ifndef VERSION "0.0.1"
!define /ifndef BUILD_DIR "../build-Debug"
!define /ifndef OUTPUT_FILE "../SteamDeckTools_Setup_${VERSION}.exe"

;--------------------------------
;Include Modern UI

  !include "MUI2.nsh"

;--------------------------------
;General

  ;Properly display all languages (Installer will not work on Windows 95, 98 or ME!)
  Unicode true

  ;Name and file
  Name "Windows Deck Tools for Steam Deck"
  OutFile "${OUTPUT_FILE}"

  ;Default installation folder
  InstallDir "$PROGRAMFILES64\SteamDeckTools"
  
  ;Get installation folder from registry if available
  InstallDirRegKey HKCU "Software\SteamDeckTools" ""

  ;Request application privileges for Windows Vista
  RequestExecutionLevel admin

  ; Version
  VIProductVersion "${VERSION}.0"
  VIFileVersion "${VERSION}.0"
  VIAddVersionKey "FileVersion" "${VERSION}"
  VIAddVersionKey "LegalCopyright" "(C) Kamil Trzciński 2022."
  VIAddVersionKey "FileDescription" "Windows Deck Tools for Steam Deck"

;--------------------------------
;Interface Settings

  !define MUI_ABORTWARNING

  ;Show all languages, despite user's codepage
  !define MUI_LANGDLL_ALLLANGUAGES

;--------------------------------
;Interface Configuration

  !define MUI_HEADERIMAGE
  #!define MUI_HEADERIMAGE_BITMAP "logo.bmp" ; optional


;--------------------------------
;Language Selection Dialog Settings

  ;Remember the installer language
  !define MUI_LANGDLL_REGISTRY_ROOT "HKCU" 
  !define MUI_LANGDLL_REGISTRY_KEY "Software\Modern UI Test" 
  !define MUI_LANGDLL_REGISTRY_VALUENAME "Installer Language"

;--------------------------------
;Pages

  !insertmacro MUI_PAGE_WELCOME
  !insertmacro MUI_PAGE_LICENSE "..\License.md"
  !insertmacro MUI_PAGE_COMPONENTS
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  !insertmacro MUI_PAGE_FINISH
  
  !insertmacro MUI_UNPAGE_WELCOME
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_LICENSE "..\License.md"
  !insertmacro MUI_UNPAGE_COMPONENTS
  !insertmacro MUI_UNPAGE_DIRECTORY
  !insertmacro MUI_UNPAGE_INSTFILES
  !insertmacro MUI_UNPAGE_FINISH

;--------------------------------
;Languages

  !insertmacro MUI_LANGUAGE "English" ; The first language is the default language
  !insertmacro MUI_LANGUAGE "French"
  !insertmacro MUI_LANGUAGE "German"
  !insertmacro MUI_LANGUAGE "Spanish"
  !insertmacro MUI_LANGUAGE "SpanishInternational"
  !insertmacro MUI_LANGUAGE "SimpChinese"
  !insertmacro MUI_LANGUAGE "TradChinese"
  !insertmacro MUI_LANGUAGE "Japanese"
  !insertmacro MUI_LANGUAGE "Korean"
  !insertmacro MUI_LANGUAGE "Italian"
  !insertmacro MUI_LANGUAGE "Dutch"
  !insertmacro MUI_LANGUAGE "Danish"
  !insertmacro MUI_LANGUAGE "Swedish"
  !insertmacro MUI_LANGUAGE "Norwegian"
  !insertmacro MUI_LANGUAGE "NorwegianNynorsk"
  !insertmacro MUI_LANGUAGE "Finnish"
  !insertmacro MUI_LANGUAGE "Greek"
  !insertmacro MUI_LANGUAGE "Russian"
  !insertmacro MUI_LANGUAGE "Portuguese"
  !insertmacro MUI_LANGUAGE "PortugueseBR"
  !insertmacro MUI_LANGUAGE "Polish"
  !insertmacro MUI_LANGUAGE "Ukrainian"
  !insertmacro MUI_LANGUAGE "Czech"
  !insertmacro MUI_LANGUAGE "Slovak"
  !insertmacro MUI_LANGUAGE "Croatian"
  !insertmacro MUI_LANGUAGE "Bulgarian"
  !insertmacro MUI_LANGUAGE "Hungarian"
  !insertmacro MUI_LANGUAGE "Thai"
  !insertmacro MUI_LANGUAGE "Romanian"
  !insertmacro MUI_LANGUAGE "Latvian"
  !insertmacro MUI_LANGUAGE "Macedonian"
  !insertmacro MUI_LANGUAGE "Estonian"
  !insertmacro MUI_LANGUAGE "Turkish"
  !insertmacro MUI_LANGUAGE "Lithuanian"
  !insertmacro MUI_LANGUAGE "Slovenian"
  !insertmacro MUI_LANGUAGE "Serbian"
  !insertmacro MUI_LANGUAGE "SerbianLatin"
  !insertmacro MUI_LANGUAGE "Arabic"
  !insertmacro MUI_LANGUAGE "Farsi"
  !insertmacro MUI_LANGUAGE "Hebrew"
  !insertmacro MUI_LANGUAGE "Indonesian"
  !insertmacro MUI_LANGUAGE "Mongolian"
  !insertmacro MUI_LANGUAGE "Luxembourgish"
  !insertmacro MUI_LANGUAGE "Albanian"
  !insertmacro MUI_LANGUAGE "Breton"
  !insertmacro MUI_LANGUAGE "Belarusian"
  !insertmacro MUI_LANGUAGE "Icelandic"
  !insertmacro MUI_LANGUAGE "Malay"
  !insertmacro MUI_LANGUAGE "Bosnian"
  !insertmacro MUI_LANGUAGE "Kurdish"
  !insertmacro MUI_LANGUAGE "Irish"
  !insertmacro MUI_LANGUAGE "Uzbek"
  !insertmacro MUI_LANGUAGE "Galician"
  !insertmacro MUI_LANGUAGE "Afrikaans"
  !insertmacro MUI_LANGUAGE "Catalan"
  !insertmacro MUI_LANGUAGE "Esperanto"
  !insertmacro MUI_LANGUAGE "Asturian"
  !insertmacro MUI_LANGUAGE "Basque"
  !insertmacro MUI_LANGUAGE "Pashto"
  !insertmacro MUI_LANGUAGE "ScotsGaelic"
  !insertmacro MUI_LANGUAGE "Georgian"
  !insertmacro MUI_LANGUAGE "Vietnamese"
  !insertmacro MUI_LANGUAGE "Welsh"
  !insertmacro MUI_LANGUAGE "Armenian"
  !insertmacro MUI_LANGUAGE "Corsican"
  !insertmacro MUI_LANGUAGE "Tatar"
  !insertmacro MUI_LANGUAGE "Hindi"

;--------------------------------
;Reserve Files
  
  ;If you are using solid compression, files that are required before
  ;the actual installation should be stored first in the data block,
  ;because this will make your installer start faster.
  
  !insertmacro MUI_RESERVEFILE_LANGDLL

;--------------------------------
;Installer Sections

Section "" ShutdownTools
  ; Shutdown existing processes
  #!if /FileExists "$INSTDIR\Updater.exe"
    ExecWait '"$INSTDIR\Updater.exe" -uninstall'
  #!endif
SectionEnd

Section "Windows Deck Tools for Steam Deck" SteamDeckTools
  SetShellVarContext All
  SectionIn RO

  SetOutPath "$INSTDIR"
  SetRegView 64

  ;ADD YOUR OWN FILES HERE...
  File /r "${BUILD_DIR}\*"
  
  ;Store installation folder
  WriteRegStr HKLM "Software\SteamDeckTools" "" $INSTDIR
  
  ;Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"

  ; Create entry in Programs
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\SteamDeckTools" \
                   "DisplayName" "Windows Deck Tools for Steam Deck - ${VERSION}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\SteamDeckTools" \
                   "UninstallString" "$\"$INSTDIR\Uninstall.exe$\""
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\SteamDeckTools" \
                   "QuietUninstallString" "$\"$INSTDIR\Uninstall.exe$\" /S"
SectionEnd

Section "Desktop Shortcut" DesktopShortcut
  SetShellVarContext All
  CreateShortcut "$DESKTOP\Fan Control.lnk" "$INSTDIR\FanControl.exe"
  CreateShortcut "$DESKTOP\Performance Overlay.lnk" "$INSTDIR\PerformanceOverlay.exe"
  CreateShortcut "$DESKTOP\Power Control.lnk" "$INSTDIR\PowerControl.exe"
  CreateShortcut "$DESKTOP\Steam Controller.lnk" "$INSTDIR\SteamController.exe"
SectionEnd

Section "Programs Folder Shortcut" ProgramsShortcut
  SetShellVarContext All
  CreateShortcut "$SMPROGRAMS\Steam Deck Tools\Fan Control.lnk" "$INSTDIR\FanControl.exe"
  CreateShortcut "$SMPROGRAMS\Steam Deck Tools\Performance Overlay.lnk" "$INSTDIR\PerformanceOverlay.exe"
  CreateShortcut "$SMPROGRAMS\Steam Deck Tools\Power Control.lnk" "$INSTDIR\PowerControl.exe"
  CreateShortcut "$SMPROGRAMS\Steam Deck Tools\Steam Controller.lnk" "$INSTDIR\SteamController.exe"
  CreateShortcut "$SMPROGRAMS\Steam Deck Tools\Check for Updates.lnk" "$INSTDIR\Updater.exe"
  CreateShortcut "$SMPROGRAMS\Steam Deck Tools\Uninstall.lnk" "$INSTDIR\Uninstall.exe"
SectionEnd

!ifdef USE_REDIST
  Section "Visual Studio Runtime" VisualStudioRuntime
    SetOutPath "$INSTDIR\Redist"
    File "Redist\vc_redist.x64.exe"
    ExecWait '"$INSTDIR\Redist\vc_redist.x64.exe" /passive /norestart'
  SectionEnd

  Section "ViGEmBus Setup 1.21.442" ViGEmBus
    SetOutPath "$INSTDIR\Redist"
    File "Redist\ViGEmBus_1.21.442_x64_x86_arm64.exe"
    ExecWait '"$INSTDIR\Redist\ViGEmBus_1.21.442_x64_x86_arm64.exe" /quiet /norestart'
  SectionEnd

  Section ".NET Desktop Runtime 6.0.11" DotNetRuntime
    SetOutPath "$INSTDIR\Redist"
    File "Redist\windowsdesktop-runtime-6.0.11-win-x64.exe"
    ExecWait '"$INSTDIR\Redist\windowsdesktop-runtime-6.0.11-win-x64.exe" /passive /norestart'
  SectionEnd
!endif

!ifdef USE_WINGET
  Section "Visual Studio Runtime" VisualStudioRuntime
    ExecWait 'winget install -e --id Microsoft.VC++2015-2022Redist-x64 --override "/passive /norestart"'
  SectionEnd

  Section "ViGEmBus Setup" ViGEmBus
    ExecWait 'winget install -e --id ViGEm.ViGEmBus'
  SectionEnd

  Section ".NET Desktop Runtime 6.0" DotNetRuntime
    ExecWait 'winget install -e --id Microsoft.DotNet.DesktopRuntime.6 --override "/passive /norestart"'
  SectionEnd
!endif

Section "Run on Startup" StartAllComponents
  Exec '"$INSTDIR\FanControl.exe" -run-on-startup'
  Exec '"$INSTDIR\PerformanceOverlay.exe" -run-on-startup'
  Exec '"$INSTDIR\PowerControl.exe" -run-on-startup'
  Exec '"$INSTDIR\SteamController.exe" -run-on-startup'
SectionEnd

Section /o "Disable Check for Updates" DisableCheckForUpdates
  SetShellVarContext All
  FileOpen $0 "$INSTDIR\DisableCheckForUpdates.txt" w
  FileClose $0
SectionEnd

;--------------------------------
;Installer Functions

Function .onInit

  !insertmacro MUI_LANGDLL_DISPLAY

FunctionEnd

;--------------------------------
;Descriptions

  ;USE A LANGUAGE STRING IF YOU WANT YOUR DESCRIPTIONS TO BE LANGAUGE SPECIFIC

  ;Assign descriptions to sections
  !insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${SteamDeckTools} "The Steam Deck Tools components."
    !ifdef USE_REDIST | USE_WINGET
      !insertmacro MUI_DESCRIPTION_TEXT ${VisualStudioRuntime} "Install Visual Studio C++ Runtime  2015-2022."
      !insertmacro MUI_DESCRIPTION_TEXT ${DotNetRuntime} "Install .NET Desktop Runtime 6.0."
      !insertmacro MUI_DESCRIPTION_TEXT ${ViGEmBus} "Install ViGEmBus Driver."
    !endif
    !insertmacro MUI_DESCRIPTION_TEXT ${DesktopShortcut} "Create a shortcut on your desktop."
    !insertmacro MUI_DESCRIPTION_TEXT ${ProgramsShortcut} "Create a shortcut in your start menu folder."
    !insertmacro MUI_DESCRIPTION_TEXT ${StartAllComponents} "Start all components on system boot."
    !insertmacro MUI_DESCRIPTION_TEXT ${DisableCheckForUpdates} "This application might connect remote server to check for updates or track errors. This helps this project development."
  !insertmacro MUI_FUNCTION_DESCRIPTION_END

;--------------------------------
;Uninstaller Section

Section "Uninstall"
  SetShellVarContext all

  ExecWait '"$INSTDIR\FanControl.exe" -uninstall'
  ExecWait '"$INSTDIR\PerformanceOverlay.exe" -uninstall'
  ExecWait '"$INSTDIR\PowerControl.exe" -uninstall'
  ExecWait '"$INSTDIR\SteamController.exe" -uninstall'
  ExecWait '"$INSTDIR\Updater.exe" -uninstall'

  Delete "$INSTDIR\*"
  Delete "$DESKTOP\Fan Control.lnk"
  Delete "$DESKTOP\Performance Overlay.lnk"
  Delete "$DESKTOP\Power Control.lnk"
  Delete "$DESKTOP\Steam Controller.lnk"
  Delete "$SMPROGRAMS\Steam Deck Tools\Fan Control.lnk"
  Delete "$SMPROGRAMS\Steam Deck Tools\Performance Overlay.lnk"
  Delete "$SMPROGRAMS\Steam Deck Tools\Power Control.lnk"
  Delete "$SMPROGRAMS\Steam Deck Tools\Steam Controller.lnk"
  Delete "$SMPROGRAMS\Steam Deck Tools\Check for Updates.lnk"
  Delete "$SMPROGRAMS\Steam Deck Tools\Uninstall.lnk"
  RMDir "$SMPROGRAMS\Steam Deck Tools"

  DeleteRegKey /ifempty HKLM "Software\SteamDeckTools"
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\SteamDeckTools"
SectionEnd

;--------------------------------
;Uninstaller Functions

Function un.onInit

  !insertmacro MUI_UNGETLANGUAGE
  
FunctionEnd
