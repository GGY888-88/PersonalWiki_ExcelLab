# PersonalWiki ExcelLab 4.2.0 開發交接文件

## 1. 版本
- 最新整合版本：4.2.0（已整合查詢計畫唯讀診斷及合成資料測試）。
- 下一開發版本：4.3.0（範圍待確認／規劃）。
- 基礎：Library 取得的 4.1.0 ZIP，實際解壓後直接修改。

## 2. 4.2.0 完成項目
- 新增 `Services/EntryQueryPlanService.cs`：唯讀 SQLite `EXPLAIN QUERY PLAN`，輸出 COUNT 與分頁結果計畫、SQLite 版本、ReadOnly 標記及說明。
- 新增 CLI `entry-query-plan <database.db> [--type value] [--tag value] [--archived exclude|include|only] [--sort ...]`。
- 新增 25,000 筆合成資料測試，驗證共用搜尋契約、頁面上限、無重複 ID、count 下限、索引計畫及查詢時間觀察。
- CLI/Desktop 版本同步 4.2.0。
- 更新舊回歸測試中原本固定比對目前版本 4.1.0 的檢查，改為 4.2.0。

## 3. 已完成版本
保留 4.1.0 交接文件列出的 1.3.4–4.1.0 全部功能；4.2.0 新增如上。不得重新開發或刪除歷史功能。

## 4. 專案結構與技術
- CLI：`.NET 10` / `net10.0`；Desktop：WinForms / `net10.0-windows`。
- NuGet：`DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`。
- SQLite `PRAGMA user_version=1`。
- 桌面設定 `%LOCALAPPDATA%\PersonalWiki\desktop-settings.json`。
- 新增服務：`Services/EntryQueryPlanService.cs`。
- 新增測試：`scripts/test-4.2.0-query-plan-and-parity.py`、`scripts/run-4.2.0-python-tests.py`。

## 5. 資料安全（必須維持）
1. Excel 原始來源唯讀；正式匯入維持 preflight、DB health、備份驗證、來源雜湊重驗、唯讀快照、單一交易、提交前 quick_check/foreign_key_check、提交後驗證。
2. 匯入啟用 `PRAGMA foreign_keys=ON`。
3. 備份／還原／匯出／報告／範本拒絕覆寫；還原只寫新路徑並檢查 DB/WAL/SHM/Journal。
4. 不自動刪除／合併 entries、notes、tags、source traces。
5. 移除圖片關聯不刪原圖；圖片說明只修改 DB metadata。
6. 未經明確批准不得變更 Schema、DB/settings 路徑或 NuGet 版本。
7. 測試只用隔離資料庫及新輸出路徑。

## 6. 測試與環境限制
- 執行：`python scripts/run-4.2.0-python-tests.py`。
- 4.2.0 新增測試 23/23 通過；歷史 Python/SQLite/靜態回歸腳本通過。
- 本環境 `dotnet` 不存在，未執行 .NET restore/build、CLI 真實 DB 驗收。
- 非 Windows，未執行 WinForms 實機、剪貼簿、多選、DPI、多螢幕及真正 CLI/Desktop 比對。
- 查詢計畫目前支援類型、標籤、封存狀態、排序；尚未納入關鍵字 FTS 與日期條件。

## 7. 下一階段
- 4.3.0 可評估：完整搜尋條件（關鍵字、日期、類型、標籤、封存）共用查詢規格與診斷；加入桌面可讀的搜尋診斷輸出；以 Windows/.NET 10 真實資料庫做 CLI/Desktop 一致性驗收。
- 不新增搜尋歷史或 Schema migration，除非使用者明確批准。

## 8. 新對話完整開發指令
```text
請接續 PersonalWiki ExcelLab 4.2.0，下一開發版本指定為 4.3.0，不可從舊版重做 3.6.0–4.2.0。請先取得並檢查最新 PersonalWiki_ExcelLab_4.2.0.zip，閱讀 README、docs/releases/INDEX.md、HANDOFF_4.2.0.md、UPDATE_NOTES_4.2.0.md、TEST_GUIDE_4.2.0.md、AUDIT_REPORT_4.2.0.md，再以 ZIP 內實際原始碼為唯一修改基礎。

保留 CLI net10.0、Desktop net10.0-windows、DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14、SQLite user_version=1、桌面設定 %LOCALAPPDATA%\PersonalWiki\desktop-settings.json。未經明確批准不得修改 Schema、DB/settings 路徑或套件版本。

保留 1.3.4–4.2.0 所有已完成功能。4.2.0 的 entry-query-plan 是唯讀診斷，支援 type/tag/archived/sort；不可誤稱包含關鍵字及日期完整計畫。所有資料安全限制依 HANDOFF_4.2.0.md 執行。

一次完成一批獨立功能再集中回報，不要頻繁輸出進度訊息或要求使用者測試。未實際執行的 .NET/Windows 測試不可宣稱通過。每個功能批次、正式版本及換對話前建立完整 ZIP，含原始碼、測試與素材、Schema/套件/專案結構、README/版本紀錄/交接、未完成項/問題/測試結果、檔案清單及 SHA256。ZIP 檔名可帶版號，解壓根目錄固定 PersonalWiki_ExcelLab。必須驗證 ZIP CRC、檔案清單、版本與內容後才能標示已備份；Library 保存必須有工具成功結果支持。
```
