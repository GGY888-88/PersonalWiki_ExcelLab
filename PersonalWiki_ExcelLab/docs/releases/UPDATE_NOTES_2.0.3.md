# PersonalWiki ExcelLab 2.0.3 更新說明

## 本版內容
1. 新增 `Services/ExcelImportInputSnapshot.cs`：正式匯入建立唯一暫存輸入快照，來源檔以唯讀方式開啟，快照以 `CreateNew` 建立，保留原始 `.xlsx`／`.xlsm` 副檔名，串流計算 SHA-256，並與預檢時指紋比對。
2. 快照複製完成後再次雜湊原始 Excel、mapping、rules；任一輸入在快照期間變更即停止匯入。
3. 對快照再執行完整預檢，並確認三個 SHA-256 與備份後預檢結果相同，才交給正式匯入服務。
4. `ExcelDatabaseImportService.Import` 增加可選原始來源追溯路徑；實際讀取快照，但 `import_batches.source_file`、`entry_sources.source_file` 及回傳結果仍使用原始 Excel 路徑。
5. 匯入流程結束時清理本次唯一暫存目錄；清理失敗不會遮蔽原匯入結果或例外。
6. CLI 與桌面版本同步為 2.0.3。

## 保留項目
- SQLite `user_version=1`，無 Schema migration。
- NuGet `DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`，無新增套件。
- 保留 1.3.4–2.0.2 所有功能及安全限制。

## 驗證狀態
- 已完成原始碼靜態檢查、專案 XML／版本欄位檢查、ZIP 完整性檢查。
- 此容器缺少 .NET SDK、Windows、PowerShell；未執行 restore/build、CLI acceptance、匯入實際執行、WinForms 或競態實機測試。
