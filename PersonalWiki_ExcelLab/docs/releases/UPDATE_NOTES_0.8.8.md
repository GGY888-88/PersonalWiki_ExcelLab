# PersonalWiki ExcelLab 0.8.8

## 新增
- 新增唯讀 `db-field-usage-report <database.db> <report.json>`。
- 彙總欄位識別鍵數量、欄位記錄總數、有欄位的條目數、空白識別鍵數及各值類型的記錄數。
- 依每個欄位識別鍵被多少條目使用，輸出頻率分布區間。
- 報告不輸出欄位識別鍵、顯示名稱或欄位值，避免額外暴露個人資料。
- 報告採 `FileMode.CreateNew`，不覆蓋既有檔案；資料庫以唯讀模式開啟。

## 相依與資料庫
- SQLite Schema Version：1（不變）。
- `DocumentFormat.OpenXml`：3.5.1（不變）。
- `Microsoft.Data.Sqlite`：10.0.14（不變）。
- 無新增第三方套件。

## 驗證狀態
- 依開發階段安排，未執行 build 或功能測試。
- ZIP 封裝完整性檢查另行記錄；本版尚未驗收。
