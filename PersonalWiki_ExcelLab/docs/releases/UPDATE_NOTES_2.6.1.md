# PersonalWiki ExcelLab 2.6.1 更新說明

## 備份安全報告重新驗證的記憶體改善
- `ExcelImportDatabaseSafetyValidationService` 不再使用 `File.ReadAllBytes` 將整份備份資料庫載入記憶體。
- 改以 128 KiB 緩衝區、`FileOptions.SequentialScan` 與 `SHA256.HashData(Stream)` 計算備份 SHA-256，記憶體使用量不再隨備份檔案大小等比例增加。
- 雜湊期間使用 `FileShare.Read`，允許其他讀取者，但拒絕同時寫入或刪除，避免雜湊計算期間備份內容被一般檔案操作改動；若檔案被占用而無法取得安全讀取控制，驗證會失敗關閉。

## 相容性與安全
- SQLite `user_version=1` 不變；沒有新增／修改資料表、欄位、索引或 migration。
- NuGet 維持 `DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`；無新增套件。
- CLI 與 WinForms 專案版本同步為 2.6.1。
- 保留 2.6.0 匯入交易提交前 `quick_check(1)`、`foreign_key_check` 與來源／備份串流雜湊。
- 不修改來源 Excel、資料庫位置、設定檔位置；不覆蓋既有備份或報告。

## 驗證狀態
- 已完成原始碼修改、版本字串檢查、專案 XML 檢查、文件與 ZIP 靜態檢查。
- 本環境沒有 .NET SDK、Windows、PowerShell；未執行 `dotnet restore/build`、CLI acceptance、自我測試或 WinForms 實機測試。
