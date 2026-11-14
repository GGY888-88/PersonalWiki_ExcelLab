# PersonalWiki ExcelLab 2.0.3 測試指南

## 環境
- Windows 10/11
- .NET 10 SDK
- PowerShell
- 解壓完整 ZIP，解壓後資料夾名稱為 `PersonalWiki_ExcelLab`

## 自動驗收
在專案根目錄執行：

```powershell
.\scripts\run-acceptance-tests.ps1 -OutputRoot .\AcceptanceTestOutput_203
```

輸出目錄必須事先不存在。腳本會自行 restore/build，並建立隔離資料庫與測試檔；新增一項安全 Excel 匯入成功測試，驗證快照匯入及原始來源路徑追溯。不要將正式資料庫或正式匯入來源放進自動測試目錄。

## 正式 Excel 安全匯入快照驗收
1. 使用 `samples/PersonalWiki_Import_Sample.xlsx` 或 `samples/PersonalWiki_MultiLayout_Test.xlsx` 的副本進行測試；不可使用正式來源檔。
2. 建立 mapping 與 rules，確認 mapping 通過且 Dry-run 沒有待人工檢查列。
3. 建立全新測試資料庫，並指定一個尚不存在的備份路徑。
4. 執行安全匯入：

```powershell
 dotnet run -c Release -- excel-import-db-safe .\AcceptanceTestOutput_203\import-test.db .\samples\PersonalWiki_Import_Sample.xlsx .\AcceptanceTestOutput_203\mapping.json .\AcceptanceTestOutput_203\rules.json .\AcceptanceTestOutput_203\import-test-backup.db
```

5. 確認匯入成功後，執行 `excel-import-batches` 與 `excel-import-verify`；批次來源路徑應是原始 Excel 路徑，不得是 `%TEMP%\PersonalWiki-Import-*`。
6. 檢查系統暫存目錄，流程正常結束後本次 `PersonalWiki-Import-*` 暫存資料夾應已清理。

## 輸入變更負向測試
- 在匯入快照建立期間，嘗試以另一個程序開啟來源檔並寫入。Windows 上若來源檔已由快照程序以 `FileShare.Read` 開啟，寫入端應被拒絕或延後；若檔案在快照前已變更，雜湊不符應停止匯入。
- 對 Excel、mapping、rules 分別測試變更；任何一項不符都不得開始匯入交易。
- 確認資料庫匯入前備份仍存在且健康；停止匯入時不得產生新的 import batch。
- 不要用正式資料庫執行負向測試。

## 未在目前開發容器執行
本版未能執行 .NET restore/build、PowerShell acceptance、Windows 檔案分享鎖定行為及 WinForms 實機測試；請以以上步驟集中驗收。
