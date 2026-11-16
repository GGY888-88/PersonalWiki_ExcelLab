# PersonalWiki ExcelLab 4.6.5 開發交接

## 版本摘要
- 基礎版本：4.6.4，直接延續原專案。
- 本版目的：修正 Windows .NET 10 建置回報的 WFO1000 三個錯誤。
- 版本：4.6.5。
- 下一整合版本規劃：4.7.0，仍以 4.6.5 實機驗收為先。

## 變更範圍
只在 `PersonalWiki.Desktop/VerticallyCenteredTextBox.cs` 加入 `using System.ComponentModel;`，並為 `Text`、`PlaceholderText`、`ReadOnly` 加上 `DesignerSerializationVisibility.Hidden`。因這些屬性由主程式碼執行期設定，不由 Visual Designer 儲存。沒有用 NoWarn 或降低分析器嚴重性。

兩個 csproj 版本號、三個 BAT 版本字樣更新為 4.6.5。其餘功能與 4.6.4 相同。

## WFO1000 根據
Microsoft Learn 說明：衍生自 Control 的公開屬性必須明確宣告序列化方式；若不應由 Designer 序列化，應使用 `DesignerSerializationVisibility.Hidden`。
參考：https://learn.microsoft.com/zh-tw/dotnet/desktop/winforms/compiler-messages/wfo1000

## 專案／Schema／套件
- CLI：net10.0；Desktop：net10.0-windows。
- DPI：PerMonitorV2。
- SQLite Schema：`PRAGMA user_version=1`，本版無 migration。
- NuGet 套件不變：DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14。
- 使用者設定路徑及資料庫路徑格式不變。

## 建置與測試
- 必須在 Windows .NET 10 SDK 執行：`dotnet build PersonalWiki.Desktop/PersonalWiki.Desktop.csproj -c Release`。
- 本工作環境沒有 dotnet SDK，未宣稱已完成實際 C# 編譯。
- 需回報建置錯誤／警告數及 300% DPI 截圖。
- 本 ZIP 包含 4.6.4 原有測試腳本；建議執行 `python scripts/run-4.6.4-python-tests.py`。

## 三個 BAT
- `01_建置程式.bat`
- `02_啟動程式.bat`
- `03_建置並啟動.bat`

## 下一步
1. 使用者先在 Windows 建置 4.6.5。
2. 確認 WFO1000 消失且建置錯誤 0。
3. 再測三個欄位文字／提示垂直位置及 100–300% DPI。
4. 未經同意不修改其他已正常介面。
