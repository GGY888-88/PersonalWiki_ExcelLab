# PersonalWiki ExcelLab 1.1.0（開發版）

## 本版變更
- 桌面搜尋結果加入頁碼導覽，每頁 100 筆，支援超過 200 筆的結果集。
- 搜尋/篩選重新執行時回到第 1 頁；顯示目前頁與總頁數。
- 修正圖片移除時使用者選取目標判斷錯誤：現在只移除明確點選的圖片關聯。
- 圖片重新載入會清除舊選取狀態。
- CLI 與桌面專案版本統一為 1.1.0。

## 安全與相容性
- SQLite Schema Version 1 不變。
- DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14；無新增套件。
- Excel 來源唯讀；匯入仍需使用者明確操作及既有安全工作流程。
- 移除圖片關聯不會刪除硬碟原圖。

## 測試狀態
- 靜態檢查及 ZIP 完整性檢查；此執行環境沒有 dotnet CLI，未能 Build 或執行 Windows Forms。
- 尚待 Windows/.NET 10 SDK 環境驗收：多頁搜尋、篩選換頁、圖片選取及移除、原圖保留、Excel 匯入回歸。

## 更新方式
- 解壓 ZIP 後使用固定資料夾名稱 `PersonalWiki_ExcelLab`。
- 不要以專案更新覆蓋個人 SQLite 資料庫、原始 Excel 或圖片資料夾。
