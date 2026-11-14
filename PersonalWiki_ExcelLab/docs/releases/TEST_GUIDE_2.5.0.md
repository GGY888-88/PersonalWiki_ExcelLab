# PersonalWiki ExcelLab 2.5.0 測試指南

## 環境

- Windows 10/11 x64
- .NET 10 SDK
- PowerShell
- NuGet 套件沿用 `DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`

## 建置與基本測試

```powershell
dotnet restore
dotnet build -c Release
dotnet run -c Release -- help
dotnet run -c Release -- self-test .\SelfTestOutput
```

## 診斷快照比較

先對同一測試資料庫產生兩份診斷快照，再比較：

```powershell
dotnet run -c Release -- db-init .\compare-test.db
dotnet run -c Release -- db-diagnostic-report .\compare-test.db .\baseline.json
dotnet run -c Release -- db-diagnostic-report .\compare-test.db .\current.json
dotnet run -c Release -- db-diagnostic-compare .\baseline.json .\current.json .\comparison.json
```

兩份快照來自同一資料庫且中間沒有寫入時，預期 `HasChanges=false`，`TableChanges`、`AddedTables`、`RemovedTables` 均為空。輸出檔不得已存在。

再對測試資料庫新增一筆測試條目並產生第三份快照，與 baseline 比較，確認 entries 表及條目數 delta 正確；測試只可使用隔離資料庫：

```powershell
dotnet run -c Release -- entry-add .\compare-test.db "診斷比較測試" "測試"
dotnet run -c Release -- db-diagnostic-report .\compare-test.db .\after-change.json
dotnet run -c Release -- db-diagnostic-compare .\baseline.json .\after-change.json .\comparison-after-change.json
```

## 隨附測試快照

專案內附兩份可直接比較的 JSON 測試資料：

- `samples/diagnostic-baseline-sample.json`
- `samples/diagnostic-current-sample.json`

執行：

```powershell
dotnet run -c Release -- db-diagnostic-compare .\samples\diagnostic-baseline-sample.json .\samples\diagnostic-current-sample.json .\sample-diagnostic-diff.json
```

預期 `HasChanges=true`、`EntriesDelta=1`、`AddedTables` 包含 `tags`，且 `entries` 的 `TableChanges.Delta=1`。此組 JSON 是比較器測試快照，不是可供正式還原或取代的 SQLite 資料庫。

## 負向測試

1. 將一個輸出目標預先建立，確認比較命令拒絕覆蓋且原始 SHA-256 不變。
2. 將輸出路徑指定成 baseline 或 current，確認命令拒絕執行。
3. 將輸入 JSON 改成損毀內容，確認命令回傳錯誤且不產生輸出。
4. 將 `Format` 改成未知版本，確認拒絕不相容報告。
5. 將 `DatabaseBytes` 或資料表 `Rows` 改成負值，確認拒絕無效統計。
6. 確認兩份輸入診斷 JSON 的 SHA-256 在比較前後一致。
7. 確認失敗後沒有留下 `*.tmp` 暫存檔。

## 整合 Acceptance

```powershell
.\scripts\run-acceptance-tests.ps1 -OutputRoot .\Acceptance_2_5_0
```

腳本會自行 restore/build，所有測試資料及資料庫都建立在新的隔離輸出資料夾。不要將正式資料庫路徑傳入測試命令。

## 尚未測試

本次開發環境缺少 `dotnet`、Windows、PowerShell，因此上述命令尚未實際執行。2.5.0 目前只有靜態檢查及 ZIP 完整性檢查，不可視為 Windows 執行期驗收通過。
