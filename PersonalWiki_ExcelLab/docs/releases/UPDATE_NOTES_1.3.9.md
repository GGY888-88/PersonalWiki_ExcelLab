# PersonalWiki ExcelLab 1.3.9 更新說明

## 維護與安全修正
- 修正 `scripts/run-acceptance-tests.ps1` 中批次來源核對及圖片完整性報告路徑使用未定義 `$temp` 的錯誤，改為使用本次指定的 `$OutputRoot`。
- 驗收摘要版本與測試包修訂資訊更新為 1.3.9。
- `scripts/build-portable.ps1` 不再自動刪除既有 `publish` 資料夾；目的資料夾已存在即停止，避免誤刪先前產物或使用者檔案。
- Portable 發布腳本新增 `dotnet publish` 結束碼檢查，失敗時明確回報。
- CLI 與 Windows 桌面專案版本號同步更新為 1.3.9，README 版本資訊重新整理。

## 相容性與資料安全
- SQLite Schema 維持 `user_version=1`，無資料庫遷移。
- `DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14` 不變，無新增 NuGet 套件。
- 本版只修改專案文件及建置／驗收腳本，不修改既有資料庫、條目、筆記、標籤、圖片關聯、原始圖片或原始 Excel。

## 驗證限制
- 已完成靜態檢查與 ZIP 封裝檢查。
- 執行環境沒有 .NET SDK，未能執行 restore/build、自動化 CLI 測試或 Windows Forms 實機測試；不可視為完整執行驗收通過。
