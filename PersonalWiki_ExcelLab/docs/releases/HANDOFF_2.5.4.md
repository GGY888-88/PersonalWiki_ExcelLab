# PersonalWiki ExcelLab 2.5.4 開發交接文件

## 目前版本與下一版本
- **目前最新版本：2.5.4**
- **下一個開發版本：2.5.5**（不可回退或從舊版重新開始）
- **唯一基準：本 ZIP 內 `PersonalWiki_ExcelLab/` 完整專案**，以 2.1.0 完整專案為唯一基礎累進修正至 2.5.4。
- ZIP 外層檔名：`PersonalWiki_ExcelLab_2.5.4.zip`；解壓後資料夾固定 `PersonalWiki_ExcelLab`，資料夾名稱不可帶版本號。

## 版本歷史（已完成）
- **2.1.0**：新增整合式唯讀資料庫診斷命令 `db-diagnostic-report`，彙整 SQLite quick_check／foreign_key_check、全文索引一致性、資料庫頁面與檔案統計、逐表筆數；同一唯讀交易快照；缺表／查詢錯誤以 warnings 記錄；異常回傳代碼 3；唯一暫存檔原子提交且拒絕覆寫；新增 acceptance 檢查來源資料庫雜湊不變及既有報告保護。Schema v1、NuGet 套件不變。
- **1.3.4**：條目多選、批次封存／還原。
- **1.3.5**：圖片牆篩選與排序。
- **1.3.6**：圖片完整性與 SHA-256 重複圖片報告，不自動刪除。
- **1.3.7**：Excel 工作表動態欄位顯示。
- **1.3.8**：同一工作表共用欄位版面，不同工作表獨立欄位名稱及順序，缺值留白。
- **1.3.9**：驗收腳本路徑、publish 目錄保護及版本文件修正。
- **1.4.0**：CLI 整合驗收及安全負向測試矩陣。
- **1.4.5**：備份／還原安全提交強化。
- **1.5.0**：備份／還原 staging、SQLite 完整性與外鍵驗證、拒絕覆蓋、逐表筆數驗證及測試矩陣。
- **1.5.1**：桌面資料庫健康檢查與立即安全備份入口。
- **2.0.0**：CLI/桌面版號整合；筆記正文及自訂欄位搜尋回歸、分頁斷言；多工作表不同版面測試活頁簿。
- **2.0.1**：修正 ZIP 套件清單 `snapshot` 輸出覆寫既有目的檔的問題，改用 `FileMode.CreateNew`；加入 SHA-256 負向測試。
- **2.0.2**：新增共用 `SafeFileOutput`，CLI 匯入預覽／品質／Dry-run／清理報告、自我測試報告、mapping／rules 範本及桌面 CSV 匯出拒絕覆寫；預檢前後比對 Excel、mapping、rules SHA-256，分析期間變更即停止。

- **2.0.3**：正式 Excel 安全匯入在備份後建立唯讀輸入快照，複製時比對預檢 SHA-256、複製後重驗原始輸入、對快照重新預檢，正式匯入只讀取快照（保留 `.xlsx`／`.xlsm` 副檔名）；來源追溯仍記錄原始 Excel 路徑。
- **2.0.4**：快照驗證完成後將 Excel／mapping／rules 快照檔標記唯讀，Dispose 清理時只解除唯一暫存目錄內檔案的唯讀屬性；SHA-256 改為 `FileStream` 串流計算及 `FileShare.Read`；self-test 新增 1 MiB `.xlsm` 快照案例，驗證唯讀、雜湊、副檔名、清理、錯誤雜湊拒絕及來源不變。
- **2.0.5**：桌面設定檔若既有 JSON 格式損毀或內容為 null，儲存前讀取會停止並保留原檔，不再靜默回退預設值覆蓋復原來源；CLI self-test 新增 SafeFileOutput 防覆寫、原子替換成功及替換失敗清理測試；快照自測擴大至 8 MiB 串流產生。

