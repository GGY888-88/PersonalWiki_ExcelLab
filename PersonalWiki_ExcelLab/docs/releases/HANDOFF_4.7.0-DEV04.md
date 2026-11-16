# PersonalWiki ExcelLab 4.7.0 DEV04 交接

## 版本定位
- 版本：4.7.0 DEV04（開發中，未驗收）
- 基礎來源：GitHub `dev` 分支 `PersonalWiki_ExcelLab_4.7.0_DEV03_repaired.zip` 導入後的 DEV03 原始碼。
- 技術：WinForms / .NET 10 / SQLite。
- NuGet：DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14（未調整）。

## 本版變更
- 新增 `PersonalWiki.Desktop/ExcelImportDryRunDialog.cs`。
- Excel 匯入精靈執行既有 Dry-run 並發現需人工檢查資料時，改以明細視窗顯示問題。
- 問題依工作表／規則分類，顯示匹配筆數及最多 20 個 Excel 列號樣本；選取項目可瀏覽詳細說明，並複製列號。
- 使用者可返回既有規則設定流程，或取消本次匯入；未增加自動忽略／自動修正行為。
- 更新 `MainForm.cs` 僅限替換 Dry-run 問題提示 UI；既有規則編輯、備份及正式匯入呼叫保留。

## 資料與安全
- SQLite `user_version`／Schema：未變更，基準仍為 1。
- 資料庫路徑與桌面設定路徑：未變更。
- Excel 來源仍唯讀；既有預檢、匯入前備份、來源快照驗證及單一 SQLite 交易流程未修改。
- Dry-run 報告原本只提供規則計數與每項最多 20 個列號樣本；新視窗不讀取或保存儲存格原文。
- 不修改圖片原檔及圖片關聯。

## 版本歷史摘要
- 4.7.0 DEV01：標籤使用統計 JSON 報告。
- 4.7.0 DEV02：Excel 匯入批次歷史唯讀檢視。
- 4.7.0 DEV03：Excel 匯入批次差異比較及 JSON 匯出。
- 4.7.0 DEV04：Excel Dry-run 問題明細檢視與列號導覽。

## 三個標準 BAT
- `01_建置程式.bat`
- `02_啟動程式.bat`
- `03_建置並啟動.bat`

## 專案結構
- `PersonalWiki.Desktop/ExcelImportDryRunDialog.cs`：本版新增問題明細視窗。
- `PersonalWiki.Desktop/MainForm.cs`：匯入精靈的 Dry-run 問題提示入口。
- `Services/ImportDryRunService.cs`：既有唯讀 Dry-run 分析服務，本版未修改。
- `docs/releases/`：更新說明、測試指南、稽核及交接文件。
- `samples/`：既有 Excel、mapping、rules 及資料庫測試素材。

## 套件與 Schema
- Desktop：`net10.0-windows`；核心 CLI：`net10.0`。
- DocumentFormat.OpenXml 3.5.1；Microsoft.Data.Sqlite 10.0.14。
- SQLite `user_version=1`；本版不新增資料表、不執行 migration。

## 待處理問題／限制
- Dry-run 每項規則最多提供 20 個列號樣本；超過部分只顯示剩餘筆數，不會逐筆列出。
- 視窗不顯示儲存格原文，使用者需依工作表及列號回到來源 Excel 檢查。
- 尚未在 Windows／.NET 10 環境編譯或實機驗收；DPI、剪貼簿及多工作表長清單仍待集中測試。

## 測試狀態
- 未執行 Python 測試、.NET restore/build、CLI acceptance、程式啟動或 Windows 實機測試。
- 此狀態符合目前先集中開發、後集中測試的安排；不可將靜態檢視或 ZIP 完整性檢查視為功能驗收。

## 下一版本
- 4.7.0 DEV05：依 5.0.0 路線持續整理下一批互相獨立的知識管理／Excel 資料管理功能；先盤點現有服務與使用者已確認需求，不預先改動 Schema 或資料安全流程。
- 4.8.0：知識管理效率強化。
- 4.9.0：Excel／資料維護效率強化。
- 5.0.0：整合、集中測試、修正、文件與正式發行包。

## 完整續開發指令
以本版 ZIP 解壓後的 `PersonalWiki_ExcelLab` 作為唯一來源，後續從 4.7.0 DEV04 延續至 5.0.0。先閱讀本交接、更新說明、測試指南及現有程式碼；不得重做或破壞已完成功能。除非使用者明確同意，不得變更 SQLite Schema／user_version、資料庫路徑、桌面設定路徑、Excel 匯入預檢、來源唯讀、匯入前備份、來源快照驗證及交易安全流程。先完成一批互相獨立功能，再集中安排測試；在使用者要求測試前不得執行測試、建置或啟動。每版保留三個標準 BAT，更新 README、發行索引、更新說明、測試指南、稽核及交接文件。完成後產出真實可下載 ZIP，解壓根目錄固定為 `PersonalWiki_ExcelLab`，ZIP 外附 SHA-256；記錄套件、Schema、結構、問題、測試狀態及完整續開發指令。
