# PersonalWiki ExcelLab 3.0.0 稽核報告

## 修改檔案
- `PersonalWiki.ExcelLab.csproj`、`PersonalWiki.Desktop/PersonalWiki.Desktop.csproj`：版本更新為 3.0.0；目標框架及套件不變。
- `Program.cs`、`scripts/run-acceptance-tests.ps1`：發行版本資訊同步為 3.0.0。
- `Services/DatabaseImportVerificationService.cs`
- `Services/DatabaseExportValidationService.cs`
- `Services/DatabaseImportPreviewService.cs`
- `Services/DatabaseImportService.cs`
  - 將 JSON 讀取從 `File.ReadAllBytes` 改為 `FileStream` + `JsonDocument.Parse(Stream)`，保留 MaxDepth 64 及原有欄位驗證／匯入流程。
- `scripts/verify-release.py`：新增跨平台、無第三方依賴的靜態發行檢查。
- README、發行索引及 3.0.0 更新說明、測試指南、稽核報告、交接文件。

## 已執行靜態檢查
- CLI／Desktop csproj XML 可解析，版本均為 3.0.0。
- TargetFramework 保持 `net10.0`／`net10.0-windows`。
- NuGet 保持 `DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`。
- 11 個 JSON 樣本皆可解析。
- 2 個 XLSX 樣本皆通過 ZIP CRC 檢查。
- 四個 JSON 路徑已使用 Stream Parse，未再以 `File.ReadAllBytes` 載入整份檔案。
- `ImportTransactionGuard` 的 `quick_check(1)`／`foreign_key_check` 與匯入流程 `foreign_keys=ON` 保留。
- 發行檢查腳本執行結果記錄於打包驗證流程。

## 未執行／限制
- 建置容器沒有 `dotnet`、Windows 或 PowerShell；未執行 `dotnet restore/build`、CLI acceptance、自我測試或 WinForms UI 實機測試。
- JSON `JsonDocument` 仍建立 DOM；本次避免整檔 byte[] 複製，不代表逐筆／常數記憶體解析。
- 交易故障注入、WAL／程序中止、超大型檔案記憶體峰值、DPI／多螢幕仍需 Windows/.NET 10 實測。

## 資料安全
- 未變更 SQLite Schema `user_version=1`。
- 未新增套件、未變更 DB／設定檔位置。
- 未修改資料清理、刪除／合併、圖片原檔處理或備份覆寫策略。
- 所有新增靜態檢查只讀取專案檔案與樣本，不連接正式資料庫。
