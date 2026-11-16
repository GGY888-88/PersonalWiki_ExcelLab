# PersonalWiki ExcelLab 2.5.1 更新說明

## 修正
- 修正 `Program.cs` 第 769、770 行 CS1061 編譯錯誤：`RunSnapshotSafetySelfTest` 與 `RunSafeFileOutputSelfTest` 原本宣告回傳 `object`，呼叫端無法存取匿名物件的 `Passed` 屬性。
- 新增 `SnapshotSafetySelfTestResult` 與 `SafeFileOutputSafetySelfTestResult` 私有 record，回傳型別明確且保留原有結果欄位。
- 原有自我測試步驟、判斷條件及 JSON 報告欄位不變。
- CLI／桌面版本及 acceptance 測試包版號更新為 2.5.1。

## 未變更
- SQLite `user_version=1`，無 Schema migration。
- NuGet 套件及版本不變。
- 不變更資料庫、設定檔位置及既有資料安全規則。

## 測試狀態
- 已完成靜態型別檢查：兩個方法的宣告回傳型別與 record 建構結果相符，`Passed` 屬性為 `bool`。
- 本打包環境沒有 .NET SDK，未能實際執行 `dotnet build`、self-test 或 PowerShell acceptance；請在 Windows/.NET 10 環境執行 `dotnet build -c Release`。
