# PersonalWiki ExcelLab 2.5.7 測試指南

## 先決條件
Windows、.NET 10 SDK、PowerShell。所有測試均使用獨立測試檔，不可指向正式資料庫。

## 建置與完整 acceptance
在解壓後的 `PersonalWiki_ExcelLab` 專案根目錄執行：

```powershell
dotnet restore .\PersonalWiki.ExcelLab.csproj
dotnet build .\PersonalWiki.ExcelLab.csproj -c Release
dotnet build .\PersonalWiki.Desktop\PersonalWiki.Desktop.csproj -c Release
.\scripts\run-acceptance-tests.ps1
```

腳本會建立新的隔離測試輸出目錄；不可指定已存在資料夾。

## 新增警告差異測試
1. 使用 `samples/diagnostic-warning-baseline-sample.json` 與 `samples/diagnostic-warning-current-sample.json` 作為 baseline/current。兩份樣本的健康旗標及其他診斷統計相同，只有警告文字不同。
2. 執行：

```powershell
$report = Join-Path $env:TEMP 'pw-diagnostic-warning-diff.json'
if (Test-Path $report) { throw '請改用新的輸出檔名，測試不得覆蓋既有檔案' }
dotnet run --project .\PersonalWiki.ExcelLab.csproj -c Release -- db-diagnostic-compare .\samples\diagnostic-warning-baseline-sample.json .\samples\diagnostic-warning-current-sample.json $report
```

3. 確認輸出 `HasChanges=true`、`AddedWarnings` 含新警告一筆、`RemovedWarnings` 含舊警告一筆，且 `WarningsBefore`／`WarningsAfter` 保留兩側清單。
4. 再以 baseline/current 原始樣本比較，確認警告差異陣列皆為空，`HasChanges` 仍依其他統計差異判定。
5. 對照 acceptance 報告，確認輸入樣本 SHA-256 前後相同、既有輸出拒絕覆寫。

## 回歸
- 2.5.6 所有診斷快照比較案例。
- 2.5.5 主畫面按鈕 100×68、自然寬度、資料庫路徑列與 DPI 測試，依 `DESKTOP_UI_TEST_MATRIX_2.5.5.md`。
- 既有 SafeFileOutput、唯讀快照及設定檔損毀保護測試。

## 環境限制
目前開發環境沒有 .NET SDK、Windows、PowerShell，故本版尚未執行 restore/build/acceptance 或 UI 實機測試。
