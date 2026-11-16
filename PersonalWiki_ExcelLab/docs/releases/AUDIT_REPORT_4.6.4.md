# PersonalWiki ExcelLab 4.6.4 稽核報告

## 稽核摘要
- 基礎來源：4.6.3 專案工作目錄。
- 變更目的：依使用者最新 300% DPI 截圖，修正資料庫路徑、全文搜尋、標籤欄位文字仍偏上的問題。
- 修改程式：`PersonalWiki.Desktop/VerticallyCenteredTextBox.cs`（複合控制項實作）、`PersonalWiki.Desktop/MainForm.cs`（資料庫／標籤 ToolTip 指向內層 TextBox）。
- 版本資料：CLI／Desktop `.csproj` 同步至 4.6.4；三個 BAT 的顯示版號同步至 4.6.4。
- 測試素材：新增三個 4.6.4 靜態／回歸腳本，保留歷史腳本；4.6.0 acceptance contract 允許 4.6.4 版本。

## 僅修改範圍
- `PersonalWiki.Desktop/VerticallyCenteredTextBox.cs`
- `PersonalWiki.Desktop/MainForm.cs`（僅新增內層輸入框 Tooltip 綁定）
- `PersonalWiki.ExcelLab.csproj`
- `PersonalWiki.Desktop/PersonalWiki.Desktop.csproj`
- `01_建置程式.bat`、`02_啟動程式.bat`、`03_建置並啟動.bat`（僅版本字串）
- `README.md`、`docs/releases/INDEX.md`、4.6.4 文件及測試執行記錄
- 新增 4.6.4 測試腳本；在 acceptance contract 的相容版本清單加入 4.6.4。

## 明確未修改範圍
- `Models.cs`、`Services/`、CLI `Program.cs`、SQLite Schema／migration、資料庫路徑、桌面設定路徑。
- Excel 欄位 mapping、預檢、備份、快照、交易及匯入流程。
- `DpiLayout.cs`、`DateFilterControl.cs`、日期／ComboBox、列表欄寬與主畫面按鈕排版。
- 圖片篩選、標籤重新命名、條目名稱／類型、多行摘要與筆記控制項。
- NuGet 套件版本及現有使用者設定；不重置 80×22 主功能按鈕設定。

## 靜態測試結果
- `test-4.6.4-input-alignment.py`：33/33。
- `test-4.6.4-dpi-regression.py`：23/23。
- `test-4.6.2-filter-layout.py`：15/15。
- `test-4.6.2-responsive-tables.py`：15/15。
- `test-4.6.2-viewport-space.py`：12/12。
- `test-4.6.2-build-fix.py`：10/10。
- `test-4.6.4-main-ui-regression.py`：15/15。
- `run-4.6.1-python-tests.py`：通過。
- `test-4.6.0-query-acceptance.py`：35/35 contract checks 通過；測試刻意模擬一次 CLI 失敗，以驗證錯誤報告及重跑命令，因此輸出會出現預期的 `Acceptance: 0/1 passed, 1 failed` 模擬訊息。這不是實際資料庫 acceptance 執行結果。
- 完整 runner：`python scripts/run-4.6.4-python-tests.py` 回傳碼 0。

## 尚未完成的驗收
- 執行環境沒有 `dotnet`、C# compiler 或 Windows WinForms runtime。
- 未完成 .NET 10 restore/build，故不能宣稱 0 errors／0 warnings。
- 未完成 100%／150%／200%／300% 的實機文字位置、Cue Banner、IME、焦點、視窗縮放及按鈕排列驗收。
- 此版本是來源／測試 ZIP，不含已在 Windows 編譯驗證的 EXE。

## 技術基準
- CLI：`net10.0`；Desktop：`net10.0-windows`。
- Desktop DPI：`PerMonitorV2`。
- NuGet：`DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`；無新增套件。
- SQLite：`PRAGMA user_version=1`；本版沒有 Schema 變更。
- 設定：`%LOCALAPPDATA%\\PersonalWiki\\desktop-settings.json`；未重設或遷移。
