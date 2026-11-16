# 4.7.0 DEV04 靜態變更稽核

## 變更範圍
- 新增 `PersonalWiki.Desktop/ExcelImportDryRunDialog.cs`。
- `PersonalWiki.Desktop/MainForm.cs` 僅將 Dry-run 問題的舊訊息方塊替換為獨立唯讀明細視窗。
- 更新兩個 csproj 版本標示、三個標準 BAT 版本文字及版本文件。

## 保護檢查（依程式修改範圍）
- `ImportDryRunService`、`ExcelImportPreflightService`、`ExcelImportWorkflowService` 未修改。
- SQLite Schema、`user_version`、資料庫路徑及 NuGet 版本未修改。
- 既有規則編輯流程、匯入前備份流程、正式匯入呼叫及單一交易流程保留。
- 新視窗只讀取傳入的 `ImportDryRunReport`；列號複製只使用系統剪貼簿。

## 未完成驗證
- 未執行 .NET 編譯、Python 測試、CLI acceptance、程式啟動及 Windows 實機測試。
- 本文件是靜態變更範圍記錄，不代表功能驗收或回歸測試通過。
