# 0.8.6 開發更新

- 新增唯讀 `db-entry-inventory-report`，依條目類型彙總條目總數、啟用/封存數，以及欄位、筆記、圖片、標籤的關聯數量。
- 報告僅含分類及統計，不輸出條目標題或任何欄位/筆記/圖片/標籤內容；JSON 使用 CreateNew，不覆蓋既有檔案。
- SQLite Schema Version 維持 1。
- 相依套件不變：DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14。
- 依使用者要求，本開發階段不執行 Build 或功能測試；僅做靜態檢查及封裝。
