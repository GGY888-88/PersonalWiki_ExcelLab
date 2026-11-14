# PersonalWiki ExcelLab 4.0.0 開發交接文件

## 1. 版本狀態
- **目前最新整合版本：4.0.0（第一批功能已整合；Python／SQLite 與靜態測試可執行；尚未完成 .NET／Windows 驗收）**。
- **下一開發版本：4.1.0**。
- 本版以 Library 中已保存的 `PersonalWiki_ExcelLab_3.9.0.zip` 為基礎實際解壓接續，未從舊版重做。

## 2. 4.0.0 已完成
- CLI `entry-query` 新增 `--output <report.json>`，把目前搜尋條件、頁碼、每頁筆數、總符合數及當頁結果輸出為 UTF-8 JSON。
- 不帶 `--output` 時維持原有 JSON stdout 行為。
- 匯出透過 `SafeFileOutput.WriteAllText` 與 `FileMode.CreateNew`；目的檔存在即拒絕覆寫。
- 匯出完成顯示輸出完整路徑、符合總筆數及當頁筆數。
- CLI 與 Desktop 專案版本同步為 4.0.0。

## 3. 已完成版本功能摘要
- 1.3.4：多選、批次封存／還原。
- 1.3.5–1.3.8：圖片牆篩選排序、圖片完整性／重複報告、工作表欄位版面。
- 1.4.0–1.5.1：CLI acceptance、安全備份還原、健康檢查、桌面備份入口。
- 2.0.0–2.0.5：CLI／桌面整合、分頁搜尋、多版面 Excel、安全輸出、SHA256 預檢、唯讀匯入快照、設定檔原子保存、自我測試。
- 2.1.0：唯讀資料庫診斷報告。
- 2.5.0–2.5.8：診斷快照比較、UI/DPI、警告差異及語意驗證。
- 2.6.0–2.6.1：匯入交易安全閘、foreign_keys、串流 SHA256、備份報告雜湊。
- 3.0.0：大型 JSON Stream Parse、發行靜態驗證器及交接文件。
- 3.6.0：五種搜尋排序、分頁、標籤批次查詢、備份／還原 sidecar 安全、唯讀相關條目、來源追溯、標籤管理、圖片快取及匯入測試樣本。
- 3.7.0：建立／更新日期起訖、類型／標籤／封存狀態複合篩選、清除條件、CLI 日期參數。
- 3.8.0：相關條目顯示共用標籤名稱；圖片說明編輯；原圖 SHA256 唯讀檢查；移除圖片關聯不刪原圖。
- 3.9.0：發行安全檢查、SHA256 manifest、固定 ZIP 解壓根目錄、拒絕覆寫與 CRC 驗證。
- 4.0.0：CLI 搜尋結果 JSON 匯出與目的檔防覆寫。

## 4. 專案結構與技術
- `Program.cs`：CLI 命令入口及自我測試。
- `Models.cs`：核心模型。
- `Services/`：Excel 匯入、mapping/rules、唯讀快照、清理、交易、SQLite、備份還原、診斷、稽核、搜尋、圖片內容及相關條目服務。
- `PersonalWiki.Desktop/`：WinForms 主視窗、匯入對話框、標籤管理、桌面設定、圖片快取。
- `scripts/`：Python fixture／靜態測試、發行驗證、ZIP 打包。
- `samples/`：Excel、mapping/rules、Schema v1 測試 DB、圖片及診斷 JSON 樣本。
- `docs/releases/`：版本更新、測試、稽核及交接文件。
- CLI `.NET 10` `net10.0`；Desktop `.NET 10 WinForms` `net10.0-windows`。
- NuGet：`DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`。
- SQLite `PRAGMA user_version=1`。
- 桌面設定 `%LOCALAPPDATA%\PersonalWiki\desktop-settings.json`。

