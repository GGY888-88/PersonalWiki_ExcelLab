# PersonalWiki ExcelLab 4.3.0 更新說明

## 新增
- 擴充 `entry-query-plan`，納入完整搜尋的關鍵字條件：FTS5 `entries_fts MATCH`、標題／摘要／自訂欄位／筆記／標籤／圖片欄位 LIKE 條件。
- 查詢計畫同時支援建立日期與更新日期起訖篩選，日期上界採次日零時排他條件，與 `EntryQueryService` 一致。
- CLI 支援位置關鍵字及 `--created-from`、`--created-to`、`--updated-from`、`--updated-to`。
- 新增隔離 SQLite/FTS5 測試，驗證關鍵字、日期邊界、封存條件、結果排序及查詢計畫。

## 安全與相容性
- 查詢計畫仍使用 SQLite ReadOnly、Pooling=false，不寫入資料庫。
- 不變更 SQLite Schema（`PRAGMA user_version=1`）、資料庫路徑、桌面設定路徑或 NuGet 套件版本。
- CLI 與 Desktop 專案版本同步為 4.3.0。

## 限制
- 查詢計畫顯示 SQLite 規劃器的預估步驟，不代表實際執行時間或保證索引必定被採用。
- Python SQLite 測試使用隔離合成資料；仍需 Windows/.NET 10 環境執行真實 Microsoft.Data.Sqlite、CLI 及 WinForms 驗收。
