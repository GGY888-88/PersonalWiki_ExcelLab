# PersonalWiki ExcelLab 4.6.6 更新紀錄

## 本次唯一修改範圍
依使用者最新畫面回報，三個輸入框文字已能置中，但外框高度過大。本版只縮小以下三欄：

| 欄位 | 4.6.5 設定 | 4.6.6 設定 |
|---|---:|---:|
| 資料庫路徑 | Height 36 基準／MinimumSize 28 | 26 logical px |
| 全文搜尋 | Height 36、EnsureTextFits 基準 40 | 26 logical px |
| 標籤 | Height 34、EnsureTextFits 基準 38 | 26 logical px |

三欄 `EnsureTextFits` 的垂直留白由 14 調為 4 logical px，避免再次由高度保護值把外框撐大。保留外層框、內層原生單行 TextBox、原生 PlaceholderText 及內層垂直置中算法。

## 明確不修改
- 不修改其他輸入框、ComboBox、日期欄位、按鈕、列表、分頁及右側內容區。
- 不修改字體、欄寬、標籤／全文搜尋邏輯、鍵盤行為及資料庫路徑複製行為。
- 不修改主功能按鈕 80×22 設定、300% DPI 排列、SQLite Schema、Models、Services、Excel 匯入流程及 NuGet 套件。

## 驗收狀態
依使用者指示，本輪不執行測試。Windows .NET 10 建置、WFO1000、DPI 實機視覺與功能回歸均待後續安排；不得將本版描述為已測試或已編譯。
