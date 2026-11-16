# PersonalWiki ExcelLab 2.6.0 靜態稽核報告

## 修改範圍
- `Services/ImportTransactionGuard.cs`：新增交易提交前 SQLite 完整性及外鍵檢查。
- `Services/ExcelDatabaseImportService.cs`：匯入連線啟用外鍵檢查；提交前呼叫 guard；失敗時盡力回滾且保留原始例外；來源雜湊改串流計算。
- `Services/ExcelImportWorkflowService.cs`：安全匯入階段記錄提交前檢查。
- `Services/ExcelImportDatabaseSafetyService.cs`：備份雜湊改串流計算。
- CLI／桌面 csproj、CLI help、自我測試報告及 acceptance 測試包版號同步為 2.6.0。
- 新增本版更新說明、測試指南、稽核報告及交接文件。

## 靜態驗證
- [x] SQLite Schema 版本常數仍為 1；未新增 migration。
- [x] NuGet 套件版本未變。
- [x] CLI 與桌面 csproj 版本同步為 2.6.0。
- [x] 既有 2.5.6–2.5.8 診斷比較樣本與測試保留。
- [x] 新增交易 guard 呼叫點位於 `tx.Commit()` 之前。
- [x] 匯入前來源及備份雜湊改為串流方式。
- [ ] .NET 10 restore/build。
- [ ] Windows CLI acceptance／self-test。
- [ ] WinForms UI／DPI 實機驗收。

## 風險與限制
`quick_check(1)` 是 SQLite 快速一致性檢查，不等同完整 `integrity_check`；匯入前備份仍會執行既有完整 `integrity_check` 與 `foreign_key_check`。本次未能執行 .NET 編譯或執行期測試，因此仍需 Windows/.NET 10 環境確認 Microsoft.Data.Sqlite 實際行為與測試結果。
