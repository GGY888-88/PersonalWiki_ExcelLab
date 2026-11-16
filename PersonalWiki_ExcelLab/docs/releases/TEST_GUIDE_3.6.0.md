# 3.6.0 測試指南

## 自動測試

```powershell
python scripts/run-3.6.0-python-tests.py
```

包含：
- `test-3.6.0-search-sorting.py`：排序、穩定排序、分頁、封存條件及唯讀 fixture。
- `test-3.6.0-database-recovery-safety.py`：備份／還原 sidecar 與失敗清理靜態安全檢查。
- `test-3.6.0-backup-wal-locking.py`：Python SQLite WAL 線上備份及資料完整性 fixture。
- `test-3.6.0-search-large-dataset.py`：25,000 筆資料、8,334（實際測試以查詢結果為準）匹配筆數、分頁不重疊及索引計畫。
- `verify-release.py`：版本、相依套件、文件、樣本與安全程式碼靜態檢查。

## 桌面人工驗收
1. 開啟 `samples/PersonalWiki_3.6.0_Test.db`。
2. 選取任一條目，確認「標籤」頁可新增／移除標籤；不可刪除條目或其他資料。
3. 確認「來源追溯」頁可查看 Excel 檔案、工作表、列號與匯入時間；複製路徑不修改來源檔案。
4. 確認「相關條目」頁只顯示建議，不建立持久關係。
5. 確認圖片有效／缺失關聯都可顯示；移除圖片關聯時，硬碟原圖必須保留。
6. 在多張圖片間切換、篩選、排序及切換條目，觀察快取與記憶體使用。
7. 備份／還原時，預先建立目的地主檔或 `-wal`／`-shm`／`-journal` 任一 sidecar，確認程式拒絕覆蓋。

## 限制
Python SQLite fixture 不等於 Microsoft.Data.Sqlite 10.0.14 執行期測試；需在 Windows + .NET 10 完成 restore/build、CLI acceptance、自我測試及 WinForms 實機驗收。
