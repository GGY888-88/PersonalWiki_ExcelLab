# 4.1.0 測試指南

在專案根目錄執行：

```bash
python scripts/run-4.1.0-python-tests.py
```

測試涵蓋：
1. 4.1.0 桌面多選摘要複製按鈕、欄位內容、選取啟用狀態、唯讀／不修改資料庫契約。
2. 4.0.0 CLI 搜尋結果 JSON 匯出與目的檔拒絕覆寫。
3. 3.9.0 發行安全、3.8.0 圖片／相關條目、3.7.0 複合篩選、3.6.0 搜尋／備份／還原回歸。

測試只使用程式碼靜態檢查、合成 fixture 與暫存目錄，不接觸使用者正式 DB／Excel。

## Windows/.NET 10 待測
- `dotnet restore PersonalWiki.ExcelLab.csproj`
- `dotnet build PersonalWiki.ExcelLab.csproj -c Release`
- `dotnet build PersonalWiki.Desktop/PersonalWiki.Desktop.csproj -c Release`
- 開啟隔離測試 DB，搜尋並以 Ctrl／Shift 選取多筆結果。
- 驗證「複製選取摘要」啟用／停用、貼上欄位及多筆分隔正確。
- 確認複製前後 DB SHA256／資料筆數不變（如 DB 有 WAL，應先在關閉連線後以一致性備份方式比對，不可只雜湊主 DB 檔）。
- 驗證剪貼簿暫時不可用時錯誤訊息正常，應用程式不崩潰。
- WinForms 高 DPI、多螢幕及真實使用者流程仍待驗收。
