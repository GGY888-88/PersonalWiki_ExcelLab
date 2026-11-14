# PersonalWiki ExcelLab 2.6.1 靜態稽核報告

## 修改範圍
- `Services/ExcelImportDatabaseSafetyValidationService.cs`：將備份 SHA-256 驗證由 `File.ReadAllBytes` 改為 128 KiB 串流雜湊，並以 `FileShare.Read` 防止雜湊期間一般寫入／刪除操作。
- CLI 與 Desktop csproj、CLI help／自我測試報告、acceptance 測試包及發行文件版號同步為 2.6.1。
- 新增 2.6.1 更新說明、測試指南、稽核報告及交接文件。

## 靜態檢查
- [x] CLI／Desktop TargetFramework 保持 `net10.0`／`net10.0-windows`。
- [x] CLI／Desktop 版本同步為 2.6.1。
- [x] NuGet 版本未變更：DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14。
- [x] SQLite Schema `user_version=1`，無 migration 或 Schema 修改。
- [x] 安全報告備份雜湊路徑不再呼叫 `File.ReadAllBytes`。
- [x] 保留 2.6.0 匯入交易安全閘及既有測試樣本。

## 未執行項目／限制
- 此環境未安裝 .NET SDK、Windows 或 PowerShell；未執行 restore/build、acceptance、自我測試及 WinForms 測試。
- 大型備份實際記憶體峰值、檔案鎖定及 1 GiB+ 測試仍須在 Windows/.NET 10 環境執行。
- 2.6.0 交接文件列出的交易故障注入與 WAL／程序中止驗收仍未完成。

## 風險控制
- 修改只涉及唯讀備份驗證的雜湊讀取方式，不寫入備份、不變更資料庫或設定檔位置。
- `FileShare.Read` 無法取得時會拋出 I/O 例外，流程 fail closed，不會將檢查標記為成功。
