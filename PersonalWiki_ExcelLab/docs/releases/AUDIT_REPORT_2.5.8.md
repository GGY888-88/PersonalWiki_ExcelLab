# PersonalWiki ExcelLab 2.5.8 靜態稽核報告

## 修改範圍
- `Services/DatabaseDiagnosticCompareService.cs`：新增診斷快照跨欄位語意驗證。
- `scripts/run-acceptance-tests.ps1`：加入三種互相矛盾快照負向測試，更新測試包版本。
- `samples/diagnostic-invalid-page-metrics-sample.json`、`diagnostic-invalid-search-index-sample.json`、`diagnostic-invalid-health-count-sample.json`：新增可重複使用測試資料。
- CLI／桌面 csproj、CLI 版本輸出、自我測試報告版本更新為 2.5.8。
- 新增本版更新說明、測試指南、稽核及交接文件。

## 驗證規則
- SQLite 頁面統計不可互相矛盾。
- 全文索引計數只允許完整不可用 sentinel（全為 -1）或完整非負計數；一致性旗標須符合實際計數。
- quick_check 成功旗標須符合其回傳內容。
- 外鍵問題數須等於明細筆數。
- 報告、健康檢查、索引檢查的資料庫路徑一致，且來源標示唯讀。
- 健康旗標為 true 時，必要子狀態必須全部符合健康條件。

## 靜態檢查狀態
- [x] 三份新增 JSON 樣本可解析。
- [x] 兩個 csproj XML 可解析且版本為 2.5.8。
- [x] 版本標記、文件索引及測試包版號更新。
- [x] Schema `user_version=1`，NuGet 版本未變。
- [ ] .NET 10 restore/build。
- [ ] Windows CLI acceptance／self-test。
- [ ] WinForms UI／DPI 實機驗收。

## 限制
本開發環境沒有 .NET SDK、Windows、PowerShell，不能宣稱已編譯或執行 acceptance。語意驗證以目前診斷報告產生器的欄位定義為準；若未來變更報告產生器，需同步更新驗證規則及樣本。
