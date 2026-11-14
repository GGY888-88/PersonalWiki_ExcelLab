# 4.2.0 稽核報告

## 變更範圍
- 新增 `Services/EntryQueryPlanService.cs` 與 CLI `entry-query-plan`。
- 新增 4.2.0 Python/SQLite 測試及統一回歸測試入口。
- 更新 CLI/Desktop 版本號及歷史靜態測試中的「目前版本」檢查。
- 保留既有 EntryQueryService、桌面搜尋 UI、匯入／備份／還原安全流程。

## 已執行
- `python scripts/run-4.2.0-python-tests.py`：通過。
- 4.2.0 新增測試：23/23。
- 既有版本回歸腳本：全部通過；詳見實際執行輸出與測試指南。
- Python 測試使用記憶體 SQLite 合成資料；不修改使用者資料庫。

## 未執行／限制
- 執行環境未安裝 .NET SDK（`dotnet` 不存在），因此 C# 編譯、NuGet restore、CLI 真實執行未驗證。
- 非 Windows 環境，WinForms 剪貼簿、多選、DPI、多螢幕及桌面／CLI 實機一致性未驗收。
- 合成資料測得的查詢耗時僅供本次執行環境觀察，不作為使用者設備效能承諾。

## 資料安全
- Schema 維持 SQLite `user_version=1`。
- 查詢計畫連線 ReadOnly、Pooling=false。
- 不變更 DB/settings 路徑與 NuGet 版本。
- 匯入、備份、還原、匯出既有安全規則維持不變。
