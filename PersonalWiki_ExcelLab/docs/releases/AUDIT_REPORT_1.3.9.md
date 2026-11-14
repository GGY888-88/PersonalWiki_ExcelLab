# PersonalWiki ExcelLab 1.3.8 原始碼檢查報告與 1.3.9 修正紀錄

## 檢查範圍

以交付的 `PersonalWiki_ExcelLab_1.3.8.zip` 為唯一基準，檢查專案結構、CLI／桌面專案設定、PowerShell 建置與驗收腳本、版本文件及資料安全相關服務。未連接或開啟任何使用者正式資料庫。

## 已確認並修正

### 1. 整合驗收腳本使用未定義變數

- 檔案：`scripts/run-acceptance-tests.ps1`
- 問題：批次來源核對及圖片檔案完整性稽核的報告路徑使用 `$temp`，但腳本未定義該變數。這兩項檢查會在建立報告前失敗。
- 修正：改用腳本既有的 `$OutputRoot`，報告集中輸出至使用者指定的驗收輸出資料夾。

### 2. Portable 建置腳本可能刪除既有發布資料

- 檔案：`scripts/build-portable.ps1`
- 問題：建置前若 `publish` 資料夾存在，腳本會直接遞迴刪除。若資料夾內有先前版本或其他檔案，可能造成非預期資料遺失；同時原腳本沒有明確檢查 `dotnet publish` 結束碼。
- 修正：目的資料夾存在即停止，不刪除任何既有內容；新增發布失敗的結束碼檢查，並允許指定不同輸出資料夾及 Runtime。

### 3. 版本資訊不一致

- 檔案：`README.md`、`scripts/run-acceptance-tests.ps1`、`Program.cs`
- 問題：README 首頁仍以 1.3.7／1.3.3／1.3.0 為目前版本；驗收報告仍標示 1.2.3；CLI 自我測試報告仍標示 0.6.7。
- 修正：README 改為 1.3.9 維護版資訊；驗收報告及自我測試報告版本同步為 1.3.9；CLI 與桌面 csproj 版本號同步為 1.3.9。

### 4. 自我測試腳本使用固定輸出資料夾

- 檔案：`scripts/run-self-test.ps1`
- 問題：固定寫入 `TestOutput`，重複執行時，既有 fixture 可能導致 `File.Copy(..., overwrite:false)` 失敗；原腳本也未檢查 dotnet 執行結束碼。
- 修正：預設使用時間戳記建立新的輸出資料夾，可由參數指定路徑；目的資料夾存在即停止，並檢查自我測試結束碼。

## 已檢查的資料安全設計（靜態檢視）

- `DatabaseRecoveryService` 還原前檢查目標檔不得存在，並先檢查備份資料庫健康狀態；驗證失敗時只清理由該次流程新建的目標檔。
- `DatabaseBackupService` 備份與還原均拒絕覆蓋既有目的檔。
- `ExcelImportDatabaseSafetyService` 匯入前檢查資料庫健康及 Schema，建立新路徑備份並驗證；失敗時清理該次建立的備份。
- 圖片關聯移除的既有程式註明只移除 SQLite 關聯，不刪除硬碟原圖。
- 既有匯入流程仍以原始 Excel 唯讀及 SQLite Schema v1 為基礎；本版沒有變更 Schema 或套件版本。

以上是原始碼靜態檢視，不等同執行時安全測試。

## 仍待後續改善／檢查

1. **Windows/.NET 10 建置與桌面執行**：本環境沒有 .NET SDK，也不是 Windows，無法確認兩個專案能否完整編譯及 WinForms 執行狀況。
2. **桌面端 UI 自動化測試不足**：目前整合驗收腳本主要涵蓋 CLI 與 SQLite 報告服務，未涵蓋 DPI、多螢幕、視窗縮放、圖片頁及批次封存等完整桌面互動。
3. **Excel 複雜格式相容性**：自我測試報告已列出 XLSM/VBA、Excel COM、Power Query、外部連線、內嵌圖片、密碼保護工作表及大型活頁簿等未涵蓋範圍；不能把基本 XLSX 測試解讀為全面相容。
4. **長期維護性**：部分 Service 使用高度壓縮的單行 SQL／程式碼，增加人工審查與除錯成本；建議後續分批整理並補足單元測試，不在本維護版大規模重構。
5. **啟動方式**：`啟動 PersonalWiki.bat` 使用 `dotnet run`，目前需要安裝 .NET 10 SDK；若未來要提供免 SDK 使用者的版本，應以獨立發佈包及啟動檔另行驗證。

## 1.3.9 版本界線

- 只修正文件、版本標記及建置／測試腳本。
- 不改動條目、筆記、標籤、圖片關聯及 Excel 匯入核心邏輯。
- SQLite Schema `user_version=1` 不變。
- NuGet：`DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14` 不變。
- 本環境沒有 .NET SDK；未執行 build、CLI 自動化測試或 Windows Forms 實機測試。

## 建議下一階段（1.4.0）

1. 將 CLI 自動化測試與桌面 UI 驗收分開，建立明確測試矩陣及版本報告。
2. 優先補上匯入預檢、備份、匯入交易及批次驗證的自動化回歸測試。
3. 補強 SQLite 備份／還原、圖片路徑失效及搜尋索引重建的邊界測試。
4. 再處理 UI DPI／縮放與多工作表欄位顯示測試。
5. 評估免安裝桌面發佈包；在確認發佈方式後再更新啟動流程，不改變現有資料庫位置或資料格式。
