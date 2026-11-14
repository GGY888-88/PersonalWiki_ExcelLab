# PersonalWiki ExcelLab 2.0.4 靜態稽核報告

## 修改範圍
- `Services/ExcelImportInputSnapshot.cs`：快照內容驗證完成後標記唯讀；Dispose 清理時只解除本次唯一暫存目錄內檔案唯讀屬性。
- `Services/ExcelImportPreflightService.cs`：SHA-256 改用固定緩衝區串流，並以 `FileShare.Read` 開啟輸入。
- `Program.cs`：self-test 加入 1 MiB 快照案例，檢查唯讀、雜湊、副檔名、清理、錯誤雜湊拒絕及來源不變；版本更新為 2.0.4。
- CLI／桌面 csproj 與 acceptance 測試包版號更新為 2.0.4。

## 安全檢查
- 不變更 SQLite Schema、資料庫位置、設定檔位置或 NuGet 套件。
- 原始輸入仍以唯讀方式處理；快照仍使用唯一 GUID 暫存目錄及 `CreateNew`。
- 正式匯入仍只讀取已驗證快照，來源追溯仍記錄原始 Excel 路徑。
- 暫存清理不會觸及暫存目錄以外的檔案。

## 靜態驗證
- 已檢查兩個 csproj XML 可解析且版本均為 2.0.4。
- 已檢查 CLI help／self-test／acceptance 版本標記、串流 SHA-256、快照唯讀屬性及清理流程。
- 已檢查 self-test 包含唯讀、雜湊、副檔名、清理、錯誤雜湊拒絕及原始來源不變斷言。
- 已確認本版修改的 C#／csproj／PowerShell 檔案無殘留 2.0.3 版號。
- ZIP CRC 與完整打包檢查於封裝後執行。
- 本環境缺少 .NET SDK、Windows、PowerShell，未執行編譯及執行期測試。

## 尚未驗證
- Windows 檔案分享鎖定、磁碟滿載、強制終止、超大型檔案記憶體表現及 WinForms DPI 仍待實機驗收。
