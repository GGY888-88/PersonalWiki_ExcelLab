# 3.6.0 稽核報告

## 修改範圍
- `Services/EntryQueryService.cs`：排序白名單、穩定排序及分頁標籤批次查詢。
- `Services/DatabaseBackupService.cs`、`DatabaseRecoveryService.cs`：完整 SQLite sidecar 檢查及本次建立檔案清理。
- `Services/RelatedEntryService.cs`：唯讀相關條目建議。
- `PersonalWiki.Desktop/ImageThumbnailCache.cs`、`MainForm.cs`：LRU 快取及標籤／來源／相關條目頁。
- 新增合成測試資料與 Python 測試。

## 安全檢查
- 不修改 Schema v1、DB 路徑或桌面設定路徑。
- 搜尋及相關條目建議使用唯讀連線。
- 相關條目僅計算建議，不寫入關係表。
- 圖片關聯刪除仍不刪除原始檔案。
- 備份與還原目的地拒絕覆蓋 SQLite 主檔及 sidecar。

## 驗證狀態
- Python fixture／靜態檢查：由 `run-3.6.0-python-tests.py` 執行。
- 未執行 .NET restore/build、CLI acceptance、自我測試或 Windows WinForms 實機測試；目前建置環境沒有 dotnet SDK／Windows。
- 圖片快取的多螢幕、長時間大量圖片及檔案同時變更情境仍待實機測試。