- **2.5.0**：新增 `db-diagnostic-compare`，驗證並比較兩份唯讀診斷 JSON 快照，輸出健康、Schema、資料庫容量、全文索引及資料表筆數差異；限制輸入 32 MiB、拒絕無效格式／統計值、拒絕輸入輸出路徑相同及覆蓋既有報告；Schema v1、NuGet 套件不變。此版直接以 2.1.0 為基礎累進至 2.5.0，2.2.0–2.4.0 未作為獨立 ZIP 發佈。

- **2.5.1**：修正 CLI self-test 編譯錯誤。`RunSnapshotSafetySelfTest` 與 `RunSafeFileOutputSelfTest` 不再以 `object` 作為回傳型別，改為明確的私有 record，保留原有 `Passed` 與詳細結果欄位及 JSON 輸出結構；SQLite Schema v1、NuGet 套件及既有測試邏輯不變。
- **2.5.2**：修正桌面專案 `DesktopSettingsService.cs` 缺少 `using PersonalWiki.ExcelLab.Services;`，造成 `SafeFileOutput` CS0103；只補必要命名空間引用，保留設定檔原子替換邏輯。SQLite Schema v1、NuGet 套件及其他功能不變。
- **2.5.3**：曾嘗試以 PreferredSize 動態計算主畫面工具列列高，並集中管理歷史文件；使用者實機回報按鈕仍裁字，該版 UI 修正未達成需求。

## 2.5.4 本次修正

- 確認 2.5.3 的 PreferredSize 估算發生在最終 DPI 配置前，固定列高未隨實際按鈕尺寸更新，故無法保證解決裁字。
- 新增「主畫面功能按鈕大小...」入口，與「輸入視窗按鈕大小...」使用相同的寬／高設定介面，但兩組設定分開保存。
- 主畫面工具列按鈕預設 220×64（96 DPI 基準），Shown 與 DpiChanged 後依實際 DPI 套用尺寸，並同步更新 TableLayoutPanel 列高；工具列保留水平捲動。
- 原輸入視窗設定只控制新增／編輯條目視窗內「確定儲存／取消」。
- 設定 JSON 新增 `MainActionButtonWidth`、`MainActionButtonHeight`，舊設定檔讀取時使用預設值，不需重建。
- SQLite Schema、NuGet 套件、資料庫位置及設定檔位置不變。

## 2.5.2 本次修正

- 修正 `DesktopSettingsService.cs` 缺少 `using PersonalWiki.ExcelLab.Services;`，造成 `SafeFileOutput` CS0103。僅補上必要命名空間引用。
- SQLite Schema、NuGet 套件、設定檔儲存行為及既有功能不變。

## 2.5.4 本次修改檔案
- `PersonalWiki.Desktop/MainForm.cs`：新增主畫面功能按鈕尺寸設定入口；主畫面工具列統一使用設定尺寸，於 Shown／DpiChanged 後同步套用按鈕尺寸及容器列高。
- `PersonalWiki.Desktop/DesktopSettingsService.cs`：新增 `MainActionButtonWidth`、`MainActionButtonHeight` 的獨立讀寫，保留舊 JSON 相容預設值。
- `README.md`、`docs/releases/UPDATE_NOTES_2.5.4.md`、`TEST_GUIDE_2.5.4.md`、`AUDIT_REPORT_2.5.4.md`、`HANDOFF_2.5.4.md`。
- CLI／桌面 csproj、CLI help、自我測試報告及 acceptance 測試包版號更新為 2.5.4。

## 2.5.2 本次修改檔案
- `PersonalWiki.Desktop/DesktopSettingsService.cs`：新增 `using PersonalWiki.ExcelLab.Services;`，修正 `SafeFileOutput` 名稱無法解析的 CS0103。
- CLI／桌面 csproj 版本更新為 2.5.2；CLI help、自我測試報告及 acceptance 測試包版號同步更新。
- 新增 `UPDATE_NOTES_2.5.2.md`、`TEST_GUIDE_2.5.2.md`、`AUDIT_REPORT_2.5.2.md`。

