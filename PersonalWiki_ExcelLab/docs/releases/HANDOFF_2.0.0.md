# PersonalWiki ExcelLab 2.0.0 開發交接文件

## 目前版本與基準
- **目前最新版本：2.0.0**
- **下一開發版本：2.0.1**
- **唯一基準：本 ZIP 內 `PersonalWiki_ExcelLab/` 完整專案**，由使用者提供的 1.5.1 原始碼累進修改。
- **最新壓縮檔：`PersonalWiki_ExcelLab_2.0.0.zip`**
- 外層 ZIP 解壓後資料夾固定為 `PersonalWiki_ExcelLab`，不可將版本號加到資料夾名稱。

## 已完成版本與功能
- **1.5.1**：桌面資料庫健康檢查及立即安全備份入口；備份採既有 Online Backup／staging 驗證流程。
- **1.5.0**：備份／還原 staging、SQLite 完整性與外鍵驗證、拒絕覆蓋、復原逐表筆數驗證、CLI 回歸測試與 UI 測試矩陣。
- **1.4.5**：備份／還原安全提交強化。
- **1.4.0**：CLI 整合驗收及安全負向測試矩陣。
- **1.3.9**：驗收腳本路徑、publish 目錄保護及版本文件修正。
- **1.3.8**：同一 Excel 工作表共用欄位版面，不同工作表獨立欄位名稱與順序，缺值留白。
- **1.3.7**：Excel 工作表動態欄位顯示。
- **1.3.6**：圖片完整性與 SHA-256 重複圖片報告，不自動刪除。
- **1.3.5**：圖片牆篩選與排序。
- **1.3.4**：條目多選、批次封存／還原。
- **2.0.0**：版本整合；CLI/桌面版本及 help/self-test 版號同步；新增筆記正文與自訂欄位內容搜尋回歸、分頁斷言；新增多工作表不同版面測試活頁簿。

## 2.0.0 本次改動檔案
- `PersonalWiki.ExcelLab.csproj`、`PersonalWiki.Desktop/PersonalWiki.Desktop.csproj`：版本 2.0.0。
- `Program.cs`：help 與 self-test 報告版號同步 2.0.0。
- `scripts/run-acceptance-tests.ps1`：新增搜尋回歸測試（筆記正文、自訂欄位值、分頁上限），並回傳 CLI 輸出供 JSON 斷言。
- `samples/PersonalWiki_MultiLayout_Test.xlsx`：三張不同版面工作表及缺值樣本。
- `README.md`、`UPDATE_NOTES_2.0.0.md`、`TEST_GUIDE_2.0.0.md`、`AUDIT_REPORT_2.0.0.md`。

## 專案結構
- `Program.cs`：CLI 路由、自我測試、JSON 報告。
- `Models.cs`：共用模型。
- `Services/`：Excel 讀取／寫入副本、預檢、mapping、Dry-run、交易匯入、SQLite 備份還原、健康檢查、索引、條目／筆記／標籤／圖片及資料稽核。
- `PersonalWiki.Desktop/`：Windows Forms 桌面端。
- `scripts/`：CLI／桌面建置、portable 發布、自我測試及整合驗收。
- `samples/`：Excel 匯入測試活頁簿，包含原有 `PersonalWiki_Import_Sample.xlsx` 及本版新增多版面樣本。

## 資料庫 Schema 與相依套件
- SQLite `user_version=1`，**未變更 Schema、無 migration**。
- CLI：`net10.0`；桌面：`net10.0-windows`、Windows Forms。
- `DocumentFormat.OpenXml 3.5.1`。
- `Microsoft.Data.Sqlite 10.0.14`。
- 無新增 NuGet 套件。

## 已保留的資料安全限制
1. 原始 Excel 唯讀；如需寫入，只能寫入使用者明確指定的新副本。
2. 正式 Excel 匯入必須預檢、健康檢查、建立並驗證新備份、交易式匯入及批次驗證。
3. 備份／還原／匯出／報告不得覆蓋既有目的檔；還原使用新路徑。
4. 備份／還原採 staging，通過 SQLite integrity_check 與 foreign_key_check 才提交。
5. 不自動刪除或合併條目、筆記、標籤或來源追溯資料。
6. 移除圖片關聯只移除資料庫關聯，不刪除或移動硬碟原圖。
7. 不擅自修改 Schema、資料庫位置或設定檔位置；若未來確有必要，先提出 migration、備份及回復方案。
8. 所有自動化測試使用新建隔離資料庫與測試檔，不碰正式資料庫。

