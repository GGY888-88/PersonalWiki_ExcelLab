# PersonalWiki ExcelLab 2.0.5 測試指南

## 環境
Windows 10/11、.NET 10 SDK、PowerShell。使用完整解壓後的 `PersonalWiki_ExcelLab` 資料夾。

## 自動驗收
在專案根目錄執行：

```powershell
.\scripts\run-acceptance-tests.ps1 -OutputRoot .\AcceptanceTestOutput_205
```

輸出目錄必須不存在。所有自動化測試應使用腳本建立的隔離資料庫與測試檔，不得指向正式資料庫。

## CLI self-test 新增項目
- 8 MiB `.xlsm` 位元組測試檔以固定 128 KiB 緩衝區串流產生。
- 驗證唯讀快照、雜湊、副檔名、Dispose 清理、錯誤雜湊拒絕及原始來源不變。
- 驗證既有輸出拒絕覆寫，且原檔 SHA-256 不變。
- 驗證設定檔原子替換完成後內容完整。
- 將目標設為既有目錄，驗證替換失敗且唯一暫存檔清理。

## 桌面設定檔損毀復原測試（Windows 實機）
1. 關閉 PersonalWiki。
2. 備份 `%LOCALAPPDATA%\PersonalWiki\desktop-settings.json` 到測試資料夾。
3. 在測試環境將設定檔內容暫時改為無效 JSON（不得操作正式使用者設定檔）。
4. 啟動桌面程式並執行會儲存設定的操作。
5. 確認儲存被拒絕、錯誤訊息指出設定檔無效，原檔內容雜湊不變。
6. 從備份還原測試設定檔，確認程式可正常儲存。

## 尚未執行
目前開發環境無 .NET SDK、Windows、PowerShell；以上 build、self-test、acceptance 與桌面測試尚未執行。8 MiB 測試檔是測試位元組流，不是有效 Excel 活頁簿。
