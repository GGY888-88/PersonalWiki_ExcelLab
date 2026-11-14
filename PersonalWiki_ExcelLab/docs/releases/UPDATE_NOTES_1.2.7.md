# 1.2.7 更新紀錄

- 修正 300% DPI 下新增／編輯條目對話框底部「確定儲存／取消」按鈕文字被裁切：按鈕高度、寬度及間距依實際 DPI 縮放。
- 修正編輯條目名稱欄初始水平捲動位置：開啟時將游標及可視位置移至文字開頭。
- 保留 1.2.6 的自動版面配置、DPI 診斷及既有功能。
- 診斷檔改為 `%LOCALAPPDATA%\PersonalWiki\diagnostics\ui-1.2.7.txt`。
- 內附 `啟動 PersonalWiki.bat`。
- 未新增 NuGet 相依套件；SQLite Schema 維持 v1。
- 尚未在本建置環境執行 Windows Forms Build／實機 DPI 驗收。
