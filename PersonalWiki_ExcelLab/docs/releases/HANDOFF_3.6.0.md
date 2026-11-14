# PersonalWiki ExcelLab 3.6.0 重建交接

- 此版本由可取得的 3.0.0 專案依既有紀錄重建，不是上一對話未打包工作樹的逐位元恢復。
- 已完成：五種排序、分頁標籤批次載入、備份／還原 sidecar 安全、唯讀相關條目建議、條目標籤管理、來源追溯檢視、48 項 LRU 圖片快取、合成資料與樣本。
- Schema：SQLite `user_version=1`，禁止未經批准變更。
- CLI：`net10.0`；Desktop：`net10.0-windows`；NuGet：`DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`。
- 桌面設定：`%LOCALAPPDATA%\PersonalWiki\desktop-settings.json`；DB／設定路徑不可擅改。
- 尚未執行 .NET build 或 Windows 實機測試。
- 下一階段 3.7.0：日期起訖篩選、標籤複合條件、搜尋條件清除、CLI 日期參數及邊界測試。
