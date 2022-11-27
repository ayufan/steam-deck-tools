if not "%1"=="am_admin" (powershell start -verb runas '%0' 'am_admin,%1' & exit /b)
if "%2"=="" (echo missing name & timeout /t 3 & exit /b)

cd "%~dp0\.."

:retry

taskkill /F /IM "%2.exe"
del %2\bin\Debug\net6.0-windows\%2.exe

dotnet build
%2\bin\Debug\net6.0-windows\%2.exe

timeout /t 3
goto retry
