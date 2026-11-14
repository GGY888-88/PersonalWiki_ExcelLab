# PersonalWiki ExcelLab 0.8.3

- 新增唯讀 `db-field-audit <database.db> <report.json>`。
- 檢查空白欄位識別鍵、顯示名稱、欄位值，以及同一條目內重複顯示名稱候選。
- 重複候選只供人工複核；JSON 報告不輸出欄位值。
- 不修改資料庫、不刪除欄位；報告以 CreateNew 寫入，不覆蓋既有檔案。
- SQLite Schema Version 1 不變。
- 相依套件維持 DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14，無新增套件。
- 依開發階段安排，未執行 Build 或功能測試；尚未驗收。
