# PersonalWiki ExcelLab 4.6.1 開發交接文件

## 1. 版本狀態
- 最新版本：4.6.1。
- 下一開發版本：4.7.0（不可從 4.6.0 或更早版本重新開始）。
- 最新來源 ZIP：`PersonalWiki_ExcelLab_4.6.1.zip`。
- 本版只修正兩項 UI：資料庫位置不再獨占一列；Excel 匯入欄位設定中的「第一筆資料列」文字避免裁切。

## 2. 已完成版本（避免重複）
- 3.6.0：大型資料集搜尋／排序、備份 WAL 鎖定及復原安全。
- 3.7.0：複合篩選與匯入相關功能。
- 3.8.0：圖片關聯管理。
- 3.9.0：發行安全與完整性檢查。
- 4.0.0：CLI entry-query JSON 安全匯出。
- 4.1.0：桌面搜尋結果 Ctrl／Shift 多選摘要複製。
- 4.2.0：唯讀 SQLite 查詢計畫診斷。
- 4.3.0：補齊 FTS／LIKE 關鍵字與日期查詢計畫。
- 4.4.0：唯讀端到端 Benchmark。
- 4.5.0：Benchmark resultIds 及 CLI 搜尋／Benchmark Acceptance。
- 4.6.0：Acceptance 失敗診斷、部分報告持續保存、單案例重跑。
- 4.6.1：主畫面資料庫路徑縮為搜尋列單行資訊；匯入欄位設定列防裁切。

## 3. 技術基準與結構
- CLI：.NET 10，`net10.0`；Desktop：WinForms，`net10.0-windows`。
- NuGet：`DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`；無新增套件。
- SQLite `PRAGMA user_version=1`；4.6.1 沒有 Schema migration。
- 桌面設定：`%LOCALAPPDATA%\PersonalWiki\desktop-settings.json`。
- 主要結構：`Services/`、`PersonalWiki.Desktop/`、`scripts/`、`samples/`、`docs/releases/`、根目錄 CLI 專案。
- 測試素材：`samples/PersonalWiki_3.6.0_Test.db`、`samples/PersonalWiki_Import_Sample.xlsx` 及既有 mapping／rules JSON。

## 4. 資料安全限制（必須維持）
1. 原始 Excel 唯讀；匯入前預檢、備份驗證、來源雜湊重驗、唯讀快照、單一交易，提交前 `quick_check`／`foreign_key_check`。
2. 備份／還原／匯出／報告拒絕覆寫；還原只能寫入新路徑。
3. 不自動刪除或合併 entries、notes、tags、source traces。
4. 移除圖片關聯不刪除原圖。
5. 未經使用者明確批准，不得修改 Schema、資料庫路徑、設定路徑或套件版本。
6. 測試只使用隔離資料庫副本與全新輸出路徑。
7. 搜尋、查詢計畫與 Benchmark 維持 ReadOnly。

## 5. 本版變更檔案
- `PersonalWiki.Desktop/MainForm.cs`：頂端區由三列改為兩列；資料庫路徑移至搜尋列尾端，仍可選取／複製。
- `PersonalWiki.Desktop/ImportDialogs.cs`：匯入欄位用途步驟第二列固定高度 48px；文字標籤固定尺寸與垂直置中。
- `PersonalWiki.ExcelLab.csproj`、`PersonalWiki.Desktop/PersonalWiki.Desktop.csproj`：版本同步 4.6.1。
- `scripts/test-4.6.1-ui.py`、`scripts/run-4.6.1-python-tests.py`：新增靜態 UI 測試與回歸入口。

## 6. 測試狀態與已知限制
- 需執行 `python scripts/test-4.6.1-ui.py`、`python scripts/run-4.6.1-python-tests.py`。
- 本建置容器未提供 Windows／.NET SDK，WinForms build、DPI／視窗實機顯示與真正 Excel 匯入尚未驗證；不得宣稱通過。
- 使用者在 4.6.0 已回報其他功能未看到錯誤；該回報不是本版所有功能的完整測試證明。

## 7. 下一階段 4.7.0
- 先依 `TEST_GUIDE_4.6.1.md` 在 Windows + .NET 10 完成 UI 實機驗收。
- 若資料庫路徑在較窄視窗下不易看見，評估搜尋列水平捲動及使用者操作；未經確認不可擅自改版。
- 之後再規劃獨立功能批次；不得重做 3.6.0–4.6.1 已完成項目。

## 8. 可直接複製到新對話的完整開發指令
```text
請接續 PersonalWiki ExcelLab 最新版本 4.6.1，下一開發版本固定為 4.7.0。以本 ZIP 內 PersonalWiki_ExcelLab 原始碼為唯一基礎，不可回退或從舊版重建。先閱讀 README.md、docs/releases/INDEX.md、docs/releases/HANDOFF_4.6.1.md、UPDATE_NOTES_4.6.1.md、TEST_GUIDE_4.6.1.md、AUDIT_REPORT_4.6.1.md。

保留 3.6.0–4.6.1 全部功能，不重複開發。技術基準：CLI .NET 10 net10.0；Desktop WinForms net10.0-windows；NuGet DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14；SQLite PRAGMA user_version=1；設定路徑 %LOCALAPPDATA%\PersonalWiki\desktop-settings.json。

資料安全必須維持：原始 Excel 唯讀；匯入前預檢、備份驗證、雜湊重驗、唯讀快照、單一交易及 quick_check/foreign_key_check；備份／還原／匯出／報告拒絕覆寫；還原只寫新路徑；不自動刪除或合併 entries/notes/tags/source traces；移除圖片關聯不刪原圖；搜尋／查詢計畫／Benchmark ReadOnly；測試使用隔離 DB 副本。未經我明確批准，不得修改 Schema、DB/settings 路徑或套件版本。

4.6.1 已完成兩項 UI 修正：主畫面資料庫路徑改放搜尋列尾端；Excel 匯入精靈「第一筆資料列」文字列預留高度。需先在 Windows + .NET 10 實測，因開發容器無 dotnet／Windows，尚未執行 WinForms build 或實機驗收。測試素材包含 samples/PersonalWiki_3.6.0_Test.db 與 samples/PersonalWiki_Import_Sample.xlsx。

每批獨立功能完成後集中交付。每版同步更新 csproj、README、INDEX、UPDATE_NOTES、TEST_GUIDE、AUDIT_REPORT、HANDOFF、測試腳本及 SHA256 manifest。產生完整 ZIP，解壓根目錄固定 PersonalWiki_ExcelLab（資料夾不可帶版本號），ZIP 可帶版號；拒絕覆寫既有檔案。驗證 CRC、SHA256、版本和必需內容後提供 ZIP 下載。不可宣稱未實際執行的測試通過。
```
