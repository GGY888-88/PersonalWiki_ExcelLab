# PersonalWiki ExcelLab 0.7.6

## 本版內容（nullable guard hardening）
- 集中加入 WorkbookPart 工作表集合及工作表關聯解析防護。
- 工作表關聯 ID 缺失、關聯目標不是 WorksheetPart 時，改為明確 InvalidDataException，不再直接 null dereference。
- OpenXmlReader 載入元素後明確確認為 Row；異常型別會清楚中止。
- Excel 匯入、預覽、品質檢查、清理分析及 Dry-run 共用上述防護。
- Dry-run 的 mapping/rules JSON 解析改為明確錯誤訊息，不使用 null-forgiving。
- Fixture 建立流程對必要 SheetData/Worksheet 節點增加檢查。
- SQLite Schema 維持 Version 1；匯入 transaction、安全備份與原始 Excel 唯讀限制不變。

## 相依套件
- DocumentFormat.OpenXml 3.5.1
- Microsoft.Data.Sqlite 10.0.14
- 無新增第三方套件。

## 測試狀態
- 已完成靜態程式碼修改與差異檢查。
- 尚未在本工作環境執行 .NET build/run；需在 Windows .NET SDK 10 環境執行 `scripts/run-acceptance-tests.ps1`。
- 驗收腳本新增 fixture 建立與 inspect smoke tests。
- 不宣稱 nullable warnings 已歸零；以 Windows build 輸出為準。

## 安全限制
- `write-copy` 仍只修改輸出副本，不提供原地覆寫來源 Excel。
- Excel 正式匯入仍以唯讀方式開啟來源，SQLite 單一交易寫入。
- 備份/報告/匯出不覆蓋既有目的檔；圖片關聯移除不刪除硬碟原圖。
