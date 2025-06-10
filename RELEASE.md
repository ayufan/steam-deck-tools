**Consider donating if you are happy with this project:**

<a href='https://ko-fi.com/ayufan' target='_blank'><img height='35' style='border:0px;height:50px;' src='https://az743702.vo.msecnd.net/cdn/kofi3.png?v=0' alt='Buy Me a Coffee at ko-fi.com' /></a> <a href="https://www.paypal.com/donate/?hosted_button_id=DHNBE2YR9D5Y2" target='_blank'><img height='35' src="https://raw.githubusercontent.com/stefan-niedermann/paypal-donate-button/master/paypal-donate-button.png" alt="Donate with PayPal" style='border:0px;height:55px;'/></a>

[**READ IF PLAYING ONLINE GAMES AND/OR GAMES THAT HAVE ANTI-CHEAT ENABLED**](https://steam-deck-tools.ayufan.dev/#anti-cheat-and-antivirus-software)

## #{GIT_TAG_NAME}

- SteamController: Persist and expose in release Lizard{Buttons,Mouse}
- PerformanceOverlay: Battery-only setting in Performance Overlay (#193)

## 0.7.3

- SteamDeck LCD: Support BIOS F7A0131

## 0.7.2

- PowerControl: Add Charge Limit (70%, 80%, 90%, 100%)

## 0.7.1

- SteamDeck OLED: Support BIOS 107 with temperature readings
- SteamDeck OLED: Remove BIOS 105 support as it is buggy

## 0.7.0

- FanControl: Support for SteamDeck OLED
- PerformanceOverlay: Support the `AMD Custom GPU 0932` found in SteamDeck OLED
- PowerControl: Support `AMD Custom GPU 0932` with a SMU at `0x80600000-0x8067ffff` ver.: `0x063F0E00`

## 0.6.22

- SteamController: Fix broken scroll on left pad introduced by 0.6.21

## 0.6.21

- SteamController: Add support for circular deadzone on left/right sticks
- FanControl: Add Silent fan profile. Configure `Silent4000RPMTemp` threshold in `FanControl.dll.ini`
- SteamController: Added `Win+D` shortcut under `Steam+RightStickPress`

## 0.6.20

- PerformanceOverlay/PowerControl: Add support for `AMD Radeon RX 670 Graphics`
