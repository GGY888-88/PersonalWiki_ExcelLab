# PersonalWiki ExcelLab 4.4.0 更新說明

## 新增
- 新增 `EntryQueryBenchmarkService` 與 CLI `entry-query-benchmark`，使用與 CLI／Desktop 相同的 `EntryQueryService` 執行唯讀搜尋。
- 支援關鍵字、類型、標籤、封存狀態、排序、建立／更新日期範圍、頁碼及頁面大小條件。
- 支援 1–20 次計時迭代，先執行一次不計時暖機，回報每次樣本、最小值、中位數、平均值及最大值。
- 量測期間若符合總數或當頁 ID 集合發生變化，拒絕產生報告，避免把資料變動誤當效能結果。
- 新增隔離 1,250 筆 SQLite fixture，驗證穩定排序、分頁不重疊、總數及查詢計畫可獨立檢視。

## 指標解讀
- `entry-query-plan` 是 SQLite 規劃器的預估步驟；`entry-query-benchmark` 是實際執行搜尋路徑的端到端耗時，兩者不可混為一談。
- Benchmark 包含連線開啟、COUNT、分頁結果及本頁標籤查詢；暖機後量測，仍會受 OS 快取、磁碟、背景負載及資料庫大小影響。
- 建議在相同電腦、相同資料庫及相同篩選條件下比較；不可把單次數字視為跨設備效能保證。

## 安全與相容性
- 透過 `EntryQueryService` 的 SQLite ReadOnly 連線執行，不寫入資料庫。
- 不變更 SQLite Schema（`PRAGMA user_version=1`）、DB/settings 路徑或 NuGet 套件版本。
- CLI 與 Desktop 專案版本同步為 4.4.0；Desktop 既有搜尋仍使用共用 `EntryQueryService`。
