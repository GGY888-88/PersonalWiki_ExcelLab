# 4.0.0 測試指南

在專案根目錄執行：

```bash
python scripts/run-4.0.0-python-tests.py
```

測試涵蓋：
1. 4.0.0 CLI 搜尋結果 JSON 匯出契約、JSON 可解析、既有目的檔拒絕覆寫。
2. 3.9.0 發行安全基線。
3. 3.8.0 圖片／相關條目、3.7.0 複合篩選、3.6.0 排序／分頁／備份／還原回歸。

測試只使用合成 fixture 與暫存目錄，不接觸使用者正式 DB／Excel。

## Windows/.NET 10 待測
- `dotnet restore PersonalWiki.ExcelLab.csproj`
- `dotnet build PersonalWiki.ExcelLab.csproj -c Release`
- `dotnet build PersonalWiki.Desktop/PersonalWiki.Desktop.csproj -c Release`
- 建立隔離 DB，執行 `entry-query ... --output <新檔案>`，確認 JSON 內容。
- 再以同一輸出路徑執行一次，確認程式拒絕覆寫且原檔 SHA256 不變。
- 確認不帶 `--output` 時仍輸出原本 JSON stdout。
- WinForms 高 DPI、多螢幕與實際使用者流程尚未測試。
