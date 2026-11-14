# Update Notes 1.3.0

- 修正新增／編輯條目視窗按鈕尺寸設定無效的根因：按鈕實際尺寸依 DPI 放大，但底部 TableLayoutPanel 列高仍固定，導致按鈕被裁切。
- 按鈕列高度現在依使用者設定的按鈕高度、Margin、Padding 及目前 DPI 動態計算。
- 保留 1.2.9 的按鈕寬高自訂設定；設定仍以 96 DPI 為基準，實際顯示依 DPI 換算。
- 診斷檔更新為 `%LOCALAPPDATA%\PersonalWiki\diagnostics\ui-1.3.0.txt`。
- 保留 `啟動 PersonalWiki.bat`。
- SQLite Schema 維持 v1，未新增 NuGet 套件，未修改既有資料內容。
- 尚未在 Windows 300% DPI 環境完成實機 Build／介面驗收。
