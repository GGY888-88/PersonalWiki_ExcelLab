# 4.5.0 程式與安全稽核

## 稽核範圍
- `Services/EntryQueryBenchmarkService.cs`
- `scripts/entry-query-acceptance.py`
- 版本資訊、測試入口、發行素材與交接文件

## 結果
- Benchmark 報告從最後一次已驗證查詢結果取得 `ResultIds`，並以 JSON `resultIds` 輸出；不另開資料庫連線、不改變搜尋。
- CLI acceptance 對來源 DB 做位元組複製後才執行，未向來源路徑傳入寫入指令。
- 比對器對總數、返回數、ID 順序、分頁、排序、封存、關鍵字、類型、標籤及建立／更新日期條件逐項比較。
- 報告輸出目錄若已存在即拒絕執行；每次 CLI 搜尋 JSON 使用新目錄，符合拒絕覆寫原則。
- SQLite Schema、`user_version=1`、NuGet 套件版本、資料庫及桌面設定路徑均未變更。

## 測試證據
- 4.5.0 新增契約測試 28/28 通過，Comparator self-test 通過；4.5.0 發行靜態驗證 103/103 通過。
- 舊版回歸 runner 在臨時工作副本（僅將 csproj 版本字串改回其測試預期 4.4.0）完整通過；正式工作樹沒有回退版本。
- .NET restore/build 與真實 CLI acceptance 尚未執行，因環境沒有 dotnet SDK；Windows WinForms 實機測試未執行。

## 限制
靜態契約與 Python 測試不能取代 .NET 編譯或真實 CLI 執行。4.5.0 仍需在 Windows／.NET 10 SDK 環境完成 TEST_GUIDE_4.5.0.md 的 B 項。
