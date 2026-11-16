# PersonalWiki ExcelLab 2.5.2 測試指南

## Windows 編譯
在專案根目錄執行：

```powershell
dotnet restore .\PersonalWiki.ExcelLab.csproj
dotnet build .\PersonalWiki.ExcelLab.csproj -c Release
dotnet build .\PersonalWiki.Desktop\PersonalWiki.Desktop.csproj -c Release
```

## 驗收
```powershell
pwsh -ExecutionPolicy Bypass -File .\scripts\run-acceptance-tests.ps1
```

## 本次重點
確認 `DesktopSettingsService.cs` 可解析 `PersonalWiki.ExcelLab.Services.SafeFileOutput`，並確認桌面專案透過既有 ProjectReference 使用 CLI 專案中的服務。

注意：本次交付環境未安裝 .NET SDK、Windows 或 PowerShell，以上命令尚未實際執行。
