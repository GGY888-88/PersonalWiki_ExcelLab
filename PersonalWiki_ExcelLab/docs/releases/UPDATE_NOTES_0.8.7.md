# PersonalWiki ExcelLab 0.8.7

## 新增
- `db-tag-usage-report <database.db> <report.json>`：唯讀彙總有標籤的條目數、標準化標籤數、標籤關聯總數，以及每個標籤被 1、2–5、6–20、21 個以上條目使用的頻率分布。
- 報告不輸出標籤文字、條目標題或其他私人內容。
- 使用 SQLite ReadOnly 模式；JSON 報告以 CreateNew 建立，不覆蓋既有檔案。

## 相容性與安全
- SQLite Schema Version：1（未變更）。
- 相依套件：DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14（無新增）。
- 不修改資料庫、不更動標籤、不刪除任何資料。
- 依開發階段安排，本版未執行 Build 或功能測試；僅進行靜態檢查與封裝。
