# 3.9.0 測試指南

## 跨平台測試
在專案根目錄執行：

```bash
python scripts/run-3.9.0-python-tests.py
```

測試順序：
1. 3.9.0 發行安全／版本／相依套件／Schema／樣本完整性檢查。
2. 3.8.0 圖片與相關條目功能測試。
3. 3.7.0 日期及複合篩選測試。
4. 3.6.0 搜尋排序、大資料分頁、備份 WAL／鎖定、還原安全及發行靜態驗證。

測試只操作專案內的合成樣本及隔離 SQLite fixture，不使用使用者正式資料庫或原始 Excel。

## 發行 ZIP

```bash
python scripts/package-release.py --output ../PersonalWiki_ExcelLab_3.9.0.zip
```

若目標 ZIP 已存在，工具會拒絕覆寫。成功後應再次檢查 ZIP CRC、根目錄、必要檔案及 `RELEASE_MANIFEST_3.9.0.sha256`。解壓根目錄固定 `PersonalWiki_ExcelLab`。

## Windows 驗收（目前環境未執行）
- `dotnet restore PersonalWiki.ExcelLab.csproj`
- `dotnet build PersonalWiki.ExcelLab.csproj -c Release`
- `dotnet build PersonalWiki.Desktop/PersonalWiki.Desktop.csproj -c Release`
- 執行 CLI acceptance／self-test。
- WinForms 搜尋、複合篩選、圖片說明、圖片雜湊檢查、相關條目、備份還原及高 DPI／多螢幕實機測試。

不得將 Python 靜態測試結果寫成上述 .NET／Windows 驗收已通過。
