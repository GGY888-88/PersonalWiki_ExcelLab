# PersonalWiki ExcelLab 1.2.0（開發版）

## 桌面 Excel 匯入精靈
- 匯入流程先顯示來源活頁簿預覽，分工作表顯示欄位、資料列數及前 8 筆樣本；來源 Excel 以唯讀方式開啟。
- 新增視覺化欄位對應編輯器，依工作表分頁編輯每個來源欄位的用途（Unmapped / Title / Content / Attribute / Ignore）、目標欄位代碼及第一筆資料列。
- 產生並保存 mapping JSON；使用既有 ImportMappingService 驗證，遇到 Error 停止匯入，Warning 需使用者確認。
- 通過 mapping 驗證後，透過既有 ImportCleaningRulesService 建立預設人工複核規則檔，再執行既有 ExcelImportWorkflowService。
- 保留預檢、資料庫健康檢查、匯入前備份、來源設定重驗、交易式匯入及批次驗證流程。
- 新增 `samples/PersonalWiki_Import_Sample.xlsx`，提供可實際操作的匯入樣本（人物工作表及示範資料）。

## 安全與相容性
- SQLite Schema Version 1 不變。
- 原始 Excel 僅唯讀；不自動匯入。
- 匯入仍須使用者明確確認；備份目的檔存在時停止，不覆蓋。
- 匯入失敗沿用既有 transaction rollback；既有匯入後批次驗證失敗的復原限制不變。
- 未新增 NuGet 套件；DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14。

## 測試狀態
- 靜態檢查及 ZIP 完整性檢查；此工作環境沒有 dotnet CLI，未執行 Build、Windows Forms 啟動、匯入流程功能測試或 Windows x64 publish。
- 需於 Windows/.NET 10 SDK 環境集中驗收，不能視為正式驗收版。
