# 2.1.0 測試指南

## 新增功能：整合式唯讀資料庫診斷

在 Windows 10/11 與 .NET 10 SDK 環境執行：

```powershell
dotnet restore .\PersonalWiki.ExcelLab.csproj
dotnet build .\PersonalWiki.ExcelLab.csproj -c Release
dotnet run -c Release --no-build -- db-init .\diagnostic-test.db
dotnet run -c Release --no-build -- db-diagnostic-report .\diagnostic-test.db .\diagnostic-test-report.json
```

預期：
- 命令結束代碼為 `0`。
- JSON `Format` 為 `personalwiki-database-diagnostic-v1`。
- `IsHealthy`、`Health.IsHealthy`、`SearchIndex.IsConsistent` 為 `true`。
- 包含資料表筆數與 SQLite 儲存頁面統計，`Warnings` 為空陣列。
- `db-diagnostic-test.db` 在報告前後 SHA-256 不變。
- 再次指定相同報告路徑時，命令失敗且原報告 SHA-256 不變。

若診斷發現 SQLite 健康檢查或全文索引不一致，命令回傳結束代碼 `3`，但仍輸出診斷 JSON，方便調查；本命令不會自動修復或重建索引。

## 完整驗收

請使用全新且不存在的輸出目錄：

```powershell
.\scripts\run-acceptance-tests.ps1 -OutputRoot .\AcceptanceTestOutput_210
```

此腳本會先 restore/build，再執行既有回歸案例及 2.1.0 診斷報告案例。請保留 `AcceptanceTestReport.json` 以供問題追蹤。

## 尚未執行的測試

本次開發容器沒有 `dotnet`、Windows 或 PowerShell，以上命令尚未實際執行。桌面 UI、DPI、多螢幕及 Excel 匯入仍需 Windows 實機驗收。
