# 0.9.1 開發更新

- 新增唯讀命令 `db-relationship-audit <database.db> <report.json>`。
- 使用 SQLite `PRAGMA foreign_key_check` 檢查所有外鍵關聯，依子資料表彙總問題筆數。
- 報告刻意不輸出資料列 rowid、父表資訊或欄位值；不修改資料庫。
- 報告使用 `FileMode.CreateNew`，不覆蓋既有檔案；拒絕將報告寫到資料庫本身。
- 更新 CLI help、README、版本號、驗收腳本及 package 快照。
- SQLite Schema Version 1 不變；DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14；無新增套件。
- 僅完成靜態檢查及封裝完整性檢查；未執行 Build/功能測試，待集中驗收。