## 5. 資料安全限制（必須保留）
1. 原始 Excel 唯讀。
2. 正式匯入依序執行 preflight、DB health、建立並驗證備份、重驗來源雜湊、唯讀快照、snapshot preflight、單一 SQLite transaction、提交前 `quick_check(1)`／`foreign_key_check`、commit、匯入後驗證。
3. 匯入連線啟用 `PRAGMA foreign_keys=ON`。
4. 備份／還原／匯出／報告／範本拒絕覆寫既有目的檔；還原只能寫入新路徑，並檢查 SQLite 主檔與 WAL／SHM／Journal。
5. 不自動刪除／合併 entries、notes、tags 或 source traces。
6. 移除圖片關聯不刪原圖；圖片說明只修改 DB metadata。
7. 相關條目唯讀，不持久化關係。
8. 不變更 Schema、DB 路徑、設定路徑或 NuGet 版本；如需變更，先提出 migration、備份與 rollback 計畫，等待使用者明確批准。
9. 測試只能使用隔離 DB 與新輸出路徑。
10. 搜尋結果 JSON 匯出不得覆寫既有目的檔。

## 6. 測試與環境限制
- 執行：`python scripts/run-4.0.0-python-tests.py`。
- 測試包括 4.0.0 匯出契約及 3.9.0–3.6.0 回歸。
- Python fixture／靜態檢查不等於 C# 編譯或 .NET 執行期測試。
- 目前執行環境沒有 .NET 10 SDK／Windows；restore/build、CLI acceptance、WinForms 實機、DPI／多螢幕均未驗收。

## 7. 下一階段建議
- 4.1.0：桌面搜尋結果管理，例如結果匯出入口、複製條目摘要、結果選取／批次操作的安全流程；先做唯讀或可回復的功能，不改 Schema。
- 搜尋效能分析與查詢計畫檢查，僅在合成大型 DB 上執行。
- CLI 與 Desktop 搜尋條件／輸出格式一致性測試。
- 不在未批准下引入永久儲存搜尋、標籤分類資料或任何 Schema migration。

## 8. 新對話可直接複製的完整開發指令
```text
請接續 PersonalWiki ExcelLab 4.0.0，下一開發版本指定為 4.1.0，不可退回舊版重做 3.6.0–4.0.0 已完成內容。先閱讀 README.md、docs/releases/INDEX.md、docs/releases/HANDOFF_4.0.0.md、UPDATE_NOTES_4.0.0.md、TEST_GUIDE_4.0.0.md、AUDIT_REPORT_4.0.0.md 及歷代對應版本文件。請以最新 4.0.0 ZIP／工作樹實際檔案為唯一修改基礎，不可只靠記憶重建或宣稱不存在的測試結果。

保留 CLI net10.0、Desktop net10.0-windows、DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14、SQLite user_version=1、桌面設定 %LOCALAPPDATA%\PersonalWiki\desktop-settings.json。未經使用者明確批准不得修改 Schema、DB／設定路徑或 NuGet 版本。

保留所有 1.3.4–4.0.0 功能。4.0.0 新增 CLI entry-query --output <report.json> 搜尋結果 JSON 匯出；不帶 --output 維持 stdout；使用 SafeFileOutput／FileMode.CreateNew 拒絕覆寫既有目的檔。

資料安全：原始 Excel 唯讀；正式匯入必須先 preflight、DB health、驗證備份、重驗來源雜湊、唯讀快照、snapshot preflight、單一 transaction、提交前 quick_check(1)/foreign_key_check、commit、匯入後驗證；匯入啟用 foreign_keys。備份／還原／匯出／報告／範本拒絕覆寫；還原只允許新路徑並檢查 SQLite 主檔與 WAL/SHM/Journal。不自動刪除／合併 entries、notes、tags、source traces；移除圖片關聯不得刪原圖。測試只能使用隔離 DB 與新輸出路徑。

一次完成一批獨立功能再集中回報，不要頻繁要求使用者測試或重問已確認需求。未實際執行的 .NET／Windows 測試不可宣稱通過。每個功能批次、每個版本、對話接近上限及換對話前都必須建立完整 ZIP 備份，內含原始碼、測試與素材、Schema／套件／專案結構、README／版本紀錄／交接文件、未完成項／問題／測試結果、檔案清單及 SHA256；ZIP 檔名可帶版號，解壓根目錄固定 PersonalWiki_ExcelLab。必須驗證 ZIP 可開啟、CRC、版本及內容後才能標示已備份；不可用記憶代替原始碼。
```
