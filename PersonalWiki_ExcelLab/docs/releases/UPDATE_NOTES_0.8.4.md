# 0.8.4 dev

- 新增唯讀 `db-tag-audit`，檢查空白標籤、標準化欄位不一致、前後空白及同一條目內重複標籤候選。
- JSON 報告不輸出標籤文字；不修改資料庫、不刪除標籤；CreateNew 不覆蓋。
- 更新 CLI help、README、版本號及驗收腳本。
- SQLite Schema Version 1 不變。
- 相依套件：DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14；無新增套件。
- 依開發安排未執行 Build 或功能測試；僅完成程式碼靜態檢查及封裝。
