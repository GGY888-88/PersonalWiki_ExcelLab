# PersonalWiki ExcelLab 2.5.0 更新說明

## 本版：唯讀資料庫診斷快照比較

新增 CLI 命令：

```powershell
dotnet run -c Release -- db-diagnostic-compare .\baseline-diagnostic.json .\current-diagnostic.json .\diagnostic-diff.json
```

比較兩份由 `db-diagnostic-report` 產生的 JSON 診斷快照，輸出：

- 資料庫路徑是否相同、SQLite Schema 版本是否變更。
- 前後健康狀態、資料庫檔案大小與容量差值。
- 條目數、全文索引缺漏數及孤兒數的前後差值。
- 共同資料表筆數變化，以及新增／消失的資料表名稱。
- `HasChanges` 彙總旗標，方便自動化監控判讀。

安全設計：

- 僅讀取診斷 JSON，不會開啟或修改來源 SQLite 資料庫。
- 僅接受指定格式 `personalwiki-database-diagnostic-v1`，驗證必要欄位、統計值及資料表名稱唯一性。
- 單份輸入報告上限 32 MiB；拒絕空檔、損毀 JSON、不相容格式及無效數值。
- 輸出不可與任一輸入報告相同；使用唯一暫存檔及 `File.Move(..., overwrite:false)` 提交，拒絕覆蓋既有目的檔。
- 比較報告只包含診斷統計與資料表名稱，不包含條目、筆記、自訂欄位或圖片內容。
- 不新增 NuGet 套件；SQLite Schema `user_version=1` 不變；資料庫及設定檔位置不變。

## 驗收

`TEST_GUIDE_2.5.0.md` 說明命令及整合驗收。Acceptance 新增相同快照零差異、輸入檔雜湊不變、輸出路徑與輸入衝突、拒絕覆蓋及損毀 JSON 拒絕案例。

目前開發容器沒有 .NET SDK、Windows 或 PowerShell，尚未執行 restore/build/self-test/acceptance/WinForms 實機測試。靜態檢查不等於執行期通過。