## 已知問題、未測試與環境限制
- 本次容器沒有 `dotnet`、PowerShell／Windows；未能執行 restore/build、PowerShell acceptance、自我測試或 WinForms 實機測試。
- UI DPI、多螢幕、縮放、工作表欄位顯示及批次封存仍待 Windows 實機驗收。
- 新增的 2.0.0 多工作表樣本已建立並靜態檢查工作表名稱、欄位及缺值；尚未透過 Windows 匯入精靈實際匯入。
- XLSM/VBA 巨集執行、Power Query、外部連線、密碼保護工作表、內嵌物件及超大型活頁簿未全面驗證。
- `啟動 PersonalWiki.bat` 仍使用 `dotnet run --project .\PersonalWiki.Desktop\PersonalWiki.Desktop.csproj -c Release`，執行端需 .NET 10 SDK。

## 下一階段開發計畫（2.0.1 起）
1. 在 Windows/.NET 10 執行 restore/build 與 `scripts/run-acceptance-tests.ps1 -OutputRoot .\AcceptanceTestOutput_200`，修正實際發現的問題。
2. 使用隨附多工作表樣本驗收每張工作表欄位順序、獨立版面與缺值留白。
3. 完成 300% DPI、多螢幕、視窗縮放及批次封存／還原實機驗收。
4. 針對 Excel 匯入預檢失敗、備份目的檔已存在、來源檔預檢後變更等分支補強隔離 fixture。
5. 評估 portable WinForms 發布，不得改變既有資料庫／設定檔位置。

## 可直接複製到新對話的完整開發指令

```text
請接續開發 PersonalWiki ExcelLab，必須以我提供的 PersonalWiki_ExcelLab_2.0.0.zip 為唯一基準。
目前最新版本是 2.0.0，下一個開發版本必須是 2.0.1；不可回退至 1.5.1 或更早版本重新開發。先閱讀 HANDOFF_2.0.0.md、AUDIT_REPORT_2.0.0.md、README.md、UPDATE_NOTES_2.0.0.md、TEST_GUIDE_2.0.0.md 及相關原始碼。

保留 1.3.4 至 2.0.0 已完成的功能，尤其批次封存／還原、圖片篩選排序與完整性報告、工作表各自獨立欄位名稱及順序、缺值留白、Excel 預檢/mapping/Dry-run/交易匯入、SQLite 備份還原 staging 安全提交、健康檢查、搜尋及分頁、資料稽核與 2.0.0 新增的搜尋回歸測試。不得重做或移除已完成部分。

SQLite user_version=1，不得未經同意修改 Schema。CLI net10.0；桌面 net10.0-windows Windows Forms。NuGet 僅 DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14，未經必要性說明及同意不得新增套件。

資料安全限制：來源 Excel 唯讀；正式匯入前必須預檢、健康檢查、建立並驗證新備份、交易式匯入及批次驗證；備份/還原/匯出/報告不得覆蓋既有檔案；還原使用新路徑；不自動刪除或合併條目、筆記、標籤或來源追溯資料；移除圖片關聯不得刪除原始圖片；不得擅自變更資料庫及設定檔位置。任何 Schema 變更必須先提出 migration、備份及回復計畫並等待確認。

使用者偏好先完成一批獨立功能再集中測試，不要每一小版要求測試。先在可用環境執行 restore/build、CLI acceptance 及靜態檢查；沒有 Windows/.NET 10 時必須明確說明，不能宣稱通過。若有新版本，必須實際修改程式、更新所有版本文件與交接資訊、提供完整 ZIP 及測試檔；ZIP 解壓後資料夾固定 PersonalWiki_ExcelLab，不帶版本號。下一版必須從 2.0.0 原始碼繼續。
```

## 交付確認
- ZIP 內保留完整專案、原有測試檔與新增 `samples/PersonalWiki_MultiLayout_Test.xlsx`。
- 版本號與交接文件一致：2.0.0；下一版 2.0.1。
- 本版未宣稱 Windows build 或桌面實機測試通過。
