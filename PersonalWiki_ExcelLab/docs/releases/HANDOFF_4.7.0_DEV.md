# PersonalWiki ExcelLab 4.7.0 開發工作區交接

## 基準
- 唯一程式基準：PersonalWiki ExcelLab 4.6.6。
- 目標：持續開發至 5.0.0；方向採全面強化，兼顧知識管理與 Excel 資料管理。
- 本工作區已新增第一項獨立功能：標籤使用統計 JSON 報告；另新增規劃與來源盤點文件。
- 使用者要求先繼續開發、暫不安排測試；因此本階段沒有執行測試或 Windows 編譯。

## 新增文件
- `docs/roadmap/ROADMAP_5.0.0.md`：4.7／4.8／4.9／5.0 路線。
- `docs/architecture/FEATURE_INVENTORY_4.6.6.md`：現有功能、控制項及候選項目盤點。

## 保護範圍
主功能按鈕 80×22、300% DPI 排列、4.6.6 三個輸入框、搜尋與篩選、SQLite user_version=1、資料庫路徑、Excel 匯入安全流程、備份還原、NuGet 版本及其他正常 UI 均維持不變。

## 技術基準
- CLI：.NET 10 (`net10.0`)
- Desktop：.NET 10 WinForms (`net10.0-windows`, PerMonitorV2)
- NuGet：DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14
- SQLite：`PRAGMA user_version=1`
- Desktop settings：`%LOCALAPPDATA%\PersonalWiki\desktop-settings.json`

## 未完成
- 尚未選定 4.7.0 第一批要實作的功能；路線圖中的功能是候選，不是已批准的規格。
- 已修改 `PersonalWiki.Desktop/TagManagementDialog.cs`，新增標籤使用統計 JSON 報告入口；已更新兩個 csproj 與三個 BAT 的版本標示。
- 尚未測試、尚未編譯、尚未 Windows 實機驗收。
- 4.6.6 三個輸入框的實際高度仍以使用者畫面驗收為準。

## 下一步
下一批優先盤點既有 Excel 匯入批次差異／驗證服務，規劃只讀的匯入批次檢視；不得更動現有預檢、備份及交易安全。集中完成一批功能後再依使用者指示安排測試。每個新增功能需附操作說明與測試素材。正式發行時保留三個 BAT、更新歷史、Schema／套件／結構、問題狀態、完整續開發指令及 SHA-256。
