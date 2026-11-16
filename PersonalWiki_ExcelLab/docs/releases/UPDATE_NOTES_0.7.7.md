# PersonalWiki ExcelLab 0.7.7

## 新增：資料庫條目唯讀稽核
- 新增 `db-entry-audit <database.db> <report.json>`。
- 檢查條目 `normalized_title` 是否符合 FormKC + trim + invariant uppercase 標準化結果。
- 找出相同標準化標題但不同類型的條目，列為人工複核候選；不判定為確定重複。
- 檢查圖片關聯指向的檔案是否存在；只報告，不移除關聯、不刪除圖片。
- JSON 報告使用 CreateNew，拒絕覆蓋既有檔案。

## 安全與相容性
- 唯讀開啟 SQLite；不修改資料庫、不變更 Schema（仍為 Version 1）。
- 不修改來源 Excel。
- 不新增第三方套件。
- 既有匯入、備份、還原及索引流程不變。

## 驗證狀態
- 本版依使用者要求先持續開發，不等待人工測試。
- 尚未執行 Windows .NET 10 build/run；不能標示為已驗收。
