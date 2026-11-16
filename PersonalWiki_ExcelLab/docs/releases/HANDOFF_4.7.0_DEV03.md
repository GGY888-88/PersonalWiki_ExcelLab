# PersonalWiki ExcelLab 4.7.0 DEV03 交接

## 版本定位
- 版本：4.7.0 DEV03（開發中，不是正式發行版）
- 基礎來源：4.7.0 DEV02 ZIP
- 技術：WinForms / .NET 10 / SQLite
- NuGet 基準：DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14（本版未調整）

## 本版變更
- 新增 `PersonalWiki.Desktop/ExcelImportBatchCompareDialog.cs`。
- `MainForm.cs` 新增「Excel 批次差異比較」入口。
- 使用既有 `ExcelImportBatchManagementService.List` 與 `ExcelImportBatchDiffService.Compare`。
- 支援條目及欄位差異摘要，並可匯出 JSON。
- 更新兩個 csproj 的版本標示與三個標準 BAT 的版本文字。
- 更新紀錄：`docs/releases/UPDATE_NOTES_4.7.0_DEV03.md`。

## 資料與安全
- SQLite `user_version` / Schema：未變更；已知基準為 1。
- 資料庫路徑：未變更。
- Excel 匯入預檢、來源唯讀、匯入前備份及交易流程：未變更。
- 匯入批次比較使用既有唯讀服務；匯出 JSON 只寫入使用者指定的外部檔案。
- 未刪除圖片原檔、未改變圖片關聯處理。

## 版本歷史摘要
- 4.6.6：三個指定輸入欄位高度調整（依前版交接記錄；尚待實機驗收）。
- 4.7.0 DEV01：標籤管理相關增強（功能是否完整需以來源檔案及後續驗收為準）。
- 4.7.0 DEV02：Excel 匯入歷史唯讀檢視。
- 4.7.0 DEV03：Excel 匯入批次差異比較及 JSON 匯出。

## 三個標準 BAT
- `01_建置程式.bat`
- `02_啟動程式.bat`
- `03_建置並啟動.bat`

## 專案結構重點
- `PersonalWiki.Desktop/`：WinForms UI。
- `Services/`：資料服務，包含批次歷史及差異比較服務。
- `docs/releases/`：版本更新及交接文件。
- `scripts/`：既有建置／測試輔助腳本。

## 套件與 Schema
- Target Framework：桌面 `net10.0-windows`，核心 `net10.0`。
- DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14 為前版基準，本版未調整套件。
- Schema 基準 `user_version=1`，本版不新增資料表、不執行 migration。

## 問題與限制
- 差異比較的條目識別與欄位對應沿用現有 `ExcelImportBatchDiffService` 邏輯；對於同一批次內同一標準化名稱／類型多筆來源的合併情境，需在集中測試時確認呈現是否符合預期。
- 批次選擇器最多載入最近 500 筆，顯示開始時間；若開始時間相同，使用者需留意批次 ID（目前下拉選單未額外呈現 ID）。
- JSON 為目前比較結果的完整序列化資料；匯出路徑由使用者選擇。

## 測試狀態
- 未執行任何測試、編譯或程式啟動（遵循使用者指示）。
- 僅靜態檢視程式碼與修改點，不能視為技術驗收或可執行保證。
- 後續集中測試建議：空批次、相同批次防呆、只新增／只移除／修改條目、自訂欄位新增移除修改、缺失批次、唯讀檔案雜湊前後一致、JSON 匯出可解析、100%–300% DPI。

## 下一版本規劃
- DEV04：整理 Excel 匯入結果檢視與錯誤明細導覽，先盤點現有服務可重用範圍。
- 4.8.0：知識管理效率強化。
- 4.9.0：Excel／資料維護效率強化。
- 5.0.0：整合、集中測試、修正、文件與正式發行包。

## 完整續開發指令
請以本 ZIP 解壓後的 `PersonalWiki_ExcelLab` 資料夾作為唯一基準，延續開發至 5.0.0。先閱讀本交接、更新紀錄及現有程式碼；不得重做或破壞既有功能。除非使用者明確同意，不得變更 SQLite Schema/user_version、資料庫路徑、Excel 匯入預檢／來源唯讀／匯入前備份／交易安全流程。先完成一批互相獨立的功能，再集中安排測試。使用者目前要求先不要測試或編譯，因此繼續開發期間不得執行測試、建置或啟動；每版需誠實標示未驗證。每版保留三個 BAT（01_建置程式.bat、02_啟動程式.bat、03_建置並啟動.bat），更新版本歷史、更新紀錄、交接文件、Schema、套件、結構、問題及測試狀態。完成後產出真實可下載 ZIP，ZIP 內解壓資料夾名稱固定為 `PersonalWiki_ExcelLab`（不帶版本號），ZIP 外另附 SHA-256 校驗檔。持續推進直到 5.0.0 完整交付，不在中間版本停止。


## DEV03 封裝修復紀錄（交付整理）
- 三個標準 BAT 已統一為指定檔名及 ASCII／CRLF 格式；舊版 BAT 保留未刪除。
- README 與 `docs/releases/INDEX.md` 已更新至 DEV03，下一開發階段標示為 DEV04。
- SHA-256 manifest 已重新產生，涵蓋除 manifest 自身外的所有專案檔案。
- 詳細修復內容：`docs/releases/PACKAGING_REPAIR_4.7.0_DEV03.md`。
- 本次僅修復封裝與文件；未修改功能程式碼，未執行測試、建置或啟動。
