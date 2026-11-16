# 4.1.0 稽核報告（第一批）

## 審查範圍
- MainForm 搜尋結果新增「複製選取摘要」按鈕。
- 支援目前頁面 ListView 多選；複製名稱、類型、摘要、更新時間、封存狀態及 ID。
- 無選取時停用；選取後啟用。
- handler 只讀取 ListView 欄位並呼叫 Clipboard.SetText；不呼叫資料庫寫入服務。

## 相依性與資料結構
- CLI `.NET 10 net10.0`；Desktop `.NET 10 net10.0-windows`。
- `DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`。
- SQLite `PRAGMA user_version=1`；未新增／修改 tables、columns、indexes 或 migrations。
- DB 路徑及 `%LOCALAPPDATA%\PersonalWiki\desktop-settings.json` 不變。

## 驗證結果
- 4.1.0 桌面結果管理靜態契約：20/20。
- 4.0.0 匯出：15/15；3.9.0 發行安全：48/48；3.8.0 圖片／相關條目：23/23；3.7.0 複合篩選：47/47。
- 3.6.0 回歸：排序 20/20、還原安全 15/15、WAL／locking 17/17、大資料查詢 91/91、發行靜態驗證 83/83。
- **整體 Python／SQLite／靜態檢查：379 項通過。**
- Python 靜態測試不代表 C# 編譯或 Windows 實機通過。

## 未測項目
- .NET 10 restore/build、CLI acceptance。
- WinForms runtime、剪貼簿、多選、DPI、多螢幕及真實使用者資料端到端測試。
