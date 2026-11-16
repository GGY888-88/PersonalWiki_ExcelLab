# PersonalWiki ExcelLab 1.0.0

## 新增
- 新增 `PersonalWiki.Desktop` Windows Forms 桌面專案，提供資料庫選擇、條目全文搜尋、基本資料/欄位/筆記/圖片瀏覽。
- 桌面端僅使用既有 EntryQueryService、EntryDetailService、EntryContentService 等服務，不改 SQLite Schema。
- 選擇資料庫時僅檢查資料庫資訊；不自動執行 Excel 匯入，不修改原始 Excel。
- 圖片以資料庫登錄路徑載入；無效/遺失圖片略過，不刪除磁碟原檔。

## 建置
Windows + .NET 10 SDK：
```powershell
dotnet build .\PersonalWiki.Desktop\PersonalWiki.Desktop.csproj -c Release
dotnet run --project .\PersonalWiki.Desktop\PersonalWiki.Desktop.csproj -c Release
```

## 限制 / 測試狀態
- 目前是唯讀瀏覽 MVP，搜尋結果最多顯示 100 筆。
- 尚未提供筆記編輯、圖片新增、Excel 匯入精靈、資料庫建立嚮導及正式安裝包。
- 本開發容器未執行 Windows Forms build/run；需在 Windows .NET 10 環境建置與驗收。
- CLI 舊功能與既有 SQLite Schema Version 1 保留；正式驗收基準仍為 0.7.5 fix4，1.0.0 尚未驗收。
