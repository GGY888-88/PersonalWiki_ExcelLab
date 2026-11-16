# PersonalWiki ExcelLab 4.6.0 開發交接文件

## 1. 版本與接續要求
- 最新整合開發版本：4.6.0（驗收診斷與單案例重跑已整合；尚待 .NET／Windows 實機驗收）。
- 下一開發版本：4.7.0。
- 以本 ZIP 內 `PersonalWiki_ExcelLab` 原始碼為唯一基礎；不可從舊版重建或回退。
- 不得擅自修改已完成的功能；如需變更既有功能，先詢問使用者。

## 2. 4.6.0 新增功能
- `EntryQueryBenchmarkReport` 增加 `ResultIds`，Benchmark JSON 新增 `resultIds`，保留當次頁面條目 ID 與順序。
- `scripts/entry-query-acceptance.py`：真實 .NET CLI acceptance runner，先複製資料庫到暫存目錄，再呼叫 `entry-query` 和 `entry-query-benchmark`。
- 比對搜尋與 Benchmark 的總數、本頁數量、ID 順序、頁碼、頁大小、排序、封存、關鍵字、類型、標籤、建立／更新日期條件。
- 覆蓋五種排序、兩個頁碼、關鍵字及多組篩選組合；產生 JSON／Markdown 總報告與逐案例原始輸出。
- `--self-test` 可在無 .NET SDK 環境驗證比較器；`--no-build` 可使用既有 Release CLI DLL。
- 4.6.0 新增 `--case-index 1..15` 指定單案例重跑；案例錯誤獨立捕捉，後續案例繼續執行。
- 每完成一組案例即保存 JSON／Markdown 部分報告；記錄命令、錯誤診斷及可複製的重跑命令。
- CLI／Desktop 版本同步為 4.6.0。

## 3. 已完成版本與功能（避免重複）
- 3.6.0：大型資料集搜尋／排序、備份 WAL 鎖定及資料庫復原安全。
- 3.7.0：複合篩選與匯入相關功能。
- 3.8.0：圖片關聯管理。
- 3.9.0：發行安全與完整性檢查。
- 4.0.0：CLI `entry-query` JSON 安全匯出，拒絕覆寫。
- 4.1.0：桌面搜尋結果多選摘要複製（Ctrl／Shift），唯讀。
- 4.2.0：唯讀 SQLite 查詢計畫診斷與合成資料 parity。
- 4.3.0：查詢計畫補齊 FTS／LIKE 關鍵字與日期範圍。
- 4.4.0：`entry-query-benchmark` 真實端到端唯讀計時、1–20 次迭代、暖機、min／median／average／max、CLI／Desktop 共用查詢驗收。
- 4.5.0：Benchmark 結果 ID 輸出及搜尋／Benchmark 真實 CLI 自動比對工作流。
- 4.6.0：Acceptance 失敗診斷、部分報告持續保存、單案例重跑及可複製命令。
- 歷史版本 1.3.4–3.5.0 功能與歷史文件均保留於專案內；不得刪除。

## 4. 技術基準、Schema、套件與結構
- CLI：.NET 10，`net10.0`。
- Desktop：WinForms，.NET 10，`net10.0-windows`。
- NuGet：`DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`。
- SQLite `PRAGMA user_version=1`；4.6.0 沒有 migration 或 schema/table/index 變更。
- 桌面設定：`%LOCALAPPDATA%\PersonalWiki\desktop-settings.json`。
- 主要結構：`Services/`、`PersonalWiki.Desktop/`、`scripts/`、`samples/`、`docs/releases/`、根目錄 CLI 專案。

## 5. 資料安全限制（必須保留）
1. 原始 Excel 唯讀；正式匯入維持預檢、DB health、備份驗證、來源雜湊重驗、唯讀快照、單一交易、提交前 `quick_check`／`foreign_key_check`、提交後驗證。
2. 匯入啟用 `PRAGMA foreign_keys=ON`。
3. 備份／還原／匯出／報告／範本拒絕覆寫；還原只能寫入新目的地。
4. 不自動刪除或合併 entries、notes、tags、source traces。
5. 移除圖片關聯不得刪除原圖；圖片說明只修改 DB metadata。
6. 未經明確批准不得修改 Schema、DB/settings 路徑或 NuGet 套件版本。
7. 測試只使用隔離 DB、副本及全新輸出路徑。
8. 查詢計畫、搜尋、Benchmark 保持 SQLite ReadOnly；4.6.0 acceptance 只能對資料庫複本執行。

