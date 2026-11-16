# PersonalWiki ExcelLab 4.6.4 完整開發交接文件

## 1. 版本摘要
- 本版：**4.6.4**（MainInputVerticalCenterContainer）。
- 基礎來源：上一輪完成的 `PersonalWiki_ExcelLab` 4.6.3 工作目錄；不是重建舊專案。
- 使用者依據：2026-11-15 提供的主畫面 300% DPI 截圖。資料庫路徑、全文搜尋提示及標籤提示仍明顯靠上。
- 本版目標：只修正三個原本已指定使用 `VerticallyCenteredTextBox` 的欄位，不重排工具列、不替換其他輸入欄位。
- 下一整合版本：**4.7.0**；詳細計畫見 `PLAN_4.7.0.md`。
- 發行 ZIP：`PersonalWiki_ExcelLab_4.6.4.zip`；解壓根資料夾固定為 `PersonalWiki_ExcelLab/`。

## 2. 版本歷史摘要
- **4.6.2**：PerMonitorV2、主畫面按鈕完整換行、兩列搜尋條件、日期篩選及 Excel 預覽 DPI 自適應。
- **4.6.3**：BAT 建置輸出改寫入 `build_release.log`，失敗時用記事本開啟；輸入框改回原生單行 EDIT，但使用 `AutoSize=false` 拉高控制項後，實際畫面仍出現文字偏上。
- **4.6.4**：改用外層固定高度容器＋內層 AutoSize 原生單行 TextBox，依目前字型的建議高度置中；保留原生提示文字及單行編輯語意。

## 3. 本版程式修改
### 3.1 技術原因
- 4.6.3 把單行 EDIT 自身拉高，依賴它在較高控制項中將文字自動置中，實際 300% 畫面證明此假設不成立。
- `EM_SETRECTNP` 只作用於多行 EDIT；原生 Cue Banner／`PlaceholderText` 不支援多行 EDIT，因此不再嘗試在同一個原生 EDIT 上同時套用兩者。
- 4.6.4 將原生單行 TextBox 保持在 Windows／WinForms 依字型決定的 `AutoSize=true` 建議高度，外層容器才維持較高的既有框體，內層文字框在容器中垂直置中。

### 3.2 變更檔案
- `PersonalWiki.Desktop/VerticallyCenteredTextBox.cs`：由 TextBox 子類改成 UserControl 外層容器，內含無邊框、AutoSize、Multiline=false 的原生 TextBox；代理 `Text`、`PlaceholderText`、`ReadOnly`、`Clear`、`SelectAll`、`Copy`、`Focus`，轉送 `TextChanged`、`KeyDown`、`MouseDoubleClick`，於 Font／DPI／尺寸改變時重排內層框。
- `PersonalWiki.Desktop/MainForm.cs`：只為資料庫路徑及標籤篩選的內層 TextBox 補上原本 ToolTip；不改搜尋條件與工具列結構。
- CLI／Desktop 專案版本同步更新為 4.6.4。
- 三個 BAT 只更新版本顯示，繼續使用 ASCII + CRLF。
- 新增 `scripts/test-4.6.4-input-alignment.py`、`scripts/test-4.6.4-dpi-regression.py`、`scripts/test-4.6.4-main-ui-regression.py`、`scripts/run-4.6.4-python-tests.py`。
- `scripts/test-4.6.0-query-acceptance.py` 的相容版本清單加入 4.6.4；保留 4.6.2／4.6.3 歷史測試腳本。

### 3.3 明確未改動
- `Models.cs`、`Services/`、CLI `Program.cs`。
- SQLite Schema、migration、DB 路徑、桌面設定路徑、設定格式。
- Excel 欄位 mapping、預檢、備份、快照、交易及匯入服務。
- `DpiLayout.cs`、`DateFilterControl.cs`、日期篩選、ComboBox、ListView 欄寬、主按鈕排列演算法。
- 圖片篩選、標籤重新命名、條目名稱／類型、多行摘要及筆記控制項。
- NuGet 套件版本。
- 使用者設定不重設；目前主功能按鈕 80×22 的值與設定檔不修改。

## 4. Schema、套件與執行環境
- CLI：`.NET 10`，`net10.0`。
- Desktop：WinForms，`net10.0-windows`。
- DPI：`ApplicationHighDpiMode=PerMonitorV2`。
- NuGet：`DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`；無新增／更新套件。
- SQLite：`PRAGMA user_version=1`；4.6.4 沒有 Schema migration。
- 桌面設定：`%LOCALAPPDATA%\PersonalWiki\desktop-settings.json`；未變更。

## 5. 專案結構
- `PersonalWiki.ExcelLab.csproj`、`Program.cs`、`Models.cs`：CLI／核心程式與資料模型。
- `PersonalWiki.Desktop/`：WinForms 主畫面、對話框、DPI、設定、日期篩選、圖片縮圖及本版複合 TextBox。
- `Services/`：SQLite 查詢、資料管理、Excel 匯入、備份、還原、報告等服務；本版未修改。
- `samples/`：SQLite 測試資料庫、Excel／JSON 測試素材。
- `scripts/`：靜態回歸、查詢 acceptance、發行封裝與驗證工具；新增 4.6.4 測試腳本。
- `docs/releases/`：版本更新、測試、稽核、交接及 4.7.0 計畫。
- 根目錄 BAT：`01_建置程式.bat`、`02_啟動程式.bat`、`03_建置並啟動.bat`。

