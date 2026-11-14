# PersonalWiki ExcelLab 2.0.1 測試指南

## 環境
Windows 10/11、.NET 10 SDK、PowerShell。解壓後使用全新測試輸出目錄。

## CLI 自動化驗收
```powershell
.\scripts\run-acceptance-tests.ps1 -OutputRoot .\AcceptanceTestOutput_201
```
腳本會 restore/build 並執行既有 2.0.0 回歸，以及 2.0.1 新增的套件清單覆寫防護測試。該測試會確認 `snapshot` 對已存在的 JSON 目的檔回傳錯誤，且原檔 SHA-256 不變。

## 手動測試 snapshot
```powershell
dotnet run -c Release -- snapshot .\samples\PersonalWiki_Import_Sample.xlsx .\package-manifest.json
```
第一次應建立清單。再次使用相同目的路徑執行，應回報錯誤且不得改變既有 `package-manifest.json`。若需另一份清單，請指定不存在的新路徑。

## 桌面人工驗收
沿用 `DESKTOP_UI_TEST_MATRIX_1.5.0.md` 及 `TEST_GUIDE_2.0.0.md`。必須使用隔離測試資料庫。

## 未執行項目
本次開發環境沒有 .NET 10 SDK、Windows、PowerShell；所有執行期測試仍待 Windows 環境完成，不可將靜態檢查視為通過。
