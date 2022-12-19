# Privacy

This project might connect to remote server and share the following information unless it is disabled
by creation of the `DisableCheckForUpdates.txt` in the root folder of the project:

## Error Tracking

To aid the application development this project uses [Sentry.io](https://sentry.io/)
to track all exceptions raised by the application. It is essential to identify bugs
and fix them with minimal user intervention superseeding the need to user
[troubleshooting](troubleshooting.md). The Sentry is configured to not track PII.

As part of Sentry error tracking the following information is being sent and is logged
including: Windows Version, .NET Framework Version, Exception Stack Trace, Application Version,
Type of installation.

You can see exact exceptions being sent in `Documents/SteamDeckTools/Logs`
- if it is empty it means nothing was sent.

## Auto-update

Application for auto-update purposes checks for latest release on a start,
or every 24 hours. As part of auto-update it sends information about installation
time of the application, application version, which SteamDeckTools apps are used.

## Disable it

Create `DisableCheckForUpdates.txt` file. To validate that this is working,
when clicking `Check for Updates` or running `Updater.exe` the application
will show `This application has explicitly disabled auto-updates`.
