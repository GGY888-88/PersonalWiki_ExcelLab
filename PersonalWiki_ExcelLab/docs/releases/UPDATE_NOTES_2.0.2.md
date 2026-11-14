# PersonalWiki ExcelLab 2.0.2 更新說明

## 版本
- 基準：2.0.1；本版：2.0.2；下一版：2.0.3。
- SQLite Schema `user_version=1` 不變；沒有新增 NuGet 套件。

## 完成項目
1. 新增 `Services/SafeFileOutput.cs`，統一使用 `FileMode.CreateNew`、`FileAccess.Write`、`FileShare.None` 建立新輸出檔。既有目的檔不會被截斷或修改；寫入失敗時只嘗試刪除本次已成功建立的半成品。
2. CLI 的 `import-preview --json`、`import-quality` JSON、`import-dry-run`、`import-clean-check` JSON、自我測試報告、mapping template、clean-rules template 改用安全輸出。
3. 桌面端重複條目 CSV、標籤 CSV 匯出及 UI DPI 診斷檔使用相同安全輸出方法；避免覆寫既有 CSV／診斷紀錄。
4. `ExcelImportPreflightService.Check` 在掃描前先取得 Excel、mapping、rules 的 SHA-256，分析結束後再次計算；任一輸入變動即丟出 IOException，不回傳可能過期的預檢結果。
5. 桌面 mapping／rules 編輯器與桌面設定檔更新改用同目錄唯一暫存檔，完整寫入後再以 move 替換；失敗時清理暫存檔，避免直接截斷既有設定。
6. Acceptance 新增五項覆寫保護測試，涵蓋預覽、品質報告、mapping 範本、清理規則範本、Dry-run 報告；每項都以 SHA-256 比對原檔不變。
7. CLI 與桌面專案版本同步為 2.0.2。

## 保留項目與安全
- SQLite Schema `user_version=1`，無 migration。
- NuGet：`DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`，無新增套件。
- 來源 Excel 唯讀；正式匯入仍執行預檢、健康檢查、建立並驗證新備份、匯入前再次比對三個輸入雜湊、交易式匯入及匯入後驗證。
- 不自動刪除／合併條目、筆記、標籤或來源追溯資料；移除圖片關聯不刪除原圖。

## 驗證狀態
- 靜態檢查：程式碼輸出點盤查、版本標記、專案 XML、腳本測試插入點、測試活頁簿 ZIP 結構。
- 未執行：dotnet restore/build、自我測試、PowerShell acceptance、WinForms 實機測試。原因：目前容器未安裝 .NET SDK、Windows、PowerShell。
