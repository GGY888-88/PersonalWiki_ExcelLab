# 3.9.0 稽核報告

## 審查範圍
- CLI／Desktop 版本字串同步為 3.9.0。
- CLI 說明版號同步修正。
- 發行驗證器與安全測試涵蓋相依套件、Schema v1、合成資料庫、樣本 Excel／JSON、備份／還原 sidecar 安全及圖片原檔保留契約。
- 打包工具產生 SHA256 manifest、固定解壓根目錄、拒絕覆寫既有 ZIP 並驗證 ZIP CRC。
- 3.6.0–3.8.0 功能回歸測試納入統一 runner。

## Schema／套件／路徑
- SQLite `PRAGMA user_version=1`，沒有新增或修改資料表、欄位、索引或 migration。
- `DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`，沒有新增套件。
- DB 路徑及 `%LOCALAPPDATA%\\PersonalWiki\\desktop-settings.json` 不變。

## 驗證結果
- 3.9.0 release safety：48/48。
- 3.8.0 image／relationship：23/23；3.7.0 composite filters：47/47。
- 3.6.0 regression：sorting 20/20、recovery safety 15/15、WAL／locking 17/17、large dataset 91/91、release verifier 71/71。
- 整體 runner：332 項檢查通過。
- 3.8.0 image／relationship：23 項。
- 3.7.0 composite filters：47 項。
- 3.6.0 regression：排序 20 項、還原安全 15 項、WAL／locking 17 項、大資料查詢 91 項、release verifier 67 項。
- ZIP 已建立，CRC 通過；328 個 manifest 檔案 SHA256 逐一比對通過，解壓根目錄為 `PersonalWiki_ExcelLab`。
- Python fixture／靜態測試不是 C# 編譯或 Windows 實機驗收。

## 未測項目
- .NET 10 restore/build、CLI acceptance、自我測試。
- Windows WinForms runtime、DPI、多螢幕及實際圖片檔案競態。
- 使用者正式資料庫與真實 Excel 的端到端匯入／還原驗收。
