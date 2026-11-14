# 4.7.0 DEV02 交接

基準：4.7.0 DEV01（以 4.6.6 為底）。新增 Excel 匯入批次歷史唯讀檢視視窗。

新增檔案：
- `PersonalWiki.Desktop/ExcelImportBatchHistoryDialog.cs`

修改檔案：
- `PersonalWiki.Desktop/MainForm.cs`：新增入口與開啟方法。
- 專案版本標示及 BAT 版本顯示更新。
- 本文件及更新紀錄。

功能依賴既有 `ExcelImportBatchManagementService.List` 與 `Report`，不新增 Schema、不寫入資料庫。

未執行測試及編譯（依使用者指示）。待後續集中測試時，需驗證：
1. 無匯入批次時顯示空狀態。
2. 多批次依時間倒序，最多 500 筆。
3. 選取批次摘要數字與資料庫一致。
4. 損毀／缺欄位資料庫錯誤訊息安全顯示。
5. 100% 至 300% DPI 視窗及欄位可讀性。
6. 確認唯讀視窗不改變 SQLite 檔案。

下一項建議：4.7.0 DEV03 增加兩批匯入紀錄差異比較 UI，重用 `ExcelImportBatchDiffService`，仍保持唯讀；實作前檢查資料量及顯示分頁策略。
