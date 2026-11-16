# PersonalWiki ExcelLab 4.6.5 更新紀錄

## 修正原因
4.6.4 在 Windows .NET 10 WinForms 建置時，`VerticallyCenteredTextBox.cs` 的 `Text`、`PlaceholderText`、`ReadOnly` 三個公開代理屬性觸發 WFO1000。

## 修正內容
- 在上述三個屬性加上 `DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)`。
- 這三個屬性由 `MainForm` 在執行期間設定，不應由 WinForms Designer 序列化；不關閉或抑制 WFO1000。
- 保留 4.6.4 的外層固定框、內層原生單行 TextBox、AutoSize、PlaceholderText、DPI 及版面算法。
- 專案版本與三個 BAT 顯示更新為 4.6.5。

## 未修改
搜尋流程、SQLite、Excel 匯入、資料 Schema、DPI 排版、按鈕尺寸、其他輸入欄位及套件版本均未修改。

## 驗收狀態
- WFO1000 靜態修正檢查：待執行／本封裝驗證。
- Python 回歸測試：待執行／本封裝驗證。
- Windows .NET 10 Release 建置：需使用者 Windows 環境確認。
- 100%／150%／200%／300% DPI 視覺驗收：待實機確認。
