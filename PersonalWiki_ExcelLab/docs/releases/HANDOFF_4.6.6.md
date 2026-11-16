# PersonalWiki ExcelLab 4.6.6 完整開發交接

## 版本摘要
- 版本：4.6.6。
- 基準：使用者提供的 4.6.5 ZIP。
- 本輪目的：三個欄位文字已置中但外框過大，只縮小外框。
- 本輪未測試：依使用者明確要求，沒有執行測試或 Windows 編譯。

## 本版修改
- 資料庫路徑、全文搜尋、標籤三欄統一 26 logical px 高度基準。
- 三欄 `EnsureTextFits` 的垂直留白改為 4 logical px，避免舊 14 px 留白放大框體。
- 置中控制項的內層原生單行 TextBox、PlaceholderText 與置中程式不變。

## 保護清單
其他欄位、ComboBox、日期控制項、按鈕、表格、列表、右側內容區、搜尋邏輯、SQLite Schema、Excel 匯入流程、NuGet 套件均未修改。主功能按鈕 80×22 及 300% DPI 排列保留。

## 專案結構與技術
- `PersonalWiki.ExcelLab.csproj`：.NET 10 CLI／核心專案。
- `PersonalWiki.Desktop/`：WinForms 桌面專案，PerMonitorV2。
- SQLite `PRAGMA user_version=1`；本版沒有 Schema migration。
- NuGet：DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14。
- 三個 BAT：`01_建置程式.bat`、`02_啟動程式.bat`、`03_建置並啟動.bat`。

## 測試與問題狀態
- 本輪測試：未執行（依使用者指示）。
- Windows .NET 10 編譯：待後續執行。
- WFO1000：4.6.5 修正保留，但 4.6.6 尚未重新建置確認。
- 300% DPI 三框高度與文字視覺：待使用者後續畫面確認。
- 其他回歸：未執行。

## 下一版本計畫：4.7.0
1. 先依使用者回報確認 4.6.6 三框高度；若仍需微調，只限三框。
2. 待使用者提出或確認獨立功能需求後，集中開發一批功能。
3. 再安排整合測試、Windows 建置、測試素材、完整交接及封裝。
4. 不擅自選定新功能，不修改已完成且正常的部分。

## 續開發指令
以 `PersonalWiki_ExcelLab_4.6.6.zip` 為唯一基準。先閱讀本文件、`UPDATE_NOTES_4.6.6.md`、`AUDIT_REPORT_4.6.6.md`、`TEST_GUIDE_4.6.6.md`、`PLAN_4.7.0.md` 及 `RELEASE_MANIFEST_4.6.6.sha256`。本版唯一 UI 調整是資料庫路徑、全文搜尋、標籤三欄高度 26 logical px 與垂直留白 4 logical px；不更動其他欄位、字體、欄寬、按鈕 80×22、300% DPI 排列、SQLite Schema、Excel 匯入及 NuGet。依使用者要求本輪不測試；後續需使用者提出測試安排後才執行。任何新功能先確認需求，再集中開發。每版保留三個 BAT，解壓根目錄固定 `PersonalWiki_ExcelLab`。