## 2.5.0 本次修改檔案
- `Services/DatabaseDiagnosticCompareService.cs`：新增診斷快照驗證及比較，僅讀取 JSON，不連線 SQLite；比較健康、Schema、容量、全文索引統計、資料表筆數、新增／消失資料表。
- `Program.cs`：新增 `db-diagnostic-compare <baseline.json> <current.json> <report.json>`；CLI help、自我測試報告及版本更新為 2.5.0。
- CLI／桌面 csproj 版本更新為 2.5.0。
- `scripts/run-acceptance-tests.ps1`：新增同快照零差異、輸入雜湊不變、輸出路徑衝突、拒絕覆寫及損毀 JSON 測試。
- 新增 `UPDATE_NOTES_2.5.0.md`、`TEST_GUIDE_2.5.0.md`、`AUDIT_REPORT_2.5.0.md`。

## 2.1.0 本次修改檔案
- `Services/DatabaseDiagnosticReportService.cs`：新增整合式唯讀診斷 JSON，包含資料庫健康、外鍵、全文索引、儲存頁面及資料表統計；所有 SQLite 查詢共用單一 deferred 唯讀交易快照，不輸出條目／筆記／欄位內容。
- `Program.cs`：新增 `db-diagnostic-report <database.db> <report.json>`；健康或索引不一致回傳代碼 3；CLI help、自我測試報告及版本更新為 2.1.0。
- CLI／桌面 csproj 版本更新為 2.1.0。
- `scripts/run-acceptance-tests.ps1`：新增診斷報告完整性、來源資料庫雜湊不變及拒絕覆寫測試。
- 新增 `UPDATE_NOTES_2.1.0.md`、`TEST_GUIDE_2.1.0.md`、`AUDIT_REPORT_2.1.0.md`。

## 2.0.5 本次修改檔案
- `PersonalWiki.Desktop/DesktopSettingsService.cs`：既有設定檔 JSON 損毀或為 null 時，儲存前停止並保留原始檔，避免用預設設定靜默覆蓋復原資料。
- `Program.cs`：新增 SafeFileOutput 防覆寫、原子替換成功、失敗暫存清理 self-test；快照測試改為 8 MiB 串流產生。
- CLI／桌面 csproj、CLI help、自我測試報告及 acceptance 測試包版號更新為 2.0.5。
- 新增 `UPDATE_NOTES_2.0.5.md`、`TEST_GUIDE_2.0.5.md`、`AUDIT_REPORT_2.0.5.md`。

## 2.0.4 本次修改檔案
- `Services/ExcelImportInputSnapshot.cs`：快照驗證完成後標記唯讀；清理時只解除唯一暫存目錄內檔案唯讀屬性。
- `Services/ExcelImportPreflightService.cs`：`HashFile` 改為串流 SHA-256，避免整個大型活頁簿載入記憶體；使用 `FileShare.Read`。
- `Program.cs`：self-test 新增 1 MiB `.xlsm` 快照測試，驗證唯讀、雜湊、副檔名、Dispose 清理、錯誤雜湊拒絕及來源不變。
- CLI／桌面 csproj、CLI help、自我測試報告、acceptance 測試包版號更新為 2.0.4。
- 新增 `UPDATE_NOTES_2.0.4.md`、`TEST_GUIDE_2.0.4.md`、`AUDIT_REPORT_2.0.4.md`。

## 2.0.3 本次修改檔案
- `Services/ExcelImportInputSnapshot.cs`：唯一暫存快照、唯讀來源、CreateNew、串流 SHA-256、來源複製後重驗與暫存清理。
- `Services/ExcelImportWorkflowService.cs`：備份後建立快照、快照預檢、正式匯入改讀快照。
- `Services/ExcelDatabaseImportService.cs`：新增可選原始來源追溯路徑，避免將暫存路徑寫入來源追溯資料。
- CLI／桌面版號、README、更新說明、測試指南、稽核報告更新為 2.0.3。
- `scripts/run-acceptance-tests.ps1`：測試包版號更新為 2.0.3，新增安全匯入成功案例，檢查批次已驗證且來源追溯記錄原始 Excel 路徑。

