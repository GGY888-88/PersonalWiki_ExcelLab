# PersonalWiki ExcelLab 2.0.1 開發交接文件

## 目前版本與下一版本
- **目前最新版本：2.0.1**
- **下一個開發版本：2.0.2**（不可回退或從舊版重新開始）
- **唯一基準：本 ZIP 內 `PersonalWiki_ExcelLab/` 完整專案**，從 2.0.0 累進修正。
- ZIP 外層檔名：`PersonalWiki_ExcelLab_2.0.1.zip`；解壓後資料夾固定 `PersonalWiki_ExcelLab`，資料夾名稱不可帶版本號。

## 版本歷史（已完成）
- **1.3.4**：條目多選、批次封存／還原。
- **1.3.5**：圖片牆篩選與排序。
- **1.3.6**：圖片完整性與 SHA-256 重複圖片報告，不自動刪除。
- **1.3.7**：Excel 工作表動態欄位顯示。
- **1.3.8**：同一工作表共用欄位版面，不同工作表獨立欄位名稱及順序，缺值留白。
- **1.3.9**：驗收腳本路徑、publish 目錄保護及版本文件修正。
- **1.4.0**：CLI 整合驗收及安全負向測試矩陣。
- **1.4.5**：備份／還原安全提交強化。
- **1.5.0**：備份／還原 staging、SQLite 完整性與外鍵驗證、拒絕覆蓋、逐表筆數驗證及測試矩陣。
- **1.5.1**：桌面資料庫健康檢查與立即安全備份入口。
- **2.0.0**：CLI/桌面版號整合；筆記正文及自訂欄位搜尋回歸、分頁斷言；多工作表不同版面測試活頁簿。
- **2.0.1**：修正 ZIP 套件清單 `snapshot` 輸出覆寫既有目的檔的問題，改用 `FileMode.CreateNew`；加入 SHA-256 負向測試。

## 2.0.1 本次修改檔案
- `Services/PackageIntegrity.cs`：`SaveManifest` 使用 `FileMode.CreateNew`，不覆寫既有 JSON。
- `Program.cs`：CLI help、自我測試報告版本更新為 2.0.1。
- `PersonalWiki.ExcelLab.csproj`、`PersonalWiki.Desktop/PersonalWiki.Desktop.csproj`：版本 2.0.1。
- `scripts/run-acceptance-tests.ps1`：新增既有套件 manifest 不覆寫的負向測試。
- `README.md`、`UPDATE_NOTES_2.0.1.md`、`TEST_GUIDE_2.0.1.md`、`AUDIT_REPORT_2.0.1.md`。

## 專案結構
- `Program.cs`：CLI 路由、自我測試及 JSON 報告。
- `Models.cs`：共用模型。
- `Services/`：Excel 讀寫副本、預檢、mapping、Dry-run、交易匯入、SQLite 備份還原、健康檢查、索引、條目／筆記／標籤／圖片與資料稽核。
- `PersonalWiki.Desktop/`：Windows Forms 桌面端。
- `scripts/`：CLI／桌面建置、portable 發布、自我測試及整合驗收。
- `samples/`：`PersonalWiki_Import_Sample.xlsx`、`PersonalWiki_MultiLayout_Test.xlsx` 等測試活頁簿。

## 資料庫 Schema 與套件
- SQLite `user_version=1`，2.0.1 無 Schema 變更、無 migration。
- CLI `net10.0`；桌面 `net10.0-windows`、Windows Forms。
- NuGet：`DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`。無新增套件。
- 桌面設定位於 `%LOCALAPPDATA%\PersonalWiki\desktop-settings.json`，與資料庫分離。

## 必須遵守的資料安全限制
1. 原始 Excel 唯讀；寫入只允許使用者明確指定的新副本。
2. 正式匯入必須預檢、健康檢查、建立並驗證新備份、交易式匯入及批次驗證。
3. 備份／還原／匯出／報告不得覆蓋既有目的檔；還原使用新路徑。新增輸出功能一律採 `FileMode.CreateNew` 或等效拒絕覆寫方式。
4. 備份／還原 staging 通過 SQLite `integrity_check` 與 `foreign_key_check` 才提交。
5. 不自動刪除或合併條目、筆記、標籤或來源追溯資料。
6. 移除圖片關聯只移除資料庫關聯，不刪除／移動硬碟原圖。
7. 不擅自修改 Schema、資料庫位置或設定檔位置；若必要，先提出 migration、備份與回復方案並等使用者確認。
8. 所有自動化測試使用新建隔離資料庫與測試檔，不碰正式資料庫。

