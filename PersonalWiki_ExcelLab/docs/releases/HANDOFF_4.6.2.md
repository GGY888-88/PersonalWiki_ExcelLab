# PersonalWiki ExcelLab 4.6.2 開發交接文件

## 1. 版本狀態
- 最新來源版本：4.6.2（由 4.6.1 原始 ZIP 延續修改）。
- 下一開發版本：4.7.0。
- 本版目標：修正高 DPI 下 WinForms 版面配置，特別是主畫面及 Excel 匯入精靈。

## 2. 4.6.2 已完成項目
- Desktop 專案設定 `ApplicationHighDpiMode=PerMonitorV2`。
- 程式碼建立的主要表單明確設定 96 DPI 基準及 `AutoScaleMode.Dpi`。
- 新增視窗工作區限制，包含 DPI 切換後重新定位／縮小。
- 主畫面資料庫路徑移至搜尋列前段；篩選區採兩列明確分組，第一列為資料庫／全文搜尋／類型／封存／標籤，第二列為建立日期起迄／更新日期起迄／排序。標籤與輸入框以固定尺寸成組排列，不允許自動換行或壓縮；每列空間不足時只在該列水平捲動。搜尋／清除／分頁按鈕獨立成操作列；主功能按鈕、兩列篩選及操作列使用單一像素配置器依實際控制項尺寸計算列高，不混用 TableLayoutPanel 自動列高與手動像素列高。最新修正主功能按鈕依可用寬度以完整按鈕為單位換行，避免最後一批功能被截斷。
- 300% 日期篩選重疊後續修正：日期啟用核取方塊從 `DateTimePicker` 內建繪製中拆出，改為獨立 `CheckBox`＋`DateTimePicker`，明確計算兩者間距、位置及總寬；保留日期篩選的 `Checked`／`Value` 查詢語義。兩個執行期狀態屬性以 `DesignerSerializationVisibility.Hidden` 標記，避免 .NET 10 WinForms WFO1000 嘗試將其當作設計器屬性序列化。
- 筆記、圖片、標籤、來源工具列會依按鈕文字／字型尺寸動態加高，避免 300% 時按鈕下半部文字被裁切。
- Excel 待匯入資料預覽表格不再依賴固定資料列高度；儲存格文字換行並自動計算列高。
- Excel 預覽、欄位設定、資料檢查三個步驟改用自動縮放；標籤換行、表頭自動高度、底部按鈕列可換行，長檢查項目會依可用寬度增加高度。
- 最新表格欄寬 Hotfix：主畫面搜尋結果及相關條目 ListView 依面板可用寬度與欄位權重動態分配寬度；先配置欄位最小寬度，再分配剩餘空間，僅在總可用寬度小於最小欄寬總和時才允許水平捲動。Excel 預覽 DataGridView 使用 Fill 欄寬，依欄名與樣本文字測量 FillWeight，保留最小欄寬、換行與列高自動調整。
- 300% 最新截圖空間修正：主篩選區分為兩列，更新日期起迄不再排在單行最右側；Excel 預覽 DataGridView 的空白 client area 與儲存格背景改為 SystemColors.Window，讓填滿高度的預覽工作區不再呈現大片預設灰色。最新再調整為四個外層獨立列，移除巢狀 50/50 百分比分配以避免不必要垂直空白並讓查詢按鈕列可見；搜尋結果清單欄寬另保留小幅原生邊框內縮以避免像素級水平溢位。

## 3. 技術基準
- CLI：.NET 10，`net10.0`；Desktop：WinForms，`net10.0-windows`。
- NuGet：`DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`，版本不變。
- SQLite `PRAGMA user_version=1`，無 Schema migration。
- 桌面設定路徑：`%LOCALAPPDATA%\PersonalWiki\desktop-settings.json`，不變。

## 4. 資料安全限制（必須維持）
1. 原始 Excel 唯讀；匯入前預檢、備份驗證、來源雜湊重驗、唯讀快照、單一交易及 quick_check／foreign_key_check 保護必須保留。
2. 備份／還原／匯出／報告拒絕覆寫；還原只寫入新路徑。
3. 不自動刪除或合併 entries／notes／tags／source traces。
4. 移除圖片關聯不刪除原圖。
5. 未經明確批准，不得修改 Schema、資料庫路徑、設定路徑或套件版本。

## 5. 變更範圍
- Desktop UI、版本資訊、測試腳本及文件。
- 本次最新 MainToolbarWrapFix 僅改 `PersonalWiki.Desktop/MainForm.cs` 的主功能按鈕排列及 ListView 欄寬分配，另更新靜態測試與文件。
- 未修改 `Services/`、`Models.cs`、CLI `Program.cs`、SQLite Schema 或 Excel mapping／匯入資料流程。

## 6. 測試狀態
- 最新整合 `python scripts/run-4.6.2-python-tests.py` 結束碼 0；23 項 DPI、15 項篩選列／日期、15 項自適應表格、12 項空間配置、10 項 WFO1000／建置修正及 8 項 UI 檢查通過；4.6.0 查詢契約檢查 35/35 通過。舊案例執行器的 0/1 是故障路徑自我測試輸出，不能宣稱正式查詢資料案例已全數驗收。
- 本容器沒有 .NET SDK／Windows，未執行 `dotnet build` 或 WinForms 實機測試。
- 100%／125%／150%／175%／200% 及 300% 邊界驗收請依 `TEST_GUIDE_4.6.2.md`。

## 7. 啟動及下一步
- 啟動時狀態列顯示「上次使用資料庫」只代表記錄上次路徑；按「開啟上次資料庫」或「開啟資料庫」後才會載入結果清單。
- 先完成 4.6.2 Windows DPI 實機驗收，再開始 4.7.0 功能規劃。
- 若發現特定控制項仍裁切，請提供縮放比例、螢幕解析度、視窗步驟及截圖；只修正對應 UI，不要改匯入／DB 邏輯。

## 4.6.2 MainUiRefine BAT 相容性修正（2026-11-15）
- 使用者在繁體中文 Windows 雙擊 BAT 時回報命令被錯誤拆解、中文訊息亂碼及視窗瞬間關閉。
- 三個 BAT 已改為純 ASCII、CRLF；移除 `chcp 65001` 及中文 `echo`，改用 `pushd "%~dp0"` 與專案相對路徑。
- `03_建置並啟動.bat` 改為獨立建置／啟動，不呼叫中文檔名的其他 BAT；所有錯誤分支均 pause 並保留結果。
- 尚待使用者 Windows 實機雙擊驗收；本環境無 .NET SDK，不能代替實際建置。
