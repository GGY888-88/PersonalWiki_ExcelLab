# PersonalWiki ExcelLab 開發交接文件

## 目前版本
- **目前最新版本：1.5.0**
- **下一開發版本：1.5.1**
- **唯一開發基準：本 ZIP 內 `PersonalWiki_ExcelLab/` 完整專案**
- **最新壓縮檔：`PersonalWiki_ExcelLab_1.5.0.zip`**
- 不可從 1.3.9 或更早版本重新開始；直接以本版原始碼修改。

## 版本功能摘要
- **1.3.4**：條目多選、批次封存／還原，保留筆記及圖片關聯。
- **1.3.5**：圖片牆篩選與排序。
- **1.3.6**：圖片完整性及 SHA-256 重複圖片報告，不自動刪除。
- **1.3.7**：自訂欄位依 Excel 工作表呈現標題及順序。
- **1.3.8**：同工作表條目共用欄位版面，缺值顯示空白，不同工作表各自獨立。
- **1.3.9**：驗收腳本路徑修正、建置腳本拒絕覆蓋既有 publish 目錄、版本與文件同步。
- **1.4.0**：CLI 整合驗收增加備份／還原、索引及安全負向測試矩陣。
- **1.4.5**：SQLite 備份／還原改為 staging、完整性與外鍵驗證後提交；同路徑拒絕；失敗清理暫存檔。
- **1.5.0**：整合 1.4.0／1.4.5、回歸測試腳本、UI 驗收矩陣、稽核報告及交接文件。

## 專案結構
- `Program.cs`：CLI 命令路由、自我測試及 JSON 報告。
- `Models.cs`：共用資料模型。
- `Services/`：Excel 讀取／寫入副本、匯入預檢／mapping／Dry-run／交易匯入、SQLite 備份還原、健康檢查、搜尋索引、條目／筆記／標籤／圖片與資料稽核。
- `PersonalWiki.Desktop/`：Windows Forms；`MainForm.cs`、`ImportDialogs.cs`、`TagManagementDialog.cs`、`DesktopSettingsService.cs`。
- `scripts/`：CLI／桌面建置、portable 發布、自我測試及整合驗收。
- `samples/`：Excel 匯入測試活頁簿。
- `UPDATE_NOTES_*.md`／`.txt`：歷史更新紀錄。
- `TEST_GUIDE_1.5.0.md`：CLI 及桌面測試指南。
- `DESKTOP_UI_TEST_MATRIX_1.5.0.md`：Windows UI 測試矩陣。
- `AUDIT_REPORT_1.5.0.md`：本版稽核與限制。

## 資料庫及相依套件
- SQLite `user_version=1`，無 Schema migration。
- CLI：`PersonalWiki.ExcelLab.csproj`，`net10.0`。
- 桌面：`PersonalWiki.Desktop/PersonalWiki.Desktop.csproj`，`net10.0-windows`、Windows Forms。
- `DocumentFormat.OpenXml 3.5.1`。
- `Microsoft.Data.Sqlite 10.0.14`。
- 無新增 NuGet 套件。

## 已完成且不得破壞的資料安全限制
1. 原始 Excel 唯讀；若需寫入，只能寫入使用者明確指定的新副本。
2. 正式 Excel 匯入必須保留預檢、資料庫健康檢查、新備份建立與驗證、交易式匯入、批次驗證流程。
3. 備份／還原／匯出／報告不得覆蓋既有目的檔；目的檔已存在即停止。
4. 備份／還原採 staging；通過 SQLite integrity_check 與 foreign_key_check 才提交最終檔。
5. 不自動刪除或合併條目、筆記、標籤或來源追溯資料。
6. 移除圖片關聯只移除資料庫關聯，不刪除或移動硬碟原始圖片。
7. 不擅自修改 SQLite Schema、資料庫位置或設定檔位置；若必要，先提出 migration、備份與回復方案。
8. 不重做或移除已完成版本功能；局部修改並補回歸測試。
9. 測試只使用新建測試資料庫及測試檔，不碰唯一正式資料庫。
10. 使用者目前無暇測試，不要要求立即測試；先自行完成能做的檢查，清楚列出環境無法執行項目。

