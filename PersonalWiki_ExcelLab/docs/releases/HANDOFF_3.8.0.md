# PersonalWiki ExcelLab 3.8.0 開發交接文件

## 1. 版本狀態
- **目前最新整合開發版本：3.8.0（重建整合候選，尚未正式發布／尚未 Windows 驗收）**。
- **下一個開發版本：3.9.0**，不可回退舊版重做。
- 本工作樹承接 3.7.0 重建整合候選；最初基準為可取得的 3.0.0 ZIP。3.6.0／3.7.0 是依先前開發紀錄重建，不是原工作樹逐位元組恢復。
- 3.8.0 ZIP 將於本批功能及文件完成後建立並驗證；交付前必須同步 ZIP 與版本文件。
- 解壓根目錄固定 `PersonalWiki_ExcelLab`，不可在資料夾名稱加入版號。

## 2. 3.8.0 新增內容
- 相關條目唯讀推薦顯示共用標籤名稱及數量，仍依共用標籤與相同類型計分，封存條目排除。
- 圖片說明可編輯，最多 2,000 字；留空可清除說明。
- 圖片檔案可進行唯讀 SHA256 驗證，顯示檔案遺失或雜湊不一致。
- 移除圖片只刪除 DB 關聯，不刪原圖。
- 未實作持久化標籤分類，因需要 Schema 變更；須先取得使用者明確批准及 migration／rollback 計畫。

## 3. 保留的既有功能
- 1.3.4：多選、批次封存／還原。
- 1.3.5–1.3.8：圖片牆篩選排序、圖片完整性／重複報告、工作表欄位版面。
- 1.4.0–1.5.1：CLI acceptance、安全備份還原、健康檢查、桌面備份入口。
- 2.0.0–2.0.5：CLI／桌面整合、分頁搜尋、多版面 Excel、安全輸出、SHA256 預檢、唯讀匯入快照、設定檔原子保存、自我測試。
- 2.1.0：唯讀資料庫診斷報告。
- 2.5.0–2.5.8：診斷快照比較、UI/DPI、警告差異及語意驗證。
- 2.6.0：匯入交易安全閘、foreign_keys、串流 SHA256。
- 2.6.1：備份安全報告串流 SHA256。
- 3.0.0：大型 JSON Stream Parse、發行靜態驗證器及交接文件。
- 3.6.0：排序、分頁、標籤批次查詢、備份／還原 sidecar 安全、唯讀相關條目、來源追溯、標籤管理、圖片快取。
- 3.7.0：建立／更新日期篩選、標籤與類型／封存複合條件、排序及清除條件、CLI 日期參數。

## 4. 專案結構及技術
- `Program.cs`：CLI 命令入口及自我測試。
- `Models.cs`：核心模型。
- `Services/`：匯入、mapping/rules、快照、清理、交易、SQLite、備份還原、診斷、稽核、搜尋、圖片內容及相關條目服務。
- `PersonalWiki.Desktop/`：WinForms 主視窗、匯入對話框、標籤管理、桌面設定、圖片快取。
- `scripts/`：Python fixture／靜態測試、發行驗證器。
- `samples/`：Excel、mapping/rules、Schema v1 測試 DB、圖片樣本。
- `docs/releases/`：版本文件、測試、稽核、交接及 UI 測試矩陣。
- CLI `.NET 10` `net10.0`；Desktop `.NET 10 WinForms` `net10.0-windows`。
- NuGet：`DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`。
- SQLite `PRAGMA user_version=1`；桌面設定 `%LOCALAPPDATA%\PersonalWiki\desktop-settings.json`。

