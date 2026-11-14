# 4.0.0 稽核報告（第一批）

## 審查範圍
- CLI `entry-query` 新增 JSON 檔案輸出選項。
- 匯出內容沿用既有 `EntryQueryResult`，包含條件、頁碼、分頁、總筆數及條目。
- 使用 `SafeFileOutput.WriteAllText`，採 `FileMode.CreateNew`，不覆寫既有檔案。
- 查詢仍使用 `SqliteOpenMode.ReadOnly`。

## 相依性與資料結構
- CLI `.NET 10 net10.0`；Desktop `.NET 10 net10.0-windows`。
- `DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`。
- SQLite `PRAGMA user_version=1`；沒有新增／修改 tables、columns、indexes 或 migrations。
- DB 路徑及 `%LOCALAPPDATA%\PersonalWiki\desktop-settings.json` 不變。

## 驗證結果
- 4.0.0 搜尋結果匯出測試：15/15。
- 3.9.0 發行安全基線：48/48。
- 3.8.0 圖片／相關條目：23/23；3.7.0 複合篩選：47/47。
- 3.6.0 回歸：排序 20/20、還原安全 15/15、WAL／locking 17/17、大資料查詢 91/91、發行靜態驗證 77/77。
- **整體 Python／SQLite／靜態檢查：353 項通過。**
- Python fixture／靜態測試不代表 C# 編譯或 Windows 實機通過。

## 未測項目
- .NET 10 restore/build、CLI 實際匯出與覆寫拒絕驗收。
- WinForms runtime、DPI、多螢幕及真實使用者資料端到端測試。
