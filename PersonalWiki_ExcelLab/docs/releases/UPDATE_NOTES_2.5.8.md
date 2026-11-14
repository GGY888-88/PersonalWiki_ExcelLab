# PersonalWiki ExcelLab 2.5.8 更新說明

## 診斷快照語意一致性驗證
- `DatabaseDiagnosticCompareService` 新增跨欄位驗證，拒絕頁面統計矛盾（空閒頁面不得大於總頁面、使用頁面必須等於總頁面減空閒頁面）。
- 全文索引計數只接受全部為 `-1` 的不可用狀態，或全部非負的有效狀態；`IsConsistent` 必須與 missing／orphan／entry／indexed 計數一致。
- `QuickCheckPassed` 必須與 quick_check 清單內容一致；外鍵問題筆數必須等於明細筆數。
- 診斷報告、健康檢查及全文索引的資料庫路徑必須一致；快照來源必須標示為唯讀。
- `IsHealthy=true` 時必須符合健康旗標、索引一致、無警告及資料表筆數有效等條件。
- 新增三份無效 JSON 樣本，並加入 acceptance 預期拒絕測試。

## 相容性與安全
- 比較報告輸出格式識別字維持 `personalwiki-database-diagnostic-compare-v1`；CLI 參數不變。
- SQLite `user_version=1` 不變；NuGet 套件及資料庫／設定檔位置不變。
- 輸入快照仍只讀；比較報告仍使用唯一暫存檔及拒絕覆寫提交。
