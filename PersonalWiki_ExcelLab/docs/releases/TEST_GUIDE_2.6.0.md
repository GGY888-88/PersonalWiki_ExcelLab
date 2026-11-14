# PersonalWiki ExcelLab 2.6.0 測試指南

## 環境
Windows 11、.NET 10 SDK、PowerShell。請在解壓後的 `PersonalWiki_ExcelLab` 根目錄執行；所有自動測試使用新建隔離資料庫與輸出資料夾。

## 建置與驗收

```powershell
dotnet restore .\PersonalWiki.ExcelLab.csproj
dotnet build .\PersonalWiki.ExcelLab.csproj -c Release
dotnet build .\PersonalWiki.Desktop\PersonalWiki.Desktop.csproj -c Release
.\scripts\run-acceptance-tests.ps1
.\scripts\run-self-test.ps1
```

## 本版新增驗收
- `Safe Excel import uses verified snapshots and preserves original source trace` 必須成功。
- `Stages` 必須包含提交前 `quick_check(1)` 與 `foreign_key_check` 已完成的紀錄。
- 匯入完成後批次驗證、來源追溯、全文索引及外鍵檢查必須通過。
- 再次執行相同 acceptance 時，必須使用全新的 `OutputRoot`，不得覆蓋既有測試資料。

## 手動檢查

```powershell
$root = (Get-Location).Path
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
.\scripts\run-acceptance-tests.ps1 -OutputRoot (Join-Path $env:TEMP ('PersonalWiki-2.6.0-' + [guid]::NewGuid().ToString('N')))
```

## 尚待實機驗收
本次開發環境缺少 .NET SDK、Windows、PowerShell，以上命令尚未執行。需在 Windows 驗證提交前檢查、交易回滾、資料庫鎖定／磁碟空間不足，以及桌面 100%–300% DPI、多螢幕行為。
