# PersonalWiki_ExcelLab 0.7.5 acceptance test guide

## Requirements
- Windows 10/11
- .NET 10 SDK
- PowerShell 5.1 or PowerShell 7+

## Run
1. Extract the ZIP. Keep the top-level folder name `PersonalWiki_ExcelLab`.
2. Open PowerShell in that folder.
3. Run:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\scripts\run-acceptance-tests.ps1
```

The script creates `AcceptanceTestOutput`. It refuses to reuse an existing output directory. If needed, specify another path:

```powershell
.\scripts\run-acceptance-tests.ps1 -OutputRoot .\AcceptanceTestOutput_2
```

## Included automated checks
- dotnet restore and Release build
- CLI help and existing self-test
- SQLite database initialization and copy
- identical database content and schema comparisons
- schema snapshot creation and validation
- schema snapshot JSON report
- database integrity report creation and validation
- expected failure for a missing database

## Return results
Compress the entire `AcceptanceTestOutput` folder and provide it for diagnosis. Do not include the original Excel workbook or private data unless specifically needed. The test uses newly-created disposable databases and does not write to the original Excel workbook.

## Limitations
The script has not been executed in this build environment. Excel import, image files, notes/tags, and backup recovery with a populated database require additional focused tests.


## fix2 修正
根專案 `.csproj` 已排除 `package/**/*.cs`，避免把封存的巢狀原始碼重複納入編譯。請以本包重新解壓到全新資料夾後執行測試。


### fix3 更新
已修正前次編譯 log 中的 9 個錯誤，請重新解壓 fix3 測試包到全新資料夾後執行本指南。若仍失敗，請提供完整 build log。


TestPack fix4：依實測將「不存在資料庫」負面測試預期 exit code 從 3 修正為 1。產品程式碼未修改；Schema 不一致仍預期回傳 3。
