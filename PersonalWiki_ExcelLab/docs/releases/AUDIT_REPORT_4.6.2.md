# 4.6.2 稽核報告

## 變更清單
- `PersonalWiki.Desktop/PersonalWiki.Desktop.csproj`：新增 `ApplicationHighDpiMode=PerMonitorV2`，版本更新至 4.6.2。
- `PersonalWiki.Desktop/DpiLayout.cs`：新增共用 DPI 自動縮放基準、目前螢幕工作區限制、文字控制項高度量測、一般及多行 FlowLayoutPanel 高度計算、說明標籤換行及核取方塊高度計算。
- `PersonalWiki.Desktop/MainForm.cs`：主畫面改用 DPI 自動縮放；資料庫路徑置於篩選列前段；篩選條件以兩列固定尺寸標籤＋輸入框成組排列（第一列一般篩選、第二列日期範圍與排序），每列必要時獨立水平捲動；搜尋／清除／分頁按鈕獨立成第四列（主功能列、一般篩選列、日期／排序列、查詢操作列）；筆記／圖片／標籤／來源工具列依內容加高。日期欄位改用 `DateFilterControl` 的 `Checked`／`Value` 語義。
- `PersonalWiki.Desktop/DateFilterControl.cs`：日期篩選核取方塊與 DateTimePicker 改為兩個獨立原生控制項；`DateTimePicker.ShowCheckBox=false`，以獨立座標和 DPI 寬度避免內嵌勾選框擠壓日期文字。`Checked`／`Value` 加入 `DesignerSerializationVisibility.Hidden`，明確排除執行期查詢狀態的設計器序列化，修正 .NET 10 WFO1000。
- `PersonalWiki.Desktop/ImportDialogs.cs`：三個 Excel 精靈步驟全面 DPI 自動縮放；Excel 預覽資料列自動列高及儲存格換行，說明文字寬度自適應、設定列可換行、按鈕列自動排版、資料表格可捲動；第 1 步 DataGridView 欄寬改為 Fill，按標題／樣本文字配置 FillWeight 與最小寬度，最新截圖修正另將空白 client area／儲存格背景設為 SystemColors.Window，消除填滿表格下方的大面積灰色。
- `PersonalWiki.Desktop/MainForm.cs`：最新 Hotfix 為搜尋結果及相關條目 ListView 增加自適應欄寬，按 client width／權重分配，Resize、Shown、DpiChanged 後重新計算；文字欄保留優先擴展空間，極窄時使用水平捲動。
- `PersonalWiki.Desktop/TagManagementDialog.cs`：標籤管理表單及重新命名視窗加入 DPI 縮放和工作區限制。
- `PersonalWiki.Desktop/MainForm.cs` 內的設定、提示及報告對話框同步加入 DPI／工作區處理。
- CLI／Desktop 專案版本同步為 4.6.2；更新 README 及發行文件。

## 安全範圍
- 本次變更只涉及 Desktop UI、版本資訊、測試及文件。
- `Services/`、`Models.cs`、CLI `Program.cs` 未修改；日期查詢資料轉換只把 UI 來源型別由 DateTimePicker 包裝為 DateFilterControl，輸出日期字串格式及查詢服務參數不變。
- SQLite Schema、資料庫路徑、桌面設定路徑及 NuGet 相依套件未變更。
- Excel 預檢、mapping 序列化、備份、雜湊重驗、唯讀快照、單一交易及匯入邏輯未修改。

## 靜態測試
- `python scripts/test-4.6.2-dpi.py`：22 項靜態 DPI／300% 版面檢查通過。
- `python scripts/test-4.6.2-filter-layout.py`：15 項篩選列／日期欄位靜態檢查，包含兩列篩選分組、獨立核取方塊、非內嵌 ShowCheckBox 與分離 Bounds。
- `python scripts/test-4.6.2-responsive-tables.py`：14 項表格欄寬靜態檢查通過，涵蓋主結果清單、相關條目、Excel 預覽欄寬填滿及白色網格背景。
- `python scripts/test-4.6.2-viewport-space.py`：12 項最新截圖空間利用檢查，涵蓋兩列篩選、更新日期可達性、Excel 預覽填滿高度及空白區背景。
- `python scripts/test-4.6.2-build-fix.py`：10 項建置修正靜態檢查通過，包含 `Checked`／`Value` 的 WFO1000 序列化中繼資料檢查。
- `python scripts/test-4.6.1-ui.py`：8 項 UI 回歸檢查通過。
- `python scripts/test-4.6.0-query-acceptance.py`：35 項查詢契約檢查通過；但內含的舊案例執行器輸出 0/1 案例通過，仍需在 Windows／完整測試環境調查，不能宣稱所有查詢案例皆通過。
- `python scripts/run-4.6.2-python-tests.py`：本版執行結束碼 0；22 項 DPI、15 項篩選列、14 項表格欄寬、11 項空間利用、10 項建置修正、8 項 UI 檢查及 35 項查詢契約檢查通過。4.6.0 舊案例執行器在故障路徑自我測試中刻意顯示 0/1，屬模擬失敗測試輸出，不代表正式資料案例已驗收。
- 使用者已回報先前 `WFO1000Fix` 版本在 Windows／.NET 10 成功建置；本次追加的自適應表格欄寬程式碼尚未重新編譯。此工作容器未安裝 `dotnet` SDK，因此最新 Hotfix 仍需在使用者 Windows 環境執行 `dotnet build`。

