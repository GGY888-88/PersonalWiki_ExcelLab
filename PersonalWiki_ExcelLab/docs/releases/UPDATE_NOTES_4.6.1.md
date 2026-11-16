# PersonalWiki ExcelLab 4.6.1 更新說明

## UI 修正
1. 主畫面「目前資料庫位置」由獨立列移至搜尋列尾端，減少主畫面垂直空間；路徑文字框仍可選取、複製，長路徑可在文字框內左右移動檢視。
2. Excel 匯入精靈第 2 步「第一筆資料列」控制列預留固定高度，標籤改為固定尺寸及垂直置中，避免文字被裁切。

## 範圍與安全
- 僅調整 `PersonalWiki.Desktop/MainForm.cs`、`PersonalWiki.Desktop/ImportDialogs.cs` 的 WinForms 版面配置。
- 未變更匯入資料處理、搜尋、SQLite Schema、資料庫路徑、桌面設定路徑或 NuGet 相依套件。
- 保留 4.6.0 全部功能及資料安全限制。

## 驗證狀態
- `scripts/test-4.6.1-ui.py`：靜態 UI 契約測試。
- `scripts/test-4.6.0-query-acceptance.py`：沿用既有 acceptance 自我測試。
- 本建置環境未提供 Windows／.NET SDK，未宣稱 WinForms 編譯或實機顯示已通過；需依測試指南驗收。
