# PersonalWiki ExcelLab 3.9.0 開發交接文件

## 1. 版本狀態
- **目前最新整合開發版本：3.9.0（發行候選；ZIP 已建立並完成 CRC／SHA256 manifest 驗證，尚未完成 Windows 驗收）**。
- **下一個開發版本：4.0.0**；不得回退舊版重做已完成內容。4.0.0 功能範圍尚未確認，先以使用者後續需求為準。
- 專案以可取得的 3.0.0 ZIP 為基礎，依 3.6.0–3.8.0 開發紀錄重建；不是舊工作樹逐位元組恢復。

## 2. 已完成版本功能
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
- 3.9.0：CLI／Desktop 版號一致、發行安全檢查、SHA256 manifest、固定 ZIP 解壓根目錄、拒絕覆寫與 CRC 驗證；整合舊版回歸測試。

## 3. 專案結構與技術
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

## 4. 資料安全限制
1. 原始 Excel 唯讀。
2. 正式匯入依序執行 preflight、DB health、建立並驗證備份、重驗來源雜湊、唯讀快照、snapshot preflight、單一 SQLite transaction、提交前 `quick_check(1)`／`foreign_key_check`、commit、匯入後驗證。
3. 匯入連線啟用 `PRAGMA foreign_keys=ON`。
4. 備份／還原／匯出／報告／範本拒絕覆寫既有目的檔；還原只能寫入新路徑，並檢查 SQLite 主檔與 WAL／SHM／Journal。
5. 不自動刪除／合併 entries、notes、tags 或 source traces。
6. 移除圖片關聯不刪原圖；圖片說明只修改 DB metadata。
7. 相關條目唯讀，不持久化關係。
8. 不變更 Schema、DB 路徑、設定路徑或 NuGet 版本；如需變更，先提出 migration、備份與 rollback 計畫，等待使用者明確批准。
9. 測試只使用隔離 DB 與新輸出路徑。

## 5. 備份狀態
- ZIP：`PersonalWiki_ExcelLab_3.9.0.zip`，329 個 ZIP entries，328 個檔案 SHA256 已逐一比對，CRC 通過。
- 解壓根目錄：`PersonalWiki_ExcelLab`（不含版號）。
- ZIP 不覆寫既有檔案；ZIP SHA256 另記於本次交付訊息，避免把自身雜湊寫入 ZIP 造成循環。
- 本次 ZIP 已附加到目前對話；ChatGPT Library 保存狀態未確認。

## 6. 測試與限制
- 執行 `python scripts/run-3.9.0-python-tests.py`。
- 3.9.0 發行安全測試、3.8.0 功能測試、3.7.0 篩選測試、3.6.0 回歸測試由 runner 串接。
- Python fixture／靜態檢查不等於 C# 編譯或 .NET 執行期測試。
- 目前執行環境未提供 .NET 10 SDK／Windows；restore/build、CLI acceptance、WinForms 實機、DPI／多螢幕均未驗收。

## 7. 下一階段
- 3.9.0 完成發行 ZIP、SHA256 manifest、CRC、內容清單及版本一致性驗證。
- 有 Windows/.NET 10 環境時完成 build、CLI acceptance、WinForms 實機測試。
- 4.0.0 先依使用者新需求定義範圍，不擅自新增 Schema 或改資料路徑。

## 8. 新對話可直接複製的開發指令
```text
請接續 PersonalWiki ExcelLab 3.9.0，下一開發版本為 4.0.0，不可退回 3.0.0 或重做 3.6.0–3.9.0 已完成內容。先閱讀 README.md、docs/releases/HANDOFF_3.9.0.md、UPDATE_NOTES_3.9.0.md、TEST_GUIDE_3.9.0.md、AUDIT_REPORT_3.9.0.md 及歷代對應版本文件。

保留 CLI net10.0、Desktop net10.0-windows、DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14、SQLite user_version=1、桌面設定 %LOCALAPPDATA%\PersonalWiki\desktop-settings.json。未經使用者明確批准不得修改 Schema、DB／設定路徑或 NuGet 版本。

保留所有 1.3.4–3.9.0 功能。3.8.0 相關條目共用標籤顯示為唯讀建議；圖片說明只修改 DB metadata；圖片 SHA256 檢查唯讀；移除圖片關聯不刪原圖。標籤分類持久化需要 Schema 變更，未經明確批准不得實作。

資料安全：原始 Excel 唯讀；正式匯入必須先 preflight、DB health、驗證備份、重驗來源雜湊、唯讀快照、snapshot preflight、單一 transaction、提交前 quick_check(1)/foreign_key_check、commit、匯入後驗證；匯入啟用 foreign_keys。備份／還原／匯出／報告／範本拒絕覆寫；還原只允許新路徑並檢查 SQLite 主檔與 WAL/SHM/Journal。不自動刪除／合併 entries、notes、tags、source traces；移除圖片關聯不得刪原圖。測試只能使用隔離 DB 與新輸出路徑。

一次完成一批獨立功能再集中回報，不要頻繁要求使用者測試或重問已確認需求。未實際執行的 .NET／Windows 測試不可宣稱通過。每個功能批次、每個版本、對話接近上限及換對話前都必須建立完整 ZIP 備份，內含原始碼、測試與素材、Schema／套件／專案結構、README／版本紀錄／交接文件、未完成項／問題／測試結果、檔案清單及 SHA256；ZIP 檔名可帶版號，解壓根目錄固定 PersonalWiki_ExcelLab。必須驗證 ZIP 可開啟、CRC、版本及內容後才能標示已備份；不可用記憶代替原始碼。
```