## 未執行項目／限制
- 此環境沒有 Windows 桌面與 .NET SDK，無法執行最新 Hotfix 的 dotnet build 或 300% WinForms 實際 DPI 顯示測試；上述 DPI／篩選／表格／空間檢查均屬靜態檢查（目前版本 22／15／14／12 項），不應視為本次表格畫面已實機驗收。
- 多螢幕 DPI 轉換、不同 Windows 版本字型度量、極窄工作區及真正 Excel 預覽／匯入仍需依 `TEST_GUIDE_4.6.2.md` 在 Windows 驗收。
- 靜態測試通過不等於已證明無文字裁切／控制項重疊；需完成實機驗收才能確認。

## 最新表格欄寬 Hotfix
- `PersonalWiki.Desktop/MainForm.cs`：搜尋結果與相關條目 ListView 改用權重欄寬，依目前 client width、欄位最小寬度及 DPI 重新分配；Resize、Shown、DpiChanged 都會重算。
- `PersonalWiki.Desktop/ImportDialogs.cs`：Excel 預覽 DataGridView 改用 Fill 欄寬，欄位權重參考標題及預覽值量測，設定最小寬度；維持文字換行及自動列高。
- 新增 `scripts/test-4.6.2-responsive-tables.py`：14 項靜態檢查。
- 此處修改未涉及任何 Service、Models、CLI、資料庫 Schema 或 Excel 匯入流程。
- 本容器仍無 Windows／.NET SDK；這個 Hotfix 尚未執行 .NET 編譯或 Windows 300% 實機測試，必須由使用者在原環境驗收。


## 最新截圖所見空間問題修正
- `MainForm.cs`：將單一長篩選列改為兩列固定分組；第二列專放建立／更新日期起迄與排序，讓更新日期不再延伸到第一列最右端；每列水平捲動獨立計算高度。
- `ImportDialogs.cs`：DataGridView 已 Dock Fill；截圖中的大片灰色是 DataGridView 預設 AppWorkspace 背景而非表格控制項高度不足。將 BackgroundColor 及儲存格 BackColor 改為 SystemColors.Window，空白預覽區保持白色。
- 初始 `scripts/test-4.6.2-viewport-space.py` 曾有 9 項靜態契約檢查；最新版本擴充為 12 項。
- 本容器未提供 .NET SDK／Windows；本輪程式尚未在本機編譯，需由使用者 Windows 再次 `dotnet build` 並確認 300% 畫面。


## 最新 300% 主畫面列高／查詢操作列可見性修正
- `MainForm.cs`：移除巢狀 50/50 百分比列高配置；改由最外層四列分別承載主功能按鈕、一般篩選、日期／排序篩選、搜尋／清除／複製摘要／分頁，避免兩列篩選之間出現不必要垂直留白，並為查詢操作列明確分配高度。
- `MainForm.cs`：自適應 ListView 欄寬計算先預留 8 個 DPI 感知像素作為原生邊框／主題量測內縮，降低可用寬度明明足夠仍出現 1–2px 水平捲軸的情況；最小欄寬總和超過可用寬度時仍保留捲動。
- 新增及更新靜態檢查：`test-4.6.2-filter-layout.py` 15/15、`test-4.6.2-responsive-tables.py` 14/14、`test-4.6.2-viewport-space.py` 12/12；總整合測試由 `run-4.6.2-python-tests.py` 執行結束碼 0。
- 本輪僅修改 Desktop 主畫面排版、測試腳本及文件；`Services/`、Models、日期篩選控制項、Excel 預覽與匯入程式、CLI 主程式及 SQLite Schema 均未變更。
- 目前容器沒有 .NET SDK／Windows，這一輪尚未重新執行 .NET 編譯或 300% 實機顯示測試。使用者先前確認 `WFO1000Fix` 可建置成功；新增加的 MainForm 排版程式碼仍需在 Windows 執行 `dotnet build` 及畫面驗收。


## 最新 MainToolbarWrapFix（依 20:12 300% 截圖）
- `PersonalWiki.Desktop/MainForm.cs`：主功能列改用 `ArrangeActionButtonRows`，依目前 client width 將完整按鈕換行；`AutoScroll=false`，避免右側工具只有一半露出。
- `PersonalWiki.Desktop/MainForm.cs`：搜尋結果／相關條目欄寬演算法改為先配置各欄 DPI 感知最小寬度，再將剩餘可用空間依權重分配，整數餘數分配至摘要欄；可用空間不足最小寬度總和時才保留 ListView 水平捲動。
- 靜態測試更新：23 項 DPI、15 項篩選／日期、15 項表格欄寬、12 項 viewport、10 項 WFO1000／建置修正、8 項 4.6.1 UI 檢查全數通過；`run-4.6.2-python-tests.py` 結束碼 0。4.6.0 查詢契約 35/35；同一腳本會執行一個故障路徑自我測試並印出 0/1，該輸出是預期的診斷情境，不代表正式案例成功率。
- 已確認目前 MainForm 建構函式只讀取並顯示上次資料庫路徑，不會自動開啟；使用者需按「開啟上次資料庫」才會載入資料。此輪未變更該啟動行為。
- 相對上一版 StableToolbarFix：只修改 MainForm、四個既有靜態測試腳本及版本文件。`Services/`、`Models.cs`、CLI、DateFilterControl、ImportDialogs、SQLite Schema 與 Excel 匯入服務未修改。
- 本工作環境無 .NET SDK／Windows；本輪 MainForm 修改尚未執行 `dotnet build` 或 300% 實機測試。請先由 Windows 執行建置，再確認按鈕換行與結果清單欄寬。
