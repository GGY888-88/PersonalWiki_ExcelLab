# 0.9.0 開發更新

- 新增唯讀命令 `db-source-trace-audit <database.db> <report.json>`。
- 統計來源追溯紀錄總數、有/無來源條目的數量、來源條目遺失、來源檔/工作表空白、來源列號無效、匯入時間空白、批次關聯失效及未關聯批次數量。
- 不輸出來源路徑、工作表名稱或條目內容；不修改資料庫；報告使用 CreateNew，不覆蓋既有檔案。
- 更新 CLI help、README、版本號及驗收腳本，並同步 package 快照。
- SQLite Schema Version 1 不變。相依套件仍為 DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14，無新增套件。
- 已完成靜態檢查與 ZIP 完整性檢查；未執行 Build/功能測試，尚待集中驗收。
