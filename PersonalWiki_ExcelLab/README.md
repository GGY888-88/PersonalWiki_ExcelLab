# PersonalWiki ExcelLab

**目前整合開發版本：4.7.0 DEV05（開發中，未驗收）**  
**下一開發版本：4.7.0 DEV06**  
**最新交接：** `docs/releases/HANDOFF_4.7.0-DEV05.md`  
**版本文件索引：** `docs/releases/INDEX.md`



## 4.7.0 DEV05 — 相關條目摘要唯讀預覽
- 相關條目頁新增摘要預覽按鈕及雙擊入口，顯示名稱、類型、更新時間、摘要與推薦依據。
- 可複製條目名稱；預覽不修改資料庫、不改變目前搜尋條件或選取條目。
- 沿用既有 `RelatedEntryService` 資料，不修改 SQLite Schema、資料路徑或推薦演算法。
- 本版尚未執行測試、.NET 編譯或 Windows 實機驗收；詳見 `docs/releases/HANDOFF_4.7.0-DEV05.md`。

## 4.7.0 DEV04 — Excel 匯入問題明細檢視與導覽
- Dry-run 發現需人工檢查資料時，使用獨立唯讀視窗依工作表／規則呈現問題、匹配筆數及最多 20 個 Excel 列號樣本。
- 可切換問題明細並複製列號；可返回既有規則設定，或取消本次匯入。
- 不顯示儲存格原文，不自動修正／略過資料；不修改 SQLite Schema、資料路徑或既有匯入安全流程。
- 本版尚未執行測試、.NET 編譯或 Windows 實機驗收；詳見 `docs/releases/HANDOFF_4.7.0-DEV04.md`。

## 4.7.0 DEV03 — Excel 匯入批次差異比較
- 新增唯讀「Excel 批次差異比較」，可比較兩次匯入批次的條目及自訂欄位新增、移除、修改摘要。
- 支援將比較結果匯出為 UTF-8 JSON；沿用既有批次管理及差異比較服務。
- 不修改 SQLite Schema、資料庫路徑、Excel 匯入安全流程或既有匯入歷史視窗。
- 本版未執行測試、.NET 編譯或 Windows 實機驗收；詳見 `docs/releases/HANDOFF_4.7.0_DEV03.md`。

## 4.7.0 DEV02 — Excel 匯入歷史檢視
- 新增 Excel 匯入批次歷史的唯讀檢視，詳見 `docs/releases/UPDATE_NOTES_4.7.0_DEV02.md`。
- 保留既有匯入安全流程；測試狀態以該版交接文件為準。

## 4.7.0 DEV01 — 標籤使用統計報告
- 標籤管理新增唯讀 JSON 使用統計報告，重用既有 `DatabaseTagUsageReportService`。
- 報告包含標籤數、含標籤條目數、關聯總數、單次／多次使用數及分布；拒絕覆蓋既有檔案。
- 不修改 SQLite Schema、資料路徑或既有標籤管理操作。
- 本階段依使用者要求未執行測試、未執行 .NET 編譯及 Windows 實機驗收。

## 5.0.0 持續開發路線
- 目標版本：5.0.0，採全面強化方向，兼顧知識管理與 Excel 資料管理。
- 4.7.0 先盤點既有功能與控制項；4.8.0 強化知識管理；4.9.0 強化 Excel 與資料維護；5.0.0 整合發行。
- 路線圖：`docs/roadmap/ROADMAP_5.0.0.md`。
- 4.6.6 功能盤點：`docs/architecture/FEATURE_INVENTORY_4.6.6.md`。
- 4.7.0 開發交接：`docs/releases/HANDOFF_4.7.0_DEV.md`。
- 本階段只新增規劃／盤點文件，未修改功能程式碼，依使用者要求未執行測試。

## 4.6.6 三個主畫面輸入框高度微調
- 依使用者最新 300% DPI 畫面回報，只縮小資料庫路徑、全文搜尋、標籤三個輸入框；三者設計高度統一為 26 logical px，保留內層原生單行文字框置中。
- 只調整這三個欄位的高度與 EnsureTextFits 基準／上下留白；不調字體、不改欄寬、不改搜尋行為、不重排其他欄位。
- 本輪依使用者要求不執行測試；Windows 編譯、畫面驗收與回歸測試均明確列為待辦。
- 主功能按鈕 80×22、300% DPI 既有排列、SQLite、Excel 匯入及其他欄位維持原狀。

