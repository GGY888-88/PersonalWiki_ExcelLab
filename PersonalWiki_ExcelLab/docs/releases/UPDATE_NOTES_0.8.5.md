# 0.8.5 開發更新

- 新增唯讀 `db-import-batch-audit`，檢查匯入批次狀態、筆數合理性、時間格式及完成時間一致性。
- 報告不輸出來源檔案路徑或批次備註；JSON 使用 CreateNew，不覆蓋既有檔案。
- SQLite Schema Version 維持 1。
- 相依套件不變：DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14。
- 依使用者要求，本開發階段不執行 Build 或功能測試；僅做靜態檢查及封裝。
