# PersonalWiki ExcelLab 2.5.3 稽核報告

## 修改範圍
- `PersonalWiki.Desktop/MainForm.cs`：主畫面工具列按鈕採內容量測，依最大 `PreferredSize`、按鈕 Margin 及 FlowLayoutPanel Padding 計算 action row 高度；主畫面頂部容器高度同步調整。
- `PersonalWiki.ExcelLab.csproj`、`PersonalWiki.Desktop/PersonalWiki.Desktop.csproj`：版本更新為 2.5.3。
- `Program.cs`、`scripts/run-acceptance-tests.ps1`：CLI 顯示及 acceptance 測試包版本更新。
- `README.md`：更新版號及說明。
- `docs/releases/`：集中管理所有版本交接及發行文件。

## 範圍隔離
- 輸入視窗按鈕尺寸設定仍由 `DesktopSettingsService.LoadEntryButtonSize()` 提供，只傳入 `EntryEditDialog`，控制「確定儲存／取消」按鈕。
- 主畫面「新增條目／編輯條目」按鈕未讀取或使用該設定。
- SQLite Schema `user_version=1` 不變；NuGet 套件不變。
- 未修改匯入、資料庫、備份、還原及資料安全流程。

## 靜態檢查
- 已檢查主畫面工具列列高由按鈕 PreferredSize 動態決定，不再使用固定 42 px。
- 已檢查輸入視窗按鈕大小設定仍只傳入 EntryEditDialog。
- 已將 151 份歷史版本文件移至 `docs/releases/`，並建立索引。
- ZIP CRC、檔案清單及專案 XML 將於封裝後驗證。

## 尚未執行
- 本環境沒有 .NET SDK、Windows、PowerShell，未執行編譯、CLI acceptance 或 WinForms DPI 實機測試。
- 100% 至 300% DPI、不同字型縮放及多螢幕移動仍待 Windows 驗收。
