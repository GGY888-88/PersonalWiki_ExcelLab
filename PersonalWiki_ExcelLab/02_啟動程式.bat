@echo off
setlocal EnableExtensions
pushd "%~dp0" >nul

echo ==================================================
echo PersonalWiki ExcelLab 4.7.0 DEV03 - START
echo ==================================================

set "APP=PersonalWiki.Desktop\bin\Release\net10.0-windows\PersonalWiki.Desktop.exe"
set "DLL=PersonalWiki.Desktop\bin\Release\net10.0-windows\PersonalWiki.Desktop.dll"

if exist "%APP%" goto START_EXE
if exist "%DLL%" goto START_DLL

echo [ERROR] Release output was not found.
echo Run the build batch file first, or use the build-and-start batch file.
popd
pause
exit /b 1

:START_EXE
start "PersonalWiki ExcelLab" "%APP%"
if errorlevel 1 goto START_FAILED
echo [SUCCESS] Application launch requested.
popd
exit /b 0

:START_DLL
where dotnet >nul 2>nul
if errorlevel 1 (
  echo [ERROR] .NET runtime command was not found.
  popd
  pause
  exit /b 1
)
start "PersonalWiki ExcelLab" dotnet "%DLL%"
if errorlevel 1 goto START_FAILED
echo [SUCCESS] Application launch requested.
popd
exit /b 0

:START_FAILED
echo [ERROR] Windows could not start the application.
popd
pause
exit /b 1
