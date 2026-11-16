# PersonalWiki ExcelLab 0.7.8

## 新增：資料庫儲存空間報告
- 新增 `db-storage-report <database.db> <report.json>`。
- 唯讀統計資料庫檔案大小、SQLite page size/page count/free page count/used page count。
- 統計使用者資料表數量、索引數量及各資料表筆數。
- JSON 報告採 CreateNew，拒絕覆蓋既有檔案。
- 驗收腳本新增儲存空間報告 smoke test。

## 安全與相容性
- 以 SQLite ReadOnly 模式開啟資料庫，不執行 VACUUM、ANALYZE 或任何寫入操作。
- SQLite Schema Version 1 不變。
- 不修改原始 Excel，不刪除或移動圖片。
- 不新增第三方套件；DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14。

## 驗證狀態
- 依使用者要求本階段只開發、不安排測試。
- 已完成靜態檢查與封裝；尚未執行 .NET build/run，未驗收。
