# 4.3.0 測試指南

## 自動測試
在專案根目錄執行：

```bash
python scripts/run-4.3.0-python-tests.py
python -m py_compile scripts/*.py
```

測試涵蓋：
- 完整關鍵字搜尋診斷（FTS5 與關聯資料表 LIKE）。
- 建立／更新日期起訖與含起含迄邊界。
- 封存篩選及結果排序。
- 4.2.0 至 3.6.0 歷史回歸測試。

## Windows/.NET 10 驗收（本次環境未執行）
```powershell
dotnet restore .\PersonalWiki.ExcelLab.csproj
dotnet build .\PersonalWiki.ExcelLab.csproj -c Release
dotnet build .\PersonalWiki.Desktop\PersonalWiki.Desktop.csproj -c Release
```
使用隔離測試 DB 執行範例：
```powershell
dotnet run --project .\PersonalWiki.ExcelLab.csproj -- entry-query-plan .\samples\PersonalWiki_3.6.0_Test.db needle --created-from 2024-01-01 --updated-to 2024-12-31
```
確認輸出包含 Keyword/日期條件、COUNT 與 Result 計畫，並確認原始測試 DB 的 SHA256 前後一致。不得對正式資料庫執行寫入型測試。
