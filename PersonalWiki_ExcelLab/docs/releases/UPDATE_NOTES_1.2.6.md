# 1.2.6 更新紀錄

- 重新配置新增／編輯條目視窗，採用表格式自動佈局，標籤欄固定寬度、輸入欄填滿可用空間。
- 停用此對話框的二次自動縮放，於視窗顯示後依實際 DeviceDpi 與螢幕工作區設定尺寸。
- 新增 UI 診斷檔：`%LOCALAPPDATA%\PersonalWiki\diagnostics\ui-1.2.6.txt`，記錄 DPI、視窗、輸入框與標籤欄實際尺寸。
- ZIP 內附 `啟動 PersonalWiki.bat`，顯示版本及啟動路徑，並保留錯誤訊息視窗。
- 未新增 NuGet 相依套件；SQLite Schema 維持 v1；不搬移、不覆蓋既有資料庫。
- 尚未在 Windows/.NET 10 實機執行 Build 與 DPI 介面驗收。
