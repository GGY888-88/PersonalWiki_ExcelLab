# PersonalWiki ExcelLab 1.5.1 更新說明

## 本次更新
- 桌面主畫面新增「資料庫健康檢查」：唯讀執行 SQLite `quick_check`、`foreign_key_check` 並顯示 Schema 版本及結果。
- 桌面主畫面新增「立即備份資料庫」：選擇新檔案路徑後，呼叫既有 SQLite Online Backup API。
- 備份目的檔不允許覆蓋；備份完成後既有服務會執行完整性及外鍵驗證。
- 新增固定名稱啟動檔 `啟動 PersonalWiki.bat`，內容使用 `dotnet run` 啟動桌面專案。
- CLI 與桌面專案版本標記更新為 1.5.1。

## 安全與相容性
- SQLite `user_version=1` 不變，無 Schema migration。
- `DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14` 不變，無新增套件。
- 健康檢查以唯讀模式開啟資料庫。
- 備份不覆蓋既有目的檔；原始 Excel、圖片原檔及既有資料庫不會被修改。
- 不新增自動還原操作；還原仍須使用既有安全流程並指定不存在的新路徑。

## 驗證狀態
- 已完成程式碼變更後的文字檢查、專案 XML 解析、版本標記檢查及 ZIP 完整性檢查。
- 開發環境沒有 .NET 10 SDK、PowerShell Core 或 Windows；未能執行 `dotnet restore/build`、WinForms 啟動及實機操作。
- 因此本版功能尚待 Windows/.NET 10 實機驗收，不宣稱編譯或執行測試通過。
