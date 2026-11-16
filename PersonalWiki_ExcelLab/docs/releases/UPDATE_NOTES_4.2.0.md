# PersonalWiki ExcelLab 4.2.0 更新說明

## 新增
- 新增 CLI `entry-query-plan` 唯讀診斷命令，可針對類型、標籤、封存狀態及排序顯示 SQLite `EXPLAIN QUERY PLAN`。
- 回報 SQLite 版本、唯讀狀態、COUNT 與分頁結果查詢計畫步驟。
- 新增 25,000 筆合成資料測試，檢查查詢計畫索引使用、分頁界限、筆數一致性及 CLI/Desktop 共用搜尋服務的契約。
- 新增 4.2.0 統一測試入口，保留並執行 4.1.0 至 3.6.0 回歸測試。

## 安全與相容性
- 診斷連線使用 SQLite ReadOnly，Pooling=false；不執行搜尋資料更新，不修改資料庫。
- 不變更 SQLite Schema（user_version=1）、資料庫路徑、桌面設定路徑或 NuGet 套件版本。
- CLI 與 Desktop 專案版本同步為 4.2.0。

## 限制
- 查詢計畫命令目前分析類型／標籤／封存狀態／排序組合；不含完整關鍵字 FTS 與日期條件。
- Python SQLite fixture 是隔離合成資料，不代表 Microsoft.Data.Sqlite/.NET 真實執行效能。
- 尚未在本環境執行 .NET restore/build、CLI 真實資料庫驗收或 Windows WinForms 實機測試。
