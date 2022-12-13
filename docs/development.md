# Development

It should be enough to run `scripts/build.bat` to see binaries in `build-release/`.

You might also run `scripts/test_debug.bat` which will build and run built version.

## Data tracking

To aid the application development this project uses [Sentry.io](https://sentry.io/)
to track all exceptions raised by the application. It is essential to identify bugs
and fix them with minimal user intervention superseeding the need to user
[troubleshooting](troubleshooting.md).

As part of Sentry error tracking the following information is being sent and is logged
including, but not only:

- Windows Version
- .NET Framework Version
- Application Version
- Unique Machine ID

Additionally for statistic purposes the Machine ID and Application Version might
be tracked as part of Update process to see active user-base vs version used.

The Unique Machine ID is one time generated GUID that is persisted on the first start
of an application.

Application for auto-update purposes checks for latest release on a start,
or every 24 hours.

All information being logged can be seen in a publically available source code
of this application.
