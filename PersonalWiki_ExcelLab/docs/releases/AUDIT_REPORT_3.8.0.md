# 3.8.0 稽核報告

## 修改範圍
- `Services/EntryContentService.cs`：新增圖片說明更新及原圖 SHA256 唯讀驗證。
- `Services/RelatedEntryService.cs`：回傳共用標籤名稱，維持唯讀關聯建議。
- `PersonalWiki.Desktop/MainForm.cs`：新增圖片說明編輯、圖片檔案檢查及相關條目共用標籤欄。
- `scripts/test-3.8.0-image-relationship.py`：隔離 fixture 與靜態契約測試。

## 安全與相容性
- 不修改原始圖片；不會因檢查而更新 DB 雜湊。
- 移除圖片關聯不刪除原始圖片。
- 相關條目仍唯讀，不建立持久關係。
- Schema 維持 v1；不新增套件、不變更 DB／設定檔路徑。

## 驗證限制
- Python fixture／靜態契約檢查由 `run-3.8.0-python-tests.py` 執行。
- 本環境沒有 .NET SDK／Windows，未執行 C# 編譯、Microsoft.Data.Sqlite 執行期、CLI acceptance 或 WinForms 實機測試。
- 圖片檔案被其他程式同時寫入、網路磁碟及高 DPI 版面仍需 Windows 實機驗收。
