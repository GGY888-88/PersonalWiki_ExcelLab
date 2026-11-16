# PersonalWiki ExcelLab 3.0.0 完整開發交接文件

## 1. 唯一版本基準
- **目前最新版本：3.0.0**。
- **下一開發版本：3.0.1**；只能從本文件同包的 `PersonalWiki_ExcelLab/` 專案繼續，禁止回退到 2.6.1、2.5.8 或更早版本重新開發。
- **正式基準壓縮檔：`PersonalWiki_ExcelLab_3.0.0.zip`**。解壓後根目錄固定 `PersonalWiki_ExcelLab`，不可在根目錄再套一層版本號。
- 本次 3.0.0 由使用者提供的 2.5.8 累進專案及 2.6.0、2.6.1 已交付整合版接續而來。
- 本文件、`UPDATE_NOTES_3.0.0.md`、`TEST_GUIDE_3.0.0.md`、`AUDIT_REPORT_3.0.0.md` 與 `scripts/verify-release.py` 必須與專案原始碼一起保留。

## 2. 3.0.0 完成內容
- CLI 與 WinForms 專案版本同步為 3.0.0。
- JSON 匯入預檢、匯入執行、匯入驗證及匯出驗證，改用 `FileStream` + `JsonDocument.Parse(Stream)`，避免整份 JSON 額外複製成 byte[]；原有 MaxDepth=64、格式及資料驗證不變。
- 新增跨平台 `scripts/verify-release.py`，無第三方 Python 套件依賴，可檢查版本、框架、套件、發行文件、JSON/XLSX 樣本、串流解析路徑及核心匯入安全閘。
- 更新 README、發行索引、更新說明、測試指南、稽核報告及本交接文件。
- 保留完整既有功能及測試素材，未移除舊版文件。

## 3. 已完成版本與功能摘要（避免重複開發）
- **1.3.4**：條目多選、批次封存／還原。
- **1.3.5–1.3.8**：圖片牆篩選排序、圖片完整性／重複報告、工作表欄位動態顯示、各工作表獨立欄位版面。
- **1.4.0–1.5.1**：CLI acceptance、安全備份還原、資料庫健康檢查、桌面備份入口。
- **2.0.0–2.0.5**：CLI／桌面整合、搜尋分頁、多版面測試活頁簿、安全輸出、輸入 SHA-256 預檢、唯讀匯入快照、設定檔原子保存及自我測試。
- **2.1.0**：整合式唯讀資料庫診斷報告。
- **2.5.0**：診斷快照比較。
- **2.5.1–2.5.5**：編譯缺陷修正、桌面 UI 調整、主畫面功能按鈕預設 100×68（96 DPI 基準）、按鈕自然寬度、資料庫路徑列填滿、主畫面與輸入視窗按鈕尺寸設定分離。
- **2.5.6**：診斷比較負向案例擴充。
- **2.5.7**：診斷警告差異比較。
- **2.5.8**：診斷快照跨欄位語意驗證。
- **2.6.0**：匯入交易提交前 `quick_check(1)`／`foreign_key_check` 安全閘、匯入前啟用 foreign_keys、來源／備份串流 SHA-256。
- **2.6.1**：備份安全報告重新驗證改用 128 KiB 串流 SHA-256，讀取採 `FileShare.Read`。
- **3.0.0**：正式版號整合、四條 JSON 大檔案讀取路徑改用 Stream Parse、跨平台發行靜態驗證器、完整交接文件。

## 4. 專案結構
- `Program.cs`：CLI 命令入口、自我測試及版本資訊。
- `Models.cs`：核心模型。
- `Services/`：Excel 工作簿讀寫與驗證、匯入預檢／快照／清理／交易、SQLite 資料服務、備份還原、診斷、稽核、報告、搜尋及安全輸出。
- `PersonalWiki.Desktop/`：WinForms 桌面程式、主視窗、匯入對話框、標籤管理及桌面設定。
- `scripts/`：`run-acceptance-tests.ps1`、`run-self-test.ps1`、`verify-release.py`。
- `samples/`：多版面 Excel、匯入 Excel、診斷 JSON 樣本。
- `docs/releases/`：逐版更新說明、測試指南、稽核報告、交接文件及 UI 測試矩陣。

