# PersonalWiki ExcelLab 3.0.0 更新說明

## 本次整合
- 以 2.6.1 完整專案為直接基準，正式版號同步至 3.0.0；保留歷代 CLI、WinForms、SQLite、Excel 匯入、備份還原、診斷及樣本資產。
- 四個 JSON 匯入／驗證路徑改用 `JsonDocument.Parse(Stream)`：`DatabaseImportVerificationService`、`DatabaseExportValidationService`、`DatabaseImportPreviewService`、`DatabaseImportService`。避免 `File.ReadAllBytes` 將整份 JSON 複製到額外的大型 byte[]；JSON 深度限制與既有驗證邏輯不變。
- 新增 `scripts/verify-release.py`，僅使用 Python 標準函式庫，可在沒有 .NET SDK 的環境驗證專案版號、TargetFramework、NuGet 版本、發行文件、JSON 樣本、XLSX ZIP CRC、串流 JSON 路徑及匯入交易安全閘。
- 新增完整 3.0.0 發行文件及跨對話交接指令。

## 保留項目
- CLI `net10.0`、桌面 WinForms `net10.0-windows`。
- `DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`；沒有新增 NuGet 套件。
- SQLite `user_version=1`；未修改 Schema、資料庫路徑或設定檔路徑。
- 2.6.0 匯入交易 `quick_check(1)`／`foreign_key_check` 安全閘與來源／備份串流 SHA-256。
- 2.6.1 備份安全報告重新驗證串流 SHA-256、128 KiB 緩衝區及 `FileShare.Read`。
- 所有既有診斷 JSON 樣本、多版面 Excel 測試活頁簿、匯入樣本及歷代文件。

## 注意
本版已完成程式碼整合與靜態發行檢查；此建置環境沒有 .NET SDK、Windows、PowerShell，故不能宣稱 .NET build、CLI acceptance、自我測試或 WinForms 實機驗收通過。請依 `TEST_GUIDE_3.0.0.md` 在 Windows/.NET 10 執行完整驗收。
