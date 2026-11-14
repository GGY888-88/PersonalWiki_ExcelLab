# PersonalWiki ExcelLab 2.0.1 稽核報告

## 基準與版本
- 最新版本：2.0.1；下一版：2.0.2。
- 唯一基準：2.0.0 完整專案 ZIP。
- CLI／桌面 csproj 版本同步 2.0.1；SQLite `user_version=1`。

## 本版修正
- `PackageIntegrity.SaveManifest` 從可能覆蓋目的檔的 `File.WriteAllText` 改為 `FileMode.CreateNew`。既有目的檔不會被截斷或修改；寫入採獨佔建立。
- acceptance 腳本增加既有 manifest 不覆寫負向測試，透過 SHA-256 前後比對確認原檔保留。
- 更新 README、更新說明、測試指南、交接文件。

## 靜態檢查
- 專案 XML 解析、版本標記、測試程式碼插入點、ZIP CRC、測試 XLSX ZIP 結構。
- 確認 `SaveManifest` 不再使用覆寫型 API。

## 未執行
- 本容器沒有 `dotnet`、Windows、PowerShell；未執行 restore/build、CLI acceptance、self-test、WinForms 或 DPI 實機測試。
- 新增負向測試尚待 Windows PowerShell 執行。

## 安全與相容性
- SQLite Schema v1 不變、無 migration。
- NuGet `DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14` 不變。
- Excel 來源唯讀；備份／還原／匯出／報告不得覆蓋既有檔；圖片關聯移除不刪除原圖。

## 結論
本版完成一項可獨立交付的檔案覆寫安全修正；執行期驗收仍待 Windows/.NET 10。
