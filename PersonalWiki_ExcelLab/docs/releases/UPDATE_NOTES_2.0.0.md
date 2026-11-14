# PersonalWiki ExcelLab 2.0.0 更新說明

## 版本定位
以 1.5.1 為唯一基準整合至 2.0.0；不重建舊功能、不變更 SQLite Schema、不新增 NuGet 套件。

## 本版完成項目
- 將 CLI 與 Windows Forms 專案版本標記統一為 2.0.0；修正 CLI help 遺留的舊版本文字。
- 將完整 CLI acceptance test pack 標記為 2.0.0，並加入全文搜尋回歸：自訂欄位值、筆記正文均可命中正確條目；另驗證分頁參數與結果上限。
- 強化 acceptance 測試執行器的輸出回傳，讓測試可解析 JSON 並對搜尋結果作明確斷言。
- 新增 `samples/PersonalWiki_MultiLayout_Test.xlsx`，包含「人物」「設備」「閱讀清單」三張欄位名稱、數量與順序不同的工作表，並包含缺值案例，供工作表獨立版面與缺值留白測試。
- 保留 1.5.1 所有匯入安全、備份／還原、圖片關聯、封存還原及資料稽核功能。

## 安全與相容性
- SQLite `user_version=1`，Schema 不變。
- `DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`，無新增套件。
- 原始 Excel 唯讀；測試活頁簿只作為匯入測試來源。備份、還原、報告仍拒絕覆蓋既有目的檔；移除圖片關聯不刪除原圖。

## 驗證狀態
- 已完成專案檔 XML、PowerShell 腳本結構、樣本活頁簿工作表結構、ZIP 完整性與版本標記靜態檢查。
- 本開發容器沒有 .NET SDK、Windows 或 PowerShell，未能執行 `dotnet restore/build`、CLI acceptance 或 WinForms 實機測試；不可視為執行驗收通過。
