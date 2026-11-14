# PersonalWiki ExcelLab 2.0.0 稽核報告

## 基準及版本
- 基準為使用者提供的完整 1.5.1 ZIP；本版未從舊版重建。
- CLI 與桌面 csproj 版本均為 2.0.0。
- CLI help、自我測試報告及 acceptance 報告版號同步 2.0.0。

## 本次程式與測試變更
- `scripts/run-acceptance-tests.ps1` 現可取得 CLI stdout，新增筆記正文及自訂欄位值搜尋斷言，以及分頁頁碼／頁大小／回傳筆數斷言。
- 新增多工作表 Excel 測試檔，三張工作表採不同欄位版面並含缺值案例。
- 原有搜尋服務使用唯讀連線、參數化條件；本版增加回歸測試以驗證既有全文搜尋涵蓋筆記與自訂欄位，不改動搜尋資料庫結構。

## 靜態檢查範圍
- 專案 csproj XML 解析。
- 新增測試活頁簿工作表名稱、欄位標題、列數及缺值檢查。
- PowerShell acceptance 腳本新增區塊與版本標記檢查。
- 全專案版本標記、ZIP CRC 及外層資料夾檢查。

## 未執行項目
- 執行環境缺少 dotnet、Windows、PowerShell；未執行 restore/build、CLI acceptance、self-test、WinForms 啟動或 DPI 實測。
- 新增多工作表活頁簿尚未在 Windows 匯入精靈實際匯入。
- 本報告不把靜態檢查描述為執行期驗收通過。

## 安全與相容性
- SQLite `user_version=1`，Schema 不變。
- DocumentFormat.OpenXml 3.5.1、Microsoft.Data.Sqlite 10.0.14，無新增套件。
- 原始 Excel 唯讀；備份／還原／匯出／報告不覆蓋既有檔案；圖片關聯移除不刪除原始圖片；測試限定隔離資料。

## 結論
- 交付版本：2.0.0；下一開發版本：2.0.1。
- Windows/.NET 10 執行驗收仍待進行。
