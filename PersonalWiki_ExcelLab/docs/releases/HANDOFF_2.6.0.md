# PersonalWiki ExcelLab 2.6.0 開發交接文件

## 版本狀態
- **目前最新版本：2.6.0**
- **下一開發版本：2.6.1**；總目標仍為 3.0.0，按 2.6.x → 2.7.x → 2.8.x → 2.9.x → 3.0.0 累進。
- **唯一基準：本 ZIP 內 `PersonalWiki_ExcelLab/` 完整專案**，由使用者提供的 2.5.8 ZIP 累進整合至 2.6.0。
- ZIP 外層名稱：`PersonalWiki_ExcelLab_2.6.0.zip`；解壓後資料夾固定 `PersonalWiki_ExcelLab`，不得帶版本號。

## 本版完成內容
- 新增 `Services/ImportTransactionGuard.cs`：SQLite 交易提交前執行 `quick_check(1)`、`foreign_key_check`，檢查失敗即丟出錯誤，匯入服務回滾。
- `ExcelDatabaseImportService` 在交易前明確設定 `PRAGMA foreign_keys=ON`，並在 `Commit` 前執行 guard。
- 來源 Excel SHA-256、匯入前備份 SHA-256 改用串流雜湊，降低大型檔案記憶體峰值。
- 安全匯入流程階段與 acceptance 斷言納入提交前檢查。
- SQLite Schema `user_version=1`、NuGet 及資料庫／設定檔位置不變。

## 已完成版本摘要
- **1.3.4**：條目多選、批次封存／還原。
- **1.3.5–1.3.8**：圖片牆篩選排序、圖片完整性／重複報告、工作表欄位動態顯示、各工作表獨立欄位版面。
- **1.4.0–1.5.1**：CLI acceptance、安全備份還原、資料庫健康檢查及桌面備份入口。
- **2.0.0–2.0.5**：CLI／桌面版本整合、搜尋分頁、多版面測試活頁簿、安全輸出、輸入 SHA-256 預檢、唯讀匯入快照及設定檔原子保存。
- **2.1.0**：整合式唯讀資料庫診斷報告。
- **2.5.0**：診斷快照比較。
- **2.5.1–2.5.5**：編譯缺陷修正、桌面按鈕尺寸及路徑列調整。
- **2.5.6**：診斷比較負向案例擴充。
- **2.5.7**：診斷警告差異比較。
- **2.5.8**：診斷快照跨欄位語意驗證。
- **2.6.0**：匯入交易提交前 SQLite 完整性／外鍵安全閘與串流雜湊。

## Schema、相依套件與結構
- SQLite `user_version=1`；Schema 不得未經批准修改。
- CLI：`.NET 10`、`net10.0`。
- Desktop：`.NET 10 Windows Forms`、`net10.0-windows`。
- NuGet：`DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`；本版無新增套件。
- 主要結構：`Program.cs`（CLI、自我測試）、`Models.cs`、`Services/`（匯入、Excel、SQLite、備份、診斷及資料服務）、`PersonalWiki.Desktop/`（WinForms）、`scripts/`（建置及驗收）、`samples/`（測試活頁簿與 JSON）、`docs/releases/`（版本文件）。
- 桌面設定：`%LOCALAPPDATA%\PersonalWiki\desktop-settings.json`；資料庫與設定檔位置不變。

## 資料安全限制（必須保留）
1. 原始 Excel 唯讀；匯入正式資料只讀取經雜湊驗證的唯讀快照。
2. 正式匯入順序：預檢 → 資料庫健康檢查 → 建立並驗證新備份 → 重驗 Excel/mapping/rules 雜湊 → 建立唯讀快照 → 快照重新預檢 → 單一 SQLite 交易匯入 → 提交前 `quick_check(1)`／`foreign_key_check` → 提交 → 匯入後批次驗證。
3. 備份／還原／匯出／報告／範本輸出不得覆蓋已存在檔案；還原必須使用新路徑。
4. 備份／還原既有 staging 仍執行完整 `integrity_check` 及 `foreign_key_check`。
5. 不自動刪除或合併條目、筆記、標籤或來源追蹤資料。
6. 移除圖片關聯不刪除原始圖片。
7. 不擅自變更 Schema、資料庫位置或設定檔位置；若必要先提出 migration、備份與回復方案並等待批准。
8. 自動測試使用隔離資料庫與新測試路徑，不碰正式資料庫。