## 5. 資料安全規則
1. 原始 Excel 唯讀。
2. 正式匯入需依序 preflight、DB health、建立並驗證備份、重驗來源雜湊、唯讀快照、快照 preflight、單一 SQLite transaction、提交前 `quick_check(1)`／`foreign_key_check`、commit、匯入後驗證。
3. 匯入連線啟用 `PRAGMA foreign_keys=ON`。
4. 備份／還原／匯出／報告／範本拒絕覆寫既有目的檔；還原只能寫入新路徑，並檢查 SQLite 主檔及 WAL／SHM／Journal。
5. 不自動刪除／合併條目、筆記、標籤或來源追溯。
6. 移除圖片關聯不刪原圖；圖片說明只修改 DB metadata。
7. 相關條目唯讀，不持久化關係。
8. 不變更 Schema、DB／設定路徑；如需變更，先提交 migration、備份、rollback 計畫並等待明確批准。
9. 測試只使用隔離 DB 與新輸出路徑。

## 6. 測試狀態及限制
- 3.8.0 Python fixture／靜態測試：`python scripts/run-3.8.0-python-tests.py`。
- 3.7.0 篩選測試與 3.6.0 回歸測試由同一 runner 執行。
- Python 測試不等於 C# 編譯或 .NET 執行期測試。
- 目前環境沒有 `dotnet` SDK／Windows；未執行 restore/build、CLI acceptance、自我測試及 WinForms 實機測試。
- 不同時區資料的日期字串比較、圖片同時被外部程式寫入、網路磁碟及多 DPI／多螢幕仍待實測。

## 7. 下一階段 3.9.0
- 整體效能與穩定性檢查、錯誤處理及故障復原。
- 搜尋、圖片、標籤、來源追溯及備份／還原的整合回歸。
- 測試資料與測試指南整理，確認樣本不會污染正式資料。
- 發行驗證器檢查版本、套件、Schema、ZIP 完整性及交接文件一致性。
- 最終 Windows .NET build、CLI acceptance、WinForms 實機驗收若環境可用才可標示通過；否則明確列為未測。

## 8. 新對話可直接複製的開發指令
```text
請直接接續 PersonalWiki ExcelLab 3.8.0，下一個開發版本必須是 3.9.0，不可回退 3.0.0 或重做舊功能。先閱讀 docs/releases/HANDOFF_3.8.0.md、README.md、docs/releases/UPDATE_NOTES_3.8.0.md、TEST_GUIDE_3.8.0.md、AUDIT_REPORT_3.8.0.md，以及 3.6.0／3.7.0 對應文件。

保留 CLI net10.0、Desktop net10.0-windows、DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14、SQLite user_version=1、桌面設定 %LOCALAPPDATA%\PersonalWiki\desktop-settings.json。不得未經明確批准修改 Schema、DB／設定路徑或新增 NuGet 套件。

保留全部 1.3.4–3.8.0 功能。3.8.0 已有唯讀相關條目共用標籤名稱、圖片說明編輯、原圖 SHA256 唯讀檢查；移除圖片關聯不刪原圖。持久化標籤分類及關係表需要 Schema 變更，未經使用者明確批准不得實作。

資料安全規則：原始 Excel 唯讀；匯入流程必須先 preflight、DB health、驗證備份、重驗來源雜湊、唯讀快照、快照 preflight、單一 transaction、提交前 quick_check(1)/foreign_key_check、commit、匯入後驗證；匯入啟用 foreign_keys。備份／還原／匯出／報告／範本拒絕覆寫；還原只允許新路徑並檢查 SQLite 主檔與 WAL/SHM/Journal。不自動刪除／合併 entries、notes、tags、source traces；移除圖片關聯不得刪原圖。測試只能使用隔離 DB 與新路徑。

3.9.0 著重整體效能、穩定性、資料安全、故障復原、跨功能回歸與發行驗收。一次完成一批獨立功能再集中回報，不要頻繁要求使用者測試或重問已確認需求。若缺少 .NET SDK／Windows，明確標示未執行，不可虛報。完成後同步更新所有版本文件、完整交接、測試素材及真正可用的 ZIP。ZIP 檔名可帶版本號，但解壓根目錄固定 PersonalWiki_ExcelLab，不可帶版號。打包後實際驗證 ZIP 可讀、CRC、內容、版本與下載連結。
```
