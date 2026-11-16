@echo off
chcp 65001 >nul
cd /d "%~dp0"
dotnet run --project .\PersonalWiki.Desktop\PersonalWiki.Desktop.csproj -c Release
pause
