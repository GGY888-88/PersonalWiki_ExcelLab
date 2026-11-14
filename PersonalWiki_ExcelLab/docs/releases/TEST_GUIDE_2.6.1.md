# PersonalWiki ExcelLab 2.6.1 測試指南

## 環境
Windows 11、.NET 10 SDK、PowerShell。請在解壓後的 `PersonalWiki_ExcelLab` 根目錄執行。所有測試必須使用隔離資料庫與新輸出路徑。

## 建置與回歸

```powershell
dotnet restore .\PersonalWiki.ExcelLab.csproj
dotnet build .\PersonalWiki.ExcelLab.csproj -c Release
dotnet build .\PersonalWiki.Desktop\PersonalWiki.Desktop.csproj -c Release
.\scripts\run-acceptance-tests.ps1
.\scripts\run-self-test.ps1
```

## 2.6.1 新增驗收：大型備份安全報告重新驗證
1. 使用隔離測試資料庫執行 `db-init`。
2. 執行 `excel-import-db-safety-json` 建立新的備份及 JSON 報告。
3. 執行 `excel-import-db-safety-check <report.json>`，確認雜湊、資料庫健康及 Schema 檢查均通過。
4. 對大型備份（建議至少 1 GiB，若測試環境資源不足可使用較小檔案）重複步驟 2–3，觀察驗證期間程序記憶體；雜湊流程應以固定 128 KiB 緩衝區串流處理，不得使用 `File.ReadAllBytes`。
5. 在另一程序持有備份檔案的寫入／刪除鎖時執行檢查，應安全失敗，不得回報雜湊通過。
6. 複製一份備份並修改其內容，再以該檔案路徑更新測試報告或使用對應測試資料，確認雜湊不符時拒絕驗證。不可修改正式備份或使用者資料。

## 安全回歸
- 匯入仍須在提交前通過 `quick_check(1)` 與 `foreign_key_check`。
- 匯入失敗必須回滾；來源 Excel 唯讀。
- 備份、報告及匯出目的檔已存在時必須拒絕覆蓋。
- SQLite `PRAGMA user_version` 必須維持 `1`。

## 未完成實機驗收
目前開發容器沒有 .NET SDK、Windows、PowerShell；本指南命令尚未在本環境執行。另需持續完成 2.6.0 原定交易故障注入、WAL／鎖定／中止情境及桌面 DPI 驗收。
