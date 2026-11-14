# PersonalWiki ExcelLab 4.6.6 修改範圍稽核

## 修改檔案
- `PersonalWiki.Desktop/MainForm.cs`：僅資料庫路徑、全文搜尋、標籤三個輸入框的初始高度、MinimumSize 與 `EnsureTextFits` 基準／垂直留白。
- `PersonalWiki.Desktop/PersonalWiki.Desktop.csproj`、`PersonalWiki.ExcelLab.csproj`：版本號更新為 4.6.6。
- 三個 BAT：版本狀態文字更新。
- README、版本索引與本版交接文件。

## 未觸及區域
未修改 `VerticallyCenteredTextBox.cs` 的置中算法、其他輸入欄位、資料層、SQLite Schema、Excel 匯入、NuGet 套件及主功能按鈕設定。

## 驗證狀態
使用者要求本輪不測試，因此沒有執行測試或編譯；封裝工具僅在打包時執行 ZIP CRC 與結構檢查。