## 5. Schema、框架及套件
- SQLite `PRAGMA user_version=1`；**未經使用者明確批准，不得修改 Schema**。
- CLI：`.NET 10`，`net10.0`。
- Desktop：`.NET 10 Windows Forms`，`net10.0-windows`，`EnableWindowsTargeting=true`。
- NuGet：`DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`。
- 3.0.0 沒有新增 NuGet 套件。
- 桌面設定檔位置：`%LOCALAPPDATA%\PersonalWiki\desktop-settings.json`；資料庫及設定檔位置不可擅自改動。

## 6. 不可破壞的資料安全限制
1. Source Excel 唯讀；匯入正式資料只能讀取已雜湊驗證的唯讀快照。
2. 正式匯入順序：preflight → DB health check → 建立並驗證新備份 → 重驗 Excel/mapping/rules 雜湊 → 建立唯讀快照 → 快照重新 preflight → 單一 SQLite transaction 匯入 → 提交前 `quick_check(1)`／`foreign_key_check` → commit → 匯入後驗證。
3. SQLite 匯入連線必須啟用 `PRAGMA foreign_keys=ON`；提交前完整性安全閘不可移除或降級。
4. 備份／還原／匯出／報告／範本輸出不得覆蓋已存在檔案；還原只能寫入新路徑。
5. 備份／還原 staging 保留完整 `integrity_check` 及 `foreign_key_check`。
6. 不自動刪除或合併條目、筆記、標籤或來源追蹤資料。
7. 移除圖片關聯不得刪除原始圖片。
8. 不變更 DB／設定檔路徑；不修改 SQLite Schema。若確實必要，先提出 migration、備份、回復及相容計畫，等待使用者批准。
9. 測試一律使用隔離資料庫及全新輸出路徑，不得操作使用者正式資料。
10. 設定檔／mapping／rules 既有檔案只有在明確編輯流程中才可透過唯一暫存檔原子替換；不得靜默覆蓋損毀設定。

## 7. 驗證狀態、已知問題與環境限制
### 已執行
- Python 靜態發行驗證器：專案版號、TargetFramework、NuGet、文件存在、11 個 JSON 樣本解析、2 個 XLSX ZIP CRC、四個 JSON Stream Parse 路徑、匯入交易安全閘檢查。
- ZIP 壓縮包本身需於交付前再執行 `unzip -t`，並確認解壓根目錄只有 `PersonalWiki_ExcelLab/`。

### 尚未執行／需實機驗收
- 本次建置環境沒有 .NET SDK、Windows、PowerShell；未執行 `dotnet restore/build`、CLI acceptance、自我測試及 WinForms 實機測試。
- 交易中途失敗、取消、磁碟空間不足、外鍵違規及提交前檢查失敗的故障注入，仍需 .NET 10 執行期驗證。
- WAL／資料庫鎖定／程序中止時備份還原驗收仍待完成。
- 100%、125%、150%、200%、300% DPI、多螢幕及不同縮放比驗收仍待 Windows 實機執行。
- XLSM/VBA、Power Query、外部連線、密碼保護工作表、超大型 Excel／JSON 實際效能仍待測試。
- `JsonDocument.Parse(Stream)` 只避免整份檔案額外複製為 byte[]；`JsonDocument` 仍建立 DOM，並非逐筆常數記憶體解析。

## 8. 下一階段開發計畫
1. **3.0.1（下一版）**：在 Windows/.NET 10 完成 restore/build、CLI self-test、acceptance；修復實際發現的編譯及執行期問題。
2. 新增可重複的交易故障注入案例，驗證 rollback、備份保留及來源不變。
3. 驗證 WAL／鎖定／程序中止下的備份還原可靠性。
4. 完成桌面 DPI、多螢幕、匯入／搜尋／備份／還原完整 UI 回歸。
5. 依實測結果進行效能修正；任何 Schema 或資料位置變更都必須先取得使用者批准。

