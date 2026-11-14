# PersonalWiki ExcelLab 1.5.0 更新說明

## 本次整合內容
- 整合 1.4.0 CLI 自動化回歸測試矩陣及 1.4.5 SQLite 備份／還原安全強化。
- 備份／還原使用同目錄 staging，通過完整性及外鍵檢查後才提交最終檔案。
- 新增備份還原回歸案例：建立備份、健康檢查、還原逐表驗證、資料內容比對、拒絕覆蓋、路徑相同拒絕、損壞來源拒絕及暫存檔清理。
- 新增全文索引檢查及交易式重建回歸案例。
- 新增桌面 UI 人工驗收矩陣文件，涵蓋 DPI、縮放、多螢幕、工作表欄位顯示及批次封存等；目前未執行 Windows UI 測試。
- 同步 CLI、桌面專案、README、驗收報告與交接文件版本為 1.5.0。

## 相容性與資料安全
- SQLite `user_version=1` 不變，無 Schema migration。
- `DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14` 不變，無新增 NuGet 套件。
- 原始 Excel 唯讀；不刪除或移動圖片原檔；不覆蓋既有資料庫、備份、報告或匯出檔。

## 驗證狀態
- 已完成原始碼、專案 XML、腳本文字結構、版本標記及 ZIP 結構靜態檢查。
- 本開發環境沒有 .NET 10 SDK、PowerShell Core 或 Windows，未能執行 `dotnet restore/build`、CLI 整合驗收或 WinForms 實機測試。
- 因此自動化回歸案例已加入交付包，但尚未聲稱執行通過。
- 復原驗證清理新增「本次呼叫已成功提交目的檔」旗標；避免遇到其他程序先建立同名目的檔時，錯誤清除外部檔案。
