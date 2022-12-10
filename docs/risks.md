# Risks

**This software is provided on best-effort basis and can break your SteamDeck.** It does a direct manipulation
of kernel memory to control usage the EC (Embedded Controller) and setting desired fan RPM via VLV0100

The Tools are written trying to check if you are truly using SteamDeck and compatible system.
The applications try it's best to start only on **validated configurations** to avoid breaking your system.

It was build based on knowledge gained in Steam Deck kernel patch and DSDT presented by bios.
The memory addresses used are hardcoded and can be changed any moment by the Bios update.

Fortunately quite amount of people are using it with a success and no problems observed.
However, you should be aware of the consequences.

## Risk of CPU and GPU frequency change

The APU manipulation of CPU and GPU frequency uses a ported implementation from Linux kernel.
It is more unstable than Fan Control (since the Fan Control is controller managing EC on Windows).
The change of the CPU and GPU frequency switch might sometimes crash system.
The AMD Display Driver periodically requests GPU metrics and in some rare circumstances the change request
might lock the GPU driver. If something like this happens it is advised to
power shutdown the device. Disconnect from charging, and hold power button for a few seconds.
Power device normally afterwards. This is rather unlikely to break the device if done right
after when such event occurs.
