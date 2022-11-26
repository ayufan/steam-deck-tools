if not "%1"=="am_admin" (powershell start -verb runas '%0' 'am_admin,%1' & exit /b)
if "%2"=="" (echo missing name & timeout /t 3 & exit /b)

cd "%~dp0"

:retry

taskkill /F /IM "%1.exe"
del %1\bin\Debug\net6.0-windows\%1.exe

dotnet build
%1\bin\Debug\net6.0-windows\%1.exe

timeout /t 3
goto retry
