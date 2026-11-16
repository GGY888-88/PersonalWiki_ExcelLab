# PersonalWiki ExcelLab 2.0.3 靜態稽核報告

## 修改範圍
- `Services/ExcelImportInputSnapshot.cs`：建立私有唯一暫存快照，使用 `FileShare.Read` 唯讀來源，`FileMode.CreateNew` 建立複本並保留 `.xlsx`／`.xlsm` 副檔名，串流 SHA-256，檢查預期指紋與來源複製後指紋，並在 dispose 時清理暫存目錄。
- `Services/ExcelImportWorkflowService.cs`：完成資料庫安全備份後，建立並重新預檢快照；正式匯入改讀快照。
- `Services/ExcelDatabaseImportService.cs`：增加來源追溯路徑參數，維持資料庫追溯記錄為原始 Excel 路徑。
- CLI／桌面版本與 acceptance 測試包版號更新為 2.0.3；新增安全匯入成功流程測試，確認批次追溯保留原始檔案路徑。

## 安全檢查
- 原始 Excel、mapping、rules 僅以唯讀方式開啟，不修改來源檔。
- 快照檔使用唯一 GUID 暫存目錄與 `CreateNew`，不覆蓋其他檔案。
- 快照內容必須等於資料庫備份後預檢所得 SHA-256；不一致即停止。
- 快照建立完成後重新驗證原始檔雜湊，再對快照執行完整預檢。
- 正式匯入只使用快照，縮小預檢與實際讀取之間的 TOCTOU 時間窗。
- 原始來源路徑仍寫入 import batch 與 entry source trace。
- 不變更 Schema、資料庫位置、設定檔位置或 NuGet 套件。

## 靜態驗證
- 檢查兩個 csproj XML 可解析且版本為 2.0.3。
- 檢查 CLI 版號文字及自我測試報告版號為 2.0.3。
- 檢查 workflow 的匯入呼叫使用快照路徑，並將原始路徑傳入追溯參數。
- 檢查新增快照檔案包含 CreateNew、FileShare.Read、SHA-256 及清理流程。
- ZIP CRC、必要文件、測試活頁簿及固定內層資料夾名稱於打包後另行確認。

## 尚未驗證
- 缺少 .NET SDK，未能編譯；缺少 Windows／PowerShell，未能執行自動驗收及 WinForms。
- Windows 檔案分享鎖定與同步雲端資料夾／網路磁碟行為需實機驗證。
- 大型活頁簿快照所需暫存空間、磁碟滿載及程序中止後暫存目錄清理需測試。
- 進程強制終止可能留下暫存資料夾；不會因此覆蓋來源或正式資料庫。
