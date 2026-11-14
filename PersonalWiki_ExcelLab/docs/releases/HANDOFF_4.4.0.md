# PersonalWiki ExcelLab 4.4.0 開發交接文件

## 1. 版本與接續要求
- 最新整合版本：4.4.0。
- 下一開發版本：4.5.0。
- 以本 ZIP 內 `PersonalWiki_ExcelLab` 原始碼為唯一基礎；不得從舊版重建。
- 不得擅自修改已完成功能；如需變更既有功能，先詢問使用者。

## 2. 4.4.0 新增
- `Services/EntryQueryBenchmarkService.cs`：使用既有 `EntryQueryService` 執行唯讀端到端搜尋計時。
- CLI `entry-query-benchmark`：支援關鍵字、類型、標籤、封存狀態、排序、建立／更新日期範圍、頁碼、頁面大小及 1–20 次迭代。
- 暖機一次後回報 samples、min、median、average、max；量測期間結果總數或當頁 ID 改變則拒絕報告。
- Desktop 搜尋與 CLI／Benchmark 共用 `EntryQueryService`，新增 1,250 筆隔離 SQLite parity fixture。

## 3. 已完成版本
- 1.3.4–4.1.0：完整歷史功能及交接文件保留。
- 4.2.0：唯讀 SQLite 查詢計畫診斷及合成資料查詢測試。
- 4.3.0：查詢計畫補齊關鍵字 FTS/LIKE 與日期範圍。
- 4.4.0：新增實際搜尋延遲基準量測及 CLI/Desktop 共用搜尋契約驗收。
- 不重複開發、不刪除歷史功能。

## 4. 技術、Schema、專案結構
- CLI：.NET 10 / `net10.0`。
- Desktop：WinForms / .NET 10 / `net10.0-windows`。
- NuGet：`DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`。
- SQLite `PRAGMA user_version=1`；本版沒有 migration、table/index/schema 變更。
- Desktop settings：`%LOCALAPPDATA%\PersonalWiki\desktop-settings.json`。
- 主要結構：`Services/`、`PersonalWiki.Desktop/`、`scripts/`、`samples/`、`docs/releases/`。

## 5. 資料安全限制
1. Excel 原始來源唯讀；正式匯入維持 preflight、DB health、備份驗證、來源雜湊重驗、唯讀快照、單一交易、提交前 quick_check/foreign_key_check、提交後驗證。
2. 匯入啟用 `PRAGMA foreign_keys=ON`。
3. 備份／還原／匯出／報告／範本拒絕覆寫；還原只能寫入新目的地。
4. 不自動刪除或合併 entries、notes、tags、source traces。
5. 移除圖片關聯不得刪除原圖；圖片說明只修改 DB metadata。
6. 未經明確批准不得修改 Schema、DB/settings 路徑或套件版本。
7. 測試只使用隔離 DB、副本及全新輸出路徑。
8. 查詢診斷與 Benchmark 必須保持 SQLite ReadOnly；不得為效能量測修改資料。

## 6. 測試與環境限制
- 執行：`python scripts/run-4.4.0-python-tests.py`。
- 新增測試：30/30 通過；完整歷史 Python／SQLite／靜態回歸 runner 通過；`python -m py_compile scripts/*.py` 通過。
- .NET restore/build、真實 Microsoft.Data.Sqlite CLI、Windows WinForms/DPI/剪貼簿尚未測試，因目前環境缺少 Windows/.NET SDK。
- Benchmark 是暖機後端到端延遲，受 OS 快取與背景負載影響，不等於查詢計畫估算或跨設備保證。

## 7. 下一階段建議
- 4.5.0：以隔離樣本資料庫建立可重複的真實 .NET CLI 驗收流程；產生 CLI 搜尋與 Benchmark JSON 的自動比對報告，涵蓋所有排序、分頁、日期、標籤、封存條件。
- 不新增 Schema 或搜尋歷史功能，除非使用者明確批准。

## 8. 可直接複製到新對話的完整開發指令
```text
請接續 PersonalWiki ExcelLab 4.4.0，下一版本指定為 4.5.0。先從 ChatGPT Library 取得 PersonalWiki_ExcelLab_4.4.0.zip，實際解壓並閱讀 README、docs/releases/INDEX.md、HANDOFF_4.4.0.md、UPDATE_NOTES_4.4.0.md、TEST_GUIDE_4.4.0.md、AUDIT_REPORT_4.4.0.md，再以 ZIP 內原始碼為唯一基礎開發，不得憑記憶重建。
保留所有 1.3.4–4.4.0 功能。CLI net10.0、Desktop net10.0-windows、DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14、SQLite user_version=1、桌面設定 %LOCALAPPDATA%\PersonalWiki\desktop-settings.json。未經我明確批准，不得變更 Schema、資料庫/設定路徑或套件版本。
維持 Excel 原始來源唯讀、安全匯入交易與備份驗證、拒絕覆寫、還原至新目的地、不自動刪除/合併資料、移除圖片關聯不刪原圖等所有安全限制。查詢診斷與 Benchmark 保持 SQLite ReadOnly。
一次完成一批獨立功能後集中回報，不要頻繁輸出進度或要求我逐項測試。未實際執行的 .NET/Windows 測試不可宣稱通過。每批功能與正式版本完成後建立完整 ZIP，包含原始碼、測試/素材、Schema/套件/專案結構、README、版本紀錄、交接文件、已知問題、測試結果、SHA256 manifest。ZIP 檔名可帶版號，解壓根目錄固定 PersonalWiki_ExcelLab。驗證 CRC、檔案雜湊、版本與內容後才可宣稱已備份；Library 上傳必須有成功結果支持。
```