## 6. 測試狀態
### 已完成
- `python scripts/test-4.6.4-input-alignment.py`：33/33。
- `python scripts/test-4.6.4-dpi-regression.py`：23/23。
- `python scripts/test-4.6.2-filter-layout.py`：15/15。
- `python scripts/test-4.6.2-responsive-tables.py`：15/15。
- `python scripts/test-4.6.2-viewport-space.py`：12/12。
- `python scripts/test-4.6.2-build-fix.py`：10/10。
- `python scripts/test-4.6.4-main-ui-regression.py`：15/15。
- `python scripts/run-4.6.1-python-tests.py`：通過。
- `python scripts/test-4.6.0-query-acceptance.py`：35/35 靜態 acceptance contract checks 通過。其內部刻意模擬一次 CLI 失敗並確認部分報告能保存，所以會顯示預期的 `Acceptance: 0/1 passed, 1 failed` 訊息；不是正式 CLI／資料庫 acceptance 測試。
- 完整命令 `python scripts/run-4.6.4-python-tests.py`：回傳碼 0。完整輸出已隨 ZIP 收錄於 `docs/releases/TEST_RUN_4.6.4.log`，摘要見 `docs/releases/TEST_RESULT_4.6.4.md`。
- BAT 三檔 ASCII、CRLF、沒有切換 CMD code page；build log 繼續導向檔案，未用 `type` 直接灌入 CMD。

### 尚待 Windows 實機完成
- 開發容器未安裝 .NET SDK／C# compiler，也沒有 Windows WinForms runtime；本版未執行 .NET restore／build，因此不能宣稱編譯 0 errors／0 warnings。
- 尚未完成 100%／150%／200%／300% 的原生文字顯示驗收、IME、焦點、跨螢幕 DPI 變更及視窗縮放測試。
- ZIP 為完整來源與測試素材，不是已在 Windows 編譯驗證的 EXE。

## 7. 啟動、建置與測試
解壓後進入 `PersonalWiki_ExcelLab`：

```powershell
python scripts/run-4.6.4-python-tests.py
dotnet build PersonalWiki.Desktop/PersonalWiki.Desktop.csproj -c Release
```

也可用：
- `01_建置程式.bat`：建置，輸出保存在 `build_release.log`。
- `02_啟動程式.bat`：啟動既有 Release 輸出。
- `03_建置並啟動.bat`：先建置成功再啟動。

正式資料庫不要拿來做寫入測試。測試素材使用 `samples/PersonalWiki_3.6.0_Test.db`；需要寫入時先複製至暫存路徑。

## 8. 已知問題與風險
- 本版採複合控制項方式處理垂直置中，靜態測試已確認結構、屬性／事件轉送及範圍，但最終是否符合使用者視覺要求仍須由 Windows 300% 截圖驗收。
- 如果 4.6.4 的三欄文字仍偏上，請提供同欄位 100% 與 300% 截圖、Windows 版本、DeviceDpi；先調整內層控制項位置，不要改字型或外層高度。
- 其他未包裝的欄位刻意未改；如後續發現它們也偏上，需另以截圖逐欄確認，不可直接全面替換。
- `ToolTip` 已分別綁定外層及實際內層輸入控制項。

## 9. 下一版本計畫
- 4.7.0 開始前先完成 4.6.4 Windows 編譯與四種 DPI 實機驗收。
- 接著建立主畫面及各對話框輸入控制項清冊和 UI 回歸矩陣，只觀察尚未確認問題的欄位，不預防性重寫正常控制項。
- 之後依使用者確認的需求，將獨立功能集中成一批開發，再統一回歸測試；功能清單尚未由使用者指定，不先自行新增或修改資料層。
- 詳見 `PLAN_4.7.0.md`。

## 10. 完整續開發指令
> 請以 PersonalWiki ExcelLab 4.6.4 為唯一來源基準。先閱讀 `README.md`、`docs/releases/HANDOFF_4.6.4.md`、`UPDATE_NOTES_4.6.4.md`、`TEST_GUIDE_4.6.4.md`、`AUDIT_REPORT_4.6.4.md`、`TEST_RESULT_4.6.4.md`、`PLAN_4.7.0.md` 及 `RELEASE_MANIFEST_4.6.4.sha256`。先執行 `python scripts/run-4.6.4-python-tests.py`，再於 Windows .NET 10 環境執行 `dotnet build PersonalWiki.Desktop/PersonalWiki.Desktop.csproj -c Release`。實機以 100%／150%／200%／300% 檢查資料庫路徑、全文搜尋、標籤三欄，確認提示文字及輸入文字上下留白大致相等，並測試唯讀路徑雙擊複製、Enter 搜尋及標籤交集搜尋。保留其他原本正常的欄位、日期篩選、ComboBox、圖片篩選、標籤重新命名、條目名稱／類型、多行摘要、筆記、列表、Excel 預覽及 300% DPI 主功能按鈕排列；主功能按鈕目前設定 80×22，不重設。未經使用者明確同意，不得修改 SQLite Schema、Models、Services、CLI、DB 路徑、設定路徑、NuGet 版本及 Excel mapping／預檢／備份／交易／匯入流程。只修改已由使用者畫面或測試確認的問題。每版保留 `01_建置程式.bat`、`02_啟動程式.bat`、`03_建置並啟動.bat`；流程為開發→自我檢查→Windows 建置→測試→打包→更新交接文件→驗證 ZIP CRC／SHA-256→提供下載。若執行環境沒有 Windows／.NET SDK，清楚標示實機驗收未完成，不可宣稱編譯成功。解壓根資料夾固定命名 `PersonalWiki_ExcelLab`，不可附版本號。
