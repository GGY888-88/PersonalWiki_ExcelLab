# PersonalWiki ExcelLab 2.0.2 稽核報告

## 版本
- 最新版本 2.0.2；下一版 2.0.3；基準 ZIP 2.0.1。
- CLI／桌面 csproj 版本同步 2.0.2；SQLite `user_version=1`。

## 輸出路徑盤查與修正
- 新增 `SafeFileOutput.WriteAllText`，建立新檔時使用 `FileMode.CreateNew`，不覆蓋既有目的檔。
- CLI 匯入預覽、品質報告、Dry-run、清理檢查報告、自我測試報告、mapping 範本、clean-rules 範本已改用共用安全輸出。
- 桌面重複條目及標籤 CSV 匯出、UI DPI 診斷檔已改用安全輸出。
- mapping／cleaning rules 編輯器與桌面設定檔保留明確更新既有檔案的語意，但改用同目錄唯一暫存檔完整寫入後再替換，避免直接截斷舊檔。
- SQLite FTS 索引以暫存資料庫建置後替換索引副本，屬內部可再生索引；來源正式資料庫不在該替換操作中被覆寫。

## 匯入穩定性
- 預檢前後比對來源 Excel、mapping、rules SHA-256；預檢期間任一輸入變更即停止並作廢結果。
- 正式匯入原有「預檢 → DB 健康檢查 → 新路徑備份並驗證 → 再預檢／比對雜湊 → SQLite 交易匯入 → 批次驗證」順序保留。

## 靜態檢查與未執行
- 已檢查程式輸出 API、版號、專案 XML、測試腳本及樣本活頁簿壓縮結構。
- 未執行 dotnet restore/build、CLI self-test、PowerShell acceptance、WinForms 實機測試；執行環境缺少 .NET SDK、Windows、PowerShell。
- 新增 PowerShell 覆寫負向測試尚待 Windows 執行。

## Schema、套件與安全限制
- SQLite Schema v1，無 migration。
- `DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`，無新增套件。
- 來源 Excel 唯讀；備份／還原／匯出／報告不得覆蓋既有目的檔；不自動刪除／合併資料；移除圖片關聯不刪除原圖。
