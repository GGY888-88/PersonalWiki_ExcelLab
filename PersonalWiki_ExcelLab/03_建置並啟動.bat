@echo off
setlocal EnableExtensions
pushd "%~dp0" >nul

echo ==================================================
echo PersonalWiki ExcelLab 4.7.0 DEV03 - BUILD AND START
echo ==================================================

where dotnet >nul 2>nul
if errorlevel 1 goto NO_DOTNET
if not exist "PersonalWiki.Desktop\PersonalWiki.Desktop.csproj" goto NO_PROJECT

echo Building Release configuration...
echo Build output: build_release.log

dotnet build "PersonalWiki.Desktop\PersonalWiki.Desktop.csproj" -c Release > "build_release.log" 2>&1
set "BUILD_RESULT=%ERRORLEVEL%"
if not "%BUILD_RESULT%"=="0" goto BUILD_FAILED

set "APP=PersonalWiki.Desktop\bin\Release\net10.0-windows\PersonalWiki.Desktop.exe"
if not exist "%APP%" goto OUTPUT_MISSING

echo.
echo [SUCCESS] Build completed. Starting application...
start "PersonalWiki ExcelLab" "%APP%"
if errorlevel 1 goto START_FAILED
echo [SUCCESS] Application launch requested.
echo Full build output is saved in build_release.log.
popd
pause
exit /b 0

:NO_DOTNET
echo [ERROR] .NET SDK was not found.
echo Install the .NET 10 SDK from:
echo https://dotnet.microsoft.com/download/dotnet/10.0
goto FAILED

:NO_PROJECT
echo [ERROR] Project file was not found.
echo Expected: PersonalWiki.Desktop\PersonalWiki.Desktop.csproj
goto FAILED

:BUILD_FAILED
echo.
echo [FAILED] Build failed. Application was not started.
echo The full log will open in Notepad.
if exist "build_release.log" start "" notepad.exe "build_release.log"
goto FAILED

:OUTPUT_MISSING
echo [ERROR] Build reported success, but the application EXE is missing.
goto FAILED

:START_FAILED
echo [ERROR] Windows could not start the application.
goto FAILED

:FAILED
popd
pause
exit /b 1