## 6. 測試與環境限制
- 新增測試：`python scripts/test-4.6.0-query-acceptance.py`；新增案例與失敗模擬測試結果以實際執行輸出為準；Comparator self-test 包含於測試流程。發行靜態驗證：`python scripts/verify-release-4.6.0.py`。
- 完整舊版 regression runner 已在臨時副本通過；該副本僅暫時將 csproj 版本字串設為 4.4.0 以符合舊測試硬編碼，正式工作樹保持 4.6.0。
- `python -m py_compile scripts/*.py` 已對新增腳本執行。
- .NET restore/build、真實 14 組 CLI acceptance、WinForms／DPI／剪貼簿尚未測試，因開發環境缺少 dotnet SDK／Windows。
- Benchmark 數值受 OS 快取、磁碟、背景負載及資料庫大小影響，不設定跨設備效能門檻。

## 7. 下一階段計畫（4.7.0）
- 優先在 Windows + .NET 10 SDK 執行 4.6.0 的真實 CLI acceptance，處理任何真實執行差異。
- 增加 acceptance 報告對失敗案例的診斷摘要與可重跑指令，維持唯讀與拒絕覆寫。
- 再規劃下一批獨立功能；不新增 Schema、不改搜尋行為或資料安全流程，除非使用者明確批准。

## 8. 可直接複製到新對話的完整開發指令

```text
請接續 PersonalWiki ExcelLab 4.6.0，下一開發版本固定為 4.7.0。必須以本次交付 ZIP 內 PersonalWiki_ExcelLab 原始碼為唯一基礎，不可從舊版重建或重做已完成項目。先讀 README.md、docs/releases/INDEX.md、HANDOFF_4.6.0.md、UPDATE_NOTES_4.6.0.md、TEST_GUIDE_4.6.0.md、AUDIT_REPORT_4.6.0.md。

保留所有歷史功能：3.6.0 大型搜尋／備份復原安全；3.7.0 複合篩選；3.8.0 圖片關聯；3.9.0 發行安全；4.0.0 CLI 搜尋 JSON 匯出；4.1.0 桌面多選摘要複製；4.2.0 查詢計畫診斷；4.3.0 完整 FTS/LIKE/日期查詢計畫；4.4.0 Benchmark；4.6.0 Benchmark resultIds 與真實 CLI acceptance 比對器。不得重複開發這些項目。

技術基準：CLI .NET 10 net10.0；Desktop WinForms net10.0-windows；NuGet DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14；SQLite PRAGMA user_version=1；桌面設定 %LOCALAPPDATA%\\PersonalWiki\\desktop-settings.json。未經我明確批准，不得修改 Schema、DB/settings 路徑或套件版本。

必須維持資料安全：原始 Excel 唯讀；正式匯入預檢、備份驗證、來源雜湊、唯讀快照、單一交易及 quick_check/foreign_key_check；備份／還原／匯出／報告拒絕覆寫；還原只寫新路徑；不自動刪除或合併 entries/notes/tags/source traces；移除圖片關聯不刪原圖；搜尋／查詢計畫／Benchmark ReadOnly；所有測試使用隔離 DB 或副本。

4.6.0 的真實 .NET CLI acceptance 尚未在原開發環境執行（無 dotnet SDK／Windows），接手後先依 TEST_GUIDE_4.6.0.md 在 Windows + .NET 10 執行：python scripts/entry-query-acceptance.py --database samples/PersonalWiki_3.6.0_Test.db。可用 `--case-index 1..15` 單獨重跑案例；報告會保留失敗診斷與重跑指令。先解決驗收問題，再依 4.7.0 開發計畫開發。不得宣稱未實際執行的 .NET/Windows 測試通過。

一次完成一批可獨立開發功能後集中交付，不要反覆要求我逐項測試。每版同步更新 csproj 版本、README、INDEX、UPDATE_NOTES、TEST_GUIDE、AUDIT_REPORT、HANDOFF、Python tests、SHA256 manifest。建立完整 ZIP，固定解壓根目錄 PersonalWiki_ExcelLab，ZIP 檔名可帶版號但資料夾名稱不可帶版號；拒絕覆寫既有 ZIP。驗證 CRC、SHA256、版本與內容後才交付，並提供可直接下載的 ZIP 連結。若上傳 Library，必須確認上傳成功。
```