## 4.6.4 主畫面輸入欄位垂直置中修正
- 依 2026-11-15 的 300% DPI 實際畫面回報，確認 4.6.3 原生單行 TextBox 仍有文字偏上情況；不再假設單行 EDIT 會自動在被拉高的控制項中置中。
- `VerticallyCenteredTextBox` 改為外層固定高度／凹陷邊框容器，內層使用 Windows 原生 `AutoSize=true`、`Multiline=false` 單行 TextBox，依目前字型的 `GetPreferredSize` 垂直置中。
- 原生 `PlaceholderText`、輸入、選取、複製、唯讀、Enter 搜尋及資料庫路徑雙擊複製行為保留；原本三個自訂欄位（資料庫路徑、全文搜尋、標籤）才使用此包裝控制項。
- 圖片篩選、條目名稱／類型、標籤重新命名、日期、ComboBox、多行摘要與筆記均不替換、不重排；原本版面高度與主功能按鈕 80×22 設定不主動更動。
- 三個 BAT 只更新版本顯示字串，仍為 ASCII + CRLF；原先 build log 相容性修正保留。
- 自動靜態回歸：`python scripts/run-4.6.4-python-tests.py`。本工作環境沒有 .NET SDK／Windows，尚未執行 .NET 編譯或 100%／150%／200%／300% WinForms 實機驗收。

## 4.6.3 單行輸入框垂直置中與 BAT 輸出修正
- 自訂 `VerticallyCenteredTextBox` 改回 Windows 原生單行 EDIT，保留 `AutoSize=false` 以維持版面高度並支援原生 `PlaceholderText` Cue Banner。
- 資料庫路徑、全文搜尋及標籤篩選欄位使用原生單行文字排版；其餘原生單行輸入欄位及多行文字內容維持原本控制項行為。
- 三個 BAT 保持 ASCII + CRLF；建置記錄保留於 `build_release.log`，錯誤時由記事本開啟，不再直接輸出整份記錄至 CMD。
- 保留主功能按鈕既有使用者設定（包含 80×22）、300% DPI 按鈕排列、資料庫 Schema、NuGet 套件及 Excel 匯入安全流程。
- 自動檢查：`python scripts/run-4.6.3-python-tests.py`。Windows `.NET 10` 編譯及 300% 實機文字位置仍需依 `docs/releases/TEST_GUIDE_4.6.3.md` 驗收。

## 4.6.2 主畫面按鈕與搜尋欄位微調（MainUiRefine）
- 主畫面功能按鈕尺寸設定可縮小至 40×20（96 DPI 基準），調整步進為寬 5、高 2；此範圍只套用主畫面功能按鈕，不改動新增／編輯條目視窗按鈕。
- 主畫面功能按鈕縮小內距及字體，仍保留依文字自動量測與完整換行排列。
- 資料庫路徑、全文搜尋及標籤搜尋欄位改用垂直置中文字框，改善 300% DPI 下文字偏上問題。
- 新增 `01_建置程式.bat`、`02_啟動程式.bat`、`03_建置並啟動.bat`；建置輸出寫入 `build_release.log`，失敗時保留錯誤畫面。三個 BAT 使用純 ASCII + CRLF，避免繁體中文 Windows CMD 對 UTF-8 批次檔的錯誤解析；`03` 內建建置流程，不再呼叫其他中文檔名 BAT。
- 本次未修改日期篩選、資料庫 Schema、Models、Services、CLI、Excel 欄位對應或匯入交易流程。
- 驗收：來源差異與封裝完整性已檢查；此環境沒有 .NET SDK，實際編譯與 300% WinForms 置中效果仍待 Windows 驗收。

## 4.6.2 DPI 自適應修正
- Desktop 專案啟用 `PerMonitorV2`，WinForms 表單以 96 DPI 為基準進行 DPI 自動縮放。
- 主畫面資料庫路徑移至篩選列前段；搜尋條件分為兩列固定分組：第一列為資料庫／全文搜尋／類型／封存／標籤，第二列為建立及更新日期起迄／排序。每組標籤＋輸入框固定成組，必要時只捲動所在列，不會自動換行或拆散欄位。日期篩選使用獨立核取方塊及日期輸入框，避免 DateTimePicker 內建核取方塊在高 DPI 下擠壓日期文字。搜尋輸入框、下拉欄位與操作按鈕依目前字型實際高度調整；主功能按鈕依可用寬度整顆換行顯示，不裁切右側工具；兩列篩選及搜尋／清除／分頁操作列採獨立高度配置；筆記／圖片等工具列依內容自動加高。主畫面與對話框會限制在目前螢幕工作區內。
- Excel 預覽表格依字型與儲存格換行自動計算資料列高度、欄寬填滿可用寬度；未使用的表格工作區背景設為白色，避免只有少量資料時出現大片預設灰色；欄位設定及資料安全檢查步驟使用自適應標籤寬度、可換行表頭／按鈕列與可捲動資料區。
- 目標縮放比例為 100%、125%、150%、175%、200%；另納入 300% 工作區縮小的防護。資料庫 Schema、資料路徑、匯入服務／交易邏輯及 NuGet 套件不變。
- 300% 回報修正：全文搜尋欄位、筆記「新增筆記」、圖片「新增圖片關聯」等控制項依活動 DPI 與字型實際高度擴張；Excel 預覽列啟用自動列高及文字換行。
- 測試：`python scripts/test-4.6.2-dpi.py`、`python scripts/test-4.6.2-viewport-space.py`、`python scripts/run-4.6.2-python-tests.py`。WinForms 實機 DPI 驗收與 `dotnet build` 仍需 Windows／.NET 10 SDK。

