# PersonalWiki ExcelLab 3.7.0 開發交接文件

## 1. 版本基準與重建說明
- **目前最新整合開發版本：3.7.0**。
- **下一開發版本：3.8.0**；整體已確認目標依序推進至 3.9.0。
- 此工作樹由 Library 中可取得的 `PersonalWiki_ExcelLab_3.0.0(1).zip` 重新建立。上一個對話的 3.6.0／3.7.0 未打包工作目錄無法直接存取，因此 3.6.0／3.7.0 是依保存的開發紀錄重建，不可宣稱為原工作樹逐位元恢復。
- 正式 3.7.0 ZIP 尚未建立；目前屬開發候選工作樹。完成 3.8.0、3.9.0 後再交付最終整合 ZIP。
- 解壓根目錄固定 `PersonalWiki_ExcelLab`，不可將版本號放入解壓後資料夾名稱。

## 2. 已完成整合內容
### 3.6.0 重建內容
- 搜尋排序：`updated-desc`、`title-asc`、`title-desc`、`created-desc`、`created-asc`；固定 SQL 白名單及穩定次排序。
- 分頁上限 200；COUNT 與結果查詢共用條件；每頁標籤以單次批次查詢載入。
- 備份／還原目的地檢查主檔、`-wal`、`-shm`、`-journal`，提交前再次檢查並拒絕覆蓋。
- 還原後驗證失敗時，只清理由本次操作建立的目標與 sidecar；清理失敗不遮蔽原始例外。
- `RelatedEntryService` 只讀依共用標籤與相同類型計算建議，不建立持久關係。
- 桌面新增「標籤」、「來源追溯」、「相關條目」頁；條目標籤可新增／移除，來源追溯與相關建議唯讀。
- `ImageThumbnailCache`：48 項 LRU、檔案大小／修改時間失效判斷、共享讀取、回傳獨立 Bitmap 副本。
- 合成 Schema v1 測試 DB：9 條目、17 標籤關聯、筆記、自訂欄位、來源追溯、2 圖片關聯（1 有效、1 缺失）。
### 3.7.0 整合內容
- 建立日期起／迄、更新日期起／迄四個可選篩選欄位；迄日包含整日。
- 日期格式 `yyyy-MM-dd`；日期無效或起日晚於迄日拒絕執行。
- 日期、關鍵字、類型、標籤、封存狀態採 AND 交集。
- 桌面新增標籤篩選、排序選項及「清除條件」按鈕；清除會重設所有搜尋條件及頁碼。
- CLI `entry-query` 新增 `--sort`、`--created-from`、`--created-to`、`--updated-from`、`--updated-to`。
- 3.6.0 排序、唯讀查詢、分頁限制及資料安全措施均保留。

## 3. 歷史版本功能（不得重做或移除）
- 1.3.4：多選、批次封存／還原。
- 1.3.5–1.3.8：圖片牆篩選排序、圖片完整性／重複報告、工作表欄位版面。
- 1.4.0–1.5.1：CLI acceptance、安全備份還原、健康檢查、桌面備份入口。
- 2.0.0–2.0.5：CLI／桌面整合、分頁搜尋、多版面 Excel、安全輸出、SHA256 預檢、唯讀匯入快照、設定檔原子保存、自我測試。
- 2.1.0：唯讀資料庫診斷報告。
- 2.5.0–2.5.8：診斷快照比較、UI/DPI、警告差異及語意驗證。
- 2.6.0：匯入交易安全閘、`foreign_keys`、串流 SHA256。
- 2.6.1：備份安全報告 128 KiB 串流 SHA256。
- 3.0.0：四個 JSON 大檔案路徑改用 Stream Parse、發行靜態驗證器及完整交接文件。
- 3.6.0／3.7.0：本交接文件第 2 節。

## 4. 專案結構
- `Program.cs`：CLI 命令入口及自我測試。
- `Models.cs`：核心模型。
- `Services/`：Excel 匯入、mapping/rules、快照、清理、交易、SQLite、備份還原、診斷、稽核、搜尋、相關條目及安全輸出。
- `PersonalWiki.Desktop/`：WinForms 主視窗、匯入對話框、標籤管理、桌面設定、圖片快取。
- `scripts/`：PowerShell acceptance/build 腳本、Python 靜態／fixture 測試及版本驗證器。
- `samples/`：Excel、mapping/rules、診斷 JSON、Schema v1 測試 DB、圖片樣本。
- `docs/releases/`：版本更新、測試、稽核、交接文件及 UI 測試矩陣。

## 5. 框架、相依套件、Schema 與路徑
- CLI：`.NET 10`，`net10.0`。
- Desktop：`.NET 10 WinForms`，`net10.0-windows`。
- NuGet：`DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`；未經同意不得新增套件。
- SQLite `PRAGMA user_version=1`；不得未經批准變更 Schema、建立新持久關係表或 migration。
- 桌面設定：`%LOCALAPPDATA%\PersonalWiki\desktop-settings.json`。
- 不變更現有 DB／設定檔位置。

