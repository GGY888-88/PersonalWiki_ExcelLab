# PersonalWiki ExcelLab 4.6.3 開發交接文件

## 1. 版本狀態
- 基礎來源：使用者上傳的 `PersonalWiki_ExcelLab_4.6.2_BatCompatibilityFix.zip`，直接延續修改，沒有重做舊版。
- 本版：4.6.3（InputVerticalAlignmentAndBatOutputFix）。
- 下一開發版本：4.7.0；先完成 4.6.3 Windows／300% 實機驗收。
- 發行 ZIP：`PersonalWiki_ExcelLab_4.6.3.zip`。
- ZIP 解壓後根資料夾固定為 `PersonalWiki_ExcelLab/`，不帶版本號。

## 2. 本版完成項目
### WinForms TextBox 垂直置中
- 前一版自訂控制項把單行輸入框設為 `Multiline = true`，再以 `EM_SETRECTNP` 調整文字矩形；這造成輸入框使用多行 EDIT，並與原生 Cue Banner（`PlaceholderText`）不相容。
- 本版改為 `Multiline = false`、`AutoSize = false`：保留既有版面高度設定，讓 Windows 原生單行 EDIT 負責文字的垂直排版及提示橫幅顯示。
- 已套用於資料庫路徑、全文搜尋與標籤篩選三個欄位；其餘原生單行 TextBox（圖片篩選、標籤重新命名、條目名稱／類型）保持原樣；多行摘要、筆記編輯維持原本多行控制項。
- 不使用自訂文字矩形或 GDI 位移、不調整字體大小，也不以增加控制項高度掩蓋問題。

### BAT 輸出相容性
- 三個 BAT 均為 ASCII + CRLF，狀態訊息只使用 ASCII 英文，沒有 `chcp`。
- 建置輸出導向 `build_release.log`，不再使用 `type` 把可能包含本機語系文字的建置日誌輸出到 CMD；錯誤時由 Notepad 開啟日誌。
- 03 一鍵流程維持「建置完成即啟動」，兩步之間沒有額外 pause。

### 保留既有設定
- 使用者目前主功能按鈕設定為 80×22，本版不修改 `DesktopSettingsService.cs` 或設定檔路徑，不重設使用者設定。
- PerMonitorV2、300% 下完整按鈕排列與既有功能保持不變。

## 3. 技術基準／Schema／套件
- CLI：.NET 10，`net10.0`。
- Desktop：WinForms，`net10.0-windows`；`ApplicationHighDpiMode=PerMonitorV2`。
- NuGet：`DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`；沒有新增或更新套件。
- SQLite：`PRAGMA user_version=1`，沒有 Schema migration。
- 桌面設定：`%LOCALAPPDATA%\PersonalWiki\desktop-settings.json`，路徑及既有欄位不變。
- DB 路徑、Excel mapping、匯入預檢、備份／快照／交易／匯入服務全部不變。

## 4. 專案結構摘要
- `PersonalWiki.ExcelLab.csproj`、`Program.cs`：CLI／核心執行入口。
- `PersonalWiki.Desktop/`：WinForms 桌面應用程式；本版涉及 `VerticallyCenteredTextBox.cs` 及專案版本資訊。
- `Services/`：SQLite／查詢／匯入／備份等服務；本版未修改。
- `Models.cs`：資料模型；本版未修改。
- `samples/`：歷史 SQLite 測試資料庫與測試素材。
- `scripts/`：靜態回歸測試、查詢 acceptance、封裝工具；新增 `test-4.6.3-input-alignment.py` 與 `run-4.6.3-python-tests.py`。
- `docs/releases/`：版本索引、更新說明、測試指南、稽核及交接文件。
- 根目錄 BAT：`01_建置程式.bat`、`02_啟動程式.bat`、`03_建置並啟動.bat`。

## 5. 測試狀態
- 靜態整合測試命令：
  ```powershell
  python scripts/run-4.6.3-python-tests.py
  ```
- 本版專項：
  ```powershell
  python scripts/test-4.6.3-input-alignment.py
  ```
- Windows 編譯命令：
  ```powershell
  dotnet build PersonalWiki.Desktop/PersonalWiki.Desktop.csproj -c Release
  ```
- 本容器沒有 .NET SDK／Windows，因此不能宣稱本版已完成 `dotnet build` 或 300% DPI 實機驗收。靜態測試通過只代表程式碼契約及封裝內容符合檢查，不能證明 Cue Banner 在每個 Windows 主題下都已完全置中。
- BAT 實機雙擊、Notepad 日誌顯示及 Windows 100%／150%／200%／300% 驗收尚待執行。

## 6. 驗收素材與操作
- 測試資料庫：`samples/PersonalWiki_3.6.0_Test.db`。需要寫入的測試請先複製資料庫至暫存位置，不得直接使用使用者正式資料庫。
- 先執行 `01_建置程式.bat` 建置，之後可用 `02_啟動程式.bat` 啟動；或使用 `03_建置並啟動.bat` 一次完成。
- 建置記錄位於根目錄 `build_release.log`。錯誤分支會自動開啟記事本。
- 詳細驗收步驟見 `TEST_GUIDE_4.6.3.md`。

## 7. 已知問題／風險
- 單行文字與 Cue Banner 仍需在 Windows 100%／150%／200%／300% DPI、焦點切換及視窗尺寸變更後實機確認；本容器未提供 Windows WinForms 視覺環境。
- 若實機仍偏上，請提供同欄位在 100% 與 300% 的截圖、Windows 版本及 `DeviceDpi`；不要直接調整字型或高度。
- 不得把 ZIP 來源交付視為已完成 Windows 可執行檔建置；ZIP 僅含來源與測試素材。

## 8. 下一版本規劃
- 4.7.0 暫不展開新功能；優先取得本版 Windows build log 及 300% 畫面驗收結果。
- 通過後再整理下一批獨立功能需求；未經使用者明確同意，不得改 SQLite Schema、資料路徑、Models、Services、CLI 或 Excel 匯入安全流程。

## 9. 完整續開發指令
> 請以 PersonalWiki ExcelLab 4.6.3 為唯一來源基準，先讀取 `docs/releases/HANDOFF_4.6.3.md`、`UPDATE_NOTES_4.6.3.md`、`TEST_GUIDE_4.6.3.md`、`AUDIT_REPORT_4.6.3.md`、README 及 `RELEASE_MANIFEST_4.6.3.sha256`。先完成 Windows .NET 10 編譯及 100%／150%／200%／300% DPI 實機驗收，特別檢查資料庫路徑、全文搜尋、標籤欄位的提示文字與實際輸入文字。保留主功能按鈕目前 80×22 設定、300% 完整按鈕換行、既有日期篩選／清單／Excel 預覽功能。未經使用者明確批准，不得修改 SQLite Schema、Models、Services、CLI、資料庫路徑、設定路徑、NuGet 版本、Excel mapping／預檢／備份／交易／匯入流程。只處理已確認的獨立問題。每版根目錄必須保留 `01_建置程式.bat`、`02_啟動程式.bat`、`03_建置並啟動.bat`；流程為開發→自我檢查→Windows 建置→測試→打包→更新交接文件→驗證 ZIP CRC／SHA256→提供下載。解壓根資料夾固定不含版本號。若執行環境沒有 Windows／.NET SDK，必須清楚標示未完成的實機驗收，不得宣稱成功。
