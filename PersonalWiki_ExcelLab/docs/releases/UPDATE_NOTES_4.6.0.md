# PersonalWiki ExcelLab 4.6.0 更新紀錄

## 新增：CLI 搜尋／Benchmark 驗收診斷與單案例重跑
- `scripts/entry-query-acceptance.py` 新增 `--case-index 1..15`，可只重跑標準 15 組驗收中的指定案例。
- 每組案例獨立捕捉 CLI 執行、JSON 解析及 parity 比對錯誤；單一案例失敗不再中斷後續案例。
- 每完成一組案例即更新 `acceptance-comparison.json` 與 `.md`，即使後續案例或環境設定失敗，也會留下已完成案例與未執行數量。
- JSON 報告保留實際搜尋／Benchmark 命令、錯誤診斷、失敗欄位及可直接複製的單案例重跑指令。
- 報告區分案例失敗與整體設定錯誤；維持拒絕覆寫既有輸出目錄。
- 新增 `test-4.6.0-query-acceptance.py`，包含無 .NET SDK 的模擬失敗路徑測試。

## 安全與相容性
- 不修改 SQLite Schema、`PRAGMA user_version`、資料庫路徑、桌面設定路徑或 NuGet 版本。
- 驗收仍只對來源資料庫的位元組複本執行查詢；CLI 搜尋與 Benchmark 維持唯讀。
- 不變更既有搜尋、排序、匯入、備份、還原及圖片管理行為。

## 驗證範圍
- 執行 `python scripts/run-4.6.0-python-tests.py` 驗證新增契約、模擬失敗路徑、發行靜態檢查及歷史 Python／SQLite 回歸。
- 真實 .NET CLI acceptance、.NET restore/build、WinForms 實機測試仍須 Windows + .NET 10 SDK；不得以模擬測試代替實機結果。
