# 3.8.0 測試指南

在專案根目錄執行：

```powershell
python scripts/run-3.8.0-python-tests.py
```

新增 `test-3.8.0-image-relationship.py`，以隔離 SQLite fixture、臨時圖片檔及靜態契約檢查驗證圖片說明更新、原始檔不變、共用標籤顯示、封存條目排除、唯讀關係查詢及 Schema v1 保留。此腳本不執行 C# 程式。

## Windows 手動驗收
1. 開啟 `samples/PersonalWiki_3.6.0_Test.db`（先複製到臨時資料夾，勿直接測試唯一樣本）。
2. 選取含圖片關聯的條目，在「圖片」頁選取圖片，編輯說明並確認重新載入後仍存在。
3. 檢查圖片檔案：有效樣本應顯示 SHA256 一致；缺失樣本應顯示找不到檔案。
4. 將樣本圖片複製到另一個臨時位置並修改副本，建立新關聯後再修改該副本，確認檢查能指出 SHA256 不一致。
5. 移除圖片關聯後，確認原始圖片檔仍存在。
6. 在「相關條目」頁確認共用標籤名稱、數量及相同類型資訊；確認封存條目不出現。
7. 操作前後檢查 `PRAGMA user_version` 仍為 1，資料庫沒有新增資料表或欄位。

Python 測試不等於 .NET 編譯、Microsoft.Data.Sqlite 執行期或 Windows WinForms 驗收。
