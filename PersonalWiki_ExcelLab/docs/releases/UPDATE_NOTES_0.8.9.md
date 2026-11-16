# 0.8.9 開發更新

- 新增唯讀命令 `db-note-usage-report <database.db> <report.json>`。
- 彙總筆記總數、有筆記/無筆記條目數、空白筆記數、不同筆記文字數量、每個條目的筆記數量分布及筆記長度分布。
- 不輸出筆記正文；不修改資料庫；報告使用 CreateNew，不覆蓋既有檔案。
- 更新 CLI help、README、版本號及驗收腳本。
- SQLite Schema Version 1 不變。相依套件仍為 DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14，無新增套件。
- 依開發階段安排，未執行 Build 或功能測試，尚待集中驗收。
