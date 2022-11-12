# Steam Deck Tools

This repository contains my own personal set of tools to help running Windows on Steam Deck.

## 1. Steam Deck Fan Control

This is a very early and highly experimental fan controller for Windows build based
on `linux/jupiter-fan-control.py` available on SteamOS.

<img src="images/fan_control.png" height="400"/>

### 1.1. Usage

You can download latest precompiled build from Releases tab. Currently the application requires
administrative privileges in order to read various temperature sensors.

It provides 3 modes of operation:

1. **Default** - use EC to control fan (as done today)
1. **SteamOS** - use reimplemented `jupiter-fan-control.py` (as in SteamOS when Fan Control is enabled)
1. **Max** - blow at full speed.

### 1.2. How it works?

This uses highly unstable (and firmware specific) direct manipulation of kernel memory
to control usage of EC (Embedded Controller) and setting desired fan RPM via VLV0100.

It was build based on knowledge gained in Steam Deck kernel patch and DSDT presented by bios.
The memory addresses used are hardcoded and can be changed any moment by the Bios update.

### 1.3. Limitations

As of Today (Nov, 2022) the GPU temperature is missing on Windows. Which makes it a little incomplete.
However, it should be expected that GPU temperature is "kind of similar" to CPU due to GPU being in the same
silicon. But! The device might overheat and break due to this missing temperature. **So, use at your own risk.**

## Author

Kamil Trzciński, 2022, License GPLv3
