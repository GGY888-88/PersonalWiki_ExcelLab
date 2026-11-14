# PersonalWiki ExcelLab 2.5.6 測試指南

## 診斷快照比較 acceptance
在 Windows 並安裝 .NET 10 SDK 後，於專案根目錄執行：

```powershell
dotnet restore .\PersonalWiki.ExcelLab.csproj
dotnet build .\PersonalWiki.ExcelLab.csproj -c Release
dotnet build .\PersonalWiki.Desktop\PersonalWiki.Desktop.csproj -c Release
.\scripts\run-acceptance-tests.ps1
```

腳本會建立全新隔離輸出資料夾；不得指定已存在資料夾。新增案例預期如下：

1. 原有 baseline/current 樣本：偵測 `entries` +1 及新增 `tags`。
2. 移除資料表樣本：偵測 `notes` 移除，且 `entries` 從 10 減至 8，Delta 為 -2。
3. Schema／異常樣本：偵測 Schema 版本 1→2、健康狀態惡化、quick_check 失敗、外鍵問題 1 筆、全文索引不一致及資料庫路徑改變。
4. 重複資料表名稱樣本：比較命令應以錯誤結束，不建立成功報告。
5. 所有輸入 JSON 只讀；輸出檔已存在時必須拒絕覆寫。

## 2.5.5 桌面 UI 回歸
仍須依 `DESKTOP_UI_TEST_MATRIX_2.5.5.md` 驗收主畫面按鈕 100×68 預設、自然寬度、資料庫路徑列填滿及 100%–300% DPI。

## 環境限制
目前開發環境無 .NET SDK、Windows、PowerShell；本次只完成原始碼／腳本／測試資料靜態檢查，尚未執行 restore、build、self-test、acceptance 或 WinForms UI。
