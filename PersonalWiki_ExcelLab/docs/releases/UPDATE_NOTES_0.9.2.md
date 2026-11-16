# 0.9.2 開發更新

- 新增唯讀命令 `db-image-file-integrity-audit <database.db> <report.json>`。
- 檢查圖片參照檔案存在狀態，並對已記錄 SHA256 的圖片重新計算雜湊比對。
- 報告僅輸出統計，不輸出圖片路徑、圖片內容、條目名稱或識別碼。
- 不修改資料庫或圖片；報告使用 CreateNew，不覆蓋既有檔案。
- 更新 CLI help、README、版本號、驗收腳本及 package 快照。
- SQLite Schema Version 1 不變；DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14；無新增套件。
- 僅完成靜態檢查及 ZIP 完整性檢查；未執行 Build/功能測試，待集中驗收。
