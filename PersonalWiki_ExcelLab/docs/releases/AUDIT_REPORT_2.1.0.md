# 2.1.0 靜態稽核報告

## 修改範圍

- 新增 `Services/DatabaseDiagnosticReportService.cs`：整合既有唯讀資料庫健康檢查、全文索引檢查、資料表筆數及 SQLite 儲存頁面統計，輸出單一 JSON 診斷報告。
- `Program.cs`：新增 `db-diagnostic-report` 命令；健康或全文索引異常時回傳代碼 3；CLI 版本更新為 2.1.0。
- CLI 與桌面 csproj 版本更新為 2.1.0。
- `scripts/run-acceptance-tests.ps1`：新增診斷報告健康狀態、報告結構、來源 DB 雜湊不變及目的檔拒絕覆寫案例。
- 新增 2.1.0 更新說明、測試指南及交接文件。

## 安全檢查

- SQLite 診斷使用唯讀連線及單一 deferred 讀取交易快照；不呼叫資料修改、索引重建、VACUUM 或修復 API。
- 報告先寫入唯一暫存檔，再以 `File.Move(..., overwrite:false)` 原子提交；不覆蓋既有輸出，失敗清理由本次建立的暫存檔。
- 報告不序列化條目、筆記、自訂欄位值或圖片檔路徑。
- Schema `user_version=1` 未變；NuGet 相依套件未變。
- 原有匯入快照、備份／還原、SafeFileOutput、設定檔復原保護均未修改。

## 靜態檢查與限制

已檢查專案 XML、版號一致性、CLI 路由、測試腳本新增案例及新增服務的唯讀／CreateNew 實作。開發環境沒有 .NET SDK、Windows、PowerShell，因此無法執行編譯、self-test、PowerShell acceptance 或桌面執行測試。靜態檢查不等同執行期通過。