## 已知問題、未測試及環境限制
- 本開發容器沒有 `dotnet`、Windows、PowerShell；未執行 .NET restore/build、CLI acceptance、self-test 或 WinForms 實機測試。
- 300% DPI、多螢幕、縮放、視窗尺寸、工作表欄位顯示、批次封存／還原仍待 Windows 實機驗收。
- 2.0.1 新增的 snapshot 負向測試尚未執行。
- XLSM/VBA 巨集、Power Query、外部連線、密碼保護工作表、內嵌物件及超大型活頁簿未全面驗證。
- `啟動 PersonalWiki.bat` 使用 `dotnet run --project .\PersonalWiki.Desktop\PersonalWiki.Desktop.csproj -c Release`，執行端需 .NET 10 SDK。

## 下一階段計畫（2.0.2 起）
1. 在 Windows/.NET 10 執行 restore/build 與完整 acceptance，修正實際發現的問題。
2. 稽核所有報告／匯出／快照輸出點，統一拒絕覆寫並為每個輸出分支加負向測試。
3. 補強匯入預檢失敗、來源檔預檢後變更、備份目的檔已存在等隔離測試。
4. 進行多工作表匯入精靈與 300% DPI 實機驗收。
5. 評估 portable WinForms 發布，不改變資料庫及設定檔位置。

## 可直接複製到新對話的完整開發指令
```text
請接續開發 PersonalWiki ExcelLab，必須以我提供的 PersonalWiki_ExcelLab_2.0.1.zip 為唯一基準。最新版本 2.0.1，下一版必須是 2.0.2，不可回退或從舊版重新開始。先閱讀 HANDOFF_2.0.1.md、AUDIT_REPORT_2.0.1.md、README.md、UPDATE_NOTES_2.0.1.md、TEST_GUIDE_2.0.1.md，再檢查原始碼。

保留 1.3.4 至 2.0.1 全部已完成功能，尤其批次封存／還原、圖片篩選排序與完整性報告、工作表各自獨立欄位名稱及順序、缺值留白、Excel 預檢/mapping/Dry-run/交易匯入、SQLite 備份還原 staging 安全提交、健康檢查、全文搜尋及分頁、資料稽核，以及 2.0.1 的 snapshot 不覆寫既有目的檔修正。不得重做、移除或破壞已完成部分。

SQLite user_version=1，不得未經同意修改 Schema。CLI net10.0；桌面 net10.0-windows Windows Forms。NuGet 僅 DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14；新增套件前先說明必要性。

安全限制：來源 Excel 唯讀；正式匯入前必須預檢、健康檢查、建立並驗證新備份、交易式匯入及批次驗證；所有備份/還原/匯出/報告不得覆蓋既有檔案；還原使用新路徑；不自動刪除或合併條目、筆記、標籤或來源追溯資料；移除圖片關聯不得刪除原始圖片；不得擅自變更資料庫及設定檔位置。任何 Schema 變更先提出 migration、備份及回復計畫並等待確認。

先完成一批可獨立開發功能再集中測試，不要每一小版要求我測試。每次交付必須實際修改程式、更新版本文件與交接資料，將完整原始碼、最新 ZIP、測試活頁簿、測試腳本、Schema/套件/專案結構、已知問題、未測試項目、安全規則及下一版任務一起打包。ZIP 解壓後資料夾固定 PersonalWiki_ExcelLab，不帶版本號。不可虛報未執行的 build 或 Windows 測試。

2.0.2 優先工作：在 Windows/.NET 10 執行 restore/build/acceptance；稽核所有輸出路徑拒絕覆寫；補強 Excel 匯入預檢與來源檔變更安全測試；維持 SQLite Schema v1 及所有既有安全行為。
```

## 打包規則
每次版本 ZIP 必須包含完整專案原始碼、最新交接文件、版本歷史、Schema 與套件資訊、專案結構、測試資料與測試方式、已知問題／未測試／環境限制、安全規則、下一階段任務及可直接複製的新對話開發指令。外層 ZIP 可帶版本號，內層資料夾固定 `PersonalWiki_ExcelLab`。
