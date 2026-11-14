# PersonalWiki ExcelLab 2.0.4 測試指南

## 環境
- Windows 10/11、.NET 10 SDK、PowerShell。
- 使用完整解壓後的 `PersonalWiki_ExcelLab` 資料夾。

## 自動驗收
在專案根目錄執行：

```powershell
.\scripts\run-acceptance-tests.ps1 -OutputRoot .\AcceptanceTestOutput_204
```

輸出目錄必須不存在；測試只使用隔離資料庫與測試檔，不得指向正式資料庫。腳本會 restore/build，並呼叫 `self-test`。

## 2.0.4 快照自我測試
`self-test` 會建立 1 MiB 測試用 `.xlsm` 位元組檔及兩份 JSON 設定檔，驗證：
1. 快照三個檔案均標記唯讀。
2. 快照 Excel 保留 `.xlsm` 副檔名。
3. 快照 SHA-256 與來源一致。
4. Dispose 後唯一暫存目錄已清除。
5. 傳入錯誤預期雜湊會拒絕建立快照。
6. 原始測試檔雜湊不變。

注意：此自測是快照服務測試，不代表該位元組檔是有效 Excel 活頁簿。正式匯入的完整 Excel 相容性仍由既有 acceptance 測試活頁簿驗證。

## Windows 實機補充測試
- 在大型 XLSX/XLSM（建議 100 MB 以上）執行預檢，觀察工作集記憶體；串流雜湊不應將整個檔案一次載入 RAM。
- 在另一個程序嘗試寫入／刪除正在被預檢或快照複製的來源檔；Windows 應依 `FileShare.Read` 拒絕衝突存取。
- 測試暫存磁碟空間不足、快照建立時取消／強制終止、暫存清理失敗；確認來源及正式資料庫不受影響，匯入前備份仍保留。
- 強制終止可能留下唯一暫存資料夾；不得自動清除其他程序或使用者的暫存資料。

## 未執行項目
目前開發容器無 .NET SDK、Windows、PowerShell，以上 build、self-test、acceptance 及實機項目均尚未執行。