## 9. 新對話可直接複製的完整開發指令
```text
請直接接續 PersonalWiki ExcelLab 3.0.0，不要重新詢問已確認需求。唯一基準是本次提供的 PersonalWiki_ExcelLab_3.0.0.zip，或本 ZIP 解壓後的完整 PersonalWiki_ExcelLab 專案。最新版本明確為 3.0.0，下一個開發版本必須是 3.0.1；不可回退到 2.6.1、2.5.8 或更早版本重新開始。請先閱讀 docs/releases/HANDOFF_3.0.0.md、UPDATE_NOTES_3.0.0.md、TEST_GUIDE_3.0.0.md、AUDIT_REPORT_3.0.0.md、README.md，再檢查完整原始碼。

保留 1.3.4–3.0.0 全部既有功能與樣本。特別保留 2.5.5 主畫面功能按鈕 100×68 預設、文字自然寬度、資料庫路徑列填滿、主畫面與輸入視窗按鈕尺寸設定分離；保留 2.5.6–2.5.8 所有診斷快照比較、警告差異、語意驗證及負向 JSON 樣本；保留 2.6.0 ImportTransactionGuard、foreign_keys=ON、提交前 quick_check(1)/foreign_key_check、來源及備份串流 SHA-256；保留 2.6.1 備份安全報告 128 KiB 串流雜湊與 FileShare.Read；保留 3.0.0 四個 JSON Stream Parse 路徑及 verify-release.py。不得移除、重做或破壞任何已完成功能。

技術環境：CLI net10.0；Desktop net10.0-windows WinForms；NuGet DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14。未經必要性說明與使用者同意，不新增 NuGet 套件。SQLite PRAGMA user_version=1，未經使用者明確批准不得修改 Schema。

資料安全規則：Source Excel 唯讀；正式匯入必須 preflight、DB health check、建立並驗證新備份、重驗 Excel/mapping/rules 雜湊、建立唯讀快照並重新 preflight、只從快照以單一交易匯入、提交前 quick_check(1) 與 foreign_key_check、匯入後驗證。備份／還原／匯出／報告／範本不得覆蓋既有檔案；還原使用新路徑；不自動刪除或合併條目、筆記、標籤、來源追蹤；移除圖片關聯不刪原始圖片；不變更 DB／設定檔位置。任何 Schema 變更先提出 migration、備份、rollback 計畫並等待批准。測試只使用隔離 DB 與新路徑。

開發工作方式：一次完成一批獨立功能再集中回報，不要逐模組停下來詢問。先在 Windows/.NET 10 執行 dotnet restore/build、scripts/run-self-test.ps1、scripts/run-acceptance-tests.ps1；若當前環境沒有 SDK/Windows，明確記錄限制，不可假稱通過。接著完成交易中途失敗、取消、磁碟空間不足、外鍵違規、提交前完整性失敗、WAL/鎖定/程序中止測試，再完成 UI DPI／多螢幕與整體回歸。

每次交付需更新 csproj/CLI/Desktop 版本資訊、README、docs/releases/INDEX.md、更新說明、測試指南、稽核報告及完整交接文件；提供完整原始碼 ZIP 及樣本測試資產。ZIP 外層檔名可帶版本，但解壓根目錄固定 PersonalWiki_ExcelLab（不得含版本號）。打包後必須執行 ZIP CRC 檢查、檢查根目錄、確認實際檔案存在及下載連結指向該 ZIP。不可虛報未執行的編譯或 Windows 驗收。交接文件必須同步最新版本、下一版、ZIP、版本功能歷史、Schema、套件、專案結構、已知問題、未測試項目、環境限制、資料安全限制與可直接複製的新對話指令，確保離開本對話仍能從 3.0.0 直接接續。
```
