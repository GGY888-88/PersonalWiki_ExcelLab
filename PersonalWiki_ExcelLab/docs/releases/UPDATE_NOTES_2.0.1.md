# PersonalWiki ExcelLab 2.0.1 更新說明

## 版本定位
- 基準：2.0.0 完整專案。
- 本版：2.0.1。下一版：2.0.2。
- SQLite Schema `user_version=1` 不變；不新增 NuGet 套件。

## 完成項目
1. 修正 `Services/PackageIntegrity.cs` 的 `SaveManifest`：原本 `File.WriteAllText` 會直接覆蓋同名目的檔；現在使用 `FileMode.CreateNew`、`FileAccess.Write`、`FileShare.None`，目的檔已存在即拒絕建立，不會修改既有內容。
2. CLI 與桌面 csproj 版本標記更新為 2.0.1；CLI help、自我測試 JSON 版號同步更新。
3. `scripts/run-acceptance-tests.ps1` 新增負向測試：先建立既有 manifest，執行 `snapshot` 預期失敗，再比對 SHA-256 確認原檔未變。
4. 更新 README、測試指南、稽核報告與交接文件。

## 安全與相容性
- 原始 Excel 唯讀規則不變。
- SQLite Schema `user_version=1`，無 migration。
- NuGet：`DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`，無新增套件。
- 不變更資料庫／設定檔位置；備份還原仍拒絕覆蓋；圖片關聯移除不刪除原圖。

## 驗證狀態
- 已執行來源碼修改點、版本字串、專案 XML、測試腳本插入點及 ZIP/測試活頁簿靜態檢查。
- 開發容器缺少 `dotnet`、Windows、PowerShell；未執行 restore/build、CLI acceptance 或 WinForms 實機測試。
- 新增的 PowerShell 負向測試尚未在 Windows 執行。