## 2.0.2 本次修改檔案
- `Services/SafeFileOutput.cs`：共用 no-clobber 文字輸出方法。
- `Services/ImportMappingService.cs`、`Services/ImportCleaningRulesService.cs`：範本輸出改用 SafeFileOutput。
- `Services/ExcelImportPreflightService.cs`：預檢前後驗證三個輸入檔案雜湊穩定。
- `Program.cs`：匯入預覽、品質、Dry-run、清理檢查及自我測試報告改用 SafeFileOutput；CLI 版號更新。
- `PersonalWiki.Desktop/MainForm.cs`、`TagManagementDialog.cs`：CSV 匯出與 DPI 診斷紀錄拒絕覆寫。
- `PersonalWiki.Desktop/ImportDialogs.cs`、`DesktopSettingsService.cs`：明確更新設定檔時改用唯一暫存檔完成寫入後替換，降低半成品風險。
- CLI 與桌面 csproj 版本更新為 2.0.2。
- `Program.cs`：CLI help、自我測試報告版本更新為 2.0.2。
- `PersonalWiki.ExcelLab.csproj`、`PersonalWiki.Desktop/PersonalWiki.Desktop.csproj`：版本 2.0.2。
- `scripts/run-acceptance-tests.ps1`：新增匯入預覽／品質報告／mapping／rules 範本／Dry-run 輸出不覆寫的 SHA-256 負向測試。
- `README.md`、`UPDATE_NOTES_2.0.2.md`、`TEST_GUIDE_2.0.2.md`、`AUDIT_REPORT_2.0.2.md`。

## 專案結構
- `docs/releases/`：集中管理歷代交接文件、更新說明、測試指南、稽核報告及桌面 UI 測試矩陣。
- `Program.cs`：CLI 路由、自我測試及 JSON 報告。
- `Models.cs`：共用模型。
- `Services/`：Excel 讀寫副本、預檢、mapping、Dry-run、交易匯入、SQLite 備份還原、健康檢查、索引、條目／筆記／標籤／圖片與資料稽核。
- `PersonalWiki.Desktop/`：Windows Forms 桌面端。
- `scripts/`：CLI／桌面建置、portable 發布、自我測試及整合驗收。
- `samples/`：`PersonalWiki_Import_Sample.xlsx`、`PersonalWiki_MultiLayout_Test.xlsx` 等測試活頁簿。

## 資料庫 Schema 與套件
- SQLite `user_version=1`，2.0.3／2.0.4 均無 Schema 變更、無 migration。
- CLI `net10.0`；桌面 `net10.0-windows`、Windows Forms。
- NuGet：`DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`。無新增套件。
- 桌面設定位於 `%LOCALAPPDATA%\PersonalWiki\desktop-settings.json`，與資料庫分離。

## 必須遵守的資料安全限制
1. 原始 Excel 唯讀；寫入只允許使用者明確指定的新副本。
2. 正式匯入必須預檢、健康檢查、建立並驗證新備份、交易式匯入及批次驗證。
3. 備份／還原／匯出／報告不得覆蓋既有目的檔；還原使用新路徑。新增輸出功能一律採 `FileMode.CreateNew` 或等效拒絕覆寫方式。
4. 備份／還原 staging 通過 SQLite `integrity_check` 與 `foreign_key_check` 才提交。
5. 不自動刪除或合併條目、筆記、標籤或來源追溯資料。
6. 移除圖片關聯只移除資料庫關聯，不刪除／移動硬碟原圖。
7. 不擅自修改 Schema、資料庫位置或設定檔位置；若必要，先提出 migration、備份與回復方案並等使用者確認。
8. 所有自動化測試使用新建隔離資料庫與測試檔，不碰正式資料庫。

