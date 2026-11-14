# PersonalWiki ExcelLab 1.3.2

## 新增
- 主畫面新增「重複條目檢查」。
- 唯讀檢查名稱重複（忽略前後空白、大小寫及 Unicode 相容字元）。
- 顯示每組條目、類型、封存狀態、更新時間及 ID。
- 可匯出 UTF-8 BOM CSV 報告。
- 不會自動合併、刪除、封存或修改任何條目。

## 保留與安全
- SQLite Schema 維持 v1，沒有新增 NuGet 套件。
- Excel 原始檔唯讀；圖片移除只刪除關聯，不刪原圖。
- 保留啟動 BAT 與 Excel 測試樣本。

## 驗證限制
- 已進行原始碼及封裝檢查。此環境未安裝 .NET SDK，未執行 Windows Build 或 WinForms 實機測試。
