# PersonalWiki ExcelLab 1.3.3

## 新增：標籤管理
- 主畫面新增「標籤管理」入口。
- 顯示標籤清單及各標籤使用條目數。
- 可重新命名標籤；若目標標籤已存在，使用交易及 INSERT OR IGNORE 合併重複關聯。
- 可刪除標籤關聯；不刪除條目、筆記、圖片關聯以外的檔案或原始圖片。
- 可匯出 UTF-8 BOM CSV 標籤清單。
- 操作後重新載入主畫面搜尋結果。

## 安全與相容性
- SQLite Schema 維持 v1，未新增 NuGet 套件。
- 沿用既有 entry_tags.normalized_tag 正規化規則。
- 重新命名使用資料庫交易；刪除前要求明確確認。
- 原始 Excel 唯讀；圖片檔不因移除標籤而刪除。
- 保留啟動 BAT 及 Excel 測試樣本。

## 驗證限制
- 已檢查程式碼結構及 ZIP 封裝。此環境沒有 .NET SDK，未執行 Windows Build 或 WinForms 實機測試。
