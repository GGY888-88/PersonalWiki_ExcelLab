# PersonalWiki ExcelLab 2.6.0 更新說明

## 匯入交易提交前安全閘
- 新增 `ImportTransactionGuard`：正式匯入交易提交前，於同一 SQLite 連線及交易內執行 `PRAGMA quick_check(1)` 與 `PRAGMA foreign_key_check`；任一檢查失敗即拒絕提交並回滾。
- 正式匯入連線在開始交易前明確啟用 `PRAGMA foreign_keys=ON`，避免依賴 SQLite 連線預設值。
- 匯入流程階段資訊明確記錄提交前檢查已完成；acceptance 增加安全匯入結果階段斷言。
- Excel 來源 SHA-256 與匯入前備份 SHA-256 改為串流計算，避免大型檔案整個載入記憶體。
- 保留原有「預檢 → 資料庫健康檢查 → 新路徑備份與驗證 → 輸入雜湊重驗 → 唯讀快照 → 單一交易匯入 → 匯入後批次驗證」順序。

## 相容性與資料安全
- SQLite `user_version=1` 不變；未新增或修改資料表、欄位、索引或 migration。
- NuGet 套件維持 `DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`。
- 不變更資料庫／設定檔位置；不覆蓋既有備份；來源 Excel 唯讀；不自動刪除或合併任何使用者資料。
- CLI 與 WinForms 專案版本同步為 2.6.0。

## 驗證狀態
- 已完成原始碼整合、專案 XML／樣本／版本／ZIP 靜態檢查。
- 此環境沒有 .NET SDK、Windows、PowerShell；未執行 restore/build、CLI acceptance、自我測試或 WinForms 實機測試。不可將本版視為已通過執行期驗收。
