# PersonalWiki ExcelLab 2.0.2 測試指南

## 環境
Windows 10/11、.NET 10 SDK、PowerShell。使用全新測試輸出目錄，不可指向正式資料庫或正式 Excel。

## 自動化驗收
```powershell
.\scripts\run-acceptance-tests.ps1 -OutputRoot .\AcceptanceTestOutput_202
```
腳本會 restore/build 並執行既有回歸與新增 no-overwrite 測試。新增測試會建立預覽 JSON、品質報告、mapping 範本、清理規則範本及 Dry-run 報告，再以同一目的路徑重跑，確認命令失敗且 SHA-256 不變。

## 手動覆寫測試
```powershell
dotnet run -c Release -- import-preview .\samples\PersonalWiki_Import_Sample.xlsx --json .\preview.json
dotnet run -c Release -- import-preview .\samples\PersonalWiki_Import_Sample.xlsx --json .\preview.json
```
第一次建立報告；第二次必須拒絕，原始 `preview.json` 內容不得改變。品質報告、mapping template、clean-rules template、Dry-run 亦應遵循相同規則。

## 預檢輸入穩定性
預檢會在分析前後比對來源 Excel、mapping、rules 的 SHA-256。若任何檔案在掃描期間被修改，預檢應停止並要求重跑。正式匯入流程還會在備份後再次預檢並比對雜湊，若變動則不匯入，匯入前備份保留。

## 未執行項目
本版開發環境沒有 .NET SDK、Windows、PowerShell；需在 Windows 執行上述自動化驗收及桌面人工驗收。300% DPI、多螢幕、XLSM/VBA、Power Query、外部連線、密碼保護工作表及超大型活頁簿仍需另行驗證。
