# PersonalWiki ExcelLab 2.1.0 更新說明

## 本版：整合式唯讀資料庫診斷

新增 CLI 命令：

```powershell
dotnet run -c Release -- db-diagnostic-report .\PersonalWiki.db .\database-diagnostic.json
```

一次輸出以下資料庫診斷資訊：

- SQLite `quick_check`、`foreign_key_check` 與 `user_version`。
- 全文索引條目數、索引數、缺漏數及孤兒數。
- 資料庫檔案大小、頁面大小、頁面總數、空閒頁數及已使用頁數。
- 使用者資料表數量及逐表筆數。
- 綜合健康狀態；資料庫健康或全文索引不一致時回傳結束代碼 `3`。

安全與相容性：

- 所有診斷查詢均以 SQLite 唯讀模式及單一 deferred 唯讀交易快照執行，不執行修復、重建索引、VACUUM 或資料修改；遇到缺少必要資料表或查詢錯誤時會記錄警告並標記診斷不健康。
- JSON 報告先寫入同目錄唯一暫存檔，再以不覆蓋方式原子提交；目的檔已存在即拒絕，不覆蓋既有報告，失敗時清理由本次建立的暫存檔。
- 報告包含資料庫完整路徑及診斷統計，不包含條目正文、筆記內容、自訂欄位值或圖片路徑。
- SQLite `user_version=1` 不變；沒有新增 NuGet 套件；資料庫及桌面設定位置不變。

## 驗收

`TEST_GUIDE_2.1.0.md` 說明新命令及 acceptance 測試。Acceptance 新增整合診斷報告案例，檢查健康狀態、全文索引一致性、資料表統計、來源資料庫檔案 SHA-256 未變，以及拒絕覆寫既有報告。

目前開發環境未安裝 .NET SDK、Windows 或 PowerShell，因此已加入測試但尚未執行 restore/build/self-test/acceptance/WinForms 實機測試。不得將未執行的測試標示為通過。