## 6. 資料安全限制（全部保留）
1. 原始 Excel 唯讀。
2. 正式匯入必須依序執行 preflight、DB health、建立並驗證新備份、重驗來源雜湊、建立唯讀快照、快照重新 preflight、單一 SQLite transaction、提交前 `quick_check(1)`／`foreign_key_check`、commit、匯入後驗證。
3. 匯入連線啟用 `PRAGMA foreign_keys=ON`。
4. 備份／還原／匯出／報告／範本不得覆蓋既有目的檔；還原只能寫入新路徑。
5. 備份／還原必須處理 SQLite 主檔及 WAL／SHM／Journal sidecar。
6. 不自動刪除或合併條目、筆記、標籤、來源追蹤。
7. 移除圖片關聯不得刪除原始圖片檔。
8. 相關條目建議唯讀，不可持久化關係。
9. 不變更 Schema、DB 路徑或設定路徑；如確有必要，先提交 migration／備份／rollback 計畫並等候使用者批准。
10. 測試只能使用隔離資料庫與新輸出路徑。

## 7. 測試狀態、已知問題與環境限制
- Python 測試使用 SQLite fixture 與靜態檢查；不能代替 C#/.NET 執行期驗證。
- 目前環境沒有 `dotnet` SDK／Windows；未執行 `dotnet restore/build`、CLI acceptance、自我測試及 WinForms 實機驗收。
- 圖片快取的長時間大量圖片、DPI、多螢幕、同時更改圖片檔案仍待實機驗證。
- 日期比較依現有 `DateTimeOffset.UtcNow.ToString("O")` ISO 字串格式；需在實機以含不同 offset 的既有資料驗證字串排序語意。
- 25,000 筆效能測試是 Python SQLite fixture，不代表 Microsoft.Data.Sqlite 實際效能。
- 本版 ZIP 尚未正式建立；不得把開發候選當成已驗收正式版。

## 8. 下一階段計畫
- **3.8.0**：條目關係瀏覽／組織、標籤分類管理、圖片管理與顯示效能、關聯完整性；持久關係仍需使用者另行批准 Schema 變更，未批准前只做唯讀或暫存視圖。
- **3.9.0**：整體效能、穩定性、資料安全、故障復原、完整回歸與發行驗收；整理最終樣本與可直接測試資料。
- 交付前同步所有版本資訊、README、INDEX、更新說明、測試指南、稽核報告及本交接文件；打包 ZIP、CRC 驗證及根目錄驗證。

## 9. 新對話可直接複製的完整開發指令
```text
請直接接續 PersonalWiki ExcelLab 3.7.0，不要重新詢問已確認需求。唯一可用工作基準是本交接所附的最新 PersonalWiki_ExcelLab 專案／ZIP；目前最新整合開發版本為 3.7.0，下一個開發版本必須是 3.8.0，整體目標依序完成至 3.9.0。不可回退到 3.0.0 重新開始，也不可重做或移除 1.3.4–3.7.0 已完成的功能。

先閱讀 docs/releases/HANDOFF_3.7.0.md、UPDATE_NOTES_3.6.0.md、UPDATE_NOTES_3.7.0.md、TEST_GUIDE_3.6.0.md、TEST_GUIDE_3.7.0.md、AUDIT_REPORT_3.6.0.md、AUDIT_REPORT_3.7.0.md、README.md。保留全部歷史版本功能、測試樣本及 docs/releases 文件。

框架：CLI net10.0；Desktop net10.0-windows WinForms；NuGet DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14。SQLite PRAGMA user_version=1。桌面設定檔 %LOCALAPPDATA%\PersonalWiki\desktop-settings.json。不可擅改 DB／設定路徑、Schema 或新增套件。

資料安全：原始 Excel 唯讀；正式匯入依序 preflight、DB health、備份驗證、重驗來源雜湊、唯讀快照、快照 preflight、單一 transaction、提交前 quick_check(1)/foreign_key_check、commit、匯入後驗證；匯入啟用 foreign_keys。備份／還原／匯出／報告／範本拒絕覆蓋；還原只能新路徑，並處理主檔與 WAL/SHM/Journal。不可自動刪除或合併條目、筆記、標籤、來源追蹤；移除圖片關聯不可刪除原圖。相關條目目前唯讀，不可新增持久關係表。任何 Schema 變更須先提交 migration、備份、rollback 計畫並等待使用者明確批准。測試只使用隔離 DB 與新路徑。

3.7.0 已有排序五模式、分頁及標籤批次查詢、備份 sidecar 安全、唯讀相關條目、條目標籤頁、來源追溯頁、48 項 LRU 圖片快取、建立／更新日期起訖篩選、標籤複合條件、清除條件、CLI 日期與排序參數。不可重做。

下一步開發 3.8.0：條目關係瀏覽／組織、標籤分類管理、圖片管理與顯示效能。持久關係不得自行改 Schema；未批准前只做唯讀／暫存功能。完成後再做 3.9.0 整體效能、穩定性、資料安全、故障復原與完整驗收。

一次完成一批可獨立開發功能再集中回報，不要逐項打斷或重問需求。若環境缺少 .NET SDK／Windows，明確標示未執行，不可虛報通過。每次版本更新需同步 csproj、CLI/Desktop 版本、README、docs/releases/INDEX、更新說明、測試指南、稽核報告及完整交接指令。ZIP 檔名可帶版號，解壓根目錄固定 PersonalWiki_ExcelLab（不可帶版號）；交付前確認 ZIP 真實存在、CRC 正確、內容完整及下載連結指向該 ZIP。最終 3.9.0 必須附可直接測試的 DB、Excel、mapping/rules、圖片樣本，並記錄相依套件、Schema、已知問題、未測項目及環境限制。
```
