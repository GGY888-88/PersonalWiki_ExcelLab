# Update Notes 1.2.5

## 修正
- 修正新增／編輯條目視窗在 Windows 高 DPI／顯示縮放環境下版面縮小、標籤逐字換行及文字裁切問題。
- 將對話框改為 `AutoScaleMode.Dpi`，設定 96 DPI 基準，並擴大視窗尺寸與最小尺寸。
- 加寬標籤欄，增加輸入框列高、內距及字體尺寸。
- 新增 `啟動 PersonalWiki.bat`，解壓縮後可雙擊啟動。

## 保留
- SQLite Schema Version 1 不變。
- 不新增 NuGet 相依套件。
- 不搬移、刪除或覆蓋使用者資料庫及原始 Excel。
- ZIP 解壓後頂層資料夾仍為 `PersonalWiki_ExcelLab`。

## 驗證限制
- 已檢查原始碼、版本資訊及 ZIP 完整性。
- 此建置環境沒有 .NET SDK／Windows Forms，未執行 Windows Build 或實際 DPI 介面驗收。
