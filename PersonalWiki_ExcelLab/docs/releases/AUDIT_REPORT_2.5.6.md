# PersonalWiki ExcelLab 2.5.6 靜態稽核報告

## 修改範圍
- `scripts/run-acceptance-tests.ps1`：擴充診斷快照比較案例，並更新 TestPackRevision／版本。
- `samples/`：新增四份 JSON 測試樣本，覆蓋資料表移除、資料列減少、Schema 版本變更、健康／索引異常、資料庫路徑改變及重複資料表名稱。
- `Program.cs`、兩個 `.csproj`：版號更新為 2.5.6。
- `README.md`、`docs/releases/`：新增更新、測試、稽核及交接文件。
- `Services/DatabaseDiagnosticCompareService.cs` 未修改；比較輸出契約維持 `personalwiki-database-diagnostic-compare-v1`。

## 靜態驗證
- [x] 兩個 csproj XML 格式有效且版本為 2.5.6。
- [x] JSON 樣本可解析；測試預期值涵蓋表新增／移除及正／負筆數差異。
- [x] Acceptance 腳本保留既有覆寫防護與輸入雜湊檢查，並新增 schema／health／index 異常案例。
- [x] SQLite Schema 未變更，NuGet 依賴未變更。
- [ ] .NET 10 restore/build。
- [ ] Windows CLI acceptance 與 self-test。
- [ ] WinForms UI／DPI 實機驗收。

## 風險與限制
- 新增案例尚未在 Windows PowerShell/.NET 10 實際執行；PowerShell 版本相容性及 JSON 反序列化結果仍待實機確認。
- 本版主要擴充測試覆蓋，不宣稱診斷比較服務已通過實際執行期驗證。
