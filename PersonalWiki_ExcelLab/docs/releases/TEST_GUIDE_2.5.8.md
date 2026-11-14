# PersonalWiki ExcelLab 2.5.8 測試指南

## 環境
Windows、.NET 10 SDK、PowerShell。測試只使用 samples 內 JSON，不連線或修改正式資料庫。

## 建置及完整 acceptance
在專案根目錄執行：

```powershell
dotnet restore .\PersonalWiki.ExcelLab.csproj
dotnet build .\PersonalWiki.ExcelLab.csproj -c Release
dotnet build .\PersonalWiki.Desktop\PersonalWiki.Desktop.csproj -c Release
.\scripts\run-acceptance-tests.ps1
```

## 新增負向案例
以下樣本必須被拒絕，且不得產生比較報告：
- `diagnostic-invalid-page-metrics-sample.json`：空閒頁面大於總頁面。
- `diagnostic-invalid-search-index-sample.json`：索引計數與 `IsConsistent=true` 矛盾。
- `diagnostic-invalid-health-count-sample.json`：外鍵問題數與明細筆數不一致。

Acceptance 會另外回歸 2.5.6 的資料表／Schema／健康狀態差異，以及 2.5.7 warning-only 比較、輸入雜湊不變和拒絕覆寫。

## 手動執行單一案例

```powershell
$report = Join-Path $env:TEMP 'pw-diagnostic-semantic-check.json'
if (Test-Path $report) { throw '請使用新的輸出檔名' }
dotnet run --project .\PersonalWiki.ExcelLab.csproj -c Release -- db-diagnostic-compare .\samples\diagnostic-invalid-page-metrics-sample.json .\samples\diagnostic-current-sample.json $report
```

預期 CLI 回傳錯誤，且 `$report` 不存在。另兩份樣本可替換第一個輸入路徑重複測試。

## 未完成實機測試
本開發環境無 .NET SDK、Windows、PowerShell；尚未執行 restore/build/acceptance。桌面 100%–300% DPI、多螢幕及正式使用者環境仍待驗收。
