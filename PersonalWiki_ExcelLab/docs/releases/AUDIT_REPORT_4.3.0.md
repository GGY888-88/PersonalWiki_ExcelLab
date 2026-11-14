# 4.3.0 稽核報告

## 變更範圍
- 只擴充 `EntryQueryPlanService` 與 CLI 查詢計畫參數。
- 新增 4.3.0 Python/SQLite 測試與統一測試入口。
- CLI/Desktop NuGet 及 Schema 均未變更。

## 安全檢查
- 查詢連線維持 `SqliteOpenMode.ReadOnly`、`Pooling=false`。
- SQL 欄位／排序片段使用白名單或固定程式碼；使用者搜尋值皆為參數。
- 日期格式嚴格驗證 `yyyy-MM-dd`，起日不可晚於迄日。
- 測試只建立暫存目錄中的隔離 SQLite DB。

## 測試狀態
- 4.3.0 新增測試：23/23 通過。
- `python -m py_compile scripts/*.py`：通過。
- 歷史回歸：須以 `python scripts/run-4.3.0-python-tests.py` 的最終輸出為準。
- .NET restore/build、真實 CLI DB 與 Windows WinForms：未測試（目前執行環境未提供 Windows/.NET SDK）。
