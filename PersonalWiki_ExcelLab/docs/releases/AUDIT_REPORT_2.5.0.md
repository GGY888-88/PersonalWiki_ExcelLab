# PersonalWiki ExcelLab 2.5.0 靜態稽核報告

## 修改範圍

- 新增 `Services/DatabaseDiagnosticCompareService.cs`：讀取並驗證兩份 2.1.0 格式診斷快照，計算資料庫健康、Schema、容量、全文索引及資料表筆數差異。
- `Program.cs`：新增 `db-diagnostic-compare <baseline.json> <current.json> <report.json>`；CLI help/self-test 版號更新為 2.5.0。
- CLI／桌面 csproj 版本更新為 2.5.0。
- `scripts/run-acceptance-tests.ps1`：新增相同快照比較、輸入雜湊不變、輸入輸出路徑衝突、拒絕覆寫及損毀 JSON 負向案例。
- 新增 2.5.0 更新說明、測試指南及交接文件。

## 安全檢查

- 比較器只讀取診斷 JSON，不開啟 SQLite，不會寫入來源資料庫。
- 限制輸入報告為已知格式，並檢查必要欄位、非負統計值及唯一資料表名稱；單檔最大 32 MiB。
- 輸出路徑不得與輸入報告相同；輸出採唯一暫存檔及不覆蓋原子提交。
- 比較報告不含條目正文、筆記、自訂欄位值、圖片內容。
- Schema `user_version=1` 未變；NuGet 相依套件未變。
- 原有 Excel 快照、正式匯入、備份／還原、SafeFileOutput、桌面設定檔保護及唯讀診斷功能未修改。

## 靜態檢查與限制

已檢查 CLI 路由、版號標記、csproj XML、比較器輸入驗證及 CreateNew 暫存輸出流程、Acceptance 測試新增案例、ZIP CRC 與封裝路徑。開發環境沒有 .NET SDK、Windows、PowerShell，未執行 restore/build/self-test/acceptance 或 WinForms 執行測試。不得將未執行測試標示為通過。