## 4.6.1 UI 修正
- 主畫面「目前資料庫位置」移至搜尋列尾端，不再獨占一整列；完整路徑仍可透過文字框選取與複製。
- Excel 匯入精靈欄位用途步驟加高「第一筆資料列」控制列，固定標籤尺寸與垂直置中，避免文字被裁切。
- 僅修改 WinForms 版面配置；未變更匯入邏輯、資料庫 Schema、資料路徑或相依套件。
- 測試：`python scripts/test-4.6.1-ui.py`；WinForms 實機仍需 Windows／.NET 10。

## 4.6.0 新增功能
- Acceptance runner 支援 `--case-index 1..15` 單案例重跑。
- 單一案例失敗會保留診斷並繼續後續案例；每完成一組即保存 JSON／Markdown 報告。
- 報告包含搜尋／Benchmark 命令、錯誤欄位、未執行案例數與可複製重跑命令。
- 測試：`python scripts/test-4.6.0-query-acceptance.py`。真實 CLI 仍需 Windows／.NET 10。

## 4.5.0 新增功能
- Benchmark JSON 新增 `resultIds`，提供當頁結果 ID 與順序。
- `scripts/entry-query-acceptance.py` 使用隔離資料庫副本呼叫真實 .NET CLI，對照搜尋與 Benchmark 的篩選條件、總數、分頁及 ID 順序。
- 覆蓋五種排序、分頁、日期、標籤、類型及封存條件，輸出 JSON／Markdown 驗收報告。
- 測試：`python scripts/test-4.5.0-query-acceptance.py`。真實 .NET acceptance 需 Windows／.NET 10，詳見 `docs/releases/TEST_GUIDE_4.5.0.md`。

## 4.4.0 新增功能
- CLI `entry-query-benchmark <database.db>`：使用既有 `EntryQueryService` 量測端到端唯讀搜尋耗時。
- 支援關鍵字、類型、標籤、封存、排序、日期範圍、分頁及 1–20 次迭代；暖機一次後回報 min／median／average／max 及各次樣本。
- 量測期間結果總數或當頁 ID 改變會中止報告。
- 新增 CLI／Desktop 共用搜尋契約與隔離 SQLite 分頁測試。
- 查詢計畫是規劃器估算；Benchmark 是實際端到端延遲，兩者分開解讀。

## 保留功能
- 4.1.0：桌面搜尋結果多選摘要複製，支援 Ctrl／Shift 多選，唯讀、不修改資料庫。
- 4.0.0：CLI `entry-query --output <report.json>` 安全匯出搜尋結果 JSON，拒絕覆寫既有目的檔。
- 3.6.0–3.9.0：複合搜尋、日期範圍、標籤、排序、相關條目、圖片管理、備份還原安全及發行完整性檢查。

## 技術基準
- CLI：.NET 10 (`net10.0`)
- Desktop：.NET 10 WinForms (`net10.0-windows`)
- NuGet：`DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`
- SQLite `PRAGMA user_version=1`；未經使用者明確批准不得修改 Schema。
- 桌面設定：`%LOCALAPPDATA%\PersonalWiki\desktop-settings.json`

## Python 測試

```powershell
python scripts/test-4.6.2-viewport-space.py
python scripts/run-4.6.2-python-tests.py
```

4.6.0 新增測試與歷史回歸結果以執行輸出為準，並執行 4.3.0 至 3.6.0 歷史 Python／SQLite／靜態回歸測試，全部通過。測試僅驗證 SQLite fixture、靜態程式碼契約及發行素材，不等於 .NET 編譯或 Windows 桌面驗收。

目前建置環境沒有 dotnet SDK／Windows，未執行 .NET restore/build、CLI 真實資料庫 acceptance 或 WinForms 實機測試。Benchmark 包含連線、COUNT、分頁及本頁標籤查詢；數值受 OS 快取及背景負載影響，詳見 `docs/releases/UPDATE_NOTES_4.4.0.md`。

## 資料安全
原始 Excel 唯讀；正式匯入需先完成預檢、備份、雜湊及唯讀快照驗證，並在單一 SQLite 交易提交前執行 `quick_check(1)` 和 `foreign_key_check`。備份／還原／匯出／報告拒絕覆蓋既有檔案；還原只寫入新路徑；移除圖片關聯不刪除原始圖片；不自動刪除或合併條目、筆記、標籤或來源追溯資料。Schema、DB 路徑及設定路徑不得擅自變更。

## 備份交付
使用 `python scripts/package-release.py --output ../PersonalWiki_ExcelLab_4.6.4.zip` 建立本版來源 ZIP；工具拒絕覆寫既有檔案，並產生 SHA256 manifest、固定解壓根目錄及執行 CRC 驗證。
