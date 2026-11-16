# 4.4.0 稽核報告

## 變更範圍
- 新增唯讀搜尋基準量測服務及 CLI 命令。
- 新增 4.4.0 Python／SQLite 測試與 runner。
- 更新歷史發行測試中的「目前專案版本」斷言為 4.4.0；不改動其歷史功能斷言。
- CLI／Desktop 專案版本同步 4.4.0。

## 資料安全
- 搜尋執行委派既有 `EntryQueryService`，其連線模式為 SQLite ReadOnly。
- 未修改資料庫 Schema、匯入、備份、還原、圖片或原始 Excel 資料處理流程。
- Benchmark 不輸出檔案、不覆寫檔案；只輸出 JSON 至標準輸出。

## 測試狀態
- 4.4.0 新增測試：30/30 通過。
- 4.3.0 至 3.6.0 歷史 Python／SQLite／靜態回歸 runner：全部通過。
- `python -m py_compile scripts/*.py`：通過。
- .NET build、真實 CLI、Windows WinForms：未測試（環境限制）。

## 已知限制
- 計時包含連線建立及多段查詢，屬端到端延遲，不是單一 SQL 執行時間。
- 暖機及 OS 快取會影響結果；跨環境比較必須控制條件。
- 量測期間偵測到總數或當頁 ID 改變時會中止；不保證偵測資料列欄位值在同一 ID 下的細微變更。
