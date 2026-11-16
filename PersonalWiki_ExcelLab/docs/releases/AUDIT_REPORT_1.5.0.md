# PersonalWiki ExcelLab 1.5.0 稽核與開發報告

## 開發基準
- 直接以 1.3.9 完整專案為基礎，未從舊版重建。
- 整合里程碑：1.4.0（回歸測試矩陣）、1.4.5（SQLite 備份／還原強化）、1.5.0（整合文件與驗收包）。

## 本次程式修正

### SQLite 備份與還原採暫存提交
`Services/DatabaseBackupService.cs`
- 備份及還原先寫入目標資料夾內以 GUID 命名的 staging 檔案。
- staging 資料庫通過 `PRAGMA integrity_check` 與 `PRAGMA foreign_key_check` 後，才使用不覆蓋的 `File.Move` 提交到正式目的路徑。
- 無效來源、備份 API 錯誤或完整性驗證失敗時，清理由該次操作建立的 staging 檔案及 SQLite sidecar。
- 明確拒絕來源與目的路徑相同。
- 不會刪除、覆蓋或修改來源資料庫。
- 復原驗證清理使用本次呼叫成功提交旗標；若因競爭條件遇到其他程序先建立目的路徑，不會刪除該外部檔案。

### 復原驗證失敗處理
`Services/DatabaseRecoveryService.cs`
- 原先還原後若健康狀態／Schema／逐表筆數不一致，會回傳 `IsVerified=false` 並留下新還原副本。
- 現改為驗證不通過時拋出明確錯誤，進入既有清理路徑，刪除僅由本次流程建立的目標副本，避免將未驗證資料庫誤認為可用還原結果。

## 自動化回歸測試增補
`scripts/run-acceptance-tests.ps1`
- 建立隔離測試資料庫及測試條目。
- 檢查／重建 FTS 索引。
- 建立備份、檢查備份健康、還原並比較逐表資料內容。
- 驗證既有目的檔不被覆蓋、來源與目的相同會拒絕。
- 驗證無效 SQLite 檔案會被拒絕，且不留下 staging 檔或還原目的檔。
- 所有輸出位於新建 OutputRoot；OutputRoot 已存在即停止。

## 靜態檢查
- CLI 與桌面 csproj 版本均為 1.5.0。
- CLI 自我測試報告版本、驗收報告版本、README、更新說明及測試指南已同步。
- 專案 XML 可由 XML parser 解析。
- ZIP 封裝後將檢查 CRC、檔案清單及外層資料夾名稱。

## 未能執行項目／限制
- 環境沒有 `dotnet`、`pwsh`，也不是 Windows；無法執行 restore/build、PowerShell 腳本、CLI 自動化測試或 WinForms 實機測試。
- 因此程式變更屬已完成的原始碼修改，但執行期行為仍待 .NET 10／Windows 環境驗證。
- UI DPI、多螢幕、縮放、工作表欄位顯示及批次封存列入 UI 測試矩陣，尚未執行。
- Excel VBA、XLSM 巨集執行、Power Query、外部連線、密碼保護工作表及超大型活頁簿相容性仍不在本次驗收範圍。

## 資料安全界線
- SQLite Schema `user_version=1` 不變；沒有 migration。
- NuGet 相依套件不變：DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14。
- 原始 Excel 唯讀。
- 不自動刪除或合併條目、筆記、標籤或圖片。
- 移除圖片關聯不刪除硬碟原圖。
- 備份、還原、報告及匯出不覆蓋既有目的檔。
- 不修改使用者正式資料庫；所有測試設計使用隔離 fixture。

## 版本結論
- 最新版本：**1.5.0**。
- 下一開發版本：**1.5.1**。
- 本次已完成原始碼及交付文件更新；不宣稱未執行的 build／UI 測試通過。
