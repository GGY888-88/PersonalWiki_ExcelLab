# PersonalWiki ExcelLab 4.7.0 DEV05 交接

## 版本定位
- 版本：4.7.0 DEV05（開發中，未驗收）
- 基礎來源：DEV04 工作樹 `PersonalWiki_ExcelLab`。
- 技術：WinForms / .NET 10 / SQLite。
- NuGet：DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14（未調整）。

## 本版變更
- 新增 `PersonalWiki.Desktop/RelatedEntryPreviewDialog.cs`。
- 「相關條目」頁新增「檢視相關條目摘要」按鈕，並支援雙擊清單列開啟預覽。
- 預覽包含名稱、類型、更新時間、摘要、共用標籤及推薦依據。
- 可複製條目名稱。
- 預覽只使用清單已取得的 `RelatedEntryItem`，不另行查詢資料庫；不會切換主畫面目前選取條目或搜尋條件。
- `MainForm.cs` 只調整相關條目頁的顯示入口及資料傳遞；既有推薦排序與搜尋操作保留。

## 資料與安全
- SQLite `user_version`／Schema：未變更，基準仍為 1。
- 資料庫路徑與桌面設定路徑：未變更。
- Excel 來源唯讀、匯入預檢、匯入前備份、來源快照驗證及單一 SQLite 交易流程未修改。
- 不修改圖片原檔及圖片關聯。
- 預覽視窗唯讀，不提供新增、編輯、封存、刪除或合併。

## 版本歷史摘要
- 4.7.0 DEV01：標籤使用統計 JSON 報告。
- 4.7.0 DEV02：Excel 匯入批次歷史唯讀檢視。
- 4.7.0 DEV03：Excel 匯入批次差異比較及 JSON 匯出。
- 4.7.0 DEV04：Excel Dry-run 問題明細檢視與列號導覽。
- 4.7.0 DEV05：相關條目摘要唯讀預覽。

## 三個標準 BAT
- `01_建置程式.bat`
- `02_啟動程式.bat`
- `03_建置並啟動.bat`

## 套件與 Schema
- Desktop：`net10.0-windows`；核心 CLI：`net10.0`。
- DocumentFormat.OpenXml 3.5.1；Microsoft.Data.Sqlite 10.0.14。
- SQLite `user_version=1`；本版不新增資料表、不執行 migration。

## 專案結構
- `PersonalWiki.Desktop/RelatedEntryPreviewDialog.cs`：本版新增唯讀摘要預覽視窗。
- `PersonalWiki.Desktop/MainForm.cs`：相關條目頁入口與清單資料傳遞。
- `Services/RelatedEntryService.cs`：既有唯讀推薦服務，本版未修改。
- `scripts/run-4.7.0-DEV05-python-tests.py`：靜態契約測試素材。
- `docs/releases/`：更新說明、測試指南、稽核及交接文件。

## 待處理問題／限制
- 預覽只顯示 RelatedEntryService 目前提供的摘要與關聯依據；不展示筆記、圖片及完整自訂欄位。
- 預覽不會直接導覽／切換主畫面目前條目，使用者仍可複製名稱後自行搜尋。
- 長摘要、超長標籤及 300% DPI 版面仍待 Windows 實機驗收。

## 測試狀態
- 已準備 `scripts/run-4.7.0-DEV05-python-tests.py`，尚未執行。
- 未執行 .NET restore/build、CLI acceptance、程式啟動或 Windows 實機測試。
- DEV04 的測試也仍待集中安排；不得把 ZIP 完整性檢查視為功能測試。

## 下一版本
- 4.7.0 DEV06：依 5.0.0 路線盤點剩餘知識管理及 Excel 資料維護候選功能；優先維持唯讀及既有服務重用，未確認前不改 Schema／資料路徑／匯入安全流程。
- 4.8.0：知識管理效率強化。
- 4.9.0：Excel／資料維護效率強化。
- 5.0.0：整合、集中測試、修正、文件與正式發行包。

## 完整續開發指令
以本版 ZIP 解壓後的 `PersonalWiki_ExcelLab` 作為唯一來源，後續從 4.7.0 DEV05 延續。先閱讀本交接、更新說明、測試指南、5.0 路線圖及現有程式碼；不得重做或破壞已完成功能。除非使用者明確同意，不得變更 SQLite Schema／user_version、資料庫路徑、桌面設定路徑、Excel 匯入預檢、來源唯讀、匯入前備份、來源快照驗證及交易安全流程。先完成一批互相獨立功能，再集中安排測試；在使用者要求測試前不得執行測試、建置或啟動。每版保留三個標準 BAT，更新 README、發行索引、更新說明、測試指南、稽核及交接文件。完成後產出真實可下載 ZIP，解壓根目錄固定為 `PersonalWiki_ExcelLab`，ZIP 外附 SHA-256；記錄套件、Schema、結構、問題、測試狀態及完整續開發指令。
