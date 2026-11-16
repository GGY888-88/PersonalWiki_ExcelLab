# PersonalWiki ExcelLab 2.0.5 更新說明

## 修改內容
1. 桌面設定檔保護：既有 `desktop-settings.json` 若 JSON 損毀或內容為 null，儲存前讀取設定會停止並保留原檔，不再靜默使用預設值覆蓋可能可復原的資料。一般載入畫面仍保留既有容錯顯示行為。
2. SafeFileOutput 自我測試：驗證 `CreateNew` 拒絕覆寫、既有檔案 SHA-256 不變、原子替換成功、替換失敗時清除暫存檔。
3. 匯入快照測試由 1 MiB 擴大至 8 MiB，測試檔以固定緩衝區串流產生，避免測試本身一次配置整份檔案。
4. CLI／桌面版本與 acceptance test pack 更新為 2.0.5。

## 未變更
- SQLite `user_version=1`，無 Schema migration。
- NuGet：`DocumentFormat.OpenXml 3.5.1`、`Microsoft.Data.Sqlite 10.0.14`，無新增套件。
- 資料庫及桌面設定檔位置不變。
- 原始 Excel 唯讀、匯入備份、快照、交易與批次驗證流程不變。

## 驗證限制
已執行原始碼靜態檢查、專案 XML 解析、版本標記檢查及 ZIP 完整性檢查。因環境缺少 .NET SDK、Windows、PowerShell，未執行 restore/build、self-test、acceptance 或 WinForms 實機測試。