## 已知問題、未測試及環境限制
- 本容器沒有 `dotnet`、Windows、PowerShell；未執行 restore/build、CLI acceptance、自我測試或 WinForms 實機測試。
- 2.6.0 guard 已整合至正式 Excel import service，但需 Windows/.NET 10 實際確認 PRAGMA 在待提交交易中的行為。
- `quick_check(1)` 是快速檢查；匯入前備份仍保留既有完整 `integrity_check`。
- 300% DPI、多螢幕、XLSM/VBA、Power Query、外部連線、密碼保護工作表及超大型活頁簿仍需實機驗收。

## 下一階段計畫
1. **2.6.1**：在 Windows/.NET 10 執行 restore/build、完整 acceptance、自我測試；修正實際編譯或執行期錯誤。
2. 擴充匯入故障注入測試：交易中途失敗、取消、磁碟空間不足、外鍵違規及提交前完整性檢查失敗，確認 rollback 與備份保留。
3. 驗證備份／還原在 WAL、鎖定、程序中止情境的完整性。
4. **2.7.x**：診斷快照、資料一致性及故障復原強化。
5. **2.8.x**：桌面 UI、DPI、多螢幕及操作體驗驗收。
6. **2.9.x**：搜尋、效能及完整整合測試。
7. **3.0.0**：整合回歸、文件一致性與正式驗收。

## 可直接複製到新對話的完整開發指令
```text
請接續開發 PersonalWiki ExcelLab。唯一基準是我提供的 PersonalWiki_ExcelLab_2.6.0.zip；目前最新版本 2.6.0，下一版必須是 2.6.1，不可回退或從舊版重新開始；整體目標版本為 3.0.0。先閱讀 docs/releases/HANDOFF_2.6.0.md、AUDIT_REPORT_2.6.0.md、UPDATE_NOTES_2.6.0.md、TEST_GUIDE_2.6.0.md、README.md，再檢查原始碼。

保留所有 1.3.4–2.6.0 功能，尤其 2.5.5 主畫面功能按鈕 100×68 預設、自然寬度、資料庫路徑列填滿、兩組按鈕尺寸設定獨立；保留 2.5.6–2.5.8 所有診斷比較與負向 JSON 樣本；保留 2.6.0 ImportTransactionGuard、匯入交易前啟用 foreign_keys、Commit 前 quick_check(1)/foreign_key_check、串流 SHA-256。不得重做、移除或破壞既有功能。

SQLite user_version=1，未經我批准不得修改 Schema。CLI net10.0；桌面 net10.0-windows WinForms；NuGet DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14。新增套件前說明必要性。

資料安全限制：來源 Excel 唯讀；正式匯入必須預檢、健康檢查、建立並驗證新備份、重驗來源 Excel/mapping/rules 雜湊、建立唯讀快照並重新預檢、只從快照交易式匯入、提交前 quick_check(1) 與 foreign_key_check、匯入後批次驗證。備份／還原／匯出／報告／範本不可覆蓋既有檔案；還原使用新路徑；不自動刪除或合併條目、筆記、標籤、來源追蹤；移除圖片關聯不得刪除原始圖片；不變更資料庫或設定檔位置。Schema 若必須修改，先提出 migration、備份與回復計畫並等待批准。

請一次完成一批可獨立開發功能再集中回報，不要每個模組停下來詢問。下一版先在 Windows/.NET 10 執行 restore/build/acceptance/self-test，然後新增交易中途失敗、取消、磁碟空間不足、外鍵違規及提交前檢查失敗的故障注入測試，驗證 rollback 與備份保留。之後依序推進 2.7.x 診斷與復原、2.8.x 桌面 UI/DPI、2.9.x 搜尋效能與整合測試，最後 3.0.0 全面驗收。

每次交付必須更新版本文件、完整原始碼 ZIP、測試資產與交接文件；ZIP 解壓根目錄固定 PersonalWiki_ExcelLab，不帶版本號。必須確認 ZIP CRC 與連結指向真實 ZIP。不可虛報未執行的 build 或 Windows 測試；如環境不支援，明確標示靜態檢查與未測試項目。
```
