# PersonalWiki ExcelLab 4.6.2 - MainUiRefine 交接文件

## 版本定位
- 基礎版本：4.6.2 MainToolbarWrapFix（使用者已在 Windows 300% DPI 環境啟動並載入測試資料庫）。
- 本次修正版：4.6.2 MainUiRefine。
- 下一個功能版本規劃：4.7.0；在使用者確認本輪主畫面按鈕及搜尋欄位後再排入。
- 不另行重做 4.6.1／4.6.2 已完成的資料功能。

## 本輪使用者回報
1. 主畫面功能按鈕縮小後仍嫌大，希望可再調小。
2. 搜尋區輸入格內文字偏上，期待垂直置中。

## 本輪修改
- `PersonalWiki.Desktop/MainForm.cs`
  - 主畫面功能按鈕內距縮小為 2,1,2,1，字體改為 9pt。
  - 主畫面按鈕尺寸設定下限改為寬 40、高 20（96 DPI 基準），寬度步進 5、高度步進 2。
  - 新增 `VerticallyCenteredTextBox` 套用於資料庫路徑、全文搜尋及標籤欄位。
- `PersonalWiki.Desktop/DesktopSettingsService.cs`
  - 主畫面按鈕設定讀取／儲存範圍改為寬 40–360、高 20–160。
  - 新增／編輯條目對話框按鈕設定範圍維持原本 100–360、36–160。
- 新增 `PersonalWiki.Desktop/VerticallyCenteredTextBox.cs`。
- 新增三個 BAT：`01_建置程式.bat`、`02_啟動程式.bat`、`03_建置並啟動.bat`。
- 新增 `scripts/test-4.6.2-main-ui-refine.py`、本輪更新說明及 SHA256 manifest。

## 未修改範圍
- SQLite Schema：未修改，仍為 `PRAGMA user_version=1`。
- Models：未修改。
- Services／資料庫查詢及寫入：未修改。
- CLI 主程式：未修改。
- Excel 匯入 mapping、預檢、備份、交易及匯入服務：未修改。
- 日期篩選控制項及其查詢語義：未修改。
- NuGet 套件：未修改。

## 建置、套件及啟動
- Desktop：.NET 10 WinForms，`net10.0-windows`。
- CLI：.NET 10，`net10.0`。
- ZIP 解壓根目錄：`PersonalWiki_ExcelLab`（資料夾名稱不含版本號）。
- 第一次建置：雙擊 `01_建置程式.bat`。
- 建置並啟動：雙擊 `03_建置並啟動.bat`（自動建置後啟動，中途不需再按鍵）。
- 已建置後啟動：雙擊 `02_啟動程式.bat`。
- 建置日誌：`build_release.log`；失敗時 BAT 保留視窗並輸出錯誤。
- 若未安裝 .NET 10 SDK，需先安裝 .NET 10 SDK；BAT 會顯示提示。

## 測試狀態
- 本輪新增靜態檢查：13/13 通過。
- 4.6.2 回歸：DPI 23/23、篩選列 15/15、表格欄寬 15/15、視窗空間 12/12、WFO1000 修正 10/10。
- 4.6.1 UI：8/8。
- 4.6.0 查詢契約：35/35；runner 自我測試會刻意輸出一筆模擬失敗路徑（0/1），用來驗證失敗診斷，不代表正式查詢契約失敗。
- 已執行 `scripts/run-4.6.2-python-tests.py`，整體成功。
- 本工作環境沒有 .NET SDK／Windows，尚未執行本輪 C# 實際編譯；BAT 已提供給使用者在 Windows 編譯。
- `VerticallyCenteredTextBox` 原生 EDIT 格式矩形及主畫面按鈕在 300% DPI 的視覺結果仍待 Windows 實機確認。

## 使用者下一步驗收
1. 解壓縮新版 ZIP 到新資料夾。
2. 雙擊 `03_建置並啟動.bat`，確認建置成功並開啟程式。
3. 在主畫面點「主畫面功能按鈕大小」，嘗試寬 80、高 28；若仍大，可再往 60×24 或 40×20 調整。數值為 96 DPI 基準，實際大小會依 DPI 縮放。
4. 檢查全文搜尋及標籤輸入框的文字是否垂直置中，輸入、Enter 搜尋及 Placeholder 是否正常。
5. 請回傳設定值及 300% 主畫面截圖；若文字被裁切，先不要再改其他 UI 區域。

## 已知限制／風險
- 主畫面按鈕的寬度設定是最小寬度；按鈕仍需容納其標籤文字，長標籤可能大於設定值，並由版面配置器換行。
- `VerticallyCenteredTextBox` 使用 Win32 `EM_SETRECTNP`，需要在 Windows WinForms 環境確認多行 EDIT 控制項下的游標、輸入法組字、Placeholder、Enter 搜尋及垂直置中。
- 未執行 .NET 編譯前，不應宣稱本輪已通過 Windows 建置。

## 建議完整續開發指令

> 請以 PersonalWiki ExcelLab 4.6.2 MainUiRefine 為唯一基準繼續開發。先閱讀本 HANDOFF、PATCH_NOTES_4.6.2_MainUiRefine.md、README.md 及 RELEASE_MANIFEST_4.6.2_MainUiRefine.sha256。保留 4.6.1／4.6.2 已完成功能；未經使用者同意不得修改 SQLite Schema、Models、Services、CLI、資料庫路徑、Excel 匯入 mapping／預檢／備份／交易流程及日期篩選語義。優先收集使用者 Windows 300% 實機建置結果與主畫面截圖，再只修正經確認的 UI 問題。每版根目錄必須附 `01_建置程式.bat`、`02_啟動程式.bat`、`03_建置並啟動.bat`，並更新版本歷史、下一版本規劃、交接文件、測試素材、Schema／套件／結構／問題／測試狀態、SHA256 manifest。流程固定為開發→自我檢查→Windows 建置驗收→打包→更新交接文件→提供 ZIP。ZIP 解壓根資料夾不可帶版本號，下載前必須驗證 ZIP CRC 及實際檔案存在。不要宣稱未執行的 Windows 編譯或實機測試已通過。
