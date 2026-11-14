# PersonalWiki ExcelLab 4.6.3 更新說明

## 本版目標
修正 4.6.2 後仍可見的 WinForms 單行輸入框文字偏上問題，並讓 Windows CMD 不直接顯示可能含非 ASCII 字元的建置記錄。

## 1. 輸入框垂直置中
- `VerticallyCenteredTextBox` 改回 Windows 原生單行 EDIT 模式（`Multiline = false`），讓系統處理單行文字的垂直排版。
- 設定 `AutoSize = false`，由既有 DPI-aware 版面配置維持輸入框尺寸；不靠增加高度或縮小字體掩蓋文字偏上問題。
- 保留 `PlaceholderText` 原生提示橫幅支援；先前多行 EDIT 與 Cue Banner 不相容，是搜尋提示文字未能正確顯示／對齊的重要原因。
- 不再使用 `EM_SETRECT`／GDI 自行計算垂直位移，避免多行 EDIT 的副作用及自訂矩形累積偏移。
- Enter／Tab 不會被當成新增文字列，全文搜尋原有 Enter 搜尋行為不變。
- 影響範圍：主畫面資料庫路徑、全文搜尋、標籤篩選欄。其餘原生單行 TextBox 維持原樣；多行摘要與筆記內容維持原樣。

## 2. BAT 建置輸出
- 三個 BAT 仍為 ASCII + CRLF，沒有中文 Console 字串，也不切換 CMD code page。
- `dotnet build` 的 stdout／stderr 完整保留在 `build_release.log`，不再使用 `type` 將日誌直接輸出到 CMD。
- 建置失敗時由記事本開啟 `build_release.log`；CMD 只顯示 ASCII 狀態訊息，減少 OEM／UTF-8 解碼差異造成的畫面亂碼。
- `03_建置並啟動.bat` 仍是一鍵建置後啟動，建置與啟動之間不要求使用者再按鍵。

## 3. 保留項目與未變更範圍
- 保留 4.6.2 主畫面四區列高配置、主功能按鈕完整換行、結果清單自適應欄寬、日期篩選與 Excel 預覽版面。
- 保留主功能按鈕目前使用者設定（包含 80×22 的設定值）；不重設 `%LOCALAPPDATA%\PersonalWiki\desktop-settings.json`，不調整按鈕尺寸範圍及 DPI 排列程式。
- `Services/`、`Models.cs`、CLI `Program.cs`、SQLite Schema、資料路徑、桌面設定路徑、Excel mapping／預檢／備份／交易／匯入流程均未修改。
- NuGet 套件版本不變。

## 4. 版本
- CLI 與 Desktop 專案版本同步為 4.6.3。
- 下一個規劃版本仍為 4.7.0；先完成本版 Windows／300% DPI 實機驗收，再進行下一批功能。
