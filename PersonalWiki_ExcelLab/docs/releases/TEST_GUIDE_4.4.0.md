# 4.4.0 測試指南

## Python／SQLite／靜態契約測試
在專案根目錄執行：

```powershell
python scripts/run-4.4.0-python-tests.py
python -m py_compile scripts/*.py
```

新增測試 `test-4.4.0-query-benchmark-and-parity.py`：檢查 Benchmark 的迭代限制、暖機、統計值、結果漂移保護、唯讀聲明、CLI 參數及 CLI/Desktop 共用查詢服務；並使用隔離 SQLite fixture 驗證總數、排序、分頁不重疊與查詢計畫輸出。

## CLI 真實資料庫驗收（Windows／.NET 10）
先複製正式資料庫到隔離測試目錄，對副本執行：

```powershell
dotnet run --project PersonalWiki.ExcelLab.csproj -- entry-query-benchmark "<測試副本.db>" "關鍵字" --type "人物" --tag "研究" --archived exclude --sort updated-desc --page 1 --page-size 50 --iterations 5
```

確認 JSON 包含 `ReadOnly=true`、迭代次數、樣本及 min/median/average/max；重複執行時結果 ID／總數應穩定。請勿對正式資料庫執行破壞性測試。

## 尚未完成的環境驗收
目前開發環境沒有 .NET SDK／Windows，因此尚未執行 .NET restore/build、真實 Microsoft.Data.Sqlite CLI、WinForms 實機及跨電腦效能比較。Python fixture 不等同於上述驗收。