## 已知限制及未測試項目
- 開發容器沒有 .NET 10 SDK、PowerShell Core 或 Windows；1.5.0 未執行 dotnet restore/build、CLI 驗收及 WinForms 實機測試。
- UI DPI、多螢幕、縮放、工作表欄位及批次封存已列矩陣但未實測。
- XLSM/VBA 巨集執行、Power Query、外部連線、密碼保護工作表、內嵌物件與超大型活頁簿未全面驗證。
- 桌面啟動批次檔仍使用 `dotnet run`，執行端需 .NET 10 SDK；免 SDK 發布包未驗收。
- 服務層部分舊程式碼較精簡，後續應分批整理，不宜大規模重構。

## 下一階段建議（1.5.1 起）
1. 在可用的 Windows／.NET 10 環境執行 restore/build 及 CLI acceptance，修正實際發現的編譯或執行錯誤。
2. 完成 UI DPI、多螢幕及批次封存人工驗收；可用時再考慮 UI 自動化。
3. 為 Excel 匯入 workflow 建立更多隔離 fixture 回歸測試，包含預檢失敗、備份目的檔存在、來源檔於預檢後變更等分支。
4. 評估 portable WinForms 發布方式，但不得改變既有資料庫／設定位置。
5. 分批整理舊服務層可讀性，任何重構均須保留既有 CLI 與桌面行為。

## 可直接複製到新對話的完整開發指令

```text
請接續開發 PersonalWiki ExcelLab。

目前最新版本是 1.5.0，下一個開發版本必須是 1.5.1。請以我提供的 PersonalWiki_ExcelLab_1.5.0.zip 為唯一原始碼基礎，直接在該版本修改，不可從舊版本重新開始。先閱讀 HANDOFF_1.5.0.md、AUDIT_REPORT_1.5.0.md、README.md、UPDATE_NOTES_1.5.0.md、TEST_GUIDE_1.5.0.md 及相關原始碼。

保留所有 1.3.4 至 1.5.0 已完成的功能，尤其批次封存／還原、圖片篩選排序與完整性報告、工作表欄位呈現、備份／還原 staging 安全提交、SQLite 完整性與外鍵驗證、拒絕覆蓋既有檔案及新增的 CLI 回歸測試。不得擅自重做或移除已完成部分。

目前 SQLite user_version=1；CLI net10.0；桌面 net10.0-windows Windows Forms。相依套件 DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14。未經確認不得新增套件或變更 Schema。

資料安全限制：原始 Excel 唯讀；正式匯入前必須預檢、健康檢查、建立並驗證新備份、交易式匯入及批次驗證；備份／還原／匯出／報告不得覆蓋既有檔案；不自動刪除或合併條目、筆記、標籤或來源追溯資料；移除圖片關聯不得刪除硬碟原圖；不得擅自改資料庫或設定檔位置。任何 Schema 變更先提出 migration、備份及回復計畫。

使用者目前沒有時間測試，不要要求使用者現在測試。先在開發環境完成可執行的靜態檢查、自動化測試及封裝檢查；環境無法執行的項目必須如實標示，不可宣稱通過。

下一階段先以 Windows/.NET 10 執行 1.5.0 已附的 CLI 回歸測試；補充 Excel 匯入 workflow 的失敗分支 fixture；完成 UI DPI、多螢幕及批次封存驗收；再評估 portable WinForms 發布。每次交付必須更新版本號、README、更新說明、測試指南、稽核及交接文件，提供完整 ZIP。ZIP 最外層資料夾固定 PersonalWiki_ExcelLab，不可帶版本號；壓縮檔可帶版本號。確認下載連結是真正 ZIP，不可用網頁代替。
```
