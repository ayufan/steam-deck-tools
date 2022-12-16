# Privacy

To aid the application development this project uses [Sentry.io](https://sentry.io/)
to track all exceptions raised by the application. It is essential to identify bugs
and fix them with minimal user intervention superseeding the need to user
[troubleshooting](troubleshooting.md).

As part of Sentry error tracking the following information is being sent and is logged
including, but not only:

- Windows Version
- .NET Framework Version
- Exception Stack Trace
- Application Version
- Type of installation
- Unique installation ID

Additionally for statistic purposes the installation ID and Application Version might
be tracked as part of Update process to see active user-base vs version used.

The installation ID is one time generated GUID that is persisted on the first start
of an application.

Application for auto-update purposes checks for latest release on a start,
or every 24 hours.

All information being sent can be seen in a publically available source code
of this application.

## Disable it

Create `DisableCheckForUpdates.txt` file. To validate that this is working,
when clicking `Check for Updates` or running `Updater.exe` the application
will show `This application has explicitly disabled auto-updates`.
