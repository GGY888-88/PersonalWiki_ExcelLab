# PersonalWiki ExcelLab 4.3.0 開發交接文件

## 1. 版本與接續要求
- 最新整合版本：4.3.0。
- 下一開發版本：4.4.0。
- 以本 ZIP 內 `PersonalWiki_ExcelLab` 原始碼為唯一基礎，不得從舊版本重建。
- 不得擅自修改已完成功能；如需變更既有功能，先向使用者詢問。

## 2. 4.3.0 新增
- `Services/EntryQueryPlanService.cs` 現可診斷完整搜尋關鍵字條件（FTS5、標題、摘要、自訂欄位、筆記、標籤、圖片欄位）及建立／更新日期範圍。
- CLI `entry-query-plan` 新增位置關鍵字與 `--created-from`、`--created-to`、`--updated-from`、`--updated-to`。
- 日期上界使用次日零時排他條件，與 `EntryQueryService` 相同；起日含、迄日含。
- 新增 `scripts/test-4.3.0-complete-query-plan.py`、`scripts/run-4.3.0-python-tests.py`。

## 3. 已完成版本
- 1.3.4–4.1.0：完整功能及交接依 4.2.0 文件與歷史 release docs 保留。
- 4.2.0：SQLite 唯讀 `EXPLAIN QUERY PLAN`，支援類型、標籤、封存及排序；25,000 筆合成資料查詢契約測試。
- 4.3.0：在 4.2.0 上補齊關鍵字 FTS/LIKE 與日期範圍診斷。
- 不重複開發、不刪除歷史功能。

## 4. 技術、Schema、專案結構
- CLI：.NET 10 / `net10.0`。
- Desktop：WinForms / .NET 10 / `net10.0-windows`。
- NuGet：`DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`。
- SQLite `PRAGMA user_version=1`；本版沒有 migration、table/index/schema 變更。
- Desktop settings：`%LOCALAPPDATA%\PersonalWiki\desktop-settings.json`。
- 主要結構：`Services/`（資料服務）、`PersonalWiki.Desktop/`（WinForms）、`scripts/`（Python 測試與 PowerShell build）、`samples/`（隔離測試 DB/Excel/JSON）、`docs/releases/`（更新、測試、稽核、交接）。

## 5. 資料安全限制（必須遵守）
1. Excel 原始來源唯讀；正式匯入維持 preflight、DB health、備份驗證、來源雜湊重驗、唯讀快照、單一交易、提交前 quick_check/foreign_key_check、提交後驗證。
2. 匯入啟用 `PRAGMA foreign_keys=ON`。
3. 備份／還原／匯出／報告／範本拒絕覆寫；還原只能寫入新目的地。
4. 不自動刪除或合併 entries、notes、tags、source traces。
5. 移除圖片關聯不得刪除原始圖片；圖片說明只修改 DB metadata。
6. 未經使用者明確批准不得修改 Schema、DB/settings 路徑或套件版本。
7. 測試只使用隔離 DB、副本與全新輸出路徑。
8. 查詢診斷必須維持唯讀連線，不得為了 EXPLAIN 執行實際資料更新。

## 6. 測試與環境限制
- 執行：`python scripts/run-4.3.0-python-tests.py`。
- 新增測試：23/23 通過；Python 語法檢查通過。
- 以最終測試 runner 實際輸出記錄歷史回歸結果；不可只依舊版紀錄宣稱通過。
- .NET restore/build、Microsoft.Data.Sqlite 真實 CLI 執行、Windows WinForms/DPI/剪貼簿尚未測試，因目前環境缺少 Windows/.NET SDK。

## 7. 下一階段建議
- 4.4.0：建立可重複的 CLI 與 Desktop 搜尋一致性驗收資料集／報告；加入查詢計畫與實際耗時的明確區隔及索引診斷說明。
- 不新增 schema 或搜尋歷史功能，除非使用者明確批准。

## 8. 可直接複製到新對話的完整開發指令
```text
請接續 PersonalWiki ExcelLab 4.3.0，下一版本指定為 4.4.0。先從 ChatGPT Library 取得 PersonalWiki_ExcelLab_4.3.0.zip，實際解壓並閱讀 README、docs/releases/INDEX.md、HANDOFF_4.3.0.md、UPDATE_NOTES_4.3.0.md、TEST_GUIDE_4.3.0.md、AUDIT_REPORT_4.3.0.md，再以 ZIP 內原始碼為唯一基礎開發，不得憑記憶重建。
保留所有 1.3.4–4.3.0 功能。CLI net10.0、Desktop net10.0-windows、DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14、SQLite user_version=1、桌面設定 %LOCALAPPDATA%\PersonalWiki\desktop-settings.json。未經我明確批准，不得變更 Schema、資料庫/設定路徑或套件版本。
維持 Excel 原始來源唯讀、安全匯入交易與備份驗證、拒絕覆寫、還原至新目的地、不自動刪除/合併資料、移除圖片關聯不刪原圖等所有安全限制。查詢診斷保持 SQLite ReadOnly。
一次完成一批獨立功能後集中回報，不要頻繁輸出進度或要求我逐項測試。未實際執行的 .NET/Windows 測試不可宣稱通過。每批功能與正式版本完成後建立完整 ZIP，包含原始碼、測試/素材、Schema/套件/專案結構、README、版本紀錄、交接文件、已知問題、測試結果、SHA256 manifest。ZIP 檔名可帶版號，解壓根目錄固定 PersonalWiki_ExcelLab。驗證 CRC、檔案雜湊、版本與內容後才可宣稱已備份；Library 上傳必須有成功結果支持。
```