## 已知問題、未測試及環境限制
- 本開發容器沒有 `dotnet`、Windows、PowerShell；未執行 .NET restore/build、CLI acceptance、self-test 或 WinForms 實機測試。
- 2.0.5 新增的設定檔損毀保護與 SafeFileOutput 自測尚未實際執行；8 MiB `.xlsm` 自測檔是測試位元組流，不是有效 Excel 活頁簿。
- 300% DPI、多螢幕、縮放、視窗尺寸、工作表欄位顯示、批次封存／還原仍待 Windows 實機驗收。
- 2.0.1 的 snapshot 負向測試與 2.0.2 新增的五項輸出防覆寫負向測試均尚未在 Windows 執行。
- XLSM/VBA 巨集、Power Query、外部連線、密碼保護工作表、內嵌物件及超大型活頁簿未全面驗證。
- `啟動 PersonalWiki.bat` 使用 `dotnet run --project .\PersonalWiki.Desktop\PersonalWiki.Desktop.csproj -c Release`，執行端需 .NET 10 SDK。

## 下一階段計畫（2.5.5 起）
1. 優先在 Windows/.NET 10 執行 restore/build 與完整 acceptance，修正實際發現的編譯或執行錯誤。
2. 實機驗收 2.5.4 新增的主畫面功能按鈕寬高設定：100%–300% DPI、不同字型、多螢幕、水平捲動、重新啟動後設定保留，並確認與輸入視窗設定互不影響。
3. 對輸入快照流程執行 Windows 檔案鎖定、來源變更競態、磁碟滿載、大型檔案及強制中止測試。
4. 實際驗證桌面設定檔損毀時的錯誤呈現、原檔保留及人工復原；驗證 mapping／rules 編輯器暫存替換失敗復原。
5. 進行多工作表匯入精靈與 300% DPI 實機驗收。
6. 評估 portable WinForms 發布，不改變資料庫及設定檔位置。
7. 擴充診斷快照比較測試，驗證資料表新增／移除、資料列增減、Schema 差異及異常健康狀態的差異輸出。

