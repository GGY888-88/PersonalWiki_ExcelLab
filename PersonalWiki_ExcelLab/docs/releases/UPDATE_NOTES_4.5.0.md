# PersonalWiki ExcelLab 4.5.0 更新紀錄

## 新增
- Benchmark JSON 新增 `resultIds`，回報實際量測頁面的條目 ID 與順序，讓搜尋結果與效能報告可進行精確比對。
- 新增 `scripts/entry-query-acceptance.py`：使用隔離複本資料庫，呼叫真實 .NET CLI 的 `entry-query` 與 `entry-query-benchmark`，自動比對總數、本頁數量、ID 順序、頁碼、頁大小、排序、關鍵字、類型、標籤、封存及日期條件。
- 驗收覆蓋五種排序、兩個頁碼、關鍵字，以及標籤／類型、封存模式、建立日期與更新日期條件；輸出 JSON 與 Markdown 比對報告。
- 新增 `--self-test`，可在沒有 .NET SDK 的環境先測試比較器邏輯。
- CLI 與 Desktop 專案版本同步更新為 4.5.0。

## 安全與相容性
- 不修改 SQLite Schema、`PRAGMA user_version`、資料庫路徑、桌面設定路徑或 NuGet 版本。
- Benchmark 與搜尋仍使用既有唯讀查詢服務；驗收流程先將來源資料庫複製到暫存目錄，只對複本執行查詢。
- 不改動既有搜尋條件、排序定義或桌面搜尋行為；Benchmark JSON 僅新增 `resultIds` 欄位。

## 驗證
- 4.5.0 新增 Python／靜態契約測試：28/28 通過；比較器 self-test 通過。
- 在暫存副本中將專案版本字串暫時還原為 4.4.0，執行完整 4.4.0 舊版回歸 runner：所有 4.4.0 至 3.6.0 歷史測試通過。正式工作樹仍為 4.5.0，未修改歷史功能。
- 真實 .NET CLI acceptance 尚未執行：目前環境沒有 dotnet SDK。Windows WinForms 實機測試亦未執行。
