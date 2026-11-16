# PersonalWiki ExcelLab 2.0.4 更新說明

## 變更
1. 匯入快照建立並完成雜湊驗證後，將三個快照檔（Excel、mapping、rules）標記為唯讀，避免後續匯入程式意外寫入。
2. Dispose 清理快照時，僅針對本次 GUID 唯一暫存目錄中的檔案解除唯讀屬性，再嘗試刪除目錄；清理失敗不會遮蔽原始匯入例外。
3. `ExcelImportPreflightService.HashFile` 改用 `FileStream` + `SHA256.HashData(Stream)` 串流計算，避免大型活頁簿整檔載入記憶體；`FileShare.Read` 允許其他讀者並阻止 Windows 上的並行寫入／刪除。
4. `self-test` 新增快照安全自測：1 MiB `.xlsm` 測試檔、快照唯讀、雜湊一致、副檔名保留、Dispose 清理、雜湊不符拒絕、原始檔未變。

## 保留項目
- SQLite `user_version=1`，無 Schema migration。
- 不新增 NuGet 套件。
- 正式匯入流程仍依序預檢、健康檢查、備份驗證、再次預檢、快照、快照預檢、交易匯入及批次驗證。
- 原始 Excel、mapping、rules 不修改；來源追溯仍使用原始 Excel 路徑。

## 驗證狀態
- 已完成程式碼靜態檢查、專案 XML／版號檢查、文件及 ZIP 檢查。
- 本環境未安裝 .NET SDK、Windows 或 PowerShell，未執行 `dotnet restore/build`、CLI self-test、acceptance 或 WinForms 實機測試。
