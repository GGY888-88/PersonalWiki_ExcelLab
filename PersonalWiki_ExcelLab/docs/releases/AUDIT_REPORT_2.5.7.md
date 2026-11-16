# PersonalWiki ExcelLab 2.5.7 靜態稽核報告

## 修改範圍
- `Services/DatabaseDiagnosticCompareService.cs`：增加警告清單差異欄位，並把警告新增／移除納入 `HasChanges`；驗證警告項目不可為 null、空白或純空格。
- `scripts/run-acceptance-tests.ps1`：新增警告-only 變更案例，更新測試包版號。
- `samples/diagnostic-warning-baseline-sample.json`、`samples/diagnostic-warning-current-sample.json`：新增可重複使用的警告差異測試樣本。
- `Program.cs`、CLI／桌面 csproj：版號更新為 2.5.7。
- 新增更新說明、測試指南、稽核報告及本交接文件。

## 靜態檢查
- [x] 新增 JSON 樣本可解析。
- [x] 比較報告 record 欄位與建構參數順序一致。
- [x] 警告差異以 Ordinal 比較，結果排序且去重。
- [x] `HasChanges` 已納入新增及移除警告。
- [x] 原有比較格式識別字及 CLI 參數未變。
- [x] SQLite Schema 與 NuGet 版本未變。
- [ ] .NET 10 restore/build。
- [ ] Windows CLI acceptance／self-test。
- [ ] WinForms UI／DPI 實機驗收。

## 風險與相容性
- 比較報告仍使用 `personalwiki-database-diagnostic-compare-v1`，僅新增 JSON 欄位；寬鬆 JSON 消費端通常可忽略未知欄位。若有嚴格 schema 驗證的外部消費端，需同步允許新增欄位。
- 警告以文字完全相同（Ordinal）作為同一項；訊息文字改寫會呈現為一筆移除及一筆新增。
- 本環境無 .NET SDK、Windows、PowerShell，不能宣稱編譯或執行期測試通過。
