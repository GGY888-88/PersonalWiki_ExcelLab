# PersonalWiki ExcelLab 2.5.6 更新說明

## 診斷快照比較測試擴充
- 保留既有 `db-diagnostic-compare` 功能與 JSON 格式，沒有修改比較服務邏輯。
- Acceptance 新增資料表移除、資料列減少與負差值、Schema 版本改變、資料庫健康狀態惡化、全文索引不一致、資料庫路徑改變等案例。
- 新增重複資料表名稱的無效 JSON 快照，確認比較服務拒絕重複表名。
- 保留原有相同快照零差異、資料表新增、筆數增加、輸入檔雜湊不變、輸出拒絕覆寫、輸入輸出路徑衝突及無效 JSON 測試。

## 新增測試資產
- `samples/diagnostic-removed-table-baseline-sample.json`
- `samples/diagnostic-removed-table-current-sample.json`
- `samples/diagnostic-schema-unhealthy-sample.json`
- `samples/diagnostic-duplicate-table-invalid-sample.json`

## 保留項目
- CLI `net10.0`；桌面 `net10.0-windows`。
- SQLite Schema `user_version=1`，未修改資料表或 migration。
- NuGet `DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14` 不變。
- 2.5.5 主畫面按鈕 100×68 預設、自然寬度及資料庫路徑列調整完整保留。
