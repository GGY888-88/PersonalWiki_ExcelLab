# 4.6.2 建置修正：WFO1000

## 使用者回報
在 Windows / .NET 10 執行 `dotnet build .\PersonalWiki.Desktop\PersonalWiki.Desktop.csproj -c Release` 時，`DateFilterControl.cs` 的 `Checked` 和 `Value` 兩個屬性產生 WFO1000。

## 根因
`DateFilterControl` 繼承 WinForms `Panel`。從 .NET 9 起，WinForms 分析器要求 `Control` 衍生類別的公開屬性明確聲明設計器序列化行為。這兩個屬性只包裝內部核取方塊及日期選擇器的執行期查詢狀態，不應被表單設計器寫入初始化程式碼。

## 修正
- 加入 `using System.ComponentModel;`。
- `Checked` 與 `Value` 都加上 `[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]`。
- 不降低 WFO1000 嚴重性、不加入 `NoWarn`、不關閉 WinForms 分析器。
- 日期勾選狀態、日期值及 `yyyy-MM-dd` 查詢轉換邏輯不變。

## 驗證限制
本工作容器沒有 .NET SDK／Windows，已加入靜態回歸檢查但未執行真正 `dotnet build`。請在 Windows 重新建置；若仍有錯誤，請提供完整輸出。
