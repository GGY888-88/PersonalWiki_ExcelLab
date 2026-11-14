# PersonalWiki ExcelLab 4.6.4 更新說明

## 本版目標
依使用者 2026-11-15 提供的 300% DPI 主畫面截圖，修正 4.6.3 後仍偏上的資料庫路徑、全文搜尋及標籤輸入文字。使用者明確要求不要改動其他已正常的欄位、按鈕及 DPI 排列。

## 1. 輸入欄位垂直置中
### 4.6.3 為何沒有完全解決
- 4.6.3 將自訂欄位直接改成 `Multiline=false`，並使用 `AutoSize=false` 拉高原生 TextBox。
- 這沒有真正控制單行 EDIT 文字在較高視窗中的繪製區域，所以 300% 畫面仍可看到文字靠上。
- Windows `EM_SETRECTNP` 只處理多行 EDIT；但 `EM_SETCUEBANNER` 不支援多行 EDIT。這兩項原生限制不能同時靠直接修改同一個 TextBox 解決。

### 4.6.4 修正方式
- `VerticallyCenteredTextBox` 改成外層 `UserControl` 容器，保留外層控制項既有高度、寬度及 `Fixed3D` 凹陷邊框。
- 內層使用真正的 Windows 原生單行 `TextBox`（`Multiline=false`、`AutoSize=true`、`BorderStyle=None`），維持系統標準字高及原生 `PlaceholderText`。
- 依內層 TextBox 的 `GetPreferredSize` 取得目前字型下的建議高度，將內層編輯框放在外層可用高度的垂直中央。視窗尺寸、字型及 DPI 改變時重新計算位置。
- 外層轉送 `TextChanged`、`KeyDown`、`MouseDoubleClick`；並代理 `Text`、`PlaceholderText`、`ReadOnly`、`Clear`、`SelectAll`、`Copy` 及 `Focus`，維持 MainForm 既有呼叫方式。
- 資料庫路徑與標籤欄位的 ToolTip 同時掛在外層與內層實際輸入控制項上，避免改成複合控制項後提示失效。

### 變更範圍
- 僅套用於原本已使用 `VerticallyCenteredTextBox` 的三個位置：資料庫路徑、全文搜尋、標籤篩選。
- 圖片篩選、條目名稱／類型、標籤重新命名、ComboBox、日期篩選、多行摘要及筆記均未替換。
- 沒有更動搜尋條件、查詢服務、SQLite、Excel 匯入或按鈕排列演算法。

## 2. 保留項目
- 主功能按鈕目前使用者設定（80×22）不重設、不改尺寸值。
- `PerMonitorV2` 與 300% DPI 主功能按鈕完整換行邏輯保留。
- BAT 只更新畫面中的版本號為 4.6.4；ASCII、CRLF、build log 導向及錯誤時記事本開啟方式保留。
- NuGet 版本、SQLite Schema、資料路徑、桌面設定路徑均不變。

## 3. 測試
- 專項靜態檢查：`python scripts/test-4.6.4-input-alignment.py`
- 完整回歸：`python scripts/run-4.6.4-python-tests.py`
- DPI 及主畫面回歸另外包含 `test-4.6.4-dpi-regression.py`、`test-4.6.4-main-ui-regression.py`。
- 本工作環境沒有 .NET SDK／Windows，未執行 `dotnet build`，也未能做原生 WinForms 的實際文字繪製驗收。靜態測試通過不等於 300% 實機已驗收。

## 4. 下一步
先在 Windows .NET 10 環境完成編譯及 100%／150%／200%／300% DPI 實機檢查。通過後才展開 4.7.0 的下一批獨立功能；不要在尚未確認這三欄文字位置前擴大改動主畫面其他輸入元件。