## 可直接複製到新對話的完整開發指令
```text
請接續開發 PersonalWiki ExcelLab，必須以我提供的 PersonalWiki_ExcelLab_2.5.4.zip 為唯一基準。最新版本 2.5.4，下一版必須是 2.5.5，不可回退或從舊版重新開始。先閱讀 docs/releases/HANDOFF_2.5.4.md、docs/releases/AUDIT_REPORT_2.5.4.md、README.md、docs/releases/UPDATE_NOTES_2.5.4.md、docs/releases/TEST_GUIDE_2.5.4.md，再檢查原始碼。

保留 1.3.4 至 2.5.3 全部功能，並保留 2.5.4 新增的主畫面功能按鈕獨立尺寸設定及 `docs/releases/` 文件集中管理：批次封存／還原、圖片篩選排序與完整性報告、各工作表獨立欄位名稱及順序、Excel 預檢/mapping/Dry-run/交易匯入、SQLite 備份還原 staging、健康檢查、全文搜尋與分頁、資料稽核、2.0.1 snapshot 防覆寫、2.0.2 SafeFileOutput 防覆寫及預檢前後 SHA-256 穩定性檢查、2.0.3 正式匯入唯讀快照及來源追溯原始路徑保留；2.0.4 快照唯讀屬性、串流 SHA-256 及快照安全自我測試；2.0.5 桌面設定檔損毀保護、SafeFileOutput 自我測試及 8 MiB 快照串流測試；2.1.0 整合式唯讀資料庫診斷報告；2.5.0 診斷快照比較（格式驗證、統計差異、32 MiB 上限、輸出防覆寫）；2.5.1 修正 self-test 結果型別造成的 CS1061 編譯錯誤；2.5.2 修正 DesktopSettingsService SafeFileOutput 命名空間造成的 CS0103；2.5.3 曾嘗試修正主畫面工具列高 DPI 裁字並集中版本文件，但使用者實機回報仍裁字；2.5.4 已改為新增獨立的主畫面按鈕尺寸設定並於實際 DPI 後同步調整父層列高。不得重做、移除或破壞已完成部分。

SQLite user_version=1，不得未經同意修改 Schema。CLI net10.0；桌面 net10.0-windows Windows Forms。NuGet DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14；新增套件前先說明必要性。

安全限制：來源 Excel 唯讀；正式匯入必須預檢、健康檢查、建立並驗證新備份、再檢查來源 Excel/mapping/rules 雜湊、建立並重新預檢唯讀輸入快照、只從快照交易式匯入及匯入後批次驗證；所有備份/還原/匯出/報告/範本不得覆蓋既有目的檔，統一採 SafeFileOutput 或 FileMode.CreateNew／等效拒絕覆寫；還原使用新路徑；不自動刪除或合併條目、筆記、標籤或來源追溯資料；移除圖片關聯不得刪除原始圖片；不得擅自變更資料庫及設定檔位置。使用者明確編輯既有 mapping/rules/設定檔時可更新原檔，但必須透過唯一暫存檔完整寫入後再替換，避免半成品破壞。Schema 變更先提出 migration、備份及回復計畫並等待確認。

先完成一批可獨立開發功能再集中測試，不要每一小版要求我測試。每次交付必須實際修改程式、更新版號與文件，將完整原始碼、最新 ZIP、測試活頁簿、測試腳本、Schema/套件/專案結構、已知問題、未測試項目、安全規則及下一版任務一起打包。ZIP 解壓後資料夾固定 PersonalWiki_ExcelLab，不帶版本號。不可虛報未執行的 build 或 Windows 測試。

2.5.5 優先工作：在 Windows/.NET 10 執行 restore/build/acceptance；優先實機驗收 2.5.4 主畫面功能按鈕寬高設定在 100%–300% DPI、多螢幕及重新啟動後的表現，確認兩組按鈕設定互不影響。驗證 2.0.3／2.0.4 輸入快照流程的檔案鎖定、變更競態、磁碟滿載、大型檔案及強制中止行為；實際驗證 2.0.5 設定檔損毀保護與原子寫入復原；檢查桌面 mapping/rules 編輯器儲存；驗證 2.1.0 診斷報告在唯讀、資料庫異常、索引不一致及輸出目的檔已存在情境下的行為；擴充 2.5.0 診斷快照比較，測試新增／移除資料表、筆數差異及 Schema 差異；維持 SQLite Schema v1 及所有既有安全行為。
```

## 打包規則
每次版本 ZIP 必須包含完整專案原始碼、最新交接文件、版本歷史、Schema 與套件資訊、專案結構、測試資料與測試方式、已知問題／未測試／環境限制、安全規則、下一階段任務及可直接複製的新對話開發指令。外層 ZIP 可帶版本號，內層資料夾固定 `PersonalWiki_ExcelLab`。


## 2.5.4 驗收重點
1. 使用「主畫面功能按鈕大小...」獨立設定工具列按鈕寬高，儲存後立即生效，重新啟動後保留。
2. Windows 顯示縮放 100%、125%、150%、200%、250%、300% 下，主畫面「新增條目／編輯條目」等工具列按鈕文字完整，列高同步調整。
3. 調整輸入視窗按鈕大小後，只改變新增／編輯對話框內「確定儲存／取消」，不改變主畫面工具列；反向亦然。
4. 主畫面工具列保持單列，超出視窗寬度時可水平捲動；跨 DPI 螢幕時尺寸重新計算。
5. 所有歷代交接、更新、測試及稽核文件均集中在 `docs/releases/`，最新索引為 `docs/releases/INDEX.md`。
