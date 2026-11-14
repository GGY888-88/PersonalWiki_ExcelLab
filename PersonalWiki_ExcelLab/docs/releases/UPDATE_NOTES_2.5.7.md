# PersonalWiki ExcelLab 2.5.7 更新說明

## 診斷快照比較：警告差異
- `db-diagnostic-compare` 現在會保留 baseline/current 的警告清單，並輸出 `WarningsBefore`、`WarningsAfter`、`AddedWarnings`、`RemovedWarnings`。
- `HasChanges` 納入警告新增／移除；即使健康旗標及計數相同，只要診斷警告內容改變，也會標示有差異。
- 警告依序以 Ordinal 比較並排序；重複警告在比較結果中去重，輸入快照本身不會被修改。
- 新增兩份警告專用 JSON 樣本與 acceptance 案例，覆蓋「只有警告文字改變」的情況。
- 無警告差異時，新增欄位為空陣列；原有 `personalwiki-database-diagnostic-compare-v1` 格式識別字保留，舊版讀取端可忽略新增欄位。

## 未變更
- SQLite Schema `user_version=1`。
- NuGet：`DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`。
- CLI `net10.0`、桌面 `net10.0-windows`。
- 資料庫位置、設定檔位置及既有資料安全限制。

## 驗證狀態
- 已完成來源靜態檢查、JSON 樣本解析、csproj XML 解析及 ZIP 完整性檢查。
- 目前環境沒有 .NET SDK、Windows、PowerShell；未執行 restore/build、CLI self-test、acceptance 或 WinForms 實機測試。
